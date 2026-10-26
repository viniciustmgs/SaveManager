namespace SaveManager.Domain.Entities
{
    public class SaveFolder
    {
        public string Name { get; set; } = string.Empty;

        public string FolderPath { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;
    }
}
