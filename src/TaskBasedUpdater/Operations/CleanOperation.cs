using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimplePipeline;
using TaskBasedUpdater.New.Product.Component;
using TaskBasedUpdater.Tasks;

namespace TaskBasedUpdater.Operations
{
    internal class CleanOperation : IOperation
    {
        private const int ConcurrentClean = 2;

        private readonly IServiceProvider _serviceProvider;
        private ILogger? _logger;
       
        private readonly IList<CleanFileTask> _cleanFileTasks;
        private readonly AsyncPipelineRunner _pipelineRunner;
        private readonly IList<string> _filesToBeCleaned = new List<string>();
        private readonly ConcurrentBag<string> _filesFailedToBeCleaned = new();
        private bool _planSuccessful;
       

        public CleanOperation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _pipelineRunner = new AsyncPipelineRunner(serviceProvider, ConcurrentClean);
            _pipelineRunner.Error += OnCleaningError;
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
            if (_logger is null)
                _logger = _serviceProvider.GetService<ILogger>();
            var files = GetFiles();

            foreach (var data in files)
            {
                var file = data.Value;
                if (!File.Exists(file)) 
                    continue;
                var cleanTask = new CleanFileTask(_serviceProvider, data.Key, file);
                _cleanFileTasks.Add(cleanTask);
                _pipelineRunner.Queue(cleanTask);
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

                _pipelineRunner.Run(token);
                try
                {
                    _pipelineRunner.Wait();
                }
                catch (Exception)
                {
                }

                // TODO: split-projects
                //BackupManager.Instance.Flush();
                //UpdateItemDownloadPathStorage.Instance.Clear();

                if (!_filesFailedToBeCleaned.Any())
                    return;
                _logger.LogTrace("These files have not been deleted because of an internal error:");
                foreach (var file in _filesFailedToBeCleaned)
                    _logger.LogTrace(file);
            }
        }

        private static IEnumerable<KeyValuePair<ProductComponent, string?>> GetFiles()
        {
            var backupsFiles = BackupManager.Instance;
            var downloadFiles = UpdateItemDownloadPathStorage.Instance;
            return backupsFiles.Concat(downloadFiles).ToHashSet();
        }
    }
}
