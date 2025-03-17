using System.Diagnostics;
using System.IO;
using EnvironMint.Models;

namespace EnvironMint.Services
{
    public class ToolDetectionService
    {
        public List<string> ScanDrives { get; set; } = new List<string> { "C:" };

        public bool IsToolInstalled(Tool tool)
        {
            if (string.IsNullOrWhiteSpace(tool.ValidationScript))
            {
                return false;
            }

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "powershell.exe";
                    process.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{tool.ValidationScript}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<Tool, bool>> ScanForInstalledTools(List<Tool> tools)
        {
            var results = new Dictionary<Tool, bool>();

            await Task.Run(() =>
            {
                foreach (var tool in tools)
                {
                    bool isInstalled = IsToolInstalled(tool);
                    results[tool] = isInstalled;

                    tool.IsInstalled = isInstalled;
                }
            });

            return results;
        }

        public async Task<bool> SearchForToolInDrives(Tool tool)
        {
            bool found = false;

            await Task.Run(() =>
            {
                if (IsToolInstalled(tool))
                {
                    found = true;
                    return;
                }

                foreach (string drive in ScanDrives)
                {
                    if (found) break;

                    var commonDirs = new[]
                    {
                        Path.Combine(drive, "Program Files"),
                        Path.Combine(drive, "Program Files (x86)"),
                        Path.Combine(drive, "Users")
                    };

                    foreach (var dir in commonDirs)
                    {
                        if (!Directory.Exists(dir)) continue;

                        try
                        {
                            var matchingDirs = Directory.GetDirectories(dir, $"*{tool.Name}*", SearchOption.AllDirectories);

                            if (matchingDirs.Length > 0)
                            {
                                found = true;
                                break;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            });

            return found;
        }
    }
}

