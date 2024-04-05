using AnotherECS.Debug;
using AnotherECS.Generator;
using System;
using System.Linq;
using UnityEditor;

namespace AnotherECS.Unity.Editor.Generator
{
    internal class Generators
    {
        private readonly IFileGenerator[] _generators;

        public string[] GetSaveFileNames(GeneratorContext context)
            => _generators.SelectMany(p => p.GetSaveFileNames(context)).ToArray();

        public Generators()
        {
            _generators = new IFileGenerator[]
            {
                new CommonLayoutInstallerGenerator(),
                new LayoutInstallerGenerator(),
                new FastAccessGenerator(),
                new StateGenerator(),
                new SystemInstallerGenerator(),
            };
        }

        public bool Compile(bool isForce = false)
            => Compile(Array.Empty<Type>(), isForce);

        public bool Compile(Type[] ignoreTypes, bool isForce = false)
        {
            if (!EditorApplication.isCompiling || isForce)
            {
                var context = new GeneratorContext(ignoreTypes, new UnityEnvironmentProvider());

                foreach (var content in GetContents(context, GeneratorSettings.IsForceFileOverride))
                {
                    UnityGeneratorUtils.SaveFile(content.path, content.text);
                }

                DeleteUnusedFiles(context);

                AssetDatabase.Refresh();
                return true;
            }

            return false;
        }

        public void DeleteUnusedFiles(GeneratorContext context)
        {
            var root = UnityGeneratorUtils.FindRootGenCommonDirectory();
            foreach (var unusedFiles in GetUnusedFiles(context))
            {
                UnityGeneratorUtils.DeleteUnusedFiles(root, unusedFiles.paths, unusedFiles.keepFilePostfixName);
            }
        }

        private ContentGenerator[] GetContents(GeneratorContext context, bool isForceOverride)
        {
            try
            {
                return _generators
                    .SelectMany(p => p.Compile(context, isForceOverride))
                    .AsParallel()
                    .ToArray();
            }
            catch
            {
                Logger.CompileFailed();
                throw;
            }
        }

        private DeleteContentGenerator[] GetUnusedFiles(GeneratorContext context)
        {
            try
            {
                return _generators
                    .Select(p => p.GetUnusedFiles(context))
                    .Where(p => !string.IsNullOrEmpty(p.keepFilePostfixName) && p.paths != null)
                    .ToArray();
            }
            catch
            {
                Logger.CompileFailed();
                throw;
            }
        }
    }
}

