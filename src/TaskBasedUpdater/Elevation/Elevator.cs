using System;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater.Elevation
{
    public class Elevator
    {
        private static Elevator? _instance;

        public event EventHandler<ElevationRequestData>? ElevationRequested;

        public static Elevator Instance => _instance ??= new Elevator();

        public static Lazy<bool> LazyProcessElevated = new(IsElevated);

        public static bool IsProcessElevated => LazyProcessElevated.Value;

        private Elevator()
        {
        }

        public bool RequestElevation(UnauthorizedAccessException accessException, ProductComponent productComponent)
        {
            if (IsElevated())
                return false;
            var data = new ElevationRequestData(accessException, productComponent);
            OnElevationRequested(data);
            return true;
        }

        public static void RestartElevated(IRestartOptions restartOptions)
        {
            ApplicationRestartManager.RestartApplication(restartOptions, true);
        }

        private static bool IsElevated()
        {
            return !ProcessIntegrity.CurrentIntegrityIsMediumOrLower();
        }

        protected virtual void OnElevationRequested(ElevationRequestData data)
        {
            ElevationRequested?.Invoke(this, data);
        }
    }
}
