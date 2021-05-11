using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
namespace Pdal {
    public class Install{

        const string packageVersion = "2.2.0";
        const string pdalcVersion = "2.1.0";

        [InitializeOnLoadMethod]
        static void OnProjectLoadedinEditor()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            EditorUtility.DisplayProgressBar("Restoring Conda Package", "PDAL", 0);

            if (Application.isEditor) {
                try
                {
                    Config config = new Config();
                    string currentVersion = config.Version;
                    if (currentVersion != packageVersion)
                        {
                            UpdatePackage();
                            AssetDatabase.Refresh();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error in Conda Package PDAL : {e.ToString()}");
                    UpdatePackage();
                    AssetDatabase.Refresh();
                };
            };

            EditorUtility.ClearProgressBar();
            stopwatch.Stop();
            Debug.Log($"Pdal refresh took {stopwatch.Elapsed.TotalSeconds} seconds");
        }


        static void UpdatePackage()
        {
            Debug.Log("Mdal Install Script Awake");
            string path = Path.GetDirectoryName(new StackTrace(true).GetFrame(0).GetFileName());
#if UNITY_EDITOR_WIN
            path = Path.Combine(path, "install_script.ps1");
#else
            path = Path.Combine(path, "install_script.sh");
#endif
            string response = Conda.Conda.Install($"pdal-c={pdalcVersion}", path);
        }
    }
}
#endif
