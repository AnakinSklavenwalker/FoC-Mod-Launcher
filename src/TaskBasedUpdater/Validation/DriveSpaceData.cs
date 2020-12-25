using System;

namespace TaskBasedUpdater.Validation
{
    public class DriveSpaceData : IEquatable<DriveSpaceData>
    {
        public long RequestedSize { get; set; }

        public bool HasEnoughDiskSpace { get; set; }

        public long AvailableDiskSpace { get; set; }

        public string DriveName { get; set; }


        public DriveSpaceData(long currentInstallSize, string driveName)
        {
            RequestedSize = currentInstallSize;
            DriveName = driveName;
        }

        public bool Equals(DriveSpaceData other)
        {
            if (string.Equals(DriveName, other?.DriveName, StringComparison.OrdinalIgnoreCase) && RequestedSize == other?.RequestedSize)
                return HasEnoughDiskSpace == other.HasEnoughDiskSpace;
            return false;
        }
    }
}