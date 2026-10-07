namespace SaveManager.Infrastructure.Persistence.Models
{
    public class AppConfig
    {
        public List<Domain.Entities.Game> Games { get; set; } = [];
        public Domain.Entities.AppSettings Settings { get; set; } = new();
    }
}
