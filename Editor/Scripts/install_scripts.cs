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

                        conda.Add($"pdal-c={pdalcVersion}", new ConfigFile.Package()
                        {
                            Name = "pdal",
                            Cleans = new ConfigFile.Clean[] { },
                            Shared_Datas = new string[] {
                                "gdal", "proj"
                            }
                        });
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
