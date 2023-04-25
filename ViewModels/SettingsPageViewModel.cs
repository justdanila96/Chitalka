using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using System;
using System.IO;
using System.Threading.Tasks;
using Chitalka.Tools;

namespace Chitalka.ViewModels {
    internal partial class SettingsPageViewModel : ObservableObject {
        public static event Action GettingBack;
        [RelayCommand]
        private void GoToStartPage() {
            GettingBack?.Invoke();
        }

        public string PathToLibrary {
            get {
                return Properties.Settings.Default.PathToLibrary;
            }
            set {
                Properties.Settings.Default.PathToLibrary = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public string PathToCache {
            get {
                return Properties.Settings.Default.PathToCache;
            }
            set {
                Properties.Settings.Default.PathToCache = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        private void ChangePathToLibrary() {
            var browser = new FolderBrowserDialog();
            var result = browser.ShowDialog();
            if (result == DialogResult.OK) {
                PathToLibrary = browser.SelectedPath;
            }
        }

        [RelayCommand]
        private void ChangePathToCache() {
            var browser = new FolderBrowserDialog();
            var result = browser.ShowDialog();
            if (result == DialogResult.OK) {
                PathToCache = browser.SelectedPath;
            }
        }

        [RelayCommand]
        private async Task RefreshLibrary() {

            var files = Directory.GetFiles(PathToLibrary, "*.fb2", SearchOption.AllDirectories);
            object locker = new();

            await Parallel.ForEachAsync(files, async (file, token) => {

                var book = await Library.GetBookFromFile(file);

                if (book != null) {

                    lock (locker) {
                        Library.SaveBook(book);
                    }
                }
            });
        }
    }
}
