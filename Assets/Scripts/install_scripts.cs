using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

namespace Pdal {

    public class Install{

#if UNITY_STANDALONE_WIN
        const string test = "test_pdalc.exe";
#elif UNITY_STANDALONE_OSX
        const string test = "test_pdalc";
#elif UNITY_STANDALONE_LINUX
        const string test = "test_pdalc";
#endif
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
#if UNITY_STANDALONE_WIN
                    string file = Path.Combine(pluginPath, test);
#else
                    string file = Path.Combine(pluginPath, "bin", test);
#endif
                    if (!File.Exists(file))
                    {
                        UpdatePackage();
                    }
                    else if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        string currentVersion = "0";
                        string response;
                        try
                        {
                            using (Process compiler = new Process())
                            {
                                compiler.StartInfo.FileName = Path.Combine(pluginPath, "bin", test);
                                compiler.StartInfo.Arguments = $" -h";
                                compiler.StartInfo.UseShellExecute = false;
                                compiler.StartInfo.RedirectStandardOutput = true;
                                compiler.StartInfo.CreateNoWindow = true;
                                compiler.Start();

                                response = compiler.StandardOutput.ReadToEnd();

                                compiler.WaitForExit();
                            }
                            currentVersion = response.Split(new char[3] { ' ', '\r', '\n' })[1];
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"Mdal Version error : {e.ToString()}");
                        }
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
                    Debug.Log($"Error in Conda Package {test} : {e.ToString()}");
                };
            };

            EditorUtility.ClearProgressBar();
            stopwatch.Stop();
            Debug.Log($"Pdal refresh took {stopwatch.Elapsed.TotalSeconds} seconds");
        }
        static void UpdatePackage() {
            Debug.Log("Pdal Install Script Awake"); 
            string pluginPath = Path.Combine(Application.dataPath, "Conda");
            string path = Path.GetDirectoryName(new StackTrace(true).GetFrame(0).GetFileName());
            string response;
            string install = $"pdal={packageVersion}";
            using (Process compiler = new Process())
            {
#if UNITY_STANDALONE_WIN
                compiler.StartInfo.FileName = "powershell.exe";
                compiler.StartInfo.Arguments = $"-ExecutionPolicy Bypass {Path.Combine(path, "install_script.ps1")} -package pdal " +
                                                    $"-install {install} " +
                                                    $"-destination {pluginPath} " +
                                                    $"-test pdal.exe";
#elif UNITY_STANDALONE_OSX
                compiler.StartInfo.FileName = "/bin/bash";
                compiler.StartInfo.Arguments = $" {Path.Combine(path, "install_script.sh")} " +
                                                "-p pdal " +
                                                $"-i {install} " +
                                                $"-d {pluginPath} " +
                                                $"-t pdal ";
#elif UNITY_STANDALONE_LINUX

#endif
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
#if UNITY_STANDALONE_WIN
                compiler.StartInfo.FileName = "powershell.exe";
                compiler.StartInfo.Arguments = $"-ExecutionPolicy Bypass {Path.Combine(path, "install_script.ps1")} -package pdal-c " +
                                                    $"-install {install} " +
                                                    $"-destination {pluginPath} " +
                                                    $"-test {test}";
#elif UNITY_STANDALONE_OSX
                compiler.StartInfo.FileName = "/bin/bash";
                compiler.StartInfo.Arguments = $" {Path.Combine(path, "install_script.sh")} " +
                                                "-p pdal-c " +
                                                $"-i {install} " +
                                                $"-d {pluginPath} " +
                                                $"-t {test} ";
#elif UNITY_STANDALONE_LINUX

#endif
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.Start();

                response = compiler.StandardOutput.ReadToEnd();

                compiler.WaitForExit();
            }
            install = $"laszip";
            using (Process compiler = new Process())
            {
#if UNITY_STANDALONE_WIN
                compiler.StartInfo.FileName = "powershell.exe";
                compiler.StartInfo.Arguments = $"-ExecutionPolicy Bypass {Path.Combine(path, "install_script.ps1")} -package laszip " +
                                                    $"-install {install} " +
                                                    $"-destination {pluginPath} " +
                                                    $"-test ";
#elif UNITY_STANDALONE_OSX
                compiler.StartInfo.FileName = "/bin/bash";
                compiler.StartInfo.Arguments = $" {Path.Combine(path, "install_script.sh")} " +
                                                "-p laszip " +
                                                $"-i {install} " +
                                                $"-d {pluginPath} " +
                                                $"-t  ";
#elif UNITY_STANDALONE_LINUX

#endif
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.Start();

                response = compiler.StandardOutput.ReadToEnd();

                compiler.WaitForExit();
            }
            Debug.Log(response);
        }
    }
}
