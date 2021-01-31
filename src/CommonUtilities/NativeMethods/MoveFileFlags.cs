using System;

namespace CommonUtilities.NativeMethods
{
    [Flags]
    internal enum MoveFileFlags
    {
        None = 0,
        MoveFileReplaceExisting = 1,
        MoveFileCopyAllowed = 2,
        MoveFileDelayUntilReboot = 4,
        MoveFileWriteThrough = 8,
    }
}