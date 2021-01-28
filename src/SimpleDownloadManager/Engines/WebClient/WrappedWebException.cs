using System;
using System.Net;
using System.Runtime.Serialization;

namespace SimpleDownloadManager.Engines
{
    [Serializable]
    public class WrappedWebException : WebException
    {
        public WrappedWebException(int errorCode, string message)
            : base(message)
        {
            HResult = errorCode;
        }

        protected WrappedWebException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static void Throw(int errorCode, string functionName, string message)
        {
            var errorCode1 = -2147024896 | errorCode;
            if (string.IsNullOrEmpty(message))
                message = "unspecified";
            throw new WrappedWebException(errorCode1, $"Function: {functionName}, HR: {errorCode1}, Message: {message}");
        }
    }
}