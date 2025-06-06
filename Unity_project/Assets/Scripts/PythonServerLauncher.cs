using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerLauncher : MonoBehaviour
{
    public string pythonExePath = @"C:\Path\To\python.exe"; // or just "python" if it's in PATH
    private Process pipeProcess;

    void Start()
    {
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
        string scriptPath = Path.Combine(rootPath, "npc_manager.py");
        UnityEngine.Debug.Log(rootPath);
        UnityEngine.Debug.Log(scriptPath);

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = pythonExePath;
        startInfo.Arguments = $"\"{scriptPath}\"";
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        try
        {
            pipeProcess = Process.Start(startInfo);
            UnityEngine.Debug.Log("[Unity] Started pipe_server.py");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to start pipe server: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (pipeProcess != null && !pipeProcess.HasExited)
        {
            pipeProcess.Kill();
            pipeProcess.Dispose();
            UnityEngine.Debug.Log("[Unity] Pipe server terminated.");
        }
    }
}