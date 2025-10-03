using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;
using Conda;

namespace Pdal {
    public class Install: AssetPostprocessor
    {
        const string pdalcVersion = "2.2.3";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!SessionState.GetBool("PdalInitDone", false))
            {
                Stopwatch stopwatch = new();
                string response = "";
                stopwatch.Start();

                EditorUtility.DisplayProgressBar("Restoring Conda Package", "PDAL", 0);

                if (Application.isEditor)
                {
                    CondaApp conda = new();
                    if (! conda.IsInstalled("pdal-c", pdalcVersion))
                    {
                        Debug.Log("Pdal Install Script Awake");
                        string path = Path.GetDirectoryName(new StackTrace(true).GetFrame(0).GetFileName());

                        conda.Install($"pdal-c={pdalcVersion}");

                        try
                        {
                            string sharedAssets = Application.streamingAssetsPath;
                            if (Directory.Exists(Path.Combine(conda.condaShared, "gdal")))
                            {
                                if (!Directory.Exists(sharedAssets)) Directory.CreateDirectory(sharedAssets);
                                string gdalDir = Path.Combine(sharedAssets, "gdal");
                                if (!Directory.Exists(gdalDir)) Directory.CreateDirectory(gdalDir);
                                string projDir = Path.Combine(sharedAssets, "proj");
                                if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);

                                if (Directory.Exists(Path.Combine(conda.condaShared, "gdal")))
                                    foreach (var file in Directory.GetFiles(Path.Combine(conda.condaShared, "gdal")))
                                    {
                                        File.Copy(file, Path.Combine(gdalDir, Path.GetFileName(file)), true);
                                    }

                                if (Directory.Exists(Path.Combine(conda.condaShared, "proj")))
                                    foreach (var file in Directory.GetFiles(Path.Combine(conda.condaShared, "proj")))
                                    {
                                        File.Copy(file, Path.Combine(projDir, Path.GetFileName(file)), true);
                                    }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        conda.TreeShake();
                        AssetDatabase.Refresh();
                    }
                };

                EditorUtility.ClearProgressBar();
                stopwatch.Stop();
                Debug.Log($"Pdal refresh took {stopwatch.Elapsed.TotalSeconds} seconds" + response);
            }
            SessionState.SetBool("PdalInitDone", true);
        }
    }
}
