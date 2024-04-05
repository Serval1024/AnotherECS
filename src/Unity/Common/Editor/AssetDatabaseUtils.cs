using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AnotherECS.Unity.Editor.Common
{
    public class AssetDatabaseUtils : MonoBehaviour
    {
        public static string FindAnchorDirectory(string anchorAsmdef)
        {
            var asms = AssetDatabase.FindAssets("t:asmdef");
            foreach (var asm in asms)
            {
                var path = AssetDatabase.GUIDToAssetPath(asm);
                if (Path.GetFileName(path) == anchorAsmdef)
                {
                    return Path.Combine(Path.GetDirectoryName(path), @"..\");
                }
            }

            throw new Exception($"Assembly '{anchorAsmdef}' not found.");
        }
    }
}
