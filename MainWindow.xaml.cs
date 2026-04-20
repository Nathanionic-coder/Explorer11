using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Explorer11
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileItem> currentFiles = new();
        private Stack<string> navigationHistory = new();
        private string currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public MainWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = currentFiles;
            LoadDirectory(currentPath);
        }

        private void LoadDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    path = currentPath;
                }

                navigationHistory.Push(path);
                currentPath = path;
                AddressBar.Text = path;

                currentFiles.Clear();

                try
                {
                    var dirs = Directory.GetDirectories(path)
                        .OrderBy(d => Path.GetFileName(d))
                        .Select(d => new FileItem
                        {
                            Name = Path.GetFileName(d),
                            Path = d,
                            IsFolder = true,
                            Icon = "📁",
                            Type = "Folder"
                        });

                    foreach (var dir in dirs)
                        currentFiles.Add(dir);
                }
                catch { }

                try
                {
                    var files = Directory.GetFiles(path)
                        .OrderBy(f => Path.GetFileName(f))
                        .Select(f => new FileItem
                        {
                            Name = Path.GetFileName(f),
                            Path = f,
                            IsFolder = false,
                            Icon = GetIconForFile(f),
                            Type = Path.GetExtension(f).TrimStart('.')
                        });

                    foreach (var file in files)
                        currentFiles.Add(file);
                }
                catch { }
            }
            catch { }
        }

        private string GetIconForFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".txt" => "📄",
                ".pdf" => "📕",
                ".doc" or ".docx" => "📘",
                ".xls" or ".xlsx" => "📊",
                ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" => "🖼️",
                ".mp4" or ".avi" or ".mkv" => "🎬",
                ".mp3" or ".wav" or ".flac" => "🎵",
                ".zip" or ".rar" or ".7z" => "📦",
                ".exe" or ".dll" => "⚙️",
                _ => "📝"
            };
        }

        private void FileListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FileListView.SelectedItem is FileItem item)
            {
                if (item.IsFolder)
                    LoadDirectory(item.Path);
                else
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = item.Path, UseShellExecute = true });
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHistory.Count > 1)
            {
                navigationHistory.Pop();
                var prevPath = navigationHistory.Peek();
                LoadDirectory(prevPath);
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
                LoadDirectory(currentPath);
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        private void DocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void DownloadsButton_Click(object sender, RoutedEventArgs e)
        {
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            LoadDirectory(downloadsPath);
        }

        private void PicturesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        }

        private void AddressBar_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                LoadDirectory(AddressBar.Text);
                e.Handled = true;
            }
        }
    }

    public class FileItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        public string Icon { get; set; }
        public string Type { get; set; }
    }
}