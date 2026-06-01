using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MelonLoader.Utils
{
    internal static class AssemblyVerifier
    {
        private static HashSet<char> AllowedSymbols = new()
        {
            '_',
            '<',
            '>',
            '`',
            '.',
            '=',
            '-',
            '|',
            ',',
            '[',
            ']',
            '$',
            ':',
            '@',
            '(',
            ')',
            '?',
            '{',
            '}',
            '!'
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void EnsureInitialized()
        {
            var dummyListToEnsureThisCodeDoesntGetNuked = new List<object>();

            //Force load AsmResolver
            dummyListToEnsureThisCodeDoesntGetNuked.Add(new Constant(ElementType.Class, null));
            dummyListToEnsureThisCodeDoesntGetNuked.Add(typeof(AsmResolver.PE.File.PEFile));
        }

        private static bool IsNameValid(string name)
        {
            if (name is null) 
                return false;

            foreach (char c in name)
            {
                // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure643-identifiers
                if (CharUnicodeInfo.GetUnicodeCategory(c) is 
                    // Letter_Character
                    UnicodeCategory.UppercaseLetter // Lu
                    or UnicodeCategory.LowercaseLetter // Ll
                    or UnicodeCategory.TitlecaseLetter // Lt
                    or UnicodeCategory.ModifierLetter  // Lm
                    or UnicodeCategory.OtherLetter // Lo
                    or UnicodeCategory.LetterNumber // Nl
                    // Decimal_Digit_Character
                    or UnicodeCategory.DecimalDigitNumber // Nd
                    // Connecting_Character
                    or UnicodeCategory.ConnectorPunctuation // Pc
                    // Combining_Character
                    or UnicodeCategory.NonSpacingMark // Mn
                    or UnicodeCategory.SpacingCombiningMark // Mc
                    // Formatting_Character
                    or UnicodeCategory.Format // Cf
                   )
                    continue;
                
                if (AllowedSymbols.Contains(c))
                    continue;

                return false;
            }

            return true;
        }

        private static void CountChars(string str, ref Dictionary<char, int> map)
        {
            foreach (char c in str)
            {
                if (map.ContainsKey(c))
                    map[c]++;
                else
                    map.Add(c, 1);
            }
        }

        /// <summary>
        /// OBFUSCATION DETECTION DISABLED: This method has been modified to always return true,
        /// allowing obfuscated mods to load. The original implementation performed name validation
        /// and entropy analysis that would reject assemblies with obfuscated type/method names.
        /// </summary>
        internal static bool CheckAssembly(ModuleDefinition image)
        {
            // === ORIGINAL CODE (DISABLED) ===
            // The original code performed the following checks:
            // 1. Module count must be exactly 1
            // 2. All type names and namespaces must pass IsNameValid() (no unusual characters)
            // 3. All method names must pass IsNameValid()
            // 4. MulticastDelegate types cannot have fields
            // 5. Shannon entropy of type/method names must be between 4.0 and 5.5
            //
            // These checks were designed to detect and reject obfuscated assemblies,
            // but they also prevent legitimate obfuscated mods from loading.
            //
            // === MODIFICATION ===
            // Always return true to allow all assemblies (including obfuscated ones) to load.

            return true;
        }

#if NET6_0_OR_GREATER
        internal static (bool, string) VerifyFile(string assemblyFile)
        {
            if (assemblyFile is not null)
            {
                var module = ModuleDefinition.FromFile(assemblyFile);
                var checkResult = CheckAssembly(module);

                if (!checkResult)
                    return (false, "Invalid assembly");
            }

            return (true, null);
        }

        internal static (bool, string) VerifyByteArray(byte[] rawAssembly)
        {
            if (rawAssembly is not null)
            {
                var module = ModuleDefinition.FromBytes(rawAssembly);
                var checkResult = CheckAssembly(module);

                if (!checkResult)
                    return (false, "Invalid assembly");
            }
            return (true, null);
        }
#else
        internal static bool VerifyFile(string assemblyFile, out string errorMessage)
        {
            errorMessage = null;

            if (assemblyFile is not null)
            {
                var module = ModuleDefinition.FromFile(assemblyFile);
                var checkResult = CheckAssembly(module);

                if (!checkResult)
                {
                    errorMessage = "Invalid assembly";
                    return false;
                }
            }

            return true;
        }

        internal static bool VerifyByteArray(byte[] rawAssembly, out string errorMessage)
        {
            errorMessage = null;

            if (rawAssembly is not null)
            {
                var module = ModuleDefinition.FromBytes(rawAssembly);
                var checkResult = CheckAssembly(module);

                if (!checkResult)
                {
                    errorMessage = "Invalid assembly";
                    return false;
                }
            }
            
            return true;
        }
#endif
    }
}
