using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Core.Enums;
using MultiRobotSimulator.Core.Models;
using MultiRobotSimulator.WPF.Events;
using MultiRobotSimulator.WPF.Utils;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public partial class EditorCanvasView : UserControl, IHandle<CanvasRedrawEvent>
    {
        private readonly DrawingGroup _backingStore = new DrawingGroup();
        private readonly ILogger<EditorCanvasView> _logger;
        private readonly double _pixelsPerDip = 1;
        private readonly Typeface _typeface = new Typeface("Arial");
        private RootViewModel? _rootVM;
        private EditorTabViewModel? _tab;

        public EditorCanvasView(IEventAggregator eventAggregator, ILogger<EditorCanvasView> logger)
        {
            DataContextChanged += OnDataContextChanged;

            eventAggregator.Subscribe(this);
            _logger = logger;
            _pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }

        public double CellSize { get; private set; }

        private double LineWidth => CellSize * 0.05;

        private void DrawTextAtCenter(DrawingContext drawingContext, object text, Point center)
        {
            var str = text.ToString() ?? string.Empty;
            var size = CellSize * (3d / 4) * Math.Min(0.9, 2d / str.Length); // experimental values

            var formattedText = new FormattedText(str.Trim(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, size, Brushes.Black, null, TextFormattingMode.Display, _pixelsPerDip)
            {
                MaxTextHeight = CellSize,
                MaxTextWidth = CellSize
            };
            var origin = new Point(center.X - (formattedText.WidthIncludingTrailingWhitespace / 2), center.Y - (formattedText.Height / 2));
            drawingContext.DrawText(formattedText, origin);
        }

        private void HandleMouse(MouseEventArgs e)
        {
            if (_tab is null || _rootVM is null)
            {
                return;
            }

            var editorAction = e.GetEditorAction();

            if (editorAction == EditorAction.Nothing)
            {
                return;
            }

            _rootVM.AlgoResult = null;

            var screenPos = e.GetPosition(this);
            var canvasPos = screenPos.ScreenToCanvas(CellSize);

            var t = _tab.Map.Vertices.GetTileOnScreenPos(screenPos, CellSize);

            if (!(t is Tile tile))
            {
                return;
            }

            if (editorAction == EditorAction.Remove)
            {
                _tab.HasChanges |= _tab.Map.RemoveFromTile(tile);
            }

            if (editorAction == EditorAction.Add)
            {
                var drawingMode = _rootVM.DrawingMode;
                _tab.HasChanges |= _tab.Map.AddToTile(tile, drawingMode);
            }

            Render();
        }

        private void Render()
        {
            if (_tab != _rootVM?.ActiveItem)
            {
                return;
            }

            _logger.LogTrace("Canvas render '{name}'", _tab?.DisplayName);

            Execute.PostToUIThread(() =>
            {
                var sw = Stopwatch.StartNew();
                // https://stackoverflow.com/a/44426783
                var drawingContext = _backingStore.Open();
                Render(drawingContext);
                drawingContext.Close();
                sw.Stop();

                _logger.LogTrace("{action} took {ms} ms", "Render", sw.ElapsedMilliseconds);
            });
        }

        private void Render(DrawingContext drawingContext)
        {
            // Map wasn't set yet - nothing to render
            if (_tab is null || CellSize == 0)
            {
                return;
            }

            // assure pixel perfect drawing
            var halfPenWidth = LineWidth / 2;
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(halfPenWidth);
            guidelines.GuidelinesY.Add(halfPenWidth);
            guidelines.Freeze();
            drawingContext.PushGuidelineSet(guidelines);

            // draw background
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(new Point(), RenderSize));

            RenderGraph(drawingContext);
            RenderGrid(drawingContext);

            if (_rootVM?.RenderPaths ?? false)
            {
                RenderPaths(drawingContext);
            }

            RenderTiles(drawingContext);

            // pop back guidelines set
            drawingContext.Pop();
        }

        private void RenderGraph(DrawingContext drawingContext)
        {
            if (_rootVM?.RenderGraph != true || _tab is null)
            {
                return;
            }

            var graph = _tab.Map;
            var edges = graph.Edges;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                foreach ((var t1, var t2) in edges)
                {
                    if (!(t1 is Tile tile1) || !(t2 is Tile tile2))
                    {
                        continue;
                    }

                    ctx.BeginFigure(tile1.GetRect(CellSize).Center(), true, true);
                    ctx.LineTo(tile2.GetRect(CellSize).Center(), false, false);
                }
            }

            geometry.Freeze();

            drawingContext.DrawGeometry(null, new Pen(Brushes.Purple, 1), geometry);
        }

        private void RenderGrid(DrawingContext drawingContext)
        {
            var width = RenderSize.Width;
            var height = RenderSize.Height;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                for (var i = 0d; i < width; i += CellSize)
                {
                    ctx.BeginFigure(new Point(i, 0), true, true);
                    ctx.LineTo(new Point(i, height), false, false);
                }

                for (var i = 0d; i < height; i += CellSize)
                {
                    ctx.BeginFigure(new Point(0, i), true, true);
                    ctx.LineTo(new Point(width, i), false, false);
                }
            }

            geometry.Freeze();

            drawingContext.DrawGeometry(null, new Pen(Brushes.Gray, LineWidth), geometry);
        }

        private void RenderPaths(DrawingContext drawingContext)
        {
            var paths = _rootVM?.AlgoResult?.Paths;
            if (paths is null || paths.Count == 0)
            {
                return;
            }

            for (var p = 0; p < paths.Count; p++)
            {
                var path = paths.ElementAt(p);
                if (path.Count == 0)
                {
                    continue;
                }

                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(path.First().GetRect(CellSize).Center(), true, false);
                    ctx.PolyLineTo(path.Select(t => t.GetRect(CellSize).Center()).ToList(), true, true);
                }

                geometry.Freeze();

                drawingContext.DrawGeometry(null, new Pen(DistinctColors.GetBrush(p), LineWidth*3), geometry);
            }
        }

        private void RenderTiles(DrawingContext drawingContext)
        {
            if (_tab is null)
            {
                return;
            }

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                foreach (var obstacle in _tab.Map.Vertices.Where(t => !t.Passable))
                {
                    var rect = obstacle.GetRect(CellSize);
                    ctx.BeginFigure(rect.TopLeft, true, true);
                    ctx.PolyLineTo(new List<Point>() { rect.TopRight, rect.BottomRight, rect.BottomLeft }, false, false);
                }
            }

            geometry.Freeze();

            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            for (var i = 0; i < _tab.Map.Starts.Count; i++)
            {
                var rect = _tab.Map.Starts[i].GetRect(CellSize);
                var center = rect.Center();
                drawingContext.DrawEllipse(Brushes.Green, null, center, CellSize / 2, CellSize / 2);
                DrawTextAtCenter(drawingContext, i + 1, center);
            }

            for (var j = 0; j < _tab.Map.Targets.Count; j++)
            {
                var rect = _tab.Map.Targets[j].GetRect(CellSize);
                var center = rect.Center();
                drawingContext.DrawEllipse(Brushes.Red, null, center, CellSize / 2, CellSize / 2);
                DrawTextAtCenter(drawingContext, j + 1, center);
            }
        }

        private void SetCanvasSize(Size size)
        {
            _logger.LogDebug("SetCanvasSize: {size}", size);

            // Map wasn't set yet - nothing to render
            if (_tab is null)
            {
                return;
            }

            var mapRatio = (double)_tab.Map.Width / _tab.Map.Height;
            var canvasRatio = size.Width / size.Height;

            double canvasWidth;
            double canvasHeight;

            if (mapRatio > canvasRatio)
            {
                canvasWidth = size.Width;
                canvasHeight = canvasWidth / mapRatio;
            }
            else
            {
                canvasHeight = size.Height;
                canvasWidth = canvasHeight * mapRatio;
            }

            Width = canvasWidth;
            Height = canvasHeight;

            CellSize = Math.Min(canvasWidth / _tab.Map.Width, canvasHeight / _tab.Map.Height); // should be same, Math.Min just to be sure
        }

        #region Event handlers

        public void Handle(CanvasRedrawEvent message)
        {
            Render();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            HandleMouse(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            HandleMouse(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Render();

            drawingContext.DrawDrawing(_backingStore);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _logger.LogDebug("DataContextChanged: '{newContext}'", e.NewValue);

            if (e.NewValue is EditorCanvasViewModel editorCanvasViewModel)
            {
                _tab = editorCanvasViewModel.EditorTab;
                _rootVM = (RootViewModel)_tab.Parent;

                editorCanvasViewModel.ResizeEventHandler = OnResize;

                SetCanvasSize(editorCanvasViewModel.InitialSize);
            }
        }

        private void OnResize(object sender, SizeChangedEventArgs e)
        {
            SetCanvasSize(e.NewSize);
        }

        #endregion Event handlers
    }
}
