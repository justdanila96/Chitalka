using Chitalka.Misc;
using Chitalka.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Chitalka.ViewModels {
    internal partial class StartPageViewModel : ObservableObject {
        public static event Action SettingsOpening;
        public static event Action<int> BookSelected;

        [RelayCommand]
        private void OpenSettings() {
            SettingsOpening?.Invoke();
        }

        [ObservableProperty]
        private ObservableCollection<BookItem> bookItems;

        [ObservableProperty]
        private BookItem selectedBook;

        [ObservableProperty]
        private ObservableCollection<FilterGroup> authors;

        [ObservableProperty]
        private ObservableCollection<FilterGroup> genres;

        [ObservableProperty]
        private ObservableCollection<FilterGroup> keywords;

        private static void RefreshFilter<T>(HashSet<T> set, T item) {
            if (!set.Contains(item)) {
                set.Add(item);
            }
            else {
                set.Remove(item);
            }
        }

        private static bool Match<T>(Lazy<HashSet<T>> set, IEnumerable<T> items) {

            return !set.IsValueCreated
                || set.Value.Count == 0
                || (items != null && items.Any(set.Value.Contains));
        }

        private static IEnumerable<FilterGroup> GetFilterGroups(IEnumerable<string> items) {

            return from item in items
                   group item by char.ToUpper(item[0]) into optGroup
                   select new FilterGroup(optGroup.Key, optGroup);
        }

        private readonly Lazy<HashSet<string>> filterAuthors = new();
        private readonly Lazy<HashSet<string>> filterGenres = new();
        private readonly Lazy<HashSet<string>> filterKeywords = new();

        [RelayCommand]
        private void LoadBooks() {

            var books = Library.GetAllBooks();

            BookItems = new(from book in books.AsParallel()
                            where Match(filterAuthors, book.Authors)
                            where Match(filterGenres, book.Genres)
                            where Match(filterKeywords, book.KeyWords)
                            select new BookItem(book));

            if (Authors == null)
                Task.Factory.StartNew(() => Authors = new(GetFilterGroups(Library.GetAllAuthors(books))));

            if (Genres == null)
                Task.Factory.StartNew(() => Genres = new(GetFilterGroups(Library.GetAllGenres(books))));

            if (Keywords == null)
                Task.Factory.StartNew(() => Keywords = new(GetFilterGroups(Library.GetAllKeywords(books))));
        }

        [RelayCommand]
        private void FilterByAuthor(string author) {

            RefreshFilter(filterAuthors.Value, author);
            LoadBooks();
        }

        [RelayCommand]
        private void FilterByGenre(string genre) {

            RefreshFilter(filterGenres.Value, genre);
            LoadBooks();
        }

        [RelayCommand]
        private void FilterByKeyword(string keyword) {

            RefreshFilter(filterKeywords.Value, keyword);
            LoadBooks();
        }
    }
}
