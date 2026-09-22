using SaveManager.Domain.Enums;

namespace SaveManager.UI.ViewModels.Dialogs
{
    public sealed class AddGameDialogResult
    {
        public string GameName { get; init; } = string.Empty;
        public string SavePath { get; init; } = string.Empty;
        public string BackupPath { get; init; } = string.Empty;
        public SaveType SaveType { get; init; }
    }
}