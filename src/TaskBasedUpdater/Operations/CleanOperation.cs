using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SimplePipeline;
using SimplePipeline.Runners;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Tasks;

namespace TaskBasedUpdater.Operations
{
    internal class CleanOperation : IOperation
    {
        private const int ConcurrentClean = 2;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;
       
        private readonly IList<CleanFileTask> _cleanFileTasks;
        private readonly AsyncTaskRunner _taskRunner;
        private readonly IList<string> _filesToBeCleaned = new List<string>();
        private readonly ConcurrentBag<string> _filesFailedToBeCleaned = new ConcurrentBag<string>();
        private bool _planSuccessful;
       

        public CleanOperation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _taskRunner = new AsyncTaskRunner(null, ConcurrentClean);
            _taskRunner.Error += OnCleaningError;
            _cleanFileTasks = new List<CleanFileTask>();
        }

        private void OnCleaningError(object sender, TaskEventArgs e)
        {
            if (e.Cancel || !(e.Task is CleanFileTask task))
                return;
            _filesFailedToBeCleaned.Add(task.File);
        }

        public bool Plan()
        {
            if (_planSuccessful)
                return true;
            var files = GetFiles();

            foreach (var data in files)
            {
                var file = data.Value;
                if (!File.Exists(file)) 
                    continue;
                var cleanTask = new CleanFileTask(null, data.Key, file);
                _cleanFileTasks.Add(cleanTask);
                _taskRunner.Queue(cleanTask);
                _filesToBeCleaned.Add(file);
            }

            _planSuccessful = true;
            return true;
        }

        public void Run(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!Plan())
                return;
            if (!_filesToBeCleaned.Any())
                _logger.LogTrace("No files to clean up");
            else
            {
                _logger.LogTrace("These files are going to be deleted:");
                foreach (var file in _filesToBeCleaned)
                    _logger.LogTrace(file);

                _taskRunner.Run(token);
                try
                {
                    _taskRunner.Wait();
                }
                catch (Exception)
                {
                }

                BackupManager.Instance.Flush();
                UpdateItemDownloadPathStorage.Instance.Clear();

                if (!_filesFailedToBeCleaned.Any())
                    return;
                _logger.LogTrace("These files have not been deleted because of an internal error:");
                foreach (var file in _filesFailedToBeCleaned)
                    _logger.LogTrace(file);
            }
        }

        private static IEnumerable<KeyValuePair<IUpdateItem, string?>> GetFiles()
        {
            var backupsFiles = BackupManager.Instance;
            var downloadFiles = UpdateItemDownloadPathStorage.Instance;
            return backupsFiles.Concat(downloadFiles).ToHashSet();
        }
    }
}
