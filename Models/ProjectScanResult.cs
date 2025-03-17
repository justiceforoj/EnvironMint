using System.Collections.Generic;

namespace EnvironMint.Models
{
    public class ProjectScanResult
    {
        public Dictionary<string, bool> DetectedTechnologies { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, string> DetectedVersions { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<Tool>> RecommendedTools { get; set; } = new Dictionary<string, List<Tool>>();
    }
}

