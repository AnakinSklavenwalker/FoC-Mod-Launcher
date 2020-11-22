using System;

namespace TaskBasedUpdater.New.Update
{
    [Flags]
    public enum UpdateRequestAction
    {
        Update = 1,
        Repair = 2
    }
}