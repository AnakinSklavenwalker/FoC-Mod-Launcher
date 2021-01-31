using System;
using System.IO.Abstractions;
using System.Threading;
using CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using ProductUpdater.FileSystem;
using SimplePipeline.Tasks;
using Validation;

namespace ProductUpdater.Tasks
{
    internal class CleanFileTask : SynchronizedPipelineTask, IComponentTask
    {
        private readonly IFileSystem _fileSystem;
        internal string File { get; set; }

        public ProductComponent ProductComponent { get; }

        public CleanFileTask(ProductComponent productComponent, string filePath, IFileSystem fileSystem, ILogger? logger = null) : base(logger)
        {
            Requires.NotNull(productComponent, nameof(productComponent));
            Requires.NotNull(fileSystem, nameof(fileSystem));
            ProductComponent = productComponent;
            File = filePath;
            _fileSystem = fileSystem;

            // TODO: split-projects
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
            if (!_fileSystem.DeleteFileWithRetry(File, out _))
                throw new Exception($"Failed to delete file: {File}");
        }
    }
}
