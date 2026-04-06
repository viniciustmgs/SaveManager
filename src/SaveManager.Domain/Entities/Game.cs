namespace SaveManager.Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SaveFolderPath { get; set; } = string.Empty;
        public string BackupFolderPath {  get; set; } = string.Empty;
        public bool IsSingleFile { get; set; }
    }
}
