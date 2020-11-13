using System;
using System.Threading;
using SimplePipeline.Tasks;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.Tasks
{
    internal class CleanFileTask : SynchronizedPipelineTask, IUpdaterTask
    {
        internal string File { get; set; }

        public IUpdateItem UpdateItem { get; }

        public CleanFileTask(IServiceProvider serviceProvider, IUpdateItem updateItem, string filePath) 
            : base(serviceProvider)
        {
            UpdateItem = updateItem;
            File = filePath;
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
