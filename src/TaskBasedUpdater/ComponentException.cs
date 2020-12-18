using System;

namespace TaskBasedUpdater
{
    [Serializable]
    public class ComponentException : Exception
    {
        public ComponentException(string message) : base(message)
        {
        }
    }
}