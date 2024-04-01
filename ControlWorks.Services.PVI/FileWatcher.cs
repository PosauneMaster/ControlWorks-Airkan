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
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess;

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
            Trace.TraceInformation($"{_counter } Network Directory Changed at {DateTime.Now:HH:mm:ss:ffff}");
            Interlocked.Increment(ref _counter);
            _cacheItemPolicy.AbsoluteExpiration =
                DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);

            _memCache.AddOrGetExisting(ConfigurationProvider.AirkanNetworkFolder, ConfigurationProvider.AirkanNetworkFolder, _cacheItemPolicy);

        }

        private volatile int _counter = 0;
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Expired)
            {
                return;
            }

            var e = args.CacheItem.Value.ToString();

            Trace.TraceInformation($"{_counter} File Watch Triggered for {e} at {DateTime.Now:HH:mm:ss:ffff}");
            Interlocked.Increment(ref _counter);

            OnFilesChanged(new FileWatchEventArgs() {Directory = ConfigurationProvider.AirkanNetworkFolder });
        }
    }

    public class FileWatchEventArgs : EventArgs
    {
        public string Directory { get; set; }
    }
}
