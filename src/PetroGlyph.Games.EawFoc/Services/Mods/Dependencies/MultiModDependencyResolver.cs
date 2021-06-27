using System;
using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public class MultiModDependencyResolver
    {
        public event EventHandler<ModDependenciesChangedEventArgs>? DependenciesChanged;

        private readonly IDependencyResolver _resolver;

        public MultiModDependencyResolver(IDependencyResolver resolver)
        {
            Requires.NotNull(resolver, nameof(resolver));
            _resolver = resolver;
        }

        public MultiResolveResult ResolveDependenciesForMods(
            IEnumerable<IMod> modsToResolve, 
            DependencyResolverOptions options, 
            bool abortOnError)
        {
            var result = new MultiResolveResult();
            foreach (var mod in modsToResolve)
            {
                try
                {
                    mod.DependenciesChanged += OnDependenciesResolved;
                    _resolver.Resolve(mod, options);
                }
                catch (Exception e)
                {
                    result.AddError(mod, e);
                    if (abortOnError)
                        break;
                }
                finally
                {
                    mod.DependenciesChanged += OnDependenciesResolved;
                }
            }
            return result;
        }

        private void OnDependenciesResolved(object sender, ModDependenciesChangedEventArgs e)
        {
            // Just pass-through the event.
            DependenciesChanged?.Invoke(sender, e);
        }

        public class MultiResolveResult
        {
            private readonly Dictionary<IMod, Exception> _errorData = new();
            
            public bool HasErrors => _errorData.Any();

            public IReadOnlyCollection<(IMod, Exception)> ErrorData =>
                _errorData.Select(x => new ValueTuple<IMod, Exception>(x.Key, x.Value)).ToList();

            internal MultiResolveResult()
            {
            }
            
            internal void AddError(IMod mod, Exception error)
            {
                Requires.NotNull(mod, nameof(mod));
                Requires.NotNull(error, nameof(error));
                _errorData[mod] = error;
            }
        }
    }
}