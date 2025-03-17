using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using EnvironMint.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EnvironMint.Services
{
    public class ProjectScannerService
    {
        private readonly ToolDetectionService _toolDetectionService;
        private Dictionary<string, List<Tool>> _recommendedTools = new Dictionary<string, List<Tool>>();
        private List<Tool> _tools;
        private Dictionary<string, bool> _detectedTechnologies = new Dictionary<string, bool>();
        private Dictionary<string, string> _detectedVersions = new Dictionary<string, string>();

        public ProjectScannerService(ToolDetectionService toolDetectionService, List<Tool> tools)
        {
            _toolDetectionService = toolDetectionService;
            _tools = tools;
        }

        public Dictionary<string, List<Tool>> RecommendedTools => _recommendedTools;
        public Dictionary<string, bool> DetectedTechnologies => _detectedTechnologies;
        public Dictionary<string, string> DetectedVersions => _detectedVersions;

        public async Task<Dictionary<string, List<Tool>>> ScanProjectAsync(string directory)
        {
            _recommendedTools.Clear();
            _detectedTechnologies.Clear();
            _detectedVersions.Clear();

            await Task.Run(() => ScanProjectDirectory(directory));

            return _recommendedTools;
        }

        private void ScanProjectDirectory(string directory)
        {
            try
            {
                ScanForCommonProjectFiles(directory);

                ScanForLanguageFiles(directory);

                ScanForFrameworks(directory);

                ScanForDatabaseTechnologies(directory);

                ScanForContainerization(directory);

                ScanForBuildSystems(directory);

                ScanForCloudServices(directory);

                GenerateToolRecommendations();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scanning project: {ex.Message}");
            }
        }

        private void ScanForCommonProjectFiles(string directory)
        {
            if (File.Exists(Path.Combine(directory, "package.json")))
            {
                _detectedTechnologies["Node.js"] = true;

                try
                {
                    string packageJsonPath = Path.Combine(directory, "package.json");
                    string packageJson = File.ReadAllText(packageJsonPath);
                    JObject packageObj = JObject.Parse(packageJson);

                    if (packageObj["engines"] != null && packageObj["engines"]["node"] != null)
                    {
                        _detectedVersions["Node.js"] = packageObj["engines"]["node"].ToString();
                    }

                    var dependencies = new Dictionary<string, JToken>();
                    if (packageObj["dependencies"] != null)
                    {
                        foreach (var dep in packageObj["dependencies"].ToObject<JObject>())
                        {
                            dependencies[dep.Key] = dep.Value;
                        }
                    }

                    if (packageObj["devDependencies"] != null)
                    {
                        foreach (var dep in packageObj["devDependencies"].ToObject<JObject>())
                        {
                            dependencies[dep.Key] = dep.Value;
                        }
                    }

                    CheckNodeDependency(dependencies, "react", "React");
                    CheckNodeDependency(dependencies, "react-dom", "React");
                    CheckNodeDependency(dependencies, "@angular/core", "Angular");
                    CheckNodeDependency(dependencies, "vue", "Vue.js");
                    CheckNodeDependency(dependencies, "next", "Next.js");
                    CheckNodeDependency(dependencies, "nuxt", "Nuxt.js");
                    CheckNodeDependency(dependencies, "express", "Express.js");
                    CheckNodeDependency(dependencies, "koa", "Koa.js");
                    CheckNodeDependency(dependencies, "fastify", "Fastify");
                    CheckNodeDependency(dependencies, "nest", "NestJS");
                    CheckNodeDependency(dependencies, "electron", "Electron");
                    CheckNodeDependency(dependencies, "svelte", "Svelte");
                    CheckNodeDependency(dependencies, "tailwindcss", "Tailwind CSS");
                    CheckNodeDependency(dependencies, "bootstrap", "Bootstrap");
                    CheckNodeDependency(dependencies, "@mui/material", "Material UI");
                    CheckNodeDependency(dependencies, "styled-components", "Styled Components");
                    CheckNodeDependency(dependencies, "emotion", "Emotion");
                    CheckNodeDependency(dependencies, "redux", "Redux");
                    CheckNodeDependency(dependencies, "mobx", "MobX");
                    CheckNodeDependency(dependencies, "recoil", "Recoil");
                    CheckNodeDependency(dependencies, "zustand", "Zustand");
                    CheckNodeDependency(dependencies, "jest", "Jest");
                    CheckNodeDependency(dependencies, "mocha", "Mocha");
                    CheckNodeDependency(dependencies, "cypress", "Cypress");
                    CheckNodeDependency(dependencies, "playwright", "Playwright");
                    CheckNodeDependency(dependencies, "storybook", "Storybook");
                    CheckNodeDependency(dependencies, "webpack", "Webpack");
                    CheckNodeDependency(dependencies, "vite", "Vite");
                    CheckNodeDependency(dependencies, "parcel", "Parcel");
                    CheckNodeDependency(dependencies, "esbuild", "esbuild");
                    CheckNodeDependency(dependencies, "typescript", "TypeScript");
                    CheckNodeDependency(dependencies, "prisma", "Prisma");
                    CheckNodeDependency(dependencies, "sequelize", "Sequelize");
                    CheckNodeDependency(dependencies, "typeorm", "TypeORM");
                    CheckNodeDependency(dependencies, "mongoose", "Mongoose");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing package.json: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "yarn.lock")))
                _detectedTechnologies["Yarn"] = true;

            if (File.Exists(Path.Combine(directory, "pnpm-lock.yaml")))
                _detectedTechnologies["pnpm"] = true;

            if (File.Exists(Path.Combine(directory, "requirements.txt")))
            {
                _detectedTechnologies["Python"] = true;

                try
                {
                    string requirementsPath = Path.Combine(directory, "requirements.txt");
                    string[] lines = File.ReadAllLines(requirementsPath);

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("django", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["Django"] = true;
                            ExtractVersionFromRequirement(line, "Django");
                        }
                        else if (line.StartsWith("flask", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["Flask"] = true;
                            ExtractVersionFromRequirement(line, "Flask");
                        }
                        else if (line.StartsWith("fastapi", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["FastAPI"] = true;
                            ExtractVersionFromRequirement(line, "FastAPI");
                        }
                        else if (line.StartsWith("sqlalchemy", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["SQLAlchemy"] = true;
                            ExtractVersionFromRequirement(line, "SQLAlchemy");
                        }
                        else if (line.StartsWith("pytest", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["pytest"] = true;
                            ExtractVersionFromRequirement(line, "pytest");
                        }
                        else if (line.StartsWith("numpy", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["NumPy"] = true;
                            ExtractVersionFromRequirement(line, "NumPy");
                        }
                        else if (line.StartsWith("pandas", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["Pandas"] = true;
                            ExtractVersionFromRequirement(line, "Pandas");
                        }
                        else if (line.StartsWith("tensorflow", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["TensorFlow"] = true;
                            ExtractVersionFromRequirement(line, "TensorFlow");
                        }
                        else if (line.StartsWith("pytorch", StringComparison.OrdinalIgnoreCase) || line.StartsWith("torch", StringComparison.OrdinalIgnoreCase))
                        {
                            _detectedTechnologies["PyTorch"] = true;
                            ExtractVersionFromRequirement(line, "PyTorch");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing requirements.txt: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "Pipfile")) || File.Exists(Path.Combine(directory, "Pipfile.lock")))
            {
                _detectedTechnologies["Python"] = true;
                _detectedTechnologies["Pipenv"] = true;

                try
                {
                    string pipfileLockPath = Path.Combine(directory, "Pipfile.lock");
                    if (File.Exists(pipfileLockPath))
                    {
                        string pipfileLockContent = File.ReadAllText(pipfileLockPath);
                        JObject pipfileLock = JObject.Parse(pipfileLockContent);

                        if (pipfileLock["default"] != null)
                        {
                            CheckPythonPackage(pipfileLock["default"], "django", "Django");
                            CheckPythonPackage(pipfileLock["default"], "flask", "Flask");
                            CheckPythonPackage(pipfileLock["default"], "fastapi", "FastAPI");
                            CheckPythonPackage(pipfileLock["default"], "sqlalchemy", "SQLAlchemy");
                            CheckPythonPackage(pipfileLock["default"], "numpy", "NumPy");
                            CheckPythonPackage(pipfileLock["default"], "pandas", "Pandas");
                            CheckPythonPackage(pipfileLock["default"], "tensorflow", "TensorFlow");
                            CheckPythonPackage(pipfileLock["default"], "torch", "PyTorch");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing Pipfile.lock: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "poetry.lock")))
            {
                _detectedTechnologies["Python"] = true;
                _detectedTechnologies["Poetry"] = true;
            }

            if (File.Exists(Path.Combine(directory, "pom.xml")))
            {
                _detectedTechnologies["Java"] = true;
                _detectedTechnologies["Maven"] = true;

                try
                {
                    string pomPath = Path.Combine(directory, "pom.xml");
                    XmlDocument pomXml = new XmlDocument();
                    pomXml.Load(pomPath);

                    XmlNode javaVersionNode = pomXml.SelectSingleNode("//properties/java.version");
                    if (javaVersionNode != null)
                    {
                        _detectedVersions["Java"] = javaVersionNode.InnerText;
                    }
                    else
                    {
                        XmlNode mavenCompilerNode = pomXml.SelectSingleNode("//plugins/plugin[artifactId='maven-compiler-plugin']/configuration/source");
                        if (mavenCompilerNode != null)
                        {
                            _detectedVersions["Java"] = mavenCompilerNode.InnerText;
                        }
                    }

                    XmlNodeList dependencies = pomXml.SelectNodes("//dependencies/dependency");
                    foreach (XmlNode dependency in dependencies)
                    {
                        XmlNode groupIdNode = dependency.SelectSingleNode("groupId");
                        XmlNode artifactIdNode = dependency.SelectSingleNode("artifactId");
                        XmlNode versionNode = dependency.SelectSingleNode("version");

                        if (groupIdNode != null && artifactIdNode != null)
                        {
                            string groupId = groupIdNode.InnerText;
                            string artifactId = artifactIdNode.InnerText;
                            string version = versionNode?.InnerText ?? "";

                            if (groupId == "org.springframework.boot")
                            {
                                _detectedTechnologies["Spring Boot"] = true;
                                if (!string.IsNullOrEmpty(version))
                                {
                                    _detectedVersions["Spring Boot"] = version;
                                }
                            }
                            else if (groupId == "org.hibernate")
                            {
                                _detectedTechnologies["Hibernate"] = true;
                                if (!string.IsNullOrEmpty(version))
                                {
                                    _detectedVersions["Hibernate"] = version;
                                }
                            }
                            else if (groupId == "junit")
                            {
                                _detectedTechnologies["JUnit"] = true;
                                if (!string.IsNullOrEmpty(version))
                                {
                                    _detectedVersions["JUnit"] = version;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing pom.xml: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "build.gradle")) || File.Exists(Path.Combine(directory, "build.gradle.kts")))
            {
                _detectedTechnologies["Java"] = true;
                _detectedTechnologies["Gradle"] = true;

                try
                {
                    string gradlePath = File.Exists(Path.Combine(directory, "build.gradle"))
                        ? Path.Combine(directory, "build.gradle")
                        : Path.Combine(directory, "build.gradle.kts");

                    string gradleContent = File.ReadAllText(gradlePath);

                    if (gradleContent.Contains("org.springframework.boot"))
                    {
                        _detectedTechnologies["Spring Boot"] = true;

                        Match springBootVersionMatch = Regex.Match(gradleContent, @"spring-boot-starter-[a-zA-Z-]+:(\d+\.\d+\.\d+)");
                        if (springBootVersionMatch.Success)
                        {
                            _detectedVersions["Spring Boot"] = springBootVersionMatch.Groups[1].Value;
                        }
                    }

                    if (gradleContent.Contains("org.jetbrains.kotlin"))
                    {
                        _detectedTechnologies["Kotlin"] = true;
                    }

                    if (gradleContent.Contains("com.android.application") || gradleContent.Contains("com.android.library"))
                    {
                        _detectedTechnologies["Android"] = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing build.gradle: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "Gemfile")))
            {
                _detectedTechnologies["Ruby"] = true;
                _detectedTechnologies["Bundler"] = true;

                try
                {
                    string gemfilePath = Path.Combine(directory, "Gemfile");
                    string[] lines = File.ReadAllLines(gemfilePath);

                    foreach (string line in lines)
                    {
                        Match rubyVersionMatch = Regex.Match(line, @"ruby\s+['""]([^'""]+)['""]");
                        if (rubyVersionMatch.Success)
                        {
                            _detectedVersions["Ruby"] = rubyVersionMatch.Groups[1].Value;
                        }

                        Match railsMatch = Regex.Match(line, @"gem\s+['""]rails['""],\s*['""]([^'""]+)['""]");
                        if (railsMatch.Success)
                        {
                            _detectedTechnologies["Ruby on Rails"] = true;
                            _detectedVersions["Ruby on Rails"] = railsMatch.Groups[1].Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing Gemfile: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "composer.json")))
            {
                _detectedTechnologies["PHP"] = true;
                _detectedTechnologies["Composer"] = true;

                try
                {
                    string composerJsonPath = Path.Combine(directory, "composer.json");
                    string composerJson = File.ReadAllText(composerJsonPath);
                    JObject composerObj = JObject.Parse(composerJson);

                    if (composerObj["require"] != null && composerObj["require"]["php"] != null)
                    {
                        _detectedVersions["PHP"] = composerObj["require"]["php"].ToString();
                    }

                    if (composerObj["require"] != null && composerObj["require"]["laravel/framework"] != null)
                    {
                        _detectedTechnologies["Laravel"] = true;
                        _detectedVersions["Laravel"] = composerObj["require"]["laravel/framework"].ToString();
                    }

                    if (composerObj["require"] != null && composerObj["require"]["symfony/symfony"] != null)
                    {
                        _detectedTechnologies["Symfony"] = true;
                        _detectedVersions["Symfony"] = composerObj["require"]["symfony/symfony"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing composer.json: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "go.mod")))
            {
                _detectedTechnologies["Go"] = true;

                try
                {
                    string goModPath = Path.Combine(directory, "go.mod");
                    string[] lines = File.ReadAllLines(goModPath);

                    foreach (string line in lines)
                    {
                        Match goVersionMatch = Regex.Match(line, @"go\s+(\d+\.\d+)");
                        if (goVersionMatch.Success)
                        {
                            _detectedVersions["Go"] = goVersionMatch.Groups[1].Value;
                        }

                        if (line.Contains("github.com/gin-gonic/gin"))
                        {
                            _detectedTechnologies["Gin"] = true;
                        }
                        else if (line.Contains("github.com/gorilla/mux"))
                        {
                            _detectedTechnologies["Gorilla Mux"] = true;
                        }
                        else if (line.Contains("github.com/labstack/echo"))
                        {
                            _detectedTechnologies["Echo"] = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing go.mod: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "cargo.toml")))
            {
                _detectedTechnologies["Rust"] = true;

                try
                {
                    string cargoTomlPath = Path.Combine(directory, "cargo.toml");
                    string[] lines = File.ReadAllLines(cargoTomlPath);

                    bool inDependencies = false;
                    foreach (string line in lines)
                    {
                        if (line.Trim() == "[dependencies]")
                        {
                            inDependencies = true;
                            continue;
                        }

                        if (inDependencies && line.Trim().StartsWith("["))
                        {
                            inDependencies = false;
                        }

                        if (inDependencies)
                        {
                            if (line.Trim().StartsWith("rocket"))
                            {
                                _detectedTechnologies["Rocket"] = true;
                            }
                            else if (line.Trim().StartsWith("actix-web"))
                            {
                                _detectedTechnologies["Actix Web"] = true;
                            }
                            else if (line.Trim().StartsWith("warp"))
                            {
                                _detectedTechnologies["Warp"] = true;
                            }
                            else if (line.Trim().StartsWith("tokio"))
                            {
                                _detectedTechnologies["Tokio"] = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing Cargo.toml: {ex.Message}");
                }
            }

            if (File.Exists(Path.Combine(directory, "CMakeLists.txt")))
                _detectedTechnologies["CMake"] = true;

            if (File.Exists(Path.Combine(directory, "Makefile")))
                _detectedTechnologies["Make"] = true;
        }

        private void CheckNodeDependency(Dictionary<string, JToken> dependencies, string packageName, string technologyName)
        {
            if (dependencies.ContainsKey(packageName))
            {
                _detectedTechnologies[technologyName] = true;
                string version = dependencies[packageName].ToString().TrimStart('^', '~', '>', '=', '<');
                _detectedVersions[technologyName] = version;
            }
        }

        private void CheckPythonPackage(JToken packages, string packageName, string technologyName)
        {
            if (packages[packageName] != null)
            {
                _detectedTechnologies[technologyName] = true;
                if (packages[packageName]["version"] != null)
                {
                    string version = packages[packageName]["version"].ToString().Trim('"', '\'');
                    version = Regex.Replace(version, @"^[=~<>]+", "");
                    _detectedVersions[technologyName] = version;
                }
            }
        }

        private void ExtractVersionFromRequirement(string requirement, string technologyName)
        {
            Match versionMatch = Regex.Match(requirement, @"[=~<>]+([0-9]+(\.[0-9]+)*)");
            if (versionMatch.Success)
            {
                _detectedVersions[technologyName] = versionMatch.Groups[1].Value;
            }
        }

        private void ScanForLanguageFiles(string directory)
        {
            if (Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["C#"] = true;

            if (Directory.GetFiles(directory, "*.fs", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["F#"] = true;

            if (Directory.GetFiles(directory, "*.vb", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Visual Basic"] = true;

            if (Directory.GetFiles(directory, "*.java", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Java"] = true;

            if (Directory.GetFiles(directory, "*.kt", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.kts", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Kotlin"] = true;

            if (Directory.GetFiles(directory, "*.py", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Python"] = true;

            if (Directory.GetFiles(directory, "*.js", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["JavaScript"] = true;

            if (Directory.GetFiles(directory, "*.ts", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["TypeScript"] = true;

            if (Directory.GetFiles(directory, "*.jsx", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.tsx", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["React"] = true;

            if (Directory.GetFiles(directory, "*.rb", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Ruby"] = true;

            if (Directory.GetFiles(directory, "*.php", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["PHP"] = true;

            if (Directory.GetFiles(directory, "*.go", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Go"] = true;

            if (Directory.GetFiles(directory, "*.rs", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Rust"] = true;

            if (Directory.GetFiles(directory, "*.swift", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Swift"] = true;

            if (Directory.GetFiles(directory, "*.cpp", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.cc", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.cxx", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["C++"] = true;

            if (Directory.GetFiles(directory, "*.c", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["C"] = true;

            if (Directory.GetFiles(directory, "*.h", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.hpp", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["C/C++ Headers"] = true;

            if (Directory.GetFiles(directory, "*.html", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.htm", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["HTML"] = true;

            if (Directory.GetFiles(directory, "*.css", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["CSS"] = true;

            if (Directory.GetFiles(directory, "*.scss", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["SCSS"] = true;

            if (Directory.GetFiles(directory, "*.sass", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Sass"] = true;

            if (Directory.GetFiles(directory, "*.less", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Less"] = true;

            if (Directory.GetFiles(directory, "*.swift", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.m", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.h", SearchOption.AllDirectories).Length > 0 &&
                Directory.Exists(Path.Combine(directory, "ios")))
                _detectedTechnologies["iOS"] = true;

            if (Directory.GetFiles(directory, "*.java", SearchOption.AllDirectories).Length > 0 &&
                Directory.Exists(Path.Combine(directory, "android")))
                _detectedTechnologies["Android"] = true;

            if (Directory.GetFiles(directory, "*.dart", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Dart"] = true;

            if (File.Exists(Path.Combine(directory, "pubspec.yaml")))
                _detectedTechnologies["Flutter"] = true;

            if (Directory.GetFiles(directory, "*.ipynb", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Jupyter Notebook"] = true;

            if (Directory.GetFiles(directory, "*.r", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.R", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["R"] = true;
        }

        private void ScanForFrameworks(string directory)
        {
            if (Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.fsproj", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.vbproj", SearchOption.AllDirectories).Length > 0)
            {
                _detectedTechnologies[".NET"] = true;

                foreach (var projFile in Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories))
                {
                    try
                    {
                        string projContent = File.ReadAllText(projFile);
                        if (projContent.Contains("<TargetFramework>net"))
                        {
                            _detectedTechnologies[".NET Core/.NET 5+"] = true;

                            Match versionMatch = Regex.Match(projContent, @"<TargetFramework>net(\d+\.\d+)</TargetFramework>");
                            if (versionMatch.Success)
                            {
                                _detectedVersions[".NET"] = versionMatch.Groups[1].Value;
                            }
                        }
                        if (projContent.Contains("<TargetFramework>netcoreapp"))
                        {
                            _detectedTechnologies[".NET Core"] = true;

                            Match versionMatch = Regex.Match(projContent, @"<TargetFramework>netcoreapp(\d+\.\d+)</TargetFramework>");
                            if (versionMatch.Success)
                            {
                                _detectedVersions[".NET Core"] = versionMatch.Groups[1].Value;
                            }
                        }
                        if (projContent.Contains("<TargetFramework>netstandard"))
                        {
                            _detectedTechnologies[".NET Standard"] = true;

                            Match versionMatch = Regex.Match(projContent, @"<TargetFramework>netstandard(\d+\.\d+)</TargetFramework>");
                            if (versionMatch.Success)
                            {
                                _detectedVersions[".NET Standard"] = versionMatch.Groups[1].Value;
                            }
                        }
                        if (projContent.Contains("<TargetFrameworkVersion>v4.") ||
                            projContent.Contains("<TargetFramework>net4"))
                        {
                            _detectedTechnologies[".NET Framework"] = true;

                            Match versionMatch = Regex.Match(projContent, @"<TargetFrameworkVersion>v(\d+\.\d+)</TargetFrameworkVersion>");
                            if (versionMatch.Success)
                            {
                                _detectedVersions[".NET Framework"] = versionMatch.Groups[1].Value;
                            }
                        }

                        if (projContent.Contains("Microsoft.AspNetCore") ||
                            projContent.Contains("Microsoft.AspNet.Mvc"))
                        {
                            _detectedTechnologies["ASP.NET"] = true;

                            if (projContent.Contains("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation"))
                            {
                                _detectedTechnologies["ASP.NET MVC"] = true;
                            }

                            if (projContent.Contains("Microsoft.AspNetCore.SignalR"))
                            {
                                _detectedTechnologies["SignalR"] = true;
                            }
                        }

                        if (projContent.Contains("PresentationFramework") ||
                            projContent.Contains("<UseWPF>true</UseWPF>"))
                            _detectedTechnologies["WPF"] = true;

                        if (projContent.Contains("System.Windows.Forms") ||
                            projContent.Contains("<UseWindowsForms>true</UseWindowsForms>"))
                            _detectedTechnologies["Windows Forms"] = true;

                        if (projContent.Contains("Xamarin") || projContent.Contains("MAUI"))
                            _detectedTechnologies["Xamarin/MAUI"] = true;

                        if (projContent.Contains("Microsoft.AspNetCore.Components.WebAssembly") ||
                            projContent.Contains("Microsoft.AspNetCore.Components.Web"))
                        {
                            _detectedTechnologies["Blazor"] = true;
                        }

                        if (projContent.Contains("Microsoft.EntityFrameworkCore"))
                        {
                            _detectedTechnologies["Entity Framework Core"] = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error analyzing project file {projFile}: {ex.Message}");
                    }
                }
            }

            if (Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Visual Studio"] = true;

            // Web frameworks
            if (File.Exists(Path.Combine(directory, "angular.json")))
                _detectedTechnologies["Angular"] = true;

            if (File.Exists(Path.Combine(directory, "vue.config.js")) ||
                Directory.GetFiles(directory, "*.vue", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Vue.js"] = true;

            if (File.Exists(Path.Combine(directory, "next.config.js")))
                _detectedTechnologies["Next.js"] = true;

            if (File.Exists(Path.Combine(directory, "nuxt.config.js")))
                _detectedTechnologies["Nuxt.js"] = true;

            if (File.Exists(Path.Combine(directory, "svelte.config.js")) ||
                Directory.GetFiles(directory, "*.svelte", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Svelte"] = true;

            if (File.Exists(Path.Combine(directory, "config/routes.rb")))
                _detectedTechnologies["Ruby on Rails"] = true;

            if (File.Exists(Path.Combine(directory, "manage.py")) &&
                Directory.Exists(Path.Combine(directory, "migrations")))
                _detectedTechnologies["Django"] = true;

            if (File.Exists(Path.Combine(directory, "app.py")) &&
                (File.ReadAllText(Path.Combine(directory, "app.py")).Contains("Flask") ||
                 File.ReadAllText(Path.Combine(directory, "app.py")).Contains("flask")))
                _detectedTechnologies["Flask"] = true;

            if (File.Exists(Path.Combine(directory, "artisan")))
                _detectedTechnologies["Laravel"] = true;

            var htmlFiles = Directory.GetFiles(directory, "*.html", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.htm", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(directory, "*.cshtml", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(directory, "*.razor", SearchOption.AllDirectories));

            var cssFiles = Directory.GetFiles(directory, "*.css", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.scss", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(directory, "*.sass", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(directory, "*.less", SearchOption.AllDirectories));

            foreach (var file in htmlFiles.Concat(cssFiles))
            {
                try
                {
                    string content = File.ReadAllText(file);

                    if (content.Contains("bootstrap") || content.Contains("Bootstrap"))
                        _detectedTechnologies["Bootstrap"] = true;

                    if (content.Contains("tailwind") || content.Contains("Tailwind"))
                        _detectedTechnologies["Tailwind CSS"] = true;

                    if (content.Contains("material-ui") || content.Contains("MuiThemeProvider"))
                        _detectedTechnologies["Material UI"] = true;

                    if (content.Contains("bulma"))
                        _detectedTechnologies["Bulma"] = true;

                    if (content.Contains("foundation"))
                        _detectedTechnologies["Foundation"] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error analyzing file {file}: {ex.Message}");
                }
            }
        }

        private void ScanForDatabaseTechnologies(string directory)
        {
            var allFiles = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                try
                {
                    if (IsBinaryFile(file) || new FileInfo(file).Length > 1024 * 1024)
                        continue;

                    string content = File.ReadAllText(file);

                    if (content.Contains("mongodb://") || content.Contains("mongodb+srv://"))
                        _detectedTechnologies["MongoDB"] = true;

                    if (content.Contains("postgresql://") || content.Contains("postgres://"))
                        _detectedTechnologies["PostgreSQL"] = true;

                    if (content.Contains("mysql://") || content.Contains("Data Source=") && content.Contains("MySql"))
                        _detectedTechnologies["MySQL"] = true;

                    if (content.Contains("Data Source=") && content.Contains("Initial Catalog=") ||
                        content.Contains("Server=") && content.Contains("Database="))
                        _detectedTechnologies["SQL Server"] = true;

                    if (content.Contains("redis://") || content.Contains("RedisConnectionFactory"))
                        _detectedTechnologies["Redis"] = true;

                    if (content.Contains("sqlite://") || content.Contains("Data Source=") && content.Contains(".db"))
                        _detectedTechnologies["SQLite"] = true;

                    if (content.Contains("firestore") || content.Contains("firebase"))
                        _detectedTechnologies["Firebase"] = true;

                    if (content.Contains("dynamodb") || content.Contains("DynamoDB"))
                        _detectedTechnologies["DynamoDB"] = true;

                    if (content.Contains("cosmosdb") || content.Contains("CosmosDB"))
                        _detectedTechnologies["CosmosDB"] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error analyzing file {file}: {ex.Message}");
                }
            }

            if (Directory.GetFiles(directory, "*.db", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.sqlite", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.sqlite3", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["SQLite"] = true;

            if (Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("DbContext") || File.ReadAllText(f).Contains("Entity Framework")))
                _detectedTechnologies["Entity Framework"] = true;

            if (Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("NHibernate")))
                _detectedTechnologies["NHibernate"] = true;

            if (Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("Dapper")))
                _detectedTechnologies["Dapper"] = true;

            if (Directory.GetFiles(directory, "*.js", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.ts", SearchOption.AllDirectories))
                .Any(f => File.ReadAllText(f).Contains("sequelize")))
                _detectedTechnologies["Sequelize"] = true;

            if (Directory.GetFiles(directory, "*.js", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.ts", SearchOption.AllDirectories))
                .Any(f => File.ReadAllText(f).Contains("mongoose")))
                _detectedTechnologies["Mongoose"] = true;

            if (Directory.GetFiles(directory, "*.js", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.ts", SearchOption.AllDirectories))
                .Any(f => File.ReadAllText(f).Contains("typeorm")))
                _detectedTechnologies["TypeORM"] = true;

            if (Directory.GetFiles(directory, "*.js", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.ts", SearchOption.AllDirectories))
                .Any(f => File.ReadAllText(f).Contains("prisma")))
                _detectedTechnologies["Prisma"] = true;

            if (Directory.GetFiles(directory, "*.py", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("sqlalchemy")))
                _detectedTechnologies["SQLAlchemy"] = true;

            if (Directory.GetFiles(directory, "*.py", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("django.db")))
                _detectedTechnologies["Django ORM"] = true;

            if (Directory.GetFiles(directory, "*.rb", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("ActiveRecord")))
                _detectedTechnologies["ActiveRecord"] = true;

            if (Directory.GetFiles(directory, "*.java", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("javax.persistence") || File.ReadAllText(f).Contains("jakarta.persistence")))
                _detectedTechnologies["JPA"] = true;

            if (Directory.GetFiles(directory, "*.java", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("org.hibernate")))
                _detectedTechnologies["Hibernate"] = true;
        }

        private void ScanForContainerization(string directory)
        {
            if (File.Exists(Path.Combine(directory, "Dockerfile")))
                _detectedTechnologies["Docker"] = true;

            if (File.Exists(Path.Combine(directory, "docker-compose.yml")) ||
                File.Exists(Path.Combine(directory, "docker-compose.yaml")))
                _detectedTechnologies["Docker Compose"] = true;

            if (Directory.Exists(Path.Combine(directory, ".github", "workflows")))
                _detectedTechnologies["GitHub Actions"] = true;

            if (File.Exists(Path.Combine(directory, "azure-pipelines.yml")))
                _detectedTechnologies["Azure DevOps"] = true;

            if (File.Exists(Path.Combine(directory, ".gitlab-ci.yml")))
                _detectedTechnologies["GitLab CI"] = true;

            if (Directory.GetFiles(directory, "*.tf", SearchOption.AllDirectories).Length > 0 ||
                Directory.GetFiles(directory, "*.tfvars", SearchOption.AllDirectories).Length > 0)
                _detectedTechnologies["Terraform"] = true;

            if (Directory.GetFiles(directory, "*.yaml", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("apiVersion:") && File.ReadAllText(f).Contains("kind:")))
                _detectedTechnologies["Kubernetes"] = true;

            if (File.Exists(Path.Combine(directory, "serverless.yml")) ||
                File.Exists(Path.Combine(directory, "serverless.yaml")))
                _detectedTechnologies["Serverless Framework"] = true;

            if (Directory.GetFiles(directory, "*.yaml", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("AWSTemplateFormatVersion")))
                _detectedTechnologies["AWS CloudFormation"] = true;

            if (Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories)
                .Any(f => File.ReadAllText(f).Contains("$schema") && File.ReadAllText(f).Contains("deploymentTemplate.json")))
                _detectedTechnologies["Azure Resource Manager"] = true;
        }

        private void ScanForBuildSystems(string directory)
        {
            if (File.Exists(Path.Combine(directory, "webpack.config.js")))
                _detectedTechnologies["Webpack"] = true;

            if (File.Exists(Path.Combine(directory, "vite.config.js")) ||
                File.Exists(Path.Combine(directory, "vite.config.ts")))
                _detectedTechnologies["Vite"] = true;

            if (File.Exists(Path.Combine(directory, "rollup.config.js")))
                _detectedTechnologies["Rollup"] = true;

            if (File.Exists(Path.Combine(directory, "gulpfile.js")))
                _detectedTechnologies["Gulp"] = true;

            if (File.Exists(Path.Combine(directory, "Gruntfile.js")))
                _detectedTechnologies["Grunt"] = true;

            if (File.Exists(Path.Combine(directory, "babel.config.js")) ||
                File.Exists(Path.Combine(directory, ".babelrc")))
                _detectedTechnologies["Babel"] = true;

            if (File.Exists(Path.Combine(directory, "tsconfig.json")))
                _detectedTechnologies["TypeScript"] = true;

            if (File.Exists(Path.Combine(directory, "jest.config.js")))
                _detectedTechnologies["Jest"] = true;

            if (File.Exists(Path.Combine(directory, "cypress.json")) ||
                File.Exists(Path.Combine(directory, "cypress.config.js")))
                _detectedTechnologies["Cypress"] = true;

            if (File.Exists(Path.Combine(directory, ".eslintrc.js")) ||
                File.Exists(Path.Combine(directory, ".eslintrc.json")) ||
                File.Exists(Path.Combine(directory, ".eslintrc")))
                _detectedTechnologies["ESLint"] = true;

            if (File.Exists(Path.Combine(directory, ".prettierrc")) ||
                File.Exists(Path.Combine(directory, ".prettierrc.js")) ||
                File.Exists(Path.Combine(directory, ".prettierrc.json")))
                _detectedTechnologies["Prettier"] = true;
        }

        private void ScanForCloudServices(string directory)
        {
            var allFiles = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                try
                {
                    if (IsBinaryFile(file) || new FileInfo(file).Length > 1024 * 1024)
                        continue;

                    string content = File.ReadAllText(file);
                    string fileName = Path.GetFileName(file);

                    // AWS
                    if (content.Contains("aws-sdk") || content.Contains("AWS.") ||
                        content.Contains("amazonaws") || fileName.Contains("aws"))
                        _detectedTechnologies["AWS"] = true;

                    // Azure
                    if (content.Contains("azure-") || content.Contains("Azure.") ||
                        content.Contains("windowsazure") || fileName.Contains("azure"))
                        _detectedTechnologies["Azure"] = true;

                    // Google Cloud
                    if (content.Contains("google-cloud") || content.Contains("gcloud") ||
                        content.Contains("firebase") || fileName.Contains("gcp") ||
                        fileName.Contains("google-cloud"))
                        _detectedTechnologies["Google Cloud"] = true;

                    // Vercel
                    if (File.Exists(Path.Combine(directory, "vercel.json")) ||
                        content.Contains("vercel") || content.Contains("now.sh"))
                        _detectedTechnologies["Vercel"] = true;

                    // Netlify
                    if (File.Exists(Path.Combine(directory, "netlify.toml")) ||
                        content.Contains("netlify"))
                        _detectedTechnologies["Netlify"] = true;

                    // Heroku
                    if (File.Exists(Path.Combine(directory, "Procfile")) ||
                        content.Contains("heroku"))
                        _detectedTechnologies["Heroku"] = true;

                    // Digital Ocean
                    if (content.Contains("digitalocean"))
                        _detectedTechnologies["Digital Ocean"] = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error analyzing file {file}: {ex.Message}");
                }
            }
        }

        private bool IsBinaryFile(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[Math.Min(4096, stream.Length)];
                    stream.Read(buffer, 0, buffer.Length);

                    return buffer.Any(b => b == 0);
                }
            }
            catch
            {
                return true; // if file cannot be parsed, default to binary
            }
        }

        private void GenerateToolRecommendations()
        {
            _recommendedTools.Clear();

            foreach (var tech in _detectedTechnologies.Keys)
            {
                switch (tech)
                {
                    case "Node.js":
                        AddRecommendedTool("Node.js Development", "Node.js",
                            "winget install OpenJS.NodeJS",
                            "if (Get-Command node -ErrorAction SilentlyContinue) { Write-Host \"Node.js is installed\" } else { Write-Error \"Node.js is not installed\" }");

                        AddRecommendedTool("Node.js Development", "npm",
                            "winget install OpenJS.NodeJS",
                            "if (Get-Command npm -ErrorAction SilentlyContinue) { Write-Host \"npm is installed\" } else { Write-Error \"npm is not installed\" }");
                        break;

                    case "React":
                    case "Angular":
                    case "Vue.js":
                    case "Next.js":
                    case "Nuxt.js":
                    case "Svelte":
                        AddRecommendedTool($"{tech} Development", "Node.js",
                            "winget install OpenJS.NodeJS",
                            "if (Get-Command node -ErrorAction SilentlyContinue) { Write-Host \"Node.js is installed\" } else { Write-Error \"Node.js is not installed\" }");

                        AddRecommendedTool($"{tech} Development", "Visual Studio Code",
                            "winget install Microsoft.VisualStudioCode",
                            "if (Get-Command code -ErrorAction SilentlyContinue) { Write-Host \"VS Code is installed\" } else { Write-Error \"VS Code is not installed\" }");
                        break;

                    case "Python":
                        AddRecommendedTool("Python Development", "Python",
                            "winget install Python.Python",
                            "if (Get-Command python -ErrorAction SilentlyContinue) { Write-Host \"Python is installed\" } else { Write-Error \"Python is not installed\" }");

                        AddRecommendedTool("Python Development", "pip",
                            "winget install Python.Python",
                            "if (Get-Command pip -ErrorAction SilentlyContinue) { Write-Host \"pip is installed\" } else { Write-Error \"pip is not installed\" }");
                        break;

                    case "Django":
                    case "Flask":
                    case "FastAPI":
                        AddRecommendedTool($"{tech} Development", "Python",
                            "winget install Python.Python",
                            "if (Get-Command python -ErrorAction SilentlyContinue) { Write-Host \"Python is installed\" } else { Write-Error \"Python is not installed\" }");

                        AddRecommendedTool($"{tech} Development", "Visual Studio Code",
                            "winget install Microsoft.VisualStudioCode",
                            "if (Get-Command code -ErrorAction SilentlyContinue) { Write-Host \"VS Code is installed\" } else { Write-Error \"VS Code is not installed\" }");
                        break;

                    case ".NET":
                    case ".NET Core/.NET 5+":
                        AddRecommendedTool(".NET Development", ".NET SDK",
                            "winget install Microsoft.DotNet.SDK.7",
                            "if (Get-Command dotnet -ErrorAction SilentlyContinue) { Write-Host \".NET SDK is installed\" } else { Write-Error \".NET SDK is not installed\" }");
                        break;

                    case "C#":
                        AddRecommendedTool("C# Development", ".NET SDK",
                            "winget install Microsoft.DotNet.SDK.7",
                            "if (Get-Command dotnet -ErrorAction SilentlyContinue) { Write-Host \".NET SDK is installed\" } else { Write-Error \".NET SDK is not installed\" }");

                        AddRecommendedTool("C# Development", "Visual Studio",
                            "winget install Microsoft.VisualStudio.2022.Community",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe\") { Write-Host \"Visual Studio is installed\" } else { Write-Error \"Visual Studio is not installed\" }");
                        break;

                    case "ASP.NET":
                    case "Blazor":
                        AddRecommendedTool($"{tech} Development", ".NET SDK",
                            "winget install Microsoft.DotNet.SDK.7",
                            "if (Get-Command dotnet -ErrorAction SilentlyContinue) { Write-Host \".NET SDK is installed\" } else { Write-Error \".NET SDK is not installed\" }");

                        AddRecommendedTool($"{tech} Development", "Visual Studio",
                            "winget install Microsoft.VisualStudio.2022.Community",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe\") { Write-Host \"Visual Studio is installed\" } else { Write-Error \"Visual Studio is not installed\" }");
                        break;

                    case "Java":
                        AddRecommendedTool("Java Development", "JDK",
                            "winget install Oracle.JDK.17",
                            "if (Get-Command java -ErrorAction SilentlyContinue) { Write-Host \"Java is installed\" } else { Write-Error \"Java is not installed\" }");

                        if (_detectedTechnologies.ContainsKey("Maven"))
                        {
                            AddRecommendedTool("Java Development", "Maven",
                                "winget install Apache.Maven",
                                "if (Get-Command mvn -ErrorAction SilentlyContinue) { Write-Host \"Maven is installed\" } else { Write-Error \"Maven is not installed\" }");
                        }

                        if (_detectedTechnologies.ContainsKey("Gradle"))
                        {
                            AddRecommendedTool("Java Development", "Gradle",
                                "winget install Gradle.Gradle",
                                "if (Get-Command gradle -ErrorAction SilentlyContinue) { Write-Host \"Gradle is installed\" } else { Write-Error \"Gradle is not installed\" }");
                        }
                        break;

                    case "Spring Boot":
                        AddRecommendedTool("Spring Boot Development", "JDK",
                            "winget install Oracle.JDK.17",
                            "if (Get-Command java -ErrorAction SilentlyContinue) { Write-Host \"Java is installed\" } else { Write-Error \"Java is not installed\" }");

                        AddRecommendedTool("Spring Boot Development", "IntelliJ IDEA",
                            "winget install JetBrains.IntelliJIDEA.Community",
                            "if (Test-Path \"${env:ProgramFiles}\\JetBrains\\IntelliJ IDEA Community Edition*\") { Write-Host \"IntelliJ IDEA is installed\" } else { Write-Error \"IntelliJ IDEA is not installed\" }");
                        break;

                    case "Docker":
                    case "Docker Compose":
                        AddRecommendedTool("Containerization", "Docker Desktop",
                            "winget install Docker.DockerDesktop",
                            "if (Get-Command docker -ErrorAction SilentlyContinue) { Write-Host \"Docker is installed\" } else { Write-Error \"Docker is not installed\" }");
                        break;

                    case "Kubernetes":
                        AddRecommendedTool("Containerization", "Docker Desktop",
                            "winget install Docker.DockerDesktop",
                            "if (Get-Command docker -ErrorAction SilentlyContinue) { Write-Host \"Docker is installed\" } else { Write-Error \"Docker is not installed\" }");

                        AddRecommendedTool("Containerization", "kubectl",
                            "winget install Kubernetes.kubectl",
                            "if (Get-Command kubectl -ErrorAction SilentlyContinue) { Write-Host \"kubectl is installed\" } else { Write-Error \"kubectl is not installed\" }");
                        break;

                    case "PostgreSQL":
                        AddRecommendedTool("Database", "PostgreSQL",
                            "winget install PostgreSQL.PostgreSQL",
                            "if (Get-Command psql -ErrorAction SilentlyContinue) { Write-Host \"PostgreSQL is installed\" } else { Write-Error \"PostgreSQL is not installed\" }");
                        break;

                    case "MySQL":
                        AddRecommendedTool("Database", "MySQL",
                            "winget install Oracle.MySQL",
                            "if (Get-Command mysql -ErrorAction SilentlyContinue) { Write-Host \"MySQL is installed\" } else { Write-Error \"MySQL is not installed\" }");
                        break;

                    case "MongoDB":
                        AddRecommendedTool("Database", "MongoDB",
                            "winget install MongoDB.Server",
                            "if (Get-Command mongo -ErrorAction SilentlyContinue) { Write-Host \"MongoDB is installed\" } else { Write-Error \"MongoDB is not installed\" }");
                        break;

                    case "SQL Server":
                        AddRecommendedTool("Database", "SQL Server Express",
                            "winget install Microsoft.SQLServerExpress",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft SQL Server\") { Write-Host \"SQL Server is installed\" } else { Write-Error \"SQL Server is not installed\" }");

                        AddRecommendedTool("Database", "SQL Server Management Studio",
                            "winget install Microsoft.SQLServerManagementStudio",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft SQL Server Management Studio 19\") { Write-Host \"SSMS is installed\" } else { Write-Error \"SSMS is not installed\" }");
                        break;

                    case "Git":
                        AddRecommendedTool("Version Control", "Git",
                            "winget install Git.Git",
                            "if (Get-Command git -ErrorAction SilentlyContinue) { Write-Host \"Git is installed\" } else { Write-Error \"Git is not installed\" }");
                        break;

                    case "Visual Studio":
                        AddRecommendedTool("Development Environment", "Visual Studio",
                            "winget install Microsoft.VisualStudio.2022.Community",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe\") { Write-Host \"Visual Studio is installed\" } else { Write-Error \"Visual Studio is not installed\" }");
                        break;

                    case "WPF":
                        AddRecommendedTool("WPF Development", ".NET SDK",
                            "winget install Microsoft.DotNet.SDK.7",
                            "if (Get-Command dotnet -ErrorAction SilentlyContinue) { Write-Host \".NET SDK is installed\" } else { Write-Error \".NET SDK is not installed\" }");

                        AddRecommendedTool("WPF Development", "Visual Studio",
                            "winget install Microsoft.VisualStudio.2022.Community",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe\") { Write-Host \"Visual Studio is installed\" } else { Write-Error \"Visual Studio is not installed\" }");
                        break;

                    case "Windows Forms":
                        AddRecommendedTool("Windows Forms Development", ".NET SDK",
                            "winget install Microsoft.DotNet.SDK.7",
                            "if (Get-Command dotnet -ErrorAction SilentlyContinue) { Write-Host \".NET SDK is installed\" } else { Write-Error \".NET SDK is not installed\" }");

                        AddRecommendedTool("Windows Forms Development", "Visual Studio",
                            "winget install Microsoft.VisualStudio.2022.Community",
                            "if (Test-Path \"${env:ProgramFiles}\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe\") { Write-Host \"Visual Studio is installed\" } else { Write-Error \"Visual Studio is not installed\" }");
                        break;

                    case "TypeScript":
                        AddRecommendedTool("TypeScript Development", "Node.js",
                            "winget install OpenJS.NodeJS",
                            "if (Get-Command node -ErrorAction SilentlyContinue) { Write-Host \"Node.js is installed\" } else { Write-Error \"Node.js is not installed\" }");

                        AddRecommendedTool("TypeScript Development", "TypeScript",
                            "npm install -g typescript",
                            "if (Get-Command tsc -ErrorAction SilentlyContinue) { Write-Host \"TypeScript is installed\" } else { Write-Error \"TypeScript is not installed\" }");
                        break;

                    case "Go":
                        AddRecommendedTool("Go Development", "Go",
                            "winget install GoLang.Go",
                            "if (Get-Command go -ErrorAction SilentlyContinue) { Write-Host \"Go is installed\" } else { Write-Error \"Go is not installed\" }");
                        break;

                    case "Rust":
                        AddRecommendedTool("Rust Development", "Rust",
                            "winget install Rustlang.Rust",
                            "if (Get-Command rustc -ErrorAction SilentlyContinue) { Write-Host \"Rust is installed\" } else { Write-Error \"Rust is not installed\" }");
                        break;

                    case "Ruby":
                    case "Ruby on Rails":
                        AddRecommendedTool("Ruby Development", "Ruby",
                            "winget install RubyInstallerTeam.Ruby",
                            "if (Get-Command ruby -ErrorAction SilentlyContinue) { Write-Host \"Ruby is installed\" } else { Write-Error \"Ruby is not installed\" }");
                        break;

                    case "PHP":
                    case "Laravel":
                    case "Symfony":
                        AddRecommendedTool("PHP Development", "PHP",
                            "winget install PHP.PHP",
                            "if (Get-Command php -ErrorAction SilentlyContinue) { Write-Host \"PHP is installed\" } else { Write-Error \"PHP is not installed\" }");

                        AddRecommendedTool("PHP Development", "Composer",
                            "winget install Composer.Composer",
                            "if (Get-Command composer -ErrorAction SilentlyContinue) { Write-Host \"Composer is installed\" } else { Write-Error \"Composer is not installed\" }");
                        break;

                    case "Flutter":
                    case "Dart":
                        AddRecommendedTool("Flutter Development", "Flutter SDK",
                            "winget install Google.Flutter",
                            "if (Get-Command flutter -ErrorAction SilentlyContinue) { Write-Host \"Flutter is installed\" } else { Write-Error \"Flutter is not installed\" }");

                        AddRecommendedTool("Flutter Development", "Android Studio",
                            "winget install Google.AndroidStudio",
                            "if (Test-Path \"${env:ProgramFiles}\\Android\\Android Studio\") { Write-Host \"Android Studio is installed\" } else { Write-Error \"Android Studio is not installed\" }");
                        break;

                    case "Android":
                        AddRecommendedTool("Android Development", "Android Studio",
                            "winget install Google.AndroidStudio",
                            "if (Test-Path \"${env:ProgramFiles}\\Android\\Android Studio\") { Write-Host \"Android Studio is installed\" } else { Write-Error \"Android Studio is not installed\" }");

                        AddRecommendedTool("Android Development", "JDK",
                            "winget install Oracle.JDK.17",
                            "if (Get-Command java -ErrorAction SilentlyContinue) { Write-Host \"Java is installed\" } else { Write-Error \"Java is not installed\" }");
                        break;

                    case "iOS":
                    case "Swift":
                        AddRecommendedTool("iOS Development", "Visual Studio for Mac",
                            "# Visual Studio for Mac must be installed manually on a Mac",
                            "# Visual Studio for Mac must be installed manually on a Mac");
                        break;

                    case "AWS":
                        AddRecommendedTool("AWS Development", "AWS CLI",
                            "winget install Amazon.AWSCLI",
                            "if (Get-Command aws -ErrorAction SilentlyContinue) { Write-Host \"AWS CLI is installed\" } else { Write-Error \"AWS CLI is not installed\" }");
                        break;

                    case "Azure":
                        AddRecommendedTool("Azure Development", "Azure CLI",
                            "winget install Microsoft.AzureCLI",
                            "if (Get-Command az -ErrorAction SilentlyContinue) { Write-Host \"Azure CLI is installed\" } else { Write-Error \"Azure CLI is not installed\" }");
                        break;

                    case "Google Cloud":
                        AddRecommendedTool("Google Cloud Development", "Google Cloud SDK",
                            "winget install Google.CloudSDK",
                            "if (Get-Command gcloud -ErrorAction SilentlyContinue) { Write-Host \"Google Cloud SDK is installed\" } else { Write-Error \"Google Cloud SDK is not installed\" }");
                        break;

                    case "Terraform":
                        AddRecommendedTool("Infrastructure as Code", "Terraform",
                            "winget install Hashicorp.Terraform",
                            "if (Get-Command terraform -ErrorAction SilentlyContinue) { Write-Host \"Terraform is installed\" } else { Write-Error \"Terraform is not installed\" }");
                        break;
                }
            }

            // always recommend vscode & git (biased- i know)
            if (!_recommendedTools.Any(r => r.Value.Any(t => t.Name.Contains("Git"))))
            {
                AddRecommendedTool("General Development Tools", "Git",
                    "winget install Git.Git",
                    "if (Get-Command git -ErrorAction SilentlyContinue) { Write-Host \"Git is installed\" } else { Write-Error \"Git is not installed\" }");
            }

            if (!_recommendedTools.Any(r => r.Value.Any(t => t.Name.Contains("Visual Studio Code"))))
            {
                AddRecommendedTool("General Development Tools", "Visual Studio Code",
                    "winget install Microsoft.VisualStudioCode",
                    "if (Get-Command code -ErrorAction SilentlyContinue) { Write-Host \"VS Code is installed\" } else { Write-Error \"VS Code is not installed\" }");
            }
        }

        private void AddRecommendedTool(string category, string toolName, string installCommand, string validationScript)
        {
            Tool? existingTool = _tools.Find(t => t.Name == toolName);

            if (existingTool == null)
            {
                existingTool = new Tool
                {
                    Name = toolName,
                    Version = "Latest",
                    Category = "Development Tools",
                    InstallCommand = installCommand,
                    ValidationScript = validationScript
                };
            }

            if (!_recommendedTools.ContainsKey(category))
            {
                _recommendedTools[category] = new List<Tool>();
            }

            if (!_recommendedTools[category].Any(t => t.Name == toolName))
            {
                _recommendedTools[category].Add(existingTool);
            }
        }
    }
}

