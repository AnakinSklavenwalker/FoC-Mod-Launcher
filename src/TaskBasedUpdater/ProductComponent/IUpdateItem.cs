using System;

namespace TaskBasedUpdater.ProductComponent
{
    public interface IUpdateItem : IEquatable<IUpdateItem>
    {
        string Destination { get; }

        string Name { get; }

        UpdateAction RequiredAction { get; init; }

        CurrentState CurrentState { get; set; }

        Version? CurrentVersion { get; set; }

        OriginInfo? OriginInfo { get; }

        long? DiskSize { get; }
    }
}