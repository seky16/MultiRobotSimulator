using System;
using System.Windows;

namespace MultiRobotSimulator.WPF.Pages
{
    public class EditorCanvasViewModel
    {
        public EditorCanvasViewModel(EditorTabViewModel editorTabViewModel)
        {
            EditorTab = editorTabViewModel;
        }

        public EditorTabViewModel EditorTab { get; }

        public Size InitialSize { get; set; }

        public Action<object, SizeChangedEventArgs>? ResizeEventHandler { get; set; }
    }
}
