namespace EnvironMint.Models
{
    public class Tool
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string InstallCommand { get; set; } = string.Empty;
        public string ValidationScript { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
    }
}