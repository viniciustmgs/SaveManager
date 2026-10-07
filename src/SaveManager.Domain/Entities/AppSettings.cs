namespace SaveManager.Domain.Entities
{
    public class AppSettings
    {
        public bool GlobalHotkeysEnabled { get; set; }
        public string CreateSave { get; set; } = string.Empty;
        public string LoadSave { get; set; } = string.Empty;
        public string NextSave { get; set; } = string.Empty;
        public string PreviousSave { get; set; } = string.Empty;
        public string ToggleGlobalHotkeys { get; set; } = string.Empty;
    }
}