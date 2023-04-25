using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Chitalka.ViewModels {
    internal partial class MainWindowViewModel : ObservableObject {
        [ObservableProperty]
        private Uri activeUri;

        public MainWindowViewModel() {
            StartPageViewModel.SettingsOpening += () => {
                Load("SettingsPage.xaml");
            };

            SettingsPageViewModel.GettingBack += () => {
                Load("StartPage.xaml");
            };

            StartPageViewModel.BookSelected += (int id) => {
                Load($"ReaderPage.xaml?id={id}");
            };

            //ReaderPageViewModel.BackToLibrary += () => {
            //    Load("StartPage.xaml");
            //};
        }

        [RelayCommand]
        private void Load(string pageName) {
            ActiveUri = new Uri($@"Pages\{pageName}", UriKind.Relative);
        }
    }
}
