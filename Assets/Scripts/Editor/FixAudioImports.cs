using UnityEditor;
using UnityEngine;

public class FixAudioImports : Editor
{
    [MenuItem("CalmspaceVR/Fix Audio Import Settings")]
    public static void FixImports()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        int fixed_count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                fixed_count++;
            }
        }

        Debug.Log($"Audio import fix complete. {fixed_count} clips updated to DecompressOnLoad.");
    }
}
