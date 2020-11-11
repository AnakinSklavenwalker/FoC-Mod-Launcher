using System;

namespace SimplePipeline
{
    public class TaskEventArgs : EventArgs
    {
        private bool _cancel;

        public IPipelineTask Task { get; }

        public bool Cancel
        {
            get => _cancel;
            set => _cancel |= value;
        }

        public TaskEventArgs(IPipelineTask task)
        {
            Task = task;
        }
    }
}