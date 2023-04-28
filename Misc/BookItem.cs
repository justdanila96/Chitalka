using Chitalka.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Chitalka.Misc {
    internal partial class BookItem : ObservableObject {
        private static readonly Uri NoImagePhoto = new("pack://application:,,,/Images/no image.png");

        public int Id { get; }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private Uri thumbnail;

        [ObservableProperty]
        private bool noImage;

        [ObservableProperty]
        private string authors;      

        private static string GetThumbnailFilePath(string thumbnail) {
            return Properties.Settings.Default.PathToCache + $@"\{thumbnail}.jpg";
        }

        public BookItem(Book book) {

            Id = book.Id;
            Name = book.Name;
            Authors = string.Join(", ", book.Authors);            
            NoImage = book.Thumbnail == null;
            Thumbnail = NoImage
                ? NoImagePhoto
                : new Uri(GetThumbnailFilePath(book.Thumbnail), UriKind.Absolute);
        }
    }
}
