using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Core.Enums;
using MultiRobotSimulator.Core.Factories;
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
        private readonly ILogger<RootViewModel> _logger;
        private readonly IMapFactory _mapFactory;
        private readonly Func<NewFileDialogViewModel> _newFileDialogFactory;
        private readonly IWindowManager _windowManager;
        private DrawingMode _drawingMode;
        private int untitledIndex = 0;

        public RootViewModel(IIOService ioService, Func<EditorTabViewModel> editorCanvasFactory, IWindowManager windowManager, Func<NewFileDialogViewModel> newFileDialogFactory, IMapFactory mapFactory, IEventAggregator eventAggregator, ILogger<RootViewModel> logger)
        {
            _ioService = ioService;
            _editorCanvasFactory = editorCanvasFactory;
            _windowManager = windowManager;
            _newFileDialogFactory = newFileDialogFactory;
            _mapFactory = mapFactory;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public DrawingMode DrawingMode
        {
            get { return _drawingMode; }
            set { SetAndNotify(ref _drawingMode, value); }
        }

        public bool HasOpenTabs => Items.Count > 0;

        public void EditorClearAll()
        {
            _logger.LogInformation("Clearing map '{displayName}'", ActiveItem.DisplayName);

            ActiveItem.HasChanges |= ActiveItem.Map.EmptyTiles();
            _eventAggregator.Publish(new CanvasRedrawEvent());
        }

        public void FileNew()
        {
            var dialog = _newFileDialogFactory();

            if (_windowManager.ShowDialog(dialog) != true || dialog.Height <= 0 || dialog.Width <= 0)
            {
                return;
            }

            _logger.LogInformation("New map [{width};{height}]", dialog.Width, dialog.Height);

            var tab = _editorCanvasFactory();
            tab.Map = _mapFactory.CreateMap(dialog.Width, dialog.Height);
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
                _logger.LogInformation("Opening file '{fullPath}'", path);

                var textReader = _ioService.OpenTextFile(path);

                tab = _editorCanvasFactory();
                tab.FullPath = path;
                tab.Map = _mapFactory.FromText(textReader);

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
            Save(ActiveItem, ActiveItem.FullPath);
        }

        public void FileSaveAs()
        {
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

            _logger.LogInformation("Closing item '{displayName}'", tab.DisplayName);

            CloseItem(tab);

            if (Items.Count == 0)
            {
                NotifyOfPropertyChange(() => HasOpenTabs);
            }
        }

        private bool Save(EditorTabViewModel tab, string? fullPath)
        {
            var defaultPath = !string.IsNullOrEmpty(tab.FullPath) ? tab.FullPath : tab.DisplayName;
            if (string.IsNullOrEmpty(fullPath))
            {
                fullPath = _ioService.SaveFileDialog(defaultPath);

                if (string.IsNullOrEmpty(fullPath))
                {
                    return false;
                }
            }

            _logger.LogInformation("Saving item '{defaultPath}' as '{fullPath}'", defaultPath, fullPath);

            _ioService.WriteAllText(fullPath, tab.Map.GetString());

            tab.HasChanges = false;
            tab.FullPath = fullPath;

            return true;
        }

        #region Logging overrides

        public override void ActivateItem(EditorTabViewModel item)
        {
            if (ActiveItem != item)
            {
                _logger.LogInformation("Activating item '{displayName}'", item.DisplayName);
            }

            base.ActivateItem(item);
        }

        protected override bool SetAndNotify<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var result = base.SetAndNotify(ref field, value, propertyName);

            if (result)
            {
                _logger?.LogTrace("Property '{propertyName}' new value: '{value}'", propertyName, value);
            }

            return result;
        }

        #endregion Logging overrides
    }
}
