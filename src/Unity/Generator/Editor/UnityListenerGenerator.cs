using AnotherECS.Debug;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;

namespace AnotherECS.Unity.Editor.Generator
{
    [InitializeOnLoad]
    internal class UnityListenerGenerator : AssetPostprocessor
    {
        private const string MENU_NAME_COMPILE = "AnotherECS/Compile";
        private const string MENU_NAME_AUTO_COMPILE = "AnotherECS/Auto Compile";

        private static readonly Generators _generators;

        static UnityListenerGenerator()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            CompilationPipeline.assemblyCompilationFinished += AssemblyCompilationFinishedEventHandler;

            _generators = new Generators();

            CheckFirstLaunch();
        }

        [MenuItem(MENU_NAME_COMPILE, false, 1)]
        public static void Compile()
        {
            Compile(false);
        }

        public static void Compile(bool isForce)
        {
            if (_generators.Compile(GetIgnoreTypes(), isForce))
            {
                DropDelay();
                Logger.CompileFinished();
            }
        }

        [MenuItem(MENU_NAME_AUTO_COMPILE)]
        public static void ToggleAutoCompile()
        {
            var enabled = !IsAutoCompile();
            Menu.SetChecked(MENU_NAME_AUTO_COMPILE, enabled);
            SetAutoCompile(enabled);
        }

        [MenuItem(MENU_NAME_AUTO_COMPILE, true)]
        private static bool ToggleAutoCompileValidate()
        {
            Menu.SetChecked(MENU_NAME_AUTO_COMPILE, IsAutoCompile());
            return true;
        }

        private static void CheckFirstLaunch()
        {
            if (IsFirstLaunch())
            {
                DropFlagFirstLaunch();
                OnAfterAssemblyReload();
            }
        }

        private static bool IsFirstLaunch()
            => SessionState.GetBool($"{nameof(UnityListenerGenerator)}.{nameof(IsFirstLaunch)}", true);

        private static void DropFlagFirstLaunch()
            => SessionState.GetBool($"{nameof(UnityListenerGenerator)}.{nameof(IsFirstLaunch)}", false);

        private static bool IsNewElements()
            => SessionState.GetString($"{nameof(UnityListenerGenerator)}.{nameof(IsNewElements)}", "") != GetHashAllElements();
        
        private static void SaveNewElements()
            => SessionState.SetString($"{nameof(UnityListenerGenerator)}.{nameof(IsNewElements)}", GetHashAllElements());

        private static string GetHashAllElements()
            => new UnityGeneratorContext().GetAllTypes()
                .Select(p => p.Name)
                .OrderBy(p => p)
                .Aggregate((s, p) => s + ":" + p)
                .GetHashCode()
                .ToString();

        private static void SaveIgnoreTypes(Type[] deletedTypes)
        {
            var elements = (deletedTypes.Length == 0) ? string.Empty : deletedTypes.Select(p => p.FullName + ", " + p.Assembly.FullName)
                .OrderBy(p => p)
                .Aggregate((s, p) => s + ":" + p);

            SessionState.SetString($"{nameof(UnityListenerGenerator)}.{nameof(SaveIgnoreTypes)}", elements);
        }

        private static Type[] GetIgnoreTypes()
        {
            var elements = SessionState.GetString($"{nameof(UnityListenerGenerator)}.{nameof(SaveIgnoreTypes)}", "");
            var typeNames = elements.Split(':');

            return typeNames
                .Select(p => Type.GetType(p, false, false))
                .Where(p => p != null)
                .ToArray();
        }

        private static void OnAfterAssemblyReload()
        {
            if (IsAutoCompile())
            {
                TryCompile();
            }
        }

        public static bool IsDelayFinished()
        {
            var time = EditorPrefs.GetFloat("AnotherECS.Editor.Generator.time", 0f);
            var dt = EditorApplication.timeSinceStartup - time;

            return dt < 0f || dt > 1f;
        }

        public static void DropDelay()
        { 
            EditorPrefs.SetFloat("AnotherECS.Editor.Generator.time", (float)EditorApplication.timeSinceStartup);
        }


        public static void SetAutoCompile(bool isOn)
        {
            EditorPrefs.SetBool("AnotherECS.Editor.Generator.isAutoCompile", isOn);
        }

        public static bool IsAutoCompile()
            => EditorPrefs.GetBool("AnotherECS.Editor.Generator.isAutoCompile", true);

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var importedAsset in deletedAssets)
            {
                if (importedAsset.EndsWith(".cs") && !_generators.GetSaveFileNames(new UnityGeneratorContext()).Any(importedAsset.EndsWith))
                {
                    TryCompile();
                    return;
                }
            }
        }

        [DidReloadScripts]
        public static void OnScriptsReloaded()
        {
            if (IsAutoCompile())
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    TryCompile();
                }
            }
        }

        private static void AssemblyCompilationFinishedEventHandler(string output, CompilerMessage[] messages)
        {
            var generatorContext = new UnityGeneratorContext();
            var deletes = UnityGeneratorUtils.GetDeletedObjectsHack(messages, generatorContext.GetAllTypes(), _generators.GetSaveFileNames(generatorContext));

            SaveIgnoreTypes(deletes);
            if (IsAutoCompile())
            {
                if (deletes.Length != 0)
                {
                    _generators.Compile(deletes, isForce: true);
                }
            }
        }

        public static void TryCompile()
        {
            if (IsAutoCompile() && IsDelayFinished() && IsNewElements())
            {
                if (_generators.Compile(GetIgnoreTypes()))
                {
                    DropDelay();
                    SaveNewElements();

                    Logger.CompileFinished();
                }
            }
        }

    }
}
