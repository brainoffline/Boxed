using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769
using Boxed.DataModel;
using Brain.Utils;

namespace Boxed.Win
{
    public sealed partial class OptionsSettings : SettingsFlyout
    {
        public const string SoundOnKey = "SoundOn";

        public OptionsSettings()
        {
            this.InitializeComponent();
        }

        public bool IsSoundOn
        {
            get { return !GameData.Current.MuteSounds; }
            set
            {
                GameData.Current.MuteSounds = !value;
                GameData.Current.SaveData();
            }
        }

        public bool IsMusicOn
        {
            get { return !GameData.Current.MuteMusic; }
            set
            {
                GameData.Current.MuteMusic = !value;
                GameData.Current.SaveData();

                if (GameData.Current.MuteMusic)
                    App.StopMusic();
                else
                    App.StartMusic();
            }
        }

        private async void ResetAllScores_OnClick(object sender, RoutedEventArgs e)
        {
            var x = await MessageBox.ShowAsync("Press Yes to erase all scores", "Are you sure?", MessageBoxButton.YesNo);
            if (x != MessageBoxResult.Yes) return;

            GameData.Current.ResetScores();

            // TODO: Force Refresh of screen
        }
    }
}
