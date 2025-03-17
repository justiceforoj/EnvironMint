using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvironMint.Models;
using Newtonsoft.Json;

namespace EnvironMint.Services
{
    public class ToolManagementService
    {
        private readonly ToolDetectionService _toolDetectionService;
        private List<Tool> _tools = new List<Tool>();

        public ToolManagementService(ToolDetectionService toolDetectionService)
        {
            _toolDetectionService = toolDetectionService;
            LoadTools();
        }

        public List<Tool> Tools => _tools;

        public void LoadTools()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EnvironMint");

                Directory.CreateDirectory(appDataPath);

                string toolsFilePath = Path.Combine(appDataPath, "tools.json");

                if (File.Exists(toolsFilePath))
                {
                    string json = File.ReadAllText(toolsFilePath);
                    var loadedTools = JsonConvert.DeserializeObject<List<Tool>>(json);
                    if (loadedTools != null)
                    {
                        _tools = loadedTools;
                    }
                }

                if (_tools.Count == 0)
                {
                    _tools.Add(new Tool
                    {
                        Name = "Git",
                        Version = "Latest",
                        Category = "Development Tools",
                        InstallCommand = "winget install --id Git.Git -e",
                        ValidationScript = "if (Get-Command git -ErrorAction SilentlyContinue) { Write-Host \"Git is installed\" } else { Write-Error \"Git is not installed\" }"
                    });

                    _tools.Add(new Tool
                    {
                        Name = "Visual Studio Code",
                        Version = "Latest",
                        Category = "Development Tools",
                        InstallCommand = "winget install --id Microsoft.VisualStudioCode -e",
                        ValidationScript = "if (Get-Command code -ErrorAction SilentlyContinue) { Write-Host \"VS Code is installed\" } else { Write-Error \"VS Code is not installed\" }"
                    });

                    _tools.Add(new Tool
                    {
                        Name = "Node.js",
                        Version = "Latest",
                        Category = "Runtimes",
                        InstallCommand = "winget install OpenJS.NodeJS",
                        ValidationScript = "if (Get-Command node -ErrorAction SilentlyContinue) { Write-Host \"Node.js is installed\" } else { Write-Error \"Node.js is not installed\" }"
                    });

                    SaveTools();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading tools: {ex.Message}", ex);
            }
        }

        public void SaveTools()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EnvironMint");

                Directory.CreateDirectory(appDataPath);

                string toolsFilePath = Path.Combine(appDataPath, "tools.json");
                string json = JsonConvert.SerializeObject(_tools, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(toolsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving tools: {ex.Message}", ex);
            }
        }

        public void AddTool(Tool tool)
        {
            if (_tools.Any(t => t.Name == tool.Name))
            {
                throw new Exception($"A tool with the name '{tool.Name}' already exists.");
            }

            _tools.Add(tool);
            SaveTools();
        }

        public void UpdateTool(Tool tool)
        {
            int index = _tools.FindIndex(t => t.Name == tool.Name);
            if (index >= 0)
            {
                _tools[index] = tool;
                SaveTools();
            }
            else
            {
                throw new Exception($"Tool '{tool.Name}' not found.");
            }
        }

        public void DeleteTool(Tool tool)
        {
            if (_tools.Remove(tool))
            {
                SaveTools();
            }
            else
            {
                throw new Exception($"Tool '{tool.Name}' not found.");
            }
        }

        public async Task<Dictionary<Tool, bool>> ScanForInstalledToolsAsync()
        {
            return await _toolDetectionService.ScanForInstalledTools(_tools);
        }

        public Tool? FindToolByName(string name)
        {
            return _tools.Find(t => t.Name == name);
        }

        public void UpdateToolInstallationStatus()
        {
            foreach (var tool in _tools)
            {
                tool.IsInstalled = _toolDetectionService.IsToolInstalled(tool);
            }
        }
    }
}

