using System;

namespace ProductUpdater
{
    [Serializable]
    public class ComponentException : Exception
    {
        public ComponentException(string message) : base(message)
        {
        }
    }
}