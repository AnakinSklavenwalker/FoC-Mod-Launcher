using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ProductMetadata.Component;

namespace ProductMetadata.Services.Detectors
{
    public sealed class GroupComponentDetector : ComponentDetectorBase
    {
        private readonly IComponentDetectorFactory _detectorFactory;

        protected override ComponentType SupportedType => ComponentType.Group;

        public GroupComponentDetector(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _detectorFactory = serviceProvider.GetRequiredService<IComponentDetectorFactory>();
        }

        protected override IProductComponent FindCore(IProductComponent manifestComponent, IInstalledProduct product)
        {
            if (manifestComponent is not IComponentGroup groupComponent)
                throw new InvalidOperationException();

            var childComponents = new List<IProductComponent>();
            ComponentGroup detectedGroup = new(manifestComponent, childComponents);

            var isAbsent = false;
            foreach (var child in groupComponent.Components)
            {
                var detector = _detectorFactory.GetDetector(child.Type, ServiceProvider);
                var detectedChildComponent = detector.Find(child, product);
                childComponents.Add(detectedChildComponent);
                if (detectedChildComponent.DetectedState != DetectionState.Present)
                    isAbsent = true;
            }

            detectedGroup.DetectedState = isAbsent ? DetectionState.Absent : DetectionState.Present;

            return detectedGroup;
        }
    }
}