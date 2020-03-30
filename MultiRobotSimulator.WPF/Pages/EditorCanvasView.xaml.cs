using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Core.Enums;
using MultiRobotSimulator.WPF.Events;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public partial class EditorCanvasView : UserControl, IHandle<CanvasRedrawEvent>
    {
        private readonly DrawingGroup _backingStore = new DrawingGroup();
        private readonly ILogger<EditorCanvasView> _logger;
        private readonly Typeface _typeface = new Typeface("Arial");
        private EditorTabViewModel? _tab;

        public EditorCanvasView(IEventAggregator eventAggregator, ILogger<EditorCanvasView> logger)
        {
            DataContextChanged += OnDataContextChanged;

            eventAggregator.Subscribe(this);
            _logger = logger;
        }

        public double CellSize { get; private set; }

        private FormattedText GetFormattedText(object text)
        {
            return new FormattedText(text.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, 12, Brushes.Black, null, TextFormattingMode.Display, VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                TextAlignment = TextAlignment.Center,
                MaxTextHeight = CellSize,
                MaxTextWidth = CellSize
            };
        }

        private void HandleMouse(MouseEventArgs e)
        {
            if (_tab is null)
            {
                return;
            }

            var editorAction = e.GetEditorAction();

            if (editorAction == EditorAction.Nothing)
            {
                return;
            }

            var screenPos = e.GetPosition(this);
            var canvasPos = screenPos.ScreenToCanvas(CellSize);

            var tile = _tab.Map.Graph.Vertices.GetTileOnScreenPos(screenPos, CellSize);

            if (tile is null)
            {
                return;
            }

            if (editorAction == EditorAction.Remove)
            {
                _tab.HasChanges |= tile.Empty();
            }

            if (editorAction == EditorAction.Add)
            {
                var drawingMode = ((RootViewModel)_tab.Parent).DrawingMode;
                _tab.HasChanges |= tile.AddToTile(drawingMode);
            }

            Render();
        }

        private void Render()
        {
            _logger.LogDebug("Canvas render");

            var sw = Stopwatch.StartNew();
            // https://stackoverflow.com/a/44426783
            var drawingContext = _backingStore.Open();
            Render(drawingContext);
            drawingContext.Close();
            sw.Stop();

            _logger.LogTrace("Render took {milliseconds} ms", sw.ElapsedMilliseconds);
        }

        private void Render(DrawingContext drawingContext)
        {
            // Map wasn't set yet - nothing to render
            if (_tab is null || CellSize == 0)
            {
                return;
            }

            var gridPen = new Pen(Brushes.Gray, CellSize * 0.05);

            // assure pixel perfect drawing
            var halfPenWidth = gridPen.Thickness / 2;
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(halfPenWidth);
            guidelines.GuidelinesY.Add(halfPenWidth);
            guidelines.Freeze();
            drawingContext.PushGuidelineSet(guidelines);

            // draw background
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(new Point(), RenderSize));

            var width = RenderSize.Width;
            var height = RenderSize.Height;

            // draw grid
            for (var i = 0d; i < width; i += CellSize)
            {
                drawingContext.DrawLine(gridPen, new Point(i, 0), new Point(i, height));
            }
            for (var i = 0d; i < height; i += CellSize)
            {
                drawingContext.DrawLine(gridPen, new Point(0, i), new Point(width, i));
            }

            RenderTiles(drawingContext);

            // pop back guidelines set
            drawingContext.Pop();
        }

        private void RenderTiles(DrawingContext drawingContext)
        {
            if (_tab is null)
            {
                return;
            }

            foreach (var obstacle in _tab.Map.Graph.Vertices.Where(t => !t.Passable))
            {
                drawingContext.DrawRectangle(Brushes.Black, null, obstacle.GetRect(CellSize));
            }

            var starts = _tab.Map.Graph.Vertices.Where(t => t.IsStart);
            for (var i = 0; i < starts.Count(); i++)
            {
                var rect = starts.ElementAt(i).GetRect(CellSize);
                drawingContext.DrawEllipse(Brushes.Green, null, rect.Center(), CellSize / 2, CellSize / 2);
                drawingContext.DrawText(GetFormattedText(i + 1), rect.TopLeft);
            }

            var finishes = _tab.Map.Graph.Vertices.Where(t => t.IsFinish);
            for (var j = 0; j < finishes.Count(); j++)
            {
                var rect = finishes.ElementAt(j).GetRect(CellSize);
                drawingContext.DrawEllipse(Brushes.Red, null, rect.Center(), CellSize / 2, CellSize / 2);
                drawingContext.DrawText(GetFormattedText(j + 1), rect.TopLeft);
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
