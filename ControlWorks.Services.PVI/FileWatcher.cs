using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ControlWorks.Services.PVI
{
    public class FileWatcher
    {
        private readonly HashSet<string> _cache = new HashSet<string>();
        private Timer _stateTimer;


        public event EventHandler<FileWatchEventArgs> FilesChanged;
        public string DirectoryPath { get; set; }

        public FileWatcher(string directory)
        {
            DirectoryPath = directory;
            InitializeCache();
            Run();
        }

        private void InitializeCache()
        {
            _cache.Clear();
            if (!String.IsNullOrEmpty(DirectoryPath) && Directory.Exists(DirectoryPath))
            {
                foreach (var file in Directory.GetFiles(DirectoryPath))
                {
                    _cache.Add(file);
                }
            }
        }

        public void Run()
        {
            _stateTimer = new Timer(CheckFiles,null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        }

        private void CheckFiles(object state)
        {
            var files = Directory.GetFiles(DirectoryPath);
            if (files.Length != _cache.Count)
            {
                OnFilesChanged(new FileWatchEventArgs() {Directory = DirectoryPath});
            }
            else if (!IsInCache())
            {
                OnFilesChanged(new FileWatchEventArgs() { Directory = DirectoryPath });
            }

            InitializeCache();
        }

        private bool IsInCache()
        {
            var files = Directory.GetFiles(DirectoryPath);
            foreach (var file in files)
            {
                if (!_cache.Contains(file))
                {
                    return false;
                }
            }
            return true;
        }

        private void OnFilesChanged(FileWatchEventArgs args)
        {
            var temp = FilesChanged;
            if (temp != null)
            {
                temp(this, args);
            }
        }
    }

    public class FileWatchEventArgs : EventArgs
    {
        public string Directory { get; set; }
    }
}
