using System;
using System.IO;

namespace ProductUpdater
{
    public class RestoreFailedException : IOException
    {
        public RestoreFailedException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}