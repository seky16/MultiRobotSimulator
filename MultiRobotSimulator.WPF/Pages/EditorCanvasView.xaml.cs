using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MultiRobotSimulator.Core.Models;
using MultiRobotSimulator.WPF.Events;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public partial class EditorCanvasView : UserControl, IHandle<CanvasRedrawEvent>
    {
        private readonly DrawingGroup _backingStore = new DrawingGroup();
        private readonly Typeface _typeface = new Typeface("Arial");
        private EditorTabViewModel? _tab;

        public EditorCanvasView(IEventAggregator eventAggregator)
        {
            DataContextChanged += OnDataContextChanged;

            eventAggregator.Subscribe(this);
        }

        public double CellSize { get; private set; }

        private void AddToTile(Tile tile, DrawingMode drawingMode)
        {
            switch (drawingMode)
            {
                case DrawingMode.Obstacle:
                    tile.Passable = SetValueAndMarkChange(tile.Passable, false);
                    _tab!.HasChanges |= _tab.Map.RemoveStart(tile);
                    _tab.HasChanges |= _tab.Map.RemoveFinish(tile);
                    break;

                case DrawingMode.Start:
                    _tab!.HasChanges |= _tab.Map.AddStart(tile);
                    break;

                case DrawingMode.Finish:
                    _tab!.HasChanges |= _tab.Map.AddFinish(tile);
                    break;

                default:
                    break;
            }
        }

        private EditorAction GetEditorAction(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    return EditorAction.Remove;
                }
                else
                {
                    return EditorAction.Nothing;
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Released)
                {
                    return EditorAction.Add;
                }
                else
                {
                    return EditorAction.Nothing;
                }
            }

            return EditorAction.Nothing;
        }

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
            if (_tab?.Map is null)
            {
                return;
            }

            var editorAction = GetEditorAction(e);

            if (editorAction == EditorAction.Nothing)
            {
                return;
            }

            var screenPos = e.GetPosition(this);
            var canvasPos = screenPos.ScreenToCanvas(CellSize);

            var tile = _tab.Map.Tiles.GetTileOnScreenPos(screenPos, CellSize);

            if (tile is null)
            {
                return;
            }

            if (editorAction == EditorAction.Remove)
            {
                RemoveFromTile(tile);
            }

            if (editorAction == EditorAction.Add)
            {
                var drawingMode = ((RootViewModel)_tab.Parent).DrawingMode;
                AddToTile(tile, drawingMode);
            }

            Render();
        }

        private void RemoveFromTile(Tile tile)
        {
            _tab!.HasChanges |= _tab.Map.RemoveFinish(tile);
            _tab.HasChanges |= _tab.Map.RemoveStart(tile);
            tile.Passable = SetValueAndMarkChange(tile.Passable, true);
        }

        private void Render()
        {
            // https://stackoverflow.com/a/44426783
            var drawingContext = _backingStore.Open();
            Render(drawingContext);
            drawingContext.Close();
        }

        private void Render(DrawingContext drawingContext)
        {
            // Map wasn't set yet - nothing to render
            if (_tab?.Map is null || CellSize == 0)
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
            foreach (var obstacle in _tab!.Map.Tiles.Where(t => !t.Passable))
            {
                drawingContext.DrawRectangle(Brushes.Black, null, obstacle.GetRect(CellSize));
            }

            for (var i = 0; i < _tab.Map.Starts.Count; i++)
            {
                var rect = _tab.Map.Starts[i].GetRect(CellSize);
                drawingContext.DrawEllipse(Brushes.Green, null, rect.Center(), CellSize / 2, CellSize / 2);
                drawingContext.DrawText(GetFormattedText(i + 1), rect.TopLeft);
            }

            for (var j = 0; j < _tab.Map.Finishes.Count; j++)
            {
                var rect = _tab.Map.Finishes[j].GetRect(CellSize);
                drawingContext.DrawEllipse(Brushes.Red, null, rect.Center(), CellSize / 2, CellSize / 2);
                drawingContext.DrawText(GetFormattedText(j + 1), rect.TopLeft);
            }
        }

        private void SetCanvasSize(Size size)
        {
            // Map wasn't set yet - nothing to render
            if (_tab?.Map is null)
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

        private bool SetValueAndMarkChange(bool valueNow, bool valueToSet)
        {
            if (valueNow != valueToSet)
            {
                _tab!.HasChanges = true;
            }
            return valueToSet;
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
