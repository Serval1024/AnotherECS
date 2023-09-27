using System;
using System.IO;
using System.Linq;
using AnotherECS.Generator;
using UnityEditor;

namespace AnotherECS.Unity.Editor.Generator
{
    public class UnityEnvironmentProvider : IEnvironmentProvider
    {
        public string GetFilePathToType(Type stateType)
        {
            var guid = AssetDatabase.FindAssets($"t:Script {stateType.Name}").FirstOrDefault();
            if (!string.IsNullOrEmpty(guid))
            {
                var fullPath = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(fullPath);
                if (name == stateType.Name)
                {
                    return Path.GetDirectoryName(fullPath);
                }
            }
            return null;
        }

        public string FindRootGenDirectory()
            => UnityGeneratorUtils.FindRootGenDirectory();
        public string FindRootGenCommonDirectory()
            => UnityGeneratorUtils.FindRootGenCommonDirectory();
        public string GetTemplate(string fileName)
            => UnityGeneratorUtils.GetTemplate(fileName);
    }
}
