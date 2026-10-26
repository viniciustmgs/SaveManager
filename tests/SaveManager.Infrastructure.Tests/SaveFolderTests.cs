using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Infrastructure.FileSystem;

namespace SaveManager.Infrastructure.Tests
{
    public class SaveFolderTests : IDisposable
    {
        private const string Marker = SaveFileService.FolderMarkerFileName;

        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-folders-" + Guid.NewGuid().ToString("N"));

        private string ProfilePath => Path.Combine(_root, "profile");
        private string FolderGameSource => Path.Combine(_root, "game-source");
        private string FileGameSource => Path.Combine(_root, "game.dat");

        private readonly SaveFileService _service = new();

        public SaveFolderTests()
        {
            Directory.CreateDirectory(ProfilePath);

            Directory.CreateDirectory(FolderGameSource);
            File.WriteAllText(Path.Combine(FolderGameSource, "slot.sav"), "payload");

            File.WriteAllText(FileGameSource, "payload");
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

        private Profile Profile() => new() { Name = "p", FolderPath = ProfilePath };

        private Game FolderGame() => new()
        {
            Name = "g",
            SavePath = FolderGameSource,
            BackupFolderPath = _root,
            SaveType = SaveType.Folder
        };

        private Game FileGame() => new()
        {
            Name = "g",
            SavePath = FileGameSource,
            BackupFolderPath = _root,
            SaveType = SaveType.SingleFile
        };

        private string MakeSaveFolder(string relative)
        {
            var path = Path.Combine(ProfilePath, relative);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, Marker), string.Empty);

            return path;
        }

        private string MakeSaveDirectory(string relative)
        {
            var path = Path.Combine(ProfilePath, relative);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "slot.sav"), "payload");

            return path;
        }

        [Fact]
        public void FoldersAreNotReportedAsSavesForFolderGames()
        {
            MakeSaveDirectory("loose");
            MakeSaveFolder("group");
            MakeSaveDirectory(Path.Combine("group", "nested"));

            var saves = _service.ReadSaves(Profile(), FolderGame());
            var folders = _service.ReadFolders(Profile(), FolderGame());

            Assert.Equal(["loose", "nested"], saves.Select(s => s.Name).OrderBy(n => n));
            Assert.Equal(["group"], folders.Select(f => f.Name));
            Assert.Equal("group",
                saves.Single(s => s.Name == "nested").RelativeFolderPath);
            Assert.Equal(string.Empty,
                saves.Single(s => s.Name == "loose").RelativeFolderPath);
        }

        [Fact]
        public void NestedFoldersAreReadRecursively()
        {
            MakeSaveFolder("a");
            MakeSaveFolder(Path.Combine("a", "b"));
            MakeSaveDirectory(Path.Combine("a", "b", "deep"));

            var folders = _service.ReadFolders(Profile(), FolderGame());
            var saves = _service.ReadSaves(Profile(), FolderGame());

            Assert.Equal(
                ["a", Path.Combine("a", "b")],
                folders.Select(f => f.RelativePath).OrderBy(p => p, StringComparer.Ordinal));

            Assert.Equal(Path.Combine("a", "b"), saves.Single().RelativeFolderPath);
        }

        [Fact]
        public void CreateFolderWritesTheMarkerAndIsReadBack()
        {
            var created = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "New");

            Assert.True(File.Exists(Path.Combine(created.FolderPath, Marker)));

            var folders = _service.ReadFolders(Profile(), FolderGame());

            Assert.Equal(["New"], folders.Select(f => f.Name));
        }

        [Fact]
        public void CreateFolderNestsUnderTheRequestedParent()
        {
            var parent = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "outer");

            var child = _service.CreateFolder(
                Profile(), FolderGame(), parent.RelativePath, "inner");

            Assert.Equal(Path.Combine("outer", "inner"), child.RelativePath);
            Assert.True(Directory.Exists(child.FolderPath));
        }

        [Fact]
        public void CreateFolderRejectsANameAlreadyTakenByASave()
        {
            MakeSaveDirectory("taken");

            var error = Assert.Throws<ArgumentException>(
                () => _service.CreateFolder(Profile(), FolderGame(), string.Empty, "taken"));

            Assert.Contains("already exists", error.Message);
        }

        [Fact]
        public void CreateFolderRejectsASeparatorInTheName()
        {
            Assert.Throws<ArgumentException>(
                () => _service.CreateFolder(Profile(), FolderGame(), string.Empty, "a/b"));
        }

        [Fact]
        public void MovingASaveIntoAFolderMovesItOnDisk()
        {
            var save = _service.CreateSave(Profile(), FolderGame(), string.Empty);
            var folder = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "group");

            var moved = _service.MoveSave(Profile(), FolderGame(), save, folder.RelativePath);

            Assert.Equal(folder.RelativePath, moved.RelativeFolderPath);
            Assert.False(Directory.Exists(save.SavePath));
            Assert.True(Directory.Exists(moved.SavePath));
            Assert.Equal(
                folder.RelativePath,
                _service.ReadSaves(Profile(), FolderGame()).Single().RelativeFolderPath);
        }

        [Fact]
        public void MovingASaveOutOfAFolderReturnsItToTheRoot()
        {
            var folder = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "group");
            var save = _service.CreateSave(Profile(), FolderGame(), folder.RelativePath);

            var moved = _service.MoveSave(Profile(), FolderGame(), save, string.Empty);

            Assert.Equal(string.Empty, moved.RelativeFolderPath);
            Assert.Equal(Path.Combine(ProfilePath, save.Name), moved.SavePath);
        }

        [Fact]
        public void MovingOntoAnExistingNameIsRefused()
        {
            var folder = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "group");
            var root = _service.CreateSave(Profile(), FolderGame(), string.Empty);
            var inside = _service.CreateSave(Profile(), FolderGame(), folder.RelativePath);

            var error = Assert.Throws<ArgumentException>(
                () => _service.MoveSave(Profile(), FolderGame(), root, folder.RelativePath));

            Assert.Contains("already exists", error.Message);
            Assert.True(Directory.Exists(root.SavePath));
            Assert.True(Directory.Exists(inside.SavePath));
        }

        [Fact]
        public void DeletingAFolderRemovesItsSaves()
        {
            var folder = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "group");
            _service.CreateSave(Profile(), FolderGame(), folder.RelativePath);

            _service.DeleteFolder(Profile(), folder);

            Assert.False(Directory.Exists(folder.FolderPath));
            Assert.Empty(_service.ReadSaves(Profile(), FolderGame()));
            Assert.Empty(_service.ReadFolders(Profile(), FolderGame()));
        }

        [Fact]
        public void RenamingAFolderKeepsItsSavesAndUpdatesTheirPaths()
        {
            var folder = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "old");
            _service.CreateSave(Profile(), FolderGame(), folder.RelativePath);

            var renamed = _service.RenameFolder(Profile(), FolderGame(), folder, "new");

            Assert.Equal("new", renamed.Name);
            Assert.Equal("new", renamed.RelativePath);
            Assert.True(Directory.Exists(renamed.FolderPath));

            var save = _service.ReadSaves(Profile(), FolderGame()).Single();
            Assert.Equal("new", save.RelativeFolderPath);
        }

        [Fact]
        public void SaveNamesAreOnlyUniqueWithinTheirFolder()
        {
            var first = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "one");
            var second = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "two");

            var a = _service.CreateSave(Profile(), FolderGame(), first.RelativePath);
            var b = _service.CreateSave(Profile(), FolderGame(), second.RelativePath);

            Assert.Equal(a.Name, b.Name);
        }

        [Fact]
        public void CreateSaveWritesInsideTheRequestedFolder()
        {
            var folder = _service.CreateFolder(Profile(), FolderGame(), string.Empty, "group");

            var save = _service.CreateSave(Profile(), FolderGame(), folder.RelativePath);

            Assert.Equal(Path.Combine(folder.FolderPath, save.Name), save.SavePath);
            Assert.Equal(folder.RelativePath, save.RelativeFolderPath);
        }

        [Fact]
        public void ForSingleFileGamesDirectoriesAreFoldersAndFilesAreSaves()
        {
            MakeSaveFolder("group");
            File.WriteAllText(Path.Combine(ProfilePath, "group", "one.dat"), "payload");
            File.WriteAllText(Path.Combine(ProfilePath, "root.dat"), "payload");

            File.WriteAllText(Path.Combine(ProfilePath, Marker), string.Empty);

            var saves = _service.ReadSaves(Profile(), FileGame());
            var folders = _service.ReadFolders(Profile(), FileGame());

            Assert.Equal(["one.dat", "root.dat"], saves.Select(s => s.Name).OrderBy(n => n));
            Assert.Equal(["group"], folders.Select(f => f.Name));
            Assert.Equal("group", saves.Single(s => s.Name == "one.dat").RelativeFolderPath);
        }

        [Fact]
        public void ForSingleFileGamesMovingAFilesSaveWorks()
        {
            var folder = _service.CreateFolder(Profile(), FileGame(), string.Empty, "group");
            var save = _service.CreateSave(Profile(), FileGame(), string.Empty);

            var moved = _service.MoveSave(Profile(), FileGame(), save, folder.RelativePath);

            Assert.True(File.Exists(moved.SavePath));
            Assert.False(File.Exists(save.SavePath));
            Assert.Equal(folder.RelativePath, moved.RelativeFolderPath);
        }

        [Fact]
        public void DeletingAProfileFolderIsRefused()
        {
            var profile = Profile();
            var fake = new SaveFolder { Name = "p", FolderPath = profile.FolderPath };

            Assert.Throws<ArgumentException>(() => _service.DeleteFolder(profile, fake));
            Assert.True(Directory.Exists(ProfilePath));
        }
    }
}
