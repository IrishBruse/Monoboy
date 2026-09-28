namespace Monoboy.Debug;

using System.Diagnostics;

/// <summary>Invokes <c>gbc build</c> so a debug session can start from a <c>.gbl</c> file.</summary>
public static class GbcHost
{
    public static bool TryBuild(string sourcePath, out string romPath, out string log, out string error)
    {
        romPath = "";
        log = "";
        error = "";

        string fullSource = Path.GetFullPath(sourcePath);
        if (!File.Exists(fullSource))
        {
            error = $"Source file not found: {fullSource}";
            return false;
        }

        string? directory = Path.GetDirectoryName(fullSource);
        string name = Path.GetFileNameWithoutExtension(fullSource);
        if (directory == null)
        {
            error = $"Source file not found: {fullSource}";
            return false;
        }

        romPath = Path.Combine(directory, "bin", name + ".gb");
        if (!TryCreateStart(fullSource, out ProcessStartInfo start, out error))
        {
            return false;
        }

        start.WorkingDirectory = directory;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.UseShellExecute = false;

        using Process process = new() { StartInfo = start };
        try
        {
            if (!process.Start())
            {
                error = "Failed to start gbc";
                return false;
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            error = $"Failed to start gbc: {ex.Message}";
            return false;
        }

        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(120_000))
        {
            process.Kill(entireProcessTree: true);
            error = "gbc timed out";
            return false;
        }

        log = (stdout.Result + stderr.Result).Trim();
        if (process.ExitCode != 0)
        {
            error = log.Length == 0 ? $"gbc exited with code {process.ExitCode}" : log;
            return false;
        }

        if (!File.Exists(romPath))
        {
            error = $"gbc did not write {romPath}";
            return false;
        }

        return true;
    }

    static bool TryCreateStart(string sourcePath, out ProcessStartInfo start, out string error)
    {
        start = new ProcessStartInfo();
        error = "";

        string? configured = Environment.GetEnvironmentVariable("GBC");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            start.FileName = configured;
            start.ArgumentList.Add("build");
            start.ArgumentList.Add(sourcePath);
            return true;
        }

        if (ExistsOnPath("gbc"))
        {
            start.FileName = "gbc";
            start.ArgumentList.Add("build");
            start.ArgumentList.Add(sourcePath);
            return true;
        }

        if (!TryFindProject(sourcePath, out string csproj) && !TryFindProject(AppContext.BaseDirectory, out csproj))
        {
            error = "gbc was not found. Put gbc on PATH, set GBC, or build this repository.";
            return false;
        }

        string projectDir = Path.GetDirectoryName(csproj)!;
        string debugBinary = Binary(projectDir, "Debug");
        string releaseBinary = Binary(projectDir, "Release");
        string binary = File.Exists(debugBinary) ? debugBinary : File.Exists(releaseBinary) ? releaseBinary : "";
        if (binary.Length > 0)
        {
            start.FileName = binary;
            start.ArgumentList.Add("build");
            start.ArgumentList.Add(sourcePath);
            return true;
        }

        string debugDll = Path.Combine(projectDir, "bin", "Debug", "net10.0", "gbc.dll");
        string releaseDll = Path.Combine(projectDir, "bin", "Release", "net10.0", "gbc.dll");
        string dll = File.Exists(debugDll) ? debugDll : File.Exists(releaseDll) ? releaseDll : "";
        if (dll.Length > 0)
        {
            start.FileName = "dotnet";
            start.ArgumentList.Add(dll);
            start.ArgumentList.Add("build");
            start.ArgumentList.Add(sourcePath);
            return true;
        }

        start.FileName = "dotnet";
        start.ArgumentList.Add("run");
        start.ArgumentList.Add("--project");
        start.ArgumentList.Add(csproj);
        start.ArgumentList.Add("--");
        start.ArgumentList.Add("build");
        start.ArgumentList.Add(sourcePath);
        return true;
    }

    static bool TryFindProject(string start, out string csproj)
    {
        csproj = "";
        DirectoryInfo? dir;
        try
        {
            dir = new DirectoryInfo(Path.GetFullPath(start));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }

        if (!Directory.Exists(start))
        {
            dir = dir.Parent;
        }

        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "GBL", "GBL.csproj");
            if (File.Exists(candidate))
            {
                csproj = candidate;
                return true;
            }

            dir = dir.Parent;
        }

        return false;
    }

    static bool ExistsOnPath(string name)
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        foreach (string entry in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (File.Exists(Path.Combine(entry, name)) || File.Exists(Path.Combine(entry, name + ".exe")))
            {
                return true;
            }
        }

        return false;
    }

    static string Binary(string projectDir, string configuration)
    {
        string name = OperatingSystem.IsWindows() ? "gbc.exe" : "gbc";
        return Path.Combine(projectDir, "bin", configuration, "net10.0", name);
    }
}
