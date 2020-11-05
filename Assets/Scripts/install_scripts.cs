using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

namespace Pdal {

    public class Install{

        const string sharedObject = "pdalc.dll";
        const string packageVersion = "2.2.0";
        const string pdalcVersion = "2.0.0";

        [InitializeOnLoadMethod]
        static void OnProjectLoadedinEditor()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            EditorUtility.DisplayProgressBar("Restoring Conda Package", "PDAL", 0);

            if (Application.isEditor) {
                try
                {
                    string pluginPath = Path.Combine(Application.dataPath, "Conda");
                    if (!Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
                    pluginPath = Path.Combine(pluginPath, "Pdal");
                    if (!Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
                    string file = Path.Combine(pluginPath, sharedObject);
                    if (!File.Exists(file))
                    {
                        UpdatePackage();
                    }
                    else if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        string currentVersion = new Config().Version;
                        if (currentVersion != packageVersion)
                        {
                            UpdatePackage();
                        }
                    }
                    AssetDatabase.Refresh();
                }
                catch (Exception e)
                {
                    // do nothing
                    Debug.Log($"Error in Conda Package {sharedObject} : {e.ToString()}");
                };
            };

            EditorUtility.ClearProgressBar();
            stopwatch.Stop();
            Debug.Log($"Pdal refresh took {stopwatch.Elapsed.TotalSeconds} seconds");
        }
        static void UpdatePackage() {
            Debug.Log("Pdal Install Script Awake"); 
            string pluginPath = Path.Combine(Application.dataPath, "Conda", "Pdal");
            string path = Path.GetDirectoryName(new StackTrace(true).GetFrame(0).GetFileName());
            string exec = Path.Combine(path, "install_script.ps1");
            string response;
            string install = $"pdal={packageVersion}";
            using (Process compiler = new Process())
            {
                compiler.StartInfo.FileName = "powershell.exe";
                compiler.StartInfo.Arguments = $"-ExecutionPolicy Bypass {exec} -package pdal " +
                                                    $"-install {install} " +
                                                    $"-destination {pluginPath} " +
                                                    $"-so_list *";
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.Start();

                response = compiler.StandardOutput.ReadToEnd();

                compiler.WaitForExit();
            }
            Debug.Log(response);
            install = $"pdal-c={pdalcVersion}";
            using (Process compiler = new Process())
            {
                compiler.StartInfo.FileName = "powershell.exe";
                compiler.StartInfo.Arguments = $"-ExecutionPolicy Bypass {exec} -package pdal-c " +
                                                    $"-install {install} " +
                                                    $"-destination {pluginPath} " +
                                                    $"-so_list pdalc";
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.Start();

                response = compiler.StandardOutput.ReadToEnd();

                compiler.WaitForExit();
            }
        }
    }
}
