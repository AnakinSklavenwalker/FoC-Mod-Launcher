using System;
using System.Threading;
using SimplePipeline.Tasks;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.UpdateItem;
using Validation;

namespace TaskBasedUpdater.Tasks
{
    internal class CleanFileTask : SynchronizedPipelineTask, IUpdaterTask
    {
        internal string File { get; set; }

        public IUpdateItem UpdateItem { get; }

        public CleanFileTask(IServiceProvider serviceProvider, IUpdateItem updateItem, string filePath) 
            : base(serviceProvider)
        {
            Requires.NotNull(updateItem, nameof(updateItem));
            UpdateItem = updateItem;
            File = filePath;

            // TODO: 1. Get download/backup location from UpdateItem (form serviceProvider).
            // TODO: 2. Delete them if they exist
            // TODO: 3. Remove filePath from ctor
        }

        public override string ToString()
        {
            return $"Cleaning file '{File}'";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            if (!FileSystemExtensions.DeleteFileWithRetry(File, out _))
                throw new Exception($"Failed to delete file: {File}");
        }
    }
}
