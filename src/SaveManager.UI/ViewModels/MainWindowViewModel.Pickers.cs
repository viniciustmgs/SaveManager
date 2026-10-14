using System;

﻿using SaveManager.Domain.Entities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {

        private async Task RefreshProfiles()
        {
            if (SelectedGame == null)
                return;

            var game = SelectedGame;
            var requestVersion = ++_selectedGameLoadVersion;

            try
            {
                var profiles = await Task.Run(() => _getProfiles.Execute(game));

                if (requestVersion != _selectedGameLoadVersion)
                    return;

                if (SelectedGame?.Id != game.Id)
                    return;

                Profiles = new ObservableCollection<Profile>(profiles);
            }
            catch (Exception ex)
            {
                if (requestVersion != _selectedGameLoadVersion)
                    return;

                await Toast.Show($"Failed to load profiles: {ex.Message}", isSuccess: false);
            }
        }

        private async Task RefreshSaves()
        {
            if (SelectedGame == null || SelectedProfile == null)
                return;

            var game = SelectedGame;
            var profile = SelectedProfile;
            var requestVersion = ++_selectedProfileLoadVersion;

            try
            {
                var saves = await Task.Run(() => _getSaves.Execute(profile, game));

                if (requestVersion != _selectedProfileLoadVersion)
                    return;

                if (SelectedGame?.Id != game.Id)
                    return;

                if (SelectedProfile?.FolderPath != profile.FolderPath)
                    return;

                SetAllSaves(saves);
            }
            catch (Exception ex)
            {
                if (requestVersion != _selectedProfileLoadVersion)
                    return;

                await Toast.Show($"Failed to load saves: {ex.Message}", isSuccess: false);
            }
        }
    }
}