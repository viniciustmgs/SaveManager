using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Infrastructure.FileSystem;

namespace SaveManager.Infrastructure.Tests
{
    public class SaveCreationStampTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-stamp-" + Guid.NewGuid().ToString("N"));

        private string GamePath => Path.Combine(_root, "game.dat");
        private string SavesPath => Path.Combine(_root, "saves");

        private readonly SaveFileService _service = new();

        public SaveCreationStampTests()
        {
            Directory.CreateDirectory(SavesPath);
            File.WriteAllText(GamePath, "payload");
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        [Fact]
        public void ANewSaveIsStampedWithThePresentRatherThanInherited()
        {
            var squatter = Path.Combine(SavesPath, "game.dat");
            File.WriteAllText(squatter, "stale");
            File.SetCreationTime(squatter, new DateTime(2020, 1, 1, 0, 0, 0));

            var save = CreateOne();

            Assert.True(
                save.CreatedAt > new DateTime(2020, 6, 1),
                $"expected a present day timestamp, got {save.CreatedAt:O}");
        }

        [Fact]
        public void CreatingASaveNeverClobbersOneWhoseNameDiffersOnlyByCase()
        {
            var existing = Path.Combine(SavesPath, "GAME.DAT");
            File.WriteAllText(existing, "precious");

            var save = CreateOne();

            Assert.NotEqual("GAME.DAT", save.Name);
            Assert.Equal("precious", File.ReadAllText(existing));
        }

        [Fact]
        public void EachNewSaveGetsItsOwnTimestamp()
        {
            var first = CreateOne();
            Thread.Sleep(30);
            var second = CreateOne();

            Assert.True(
                second.CreatedAt > first.CreatedAt,
                $"second ({second.CreatedAt:O}) should be later than first ({first.CreatedAt:O})");
        }

        [Fact]
        public void SuccessiveSavesLandInTheOrderTheyWereMade()
        {
            var first = CreateOne();
            Thread.Sleep(30);
            var second = CreateOne();
            Thread.Sleep(30);
            var third = CreateOne();

            var ordered = new[] { first, second, third }
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.Name);

            Assert.Equal(new[] { first.Name, second.Name, third.Name }, ordered);
        }

        [Fact]
        public void RenamingKeepsTheCreationTime()
        {
            var save = CreateOne();
            var before = save.CreatedAt;

            var renamed = _service.RenameSave(MakeGame(), save, "renamed");

            Assert.Equal(before, renamed.CreatedAt);
        }

        [Fact]
        public void ReplacingASaveDoesNotMakeItLookNewlyCreated()
        {
            var save = CreateOne();
            var before = save.CreatedAt;

            Thread.Sleep(30);
            _service.ReplaceSave(MakeGame(), save);

            Assert.Equal(before, _service.ReadSaves(MakeProfile(), MakeGame())
                .Single(s => s.Name == save.Name).CreatedAt);
        }

        private Domain.Entities.Save CreateOne() =>
            _service.CreateSave(MakeProfile(), MakeGame());

        private Profile MakeProfile() => new() { Name = "p", FolderPath = SavesPath };

        private Game MakeGame() => new()
        {
            Name = "g",
            SavePath = GamePath,
            BackupFolderPath = _root,
            SaveType = SaveType.SingleFile
        };
    }
}