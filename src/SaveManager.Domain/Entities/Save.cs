namespace SaveManager.Domain.Entities
{
    public class Save
    {
        public string Name { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
