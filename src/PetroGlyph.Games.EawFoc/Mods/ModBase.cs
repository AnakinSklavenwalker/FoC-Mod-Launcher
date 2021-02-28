using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using NuGet.Versioning;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Mods
{
    [DebuggerDisplay("Mod:{Name};{Type}")]
    public abstract class ModBase : IMod
    {
        private ICollection<ILanguageInfo>? _languageInfos;
        private IModinfo? _modInfo;
        private string? _name;
        private string? _description;
        private string? _iconFile;
        private SemanticVersion? _modVersion;
        private bool _expectedDependenciesCalculated;
        private int _expectedDependencyCount;
        private bool _isResolving;

        public event EventHandler<ModCollectionChangedEventArgs> ModCollectionModified;

        public abstract string Identifier { get; }


        public IModinfo? ModInfo
        {
            get
            {
                if (_modInfo != null)
                    return _modInfo;
                _modInfo = ResolveModInfo();
                return _modInfo;
            }
        }

        public ICollection<ILanguageInfo> InstalledLanguages
        {
            get
            {
                if (_languageInfos != null)
                    return _languageInfos;
                _languageInfos = ResolveInstalledLanguages();
                if (!_languageInfos.Any())
                    _languageInfos.Add(EawModinfo.Model.LanguageInfo.Default);
                return _languageInfos.ToList();
            }
        }
        
        public string Name
        {
            get
            {
                if (_name != null)
                    return _name;
                _name = InitializeName();
                return _name;
            }
        }

        public string Description
        {
            get
            {
                if (_description != null)
                    return _description;
                _description = InitializeDescription();
                return _description;
            }
        }

        public string? IconFile
        {
            get
            {
                if (_iconFile != null)
                    return _iconFile;
                _iconFile = InitializeIcon();
                return _iconFile;
            }
        }

        public SemanticVersion? Version
        {
            get
            {
                if (_modVersion != null)
                    return _modVersion;
                _modVersion = InitializeVersion();
                return _modVersion;
            }
        }

        public int ExpectedDependencies
        {
            get
            {
                if (_expectedDependenciesCalculated)
                    return _expectedDependencyCount;
                _expectedDependencyCount = CalculateExpectedDependencyCount();
                _expectedDependenciesCalculated = true;
                return _expectedDependencyCount;
            }
        }


        public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

        public bool DependenciesResolved { get; protected set; }

        IList<IModReference> IModIdentity.Dependencies => new List<IModReference>(DependenciesInternal);

        public bool WorkshopMod => Type == ModType.Workshops;

        public bool Virtual => Type == ModType.Virtual;

        public bool HasDependencies => DependenciesInternal.Count > 0;
        

        public IGame Game { get; }

        public ModType Type { get; }


        protected internal HashSet<IMod> ModsInternal { get; } = new HashSet<IMod>();

        protected internal List<IMod> DependenciesInternal { get; } = new List<IMod>();


        internal ModBase(IGame game, ModType type)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Type = type;
        }

        protected ModBase(string name, IGame game, ModType type) : this(game, type)
        {
            if (string.IsNullOrEmpty(name))
                throw new PetroglyphModException("The mod's name must not be null or empty!");
            _name = name;
        }

        protected ModBase(IGame game, ModType type, IModinfo? modInfoData) : this(game, type)
        {
            if (modInfoData != null)
            {
                try
                {
                    modInfoData.Validate();
                    _modInfo = modInfoData;
                }
                catch (ModinfoException)
                {
                    _modInfo = null;
                }
            }
        }

        public virtual bool AddMod(IMod mod)
        {
            var result = ModsInternal.Add(mod);
            if (result)
                OnModCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
            return result;
        }

        public virtual bool RemoveMod(IMod mod)
        {
            var result = ModsInternal.Remove(mod);
            if (result)
                OnModCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
            return result;
        }
        
        public IMod? SearchMod(IModReference modReference, ModSearchOptions modSearchOptions, bool add)
        {
            throw new NotImplementedException();
        }

        public abstract bool Equals(IMod other);

        public abstract bool Equals(IModIdentity other);

        public abstract bool Equals(IModReference other);


        public bool ResolveDependencies(ModDependencyResolveStrategy resolveStrategy)
        {
            if (_isResolving)
                throw new PetroglyphModException("Detected a cycle while resolving Mod dependencies.");

            var result = true;
            try
            {
                _isResolving = true;
                if (ExpectedDependencies == 0 || DependenciesResolved)
                    return true;


                if (ModInfo is null)
                    return true;

                foreach (var dependency in ModInfo.Dependencies)
                {
                    if (dependency.Type == ModType.Virtual)
                        throw new NotImplementedException("Virtual linking not supported yet");

                    var searchOptions = ModSearchOptions.Registered;
                    if (resolveStrategy.IsCreative())
                        searchOptions |= ModSearchOptions.FileSystem;

                    var dm = Game.SearchMod(dependency, searchOptions, true);
                    if (dm is null)
                        return false;
                    if (dm.Equals(this))
                        throw new PetroglyphModException($"The mod '{Name}' can not be dependent on itself!");
                    
                    DependenciesInternal.Add(dm);

                    if (resolveStrategy.IsRecursive())
                    {
                        dm.ResolveDependencies(resolveStrategy);
                    }
                }
            }
            finally
            {
                _isResolving = false;
                DependenciesResolved = true;
            }

            return result;
        }

        public abstract string ToArgs(bool includeDependencies);

        internal void SetModInfo(IModinfo modInfo)
        {
            _modInfo = modInfo;
        }

        internal void ResetDependencies()
        {
            DependenciesInternal.Clear();
            DependenciesResolved = false;
        }

        protected abstract bool ResolveDependenciesCore();


        protected virtual ICollection<ILanguageInfo> ResolveInstalledLanguages()
        {
            return ModInfo != null ? 
                ModInfo.Languages.ToList() : 
                new List<ILanguageInfo>();
        }

        protected virtual ModinfoData? ResolveModInfo()
        {
            return null;
        }

        protected virtual string InitializeName()
        {
            var name = string.Empty;
            if (ModInfo != null)
                name = ModInfo.Name;
            return name;
        }

        protected virtual string InitializeDescription()
        {
            var name = string.Empty;
            if (ModInfo != null)
                name = ModInfo.Summary;
            return name;
        }

        protected virtual SemanticVersion? InitializeVersion()
        {
            return ModInfo?.Version;
        }

        protected virtual string? InitializeIcon()
        {
            var name = string.Empty;
            if (ModInfo != null)
                name = ModInfo.Icon;
            return name;
        }

        protected virtual int CalculateExpectedDependencyCount()
        {
            return ModInfo?.Dependencies.Count ?? 0;
        }

        protected virtual IEnumerable<IMod> SearchModsCore()
        {
            return Enumerable.Empty<IMod>();
        }


        protected virtual void OnModCollectionModified(ModCollectionChangedEventArgs e)
        {
            ModCollectionModified?.Invoke(this, e);
        }




        //public ModInfoData? ModInfo
        //{
        //    get
        //    {
        //        if (_modInfo != null || _modInfoFileLookupFlag || ModInfoFile is null)
        //            return _modInfo;
        //        _modInfoFileLookupFlag = true;
        //        ModInfoFile.TryGetModInfo(out var modInfo);
        //        _modInfo = modInfo;
        //        return _modInfo;
        //    }
        //}

        //public ModInfoFile? ModInfoFile { get; }

        //protected ModBase(IGame game, ModType type, ModInfoFile? modInfoFile) : this(game, type)
        //{
        //    if (modInfoFile != null)
        //    {
        //        try
        //        {
        //            modInfoFile.Validate();
        //        }
        //        catch (Exception e)
        //        {
        //            throw new PetroglyphModException(e.Message, e);
        //        }
        //        ModInfoFile = modInfoFile;
        //    }
        //}
    }
}