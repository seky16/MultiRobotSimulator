using System.IO;
using Microsoft.Win32;

namespace MultiRobotSimulator.WPF.Services
{
    public class IOService : IIOService
    {
        public string OpenFileDialog(string? defaultPath = null)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = ".map file|*.map",
                DefaultExt = ".map"
            };

            if (!string.IsNullOrEmpty(defaultPath))
            {
                dialog.FileName = Path.GetFileName(defaultPath);
                var directory = Path.GetDirectoryName(defaultPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    dialog.InitialDirectory = directory;
                }
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        public TextReader OpenTextFile(string path)
        {
            return File.OpenText(path);
        }

        public string SaveFileDialog(string? defaultPath = null)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = ".map file|*.map",
                DefaultExt = ".map"
            };

            if (!string.IsNullOrEmpty(defaultPath))
            {
                dialog.FileName = Path.GetFileName(defaultPath);
                var directory = Path.GetDirectoryName(defaultPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    dialog.InitialDirectory = directory;
                }
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}
