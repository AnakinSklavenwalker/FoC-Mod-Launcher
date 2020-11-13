﻿using System;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.Elevation
{
    public class ElevationRequestData : IEquatable<ElevationRequestData>
    {
        public IUpdateItem UpdateItem { get; }

        public Exception Exception { get; }

        public ElevationRequestData(Exception exception, IUpdateItem component)
        {
            Exception = exception;
            UpdateItem = component;
        }

        public bool Equals(ElevationRequestData? other)
        {
            if (other is null)
                return false;
            return ReferenceEquals(this, other) || UpdateItem.Equals(other.UpdateItem);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            return obj.GetType() == GetType() && Equals((ElevationRequestData) obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(UpdateItem);
        }
    }
}