namespace SaveManager.Domain.Entities
{
    public class Profile
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
    }
}
