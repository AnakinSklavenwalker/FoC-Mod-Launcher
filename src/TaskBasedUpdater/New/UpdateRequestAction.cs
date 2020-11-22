using System;

namespace TaskBasedUpdater.New
{
    [Flags]
    public enum UpdateRequestAction
    {
        Update = 1,
        Repair = 2
    }
}