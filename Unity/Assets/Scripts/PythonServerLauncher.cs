using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerLauncher : MonoBehaviour
{
    private static readonly string _className = "PYTHON LAUNCHER";
    public string pythonExePath = @"C:\Path\To\python.exe";
    public string pythonScriptPath;
    private Process pipeProcess;

    void Start()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = pythonExePath;
        startInfo.Arguments = $"\"{pythonScriptPath}\"";
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        try
        {
            pipeProcess = Process.Start(startInfo);
            LogManager.Log(_className, LogType.LOG, "Started script: " + pythonScriptPath);
        }
        catch (System.Exception ex)
        {
            LogManager.Log(_className, LogType.ERROR, "Failed to start pipe server: " + ex.Message);
        }
    }
}
