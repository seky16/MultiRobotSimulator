using System;
using System.IO;
using System.Windows;
using MultiRobotSimulator.Core.Models;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public class EditorTabViewModel : Screen
    {
        private string _fullPath = string.Empty;
        private bool _hasChanges;
        private Map? _map;

        public EditorTabViewModel()
        {
            EditorCanvas = new EditorCanvasViewModel(this);
        }

        public EditorCanvasViewModel EditorCanvas { get; }

        public string FullPath
        {
            get { return _fullPath; }
            set
            {
                SetAndNotify(ref _fullPath, value);
                DisplayName = Path.GetFileName(value);
            }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            set { SetAndNotify(ref _hasChanges, value); }
        }

        public string Id { get; } = Guid.NewGuid().ToString();

        public Map Map
        {
            get
            {
                if (_map is null)
                {
                    throw new InvalidOperationException($"{nameof(Map)} cannot be null.");
                }

                return _map;
            }
            set { SetAndNotify(ref _map, value); }
        }

        public void CanvasResized(object sender, SizeChangedEventArgs e)
        {
            if (EditorCanvas.ResizeEventHandler is null)
            {
                EditorCanvas.InitialSize = e.NewSize;
            }
            else
            {
                EditorCanvas.ResizeEventHandler.Invoke(sender, e);
            }
        }
    }
}
