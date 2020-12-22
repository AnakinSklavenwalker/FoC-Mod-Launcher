using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace TaskBasedUpdater.Component
{
    public class ComponentFileFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ComponentFileFactory(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }
        
        public ProductComponent FromFile(ProductComponent baseComponent, string path, IProductComponentBuilder builder)
        {
            Requires.NotNull(baseComponent, nameof(baseComponent));
            Requires.NotNull(builder, nameof(builder));

            var fs = _serviceProvider.GetRequiredService<IFileSystem>();
            var file = fs.FileInfo.FromFileName(path);
            if (!file.Exists)
                return baseComponent with { CurrentState = CurrentState.Removed };

            var version = builder.GetVersion(file);

            // TODO: split-project
            ValidationContext? validationContext = null;
            if (builder.HashType != HashType.None)
            {
            }

            return baseComponent with
                {
                CurrentState = CurrentState.Installed,
                CurrentVersion = version,
                ValidationContext = validationContext
                };
        }
    }
}