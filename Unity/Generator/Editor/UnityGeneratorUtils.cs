using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using AnotherECS.Generator;

namespace AnotherECS.Unity.Editor.Generator
{
    internal static class UnityGeneratorUtils
    {
        public static string CompileCommonDirectory => "Common";

        public static string GetTemplate(string fileName)
        {
            var root = FindRootTemplateDirectory();
            var pathTemplate = Path.Combine(root, fileName);

            if (File.Exists(pathTemplate))
            {
                return File.ReadAllText(pathTemplate);
            }

            throw new Exception($"Template for auto generation code not founded '{pathTemplate}'.");
        }

        public static string FindRootTemplateDirectory()
        {
            var result = Path.GetFullPath(Path.Combine(FindRootDirectory(), GeneratorSettings.TemplatesFolder));

            if (Directory.Exists(result))
            {
                return result;
            }
            throw new Exception($"Directory for templates code not founded '{result}'.");
        }

        public static string FindRootGenCommonDirectory()
            => Path.Combine(FindRootGenDirectory(), CompileCommonDirectory);

        public static string FindRootGenDirectory()
        {
            var result = Path.GetFullPath(Path.Combine(FindRootDirectory(), GeneratorSettings.AutoGenFolder));
            if (Directory.Exists(result))
            {
                return result;
            }
            throw new Exception($"Directory for auto generation code not founded '{result}'.");
        }

        public static string FindRootDirectory()
        {
            var asms = AssetDatabase.FindAssets("t:asmdef");
            foreach (var asm in asms)
            {
                var path = AssetDatabase.GUIDToAssetPath(asm);
                if (Path.GetFileName(path) == GeneratorSettings.EcsRoot)
                {
                    return Path.Combine(Path.GetDirectoryName(path), @"..\");
                }
            }

            throw new Exception($"Assembly '{GeneratorSettings.EcsRoot}' not founded.");
        }

        public static void SaveFile(string path, string content)
        {
            File.WriteAllText(path, content);
            AssetDatabase.ImportAsset(GetAssetsRelativePath(path), ImportAssetOptions.ForceSynchronousImport);
        }

        public static string GetAssetsRelativePath(string absolutePath)
            => absolutePath.StartsWith("Assets")
            ? absolutePath 
            :
                (
                    absolutePath.StartsWith(Path.GetFullPath(Application.dataPath))
                    ? "Assets" + absolutePath[Application.dataPath.Length..]
                    : throw new Exception($"Path must start with {Application.dataPath}. Path: '{absolutePath}'")
                );

        public static Type[] GetDeletedObjectsHack(CompilerMessage[] messages, Type[] types, string[] fileNames)
        {
            var filenames = fileNames.Select(Path.GetFileName).ToArray();

            var regex = new Regex("'(.*?)'");

            var errorTypeNames = messages
                .Where(p => p.type == CompilerMessageType.Error && p.message.Contains("CS0234") && filenames.Any(p.file.Contains))
                .Select(p => regex.Match(p.message))
                .Where(p => p.Success && p.Value.Length > 2)
                .Select(p => p.Value[1..^1])
                .Distinct()
                .ToArray();

            return types
                .Where(p => errorTypeNames.Contains(p.Name))
                .ToArray();
        }

        public static void DeleteUnusedFiles(string rootGenDirectory, string[] saveFileNames, string saveFilePostfixName)
        {
            var files = Directory.GetFiles(rootGenDirectory);

            var fileToDelete = files
                .Where(p => p.EndsWith(saveFilePostfixName))
                .Where(p => !saveFileNames.Any(p0 => p0 == p));

            if (fileToDelete.Any())
            {
                foreach (var file in fileToDelete)
                {
                    var realatiePath = GetAssetsRelativePath(file);
                    if (AssetDatabase.DeleteAsset(realatiePath))
                    {
                        Debug.Logger.FileDeleted(realatiePath);
                    }
                }
            }
        }
    }
}    

