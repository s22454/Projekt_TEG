using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerLauncher : MonoBehaviour
{
    public string pythonExePath = @"C:\Path\To\python.exe";
    public string pythonScriptPath;
    private Process pipeProcess;

    void Start()
    {
        UnityEngine.Debug.Log(pythonScriptPath);

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
            UnityEngine.Debug.Log("[Unity] Started " + pythonScriptPath);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to start pipe server: " + ex.Message);
        }
    }
}
