using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public class CompositeGameDetector : IGameDetector
    {
        public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;

        public IList<IGameDetector> SortedDetectors { get; }
        protected ILogger? Logger;

        public CompositeGameDetector(IList<IGameDetector> sortedDetectors, IServiceProvider serviceProvider)
        {
            Requires.NotNullOrEmpty(sortedDetectors, nameof(sortedDetectors));
            SortedDetectors = sortedDetectors;
            Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        }

        
        public GameDetectionResult Detect(GameDetectorOptions options)
        {
            var errors = new List<Exception>();
            foreach (var sortedDetector in SortedDetectors)
            {
                Logger?.LogDebug($"Searching for game {options.Type} with detector: {sortedDetector}");
                sortedDetector.InitializationRequested += PassThroughInitializationRequest;
                var result = sortedDetector.Detect(options);
                sortedDetector.InitializationRequested -= PassThroughInitializationRequest;
                if (result.GameLocation is not null)
                    return result;
                if (result.Error is not null)
                    errors.Add(result.Error);
            }

            return errors.Any()
                ? new GameDetectionResult(options.Type, new AggregateException(errors))
                : GameDetectionResult.NotInstalled(options.Type);
        }

        public bool TryDetect(GameDetectorOptions options, out GameDetectionResult result)
        {
            result = Detect(options);
            if (result.Error is not null)
                return false;
            return result.GameLocation is not null;
        }

        private void PassThroughInitializationRequest(object? sender, GameInitializeRequestEventArgs e)
        {
            OnInitializationRequested(e);
        }

        private void OnInitializationRequested(GameInitializeRequestEventArgs e)
        {
            InitializationRequested?.Invoke(this, e);
        }
    }
}