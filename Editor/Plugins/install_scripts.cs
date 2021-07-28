using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
namespace Pdal {
    public class Install{

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
            Debug.Log("Pdal Install Script Awake");
            string path = Path.GetDirectoryName(new StackTrace(true).GetFrame(0).GetFileName());
#if UNITY_EDITOR_WIN
            string script = "install_script.ps1";
#else
            string script = "install_script.sh";
#endif
            string response = Conda.Conda.Install($"pdal-c={pdalcVersion}",script, path);
        }
    }
}
#endif
