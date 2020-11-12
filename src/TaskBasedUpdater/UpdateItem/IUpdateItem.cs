using System;

namespace TaskBasedUpdater.Component
{
    public interface IUpdateItem : IEquatable<IUpdateItem>
    {
        string Destination { get; set; }

        string Name { get; set; }

        UpdateAction RequiredAction { get; set; }

        CurrentState CurrentState { get; set; }

        Version? CurrentVersion { get; set; }

        OriginInfo? OriginInfo { get; set; }

        long? DiskSize { get; set; }
    }
}