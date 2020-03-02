using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MultiRobotSimulator.WPF.Events;
using MultiRobotSimulator.WPF.Services;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public class RootViewModel : Conductor<EditorTabViewModel>.Collection.OneActive
    {
        private readonly Func<EditorTabViewModel> _editorCanvasFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IIOService _ioService;
        private readonly IMapService _mapService;
        private readonly Func<NewFileDialogViewModel> _newFileDialogFactory;
        private readonly IWindowManager _windowManager;
        private DrawingMode _drawingMode;
        private int untitledIndex = 0;

        public RootViewModel(IIOService ioService, Func<EditorTabViewModel> editorCanvasFactory, IWindowManager windowManager, Func<NewFileDialogViewModel> newFileDialogFactory, IMapService mapService, IEventAggregator eventAggregator)
        {
            _ioService = ioService;
            _editorCanvasFactory = editorCanvasFactory;
            _windowManager = windowManager;
            _newFileDialogFactory = newFileDialogFactory;
            _mapService = mapService;
            _eventAggregator = eventAggregator;
        }

        public DrawingMode DrawingMode
        {
            get { return _drawingMode; }
            set { SetAndNotify(ref _drawingMode, value); }
        }

        public bool HasOpenTabs => Items.Count > 0;

        public void EditorClearAll()
        {
            ActiveItem.Map.EmptyTiles();
            _eventAggregator.Publish(new CanvasRedrawEvent());
        }

        public void FileNew()
        {
            Trace.WriteLine(nameof(FileNew));

            var dialog = _newFileDialogFactory();

            if (_windowManager.ShowDialog(dialog) != true || dialog.Height <= 0 || dialog.Width <= 0)
            {
                return;
            }

            var tab = _editorCanvasFactory();
            tab.Map = _mapService.GetNewMap(dialog.Width, dialog.Height);
            tab.DisplayName = $"Untitled{++untitledIndex}.map";

            Items.Add(tab);
            ActivateItem(tab);

            if (Items.Count == 1)
            {
                NotifyOfPropertyChange(() => HasOpenTabs);
            }
        }

        public void FileOpen()
        {
            Trace.WriteLine(nameof(FileOpen));

            var path = _ioService.OpenFileDialog();

            if (string.IsNullOrEmpty(path))
                return;

            EditorTabViewModel tab;
            if (Items.SingleOrDefault(t => t.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)) is EditorTabViewModel item)
            {
                tab = item;
            }
            else
            {
                var textReader = _ioService.OpenTextFile(path);
                var map = _mapService.ImportMap(textReader);

                tab = _editorCanvasFactory();
                tab.FullPath = path;
                tab.Map = map;

                Items.Add(tab);

                if (Items.Count == 1)
                {
                    NotifyOfPropertyChange(() => HasOpenTabs);
                }
            }

            ActivateItem(tab);
        }

        public void FileSave()
        {
            Trace.WriteLine(nameof(FileSave));
            Save(ActiveItem, ActiveItem.FullPath);
        }

        public void FileSaveAs()
        {
            Trace.WriteLine(nameof(FileSaveAs));
            Save(ActiveItem, null);
        }

        public void TabClose(string param)
        {
            var tab = Items.Single(t => t.Id.Equals(param));
            ActivateItem(tab);

            if (tab.HasChanges)
            {
                var result = _windowManager.ShowMessageBox($"Save file '{tab.DisplayName}'?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.OK:
                    case MessageBoxResult.Yes:
                        if (!Save(tab, tab.FullPath))
                        {
                            return;
                        }
                        break;

                    case MessageBoxResult.No:
                        break;

                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                    default:
                        return;
                }
            }

            CloseItem(tab);

            if (Items.Count == 0)
            {
                NotifyOfPropertyChange(() => HasOpenTabs);
            }
        }

        private bool Save(EditorTabViewModel tab, string? fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                var defaultPath = !string.IsNullOrEmpty(tab.FullPath) ? tab.FullPath : tab.DisplayName;
                fullPath = _ioService.SaveFileDialog(defaultPath);

                if (string.IsNullOrEmpty(fullPath))
                {
                    return false;
                }
            }

            _mapService.SaveMap(tab.Map, fullPath);

            tab.HasChanges = false;
            tab.FullPath = fullPath;

            return true;
        }
    }
}
