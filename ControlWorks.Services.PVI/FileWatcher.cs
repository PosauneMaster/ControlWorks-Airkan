using ControlWorks.Common;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Caching;
using System.Threading;

namespace ControlWorks.Services.PVI
{
    public class FileWatcher
    {
        private readonly FileSystemWatcher _watcher;
        private readonly MemoryCache _memCache;
        private readonly CacheItemPolicy _cacheItemPolicy;
        private const int CacheTimeMilliseconds = 1000;

        public event EventHandler<FileWatchEventArgs> FilesChanged;

        public FileWatcher()
        {
            _memCache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy()
            {
                RemovedCallback = OnRemovedFromCache
            };

            _watcher = new FileSystemWatcher(ConfigurationProvider.AirkanNetworkFolder);

            _watcher.NotifyFilter = NotifyFilters.Attributes
                                   | NotifyFilters.CreationTime
                                   | NotifyFilters.DirectoryName
                                   | NotifyFilters.FileName
                                   | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.Security
                                   | NotifyFilters.Size;

            _watcher.Changed += OnNetworkFilesChanged;
            _watcher.Deleted += OnNetworkFilesChanged;
            _watcher.Created += OnNetworkFilesChanged;
            _watcher.Renamed += OnNetworkFilesChanged;

            _watcher.Filter = "*.*";
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
        }

        private void OnFilesChanged(FileWatchEventArgs args)
        {
            var temp = FilesChanged;
            if (temp != null)
            {
                temp(this, args);
            }
        }

        private void OnNetworkFilesChanged(object sender, FileSystemEventArgs e)
        {
            _cacheItemPolicy.AbsoluteExpiration =
                DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);

            _memCache.AddOrGetExisting(ConfigurationProvider.AirkanNetworkFolder, ConfigurationProvider.AirkanNetworkFolder, _cacheItemPolicy);

        }

        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Expired)
            {
                return;
            }

            var e = args.CacheItem.Value.ToString();

            Trace.TraceInformation($"File Watch Triggered for {e}");

            OnFilesChanged(new FileWatchEventArgs() {Directory = ConfigurationProvider.AirkanNetworkFolder });
        }
    }

    public class FileWatchEventArgs : EventArgs
    {
        public string Directory { get; set; }
    }
}
