using System;
using TaskBasedUpdater.Verification;

namespace TaskBasedUpdater.Component
{
    public sealed record OriginInfo(Uri Origin)
    {
        public long? Size { get; init; }

        public VerificationContext VerificationContext { get; init; }
    }
}