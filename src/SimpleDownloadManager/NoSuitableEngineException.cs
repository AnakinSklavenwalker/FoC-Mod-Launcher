using System;
using System.Runtime.Serialization;

namespace SimpleDownloadManager
{
    [Serializable]
    public class NoSuitableEngineException : InvalidOperationException
    {
        public NoSuitableEngineException(string message)
            : base(message)
        {
        }

        protected NoSuitableEngineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}