using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Core.Models;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public class EditorTabViewModel : Screen
    {
        private readonly ILogger<EditorTabViewModel> _logger;
        private string _fullPath = string.Empty;
        private bool _hasChanges;
        private Map? _map;

        public EditorTabViewModel(ILogger<EditorTabViewModel> logger)
        {
            _logger = logger;
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

        #region Logging overrides

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
