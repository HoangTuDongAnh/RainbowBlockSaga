using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class AppIconSetup
{
    const string Path = "Assets/_Game/Art/AppIcon/RainbowBlockSaga-icon.png";
    const string Result = "Temp/app-icon-result.txt";
    static AppIconSetup() { EditorApplication.update += Apply; }
    static void Apply()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= Apply;
        if (File.Exists(Result)) return;
        try
        {
            AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });
            AssetDatabase.SaveAssets();
            var saved = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
            if (saved.Length == 0 || saved[0] != icon) throw new Exception("Default icon was not assigned.");
            File.WriteAllText(Result, "PASS: Default Icon = " + Path + " (" + icon.width + "x" + icon.height + ")");
        }
        catch (Exception e) { File.WriteAllText(Result, e.ToString()); }
    }
}
