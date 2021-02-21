using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CommonUtilities;
using Validation;

namespace ProductMetadata.Services
{
    internal static class ResolverExtensions
    {
        private static readonly string ProgramFiles64Env = "%ProgramW6432%";
        private static readonly string ProgramFiles = nameof(ProgramFiles);
        private static readonly string ProgramFilesX86 = nameof(ProgramFilesX86);
        private static readonly string ProgramFiles64 = nameof(ProgramFiles64);
        private static readonly string ProgramFilesX64 = nameof(ProgramFilesX64);
        private static readonly string CommonProgramFiles64Env = "%CommonProgramW6432%";
        private static readonly string CommonProgramFiles = nameof(CommonProgramFiles);
        private static readonly string CommonProgramFilesX86 = nameof(CommonProgramFilesX86);
        private static readonly string CommonProgramFiles64 = nameof(CommonProgramFiles64);
        private static readonly string CommonProgramFilesX64 = nameof(CommonProgramFilesX64);
        private static readonly string ProgramData = nameof(ProgramData);
        private static readonly string SystemToken = "System";
        private static readonly string SystemFolder = nameof(SystemFolder);
        private static readonly string SystemX86 = nameof(SystemX86);
        private static readonly string System64 = nameof(System64);
        private static readonly string SystemX64 = nameof(SystemX64);
        private static readonly string Windows = nameof(Windows);
        private static readonly string FolderSuffix = "folder";

        private static readonly IDictionary<string, Func<string>> SpecialFolders = new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [ProgramFiles] = () => GetProgramFilesFolder(ProgramFilesX86),
            [ProgramFilesX86] = () => GetProgramFilesFolder(ProgramFilesX86),
            [ProgramFiles64] = () => GetProgramFilesFolder(ProgramFilesX64),
            [ProgramFilesX64] = () => GetProgramFilesFolder(ProgramFilesX64),
            [CommonProgramFiles] = () => GetCommonProgramFilesFolder(CommonProgramFilesX86),
            [CommonProgramFilesX86] = () => GetCommonProgramFilesFolder(CommonProgramFilesX86),
            [CommonProgramFiles64] = () => GetCommonProgramFilesFolder(CommonProgramFilesX64),
            [CommonProgramFilesX64] = () => GetCommonProgramFilesFolder(CommonProgramFilesX64),
            [SystemToken] = () => GetSystemFolder(SystemX86),
            [SystemX86] = () => GetSystemFolder(SystemX86),
            [System64] = () => GetSystemFolder(SystemX64),
            [SystemX64] = () => GetSystemFolder(SystemX64),
            [SystemFolder] = () => GetSystemFolder(SystemX86),
            [ProgramData] = () => GetSystemFolder(ProgramData),
            [Windows] = () => Environment.GetFolderPath(Environment.SpecialFolder.Windows)
        };


        internal static string ReplaceVariables(
            this string value,
            IDictionary<string, string?>? variables,
            ISet<string>? variablesToIgnore = null,
            bool recursive = false)
        {
            IReadOnlyDictionary<string, string?>? wrappedDict = null;
            if (variables is not null)
                wrappedDict = new ReadOnlyDictionary<string, string?>(variables);
            return value.ReplaceVariables(wrappedDict, variablesToIgnore, recursive);
        }

        internal static string ReplaceVariables(
            this string value,
            IReadOnlyDictionary<string, string?>? properties = null,
            ISet<string>? variablesToIgnore = null,
            bool recursive = false)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string expandedString = value;
            string currentValue;

            do
            {
                currentValue = expandedString;
                var stringBuilder = new StringBuilder();
                var startIndex = 0;
                int? varStartIndex = null;
                for (var i = 0; i < expandedString.Length; ++i)
                {
                    var ch = expandedString[i];
                    if (!varStartIndex.HasValue)
                    {
                        switch (ch)
                        {
                            case '[':
                                varStartIndex = i;
                                continue;
                            default:
                                continue;
                        }
                    }
                    if (ch == ']')
                    {
                        var varName = expandedString.Substring(varStartIndex.Value + 1, i - varStartIndex.Value - 1);
                        if (TryResolveVariable(varName, properties, out var str3, variablesToIgnore))
                        {
                            stringBuilder.Append(expandedString, startIndex, varStartIndex.Value - startIndex);
                            stringBuilder.Append(str3);
                            startIndex = i + 1;
                        }
                        varStartIndex = null;
                    }
                }

                if (startIndex == 0)
                    return expandedString;
                if (startIndex < expandedString.Length)
                    stringBuilder.Append(expandedString, startIndex, expandedString.Length - startIndex);
                expandedString = stringBuilder.ToString();


            } while (recursive && !currentValue.Equals(expandedString, StringComparison.OrdinalIgnoreCase));
            return expandedString;
        }

        private static bool TryResolveVariable(
            string name,
            IReadOnlyDictionary<string, string?>? properties,
            out string? value,
            ICollection<string>? variablesToIgnore)
        {
            value = null;
            if (!IsValidVariableName(name))
                return false;
            if (properties != null && (properties.TryGetValue(name, out value) 
                                       || properties.TryGetValue("[" + name + "]", out value))
                || TryGetSpecialFolder(name, out value))
                return true;
            value = Environment.GetEnvironmentVariable(name);
            if (value != null)
                return true;
            value = variablesToIgnore == null || !variablesToIgnore.Contains(name) ? string.Empty : "[" + name + "]";
            return true;
        }

        private static bool IsValidVariableName(string name) => !string.IsNullOrEmpty(name) && name.IndexOf('.') == -1;

        private static bool TryGetSpecialFolder(string name, out string? value)
        {
            if (TryGetSpecialFolderHelper(name, out value))
                return true;
            if (name.EndsWith(FolderSuffix, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - FolderSuffix.Length);
                if (TryGetSpecialFolderHelper(name, out value))
                    return true;
            }
            return false;
        }

        private static bool TryGetSpecialFolderHelper(string name, out string? value)
        {
            value = null;
            try
            {
                if (SpecialFolders.TryGetValue(name, out Func<string> func))
                    value = func();
                else
                {
                    if (Enum.TryParse(name, true, out Environment.SpecialFolder result))
                        value = Environment.GetFolderPath(result, Environment.SpecialFolderOption.DoNotVerify);
                }
                if (!string.IsNullOrEmpty(value))
                {
                    value = AddDirSeparatorToPath(value!);
                    return true;
                }
            }
            catch (ArgumentException)
            {
            }
            return false;
        }

        private static string AddDirSeparatorToPath(string path) =>
            !string.IsNullOrEmpty(path) ? PathExtensions.NormalizePath(path, true) : path;

        private static string GetProgramFilesFolder(string programFilesName)
        {
            Requires.NotNullOrEmpty(programFilesName, nameof(programFilesName));
            if (programFilesName.Equals(ProgramFilesX64, StringComparison.OrdinalIgnoreCase))
            {
                if (Environment.Is64BitOperatingSystem)
                    return Environment.Is64BitProcess ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Environment.ExpandEnvironmentVariables(ProgramFiles64Env);
            }
            else if (programFilesName.Equals(ProgramFilesX86, StringComparison.OrdinalIgnoreCase))
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            return programFilesName;
        }

        private static string GetCommonProgramFilesFolder(string commonProgramFilesName)
        {
            Requires.NotNullOrEmpty(commonProgramFilesName, nameof(commonProgramFilesName));
            if (commonProgramFilesName.Equals(CommonProgramFilesX64, StringComparison.OrdinalIgnoreCase))
            {
                if (Environment.Is64BitOperatingSystem)
                    return Environment.Is64BitProcess ? Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) : Environment.ExpandEnvironmentVariables(CommonProgramFiles64Env);
            }
            else if (commonProgramFilesName.Equals(CommonProgramFilesX86, StringComparison.OrdinalIgnoreCase))
                return Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
            return commonProgramFilesName;
        }

        private static string GetSystemFolder(string systemFolder)
        {
            Requires.NotNullOrEmpty(systemFolder, nameof(systemFolder));
            if (systemFolder.Equals(SystemX64, StringComparison.OrdinalIgnoreCase))
            {
                if (Environment.Is64BitOperatingSystem)
                    return Environment.GetFolderPath(Environment.SpecialFolder.System);
            }
            else
            {
                if (systemFolder.Equals(SystemX86, StringComparison.OrdinalIgnoreCase))
                    return Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                if (systemFolder.Equals(ProgramData, StringComparison.OrdinalIgnoreCase))
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }
            return systemFolder;
        }
    }
}