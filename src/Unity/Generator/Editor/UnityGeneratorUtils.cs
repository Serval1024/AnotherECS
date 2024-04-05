using AnotherECS.Generator;
using AnotherECS.Unity.Editor.Common;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

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

            throw new Exception($"Template for auto generation code not found '{pathTemplate}'.");
        }

        public static string FindRootTemplateDirectory()
        {
            var result = Path.GetFullPath(Path.Combine(FindRootDirectory(), GeneratorSettings.TemplatesFolder));

            if (Directory.Exists(result))
            {
                return result;
            }
            throw new Exception($"Directory for templates code not found '{result}'.");
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
            throw new Exception($"Directory for auto generation code not found '{result}'.");
        }

        public static string FindRootDirectory()
            => AssetDatabaseUtils.FindAnchorDirectory(GeneratorSettings.EcsRoot);

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
            var errorCodes = new string[] { "CS0246", "CS0234" };

            var errorTypeNames = messages
                .Where(p => p.type == CompilerMessageType.Error)
                .Where(p => errorCodes.Any(p1 => p.message.Contains(p1)))
                .Where(p => filenames.Any(p.file.Contains))
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
                    var relativePath = GetAssetsRelativePath(file);
                    if (AssetDatabase.DeleteAsset(relativePath))
                    {
                        Debug.Logger.FileDeleted(relativePath);
                    }
                }
            }
        }

        public static string GetSelectedFolder()
        {
            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    return GetDirectory(path);
                }
            }
            return "Assets";
        }

        public static (string path, string subName) FindNoExistsFile(Func<string, string> getPath)
        {
            var subName = string.Empty;
            var path = getPath(subName);
            int counter = 0;
            while (File.Exists(path))
            {
                subName = (++counter).ToString();
                path = getPath(subName);
            }
            return (path, subName);
        }

        public static string GetDirectory(string path)
            => ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                ? path
                : Path.GetDirectoryName(path);

        public static string GetFileNameWithoutExtension(string path)
        {
            while (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.GetFileNameWithoutExtension(path);
            }
            return path;
        }
    }
}    

