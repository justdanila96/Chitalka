using Chitalka.Models;
using FB2Library;
using FB2Library.Elements;
using FB2Library.HeaderItems;
using LiteDB;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Chitalka.Tools {
    internal static partial class Library {
        private const string DbName = "books.db";
        private const string TabName = "books";

        public static void SaveBook(Book book) {
            using var db = new LiteDatabase(DbName);
            ILiteCollection<Book> books = db.GetCollection<Book>(TabName);
            books.Insert(book);
        }

        public static Book[] GetAllBooks() {
            using var db = new LiteDatabase(DbName);
            ILiteCollection<Book> books = db.GetCollection<Book>(TabName);
            return books.FindAll().ToArray();
        }

        public static IEnumerable<string> GetAllAuthors(IEnumerable<Book> books) {
            var authors = new SortedSet<string>();
            foreach (Book book in books) {
                if (book.Authors != null) {
                    foreach (string author in book.Authors) {
                        authors.Add(author);
                    }
                }
            }
            return authors.AsEnumerable();
        }

        public static IEnumerable<string> GetAllGenres(IEnumerable<Book> books) {
            var genres = new SortedSet<string>();
            foreach (Book book in books) {
                if (book.Genres != null) {
                    foreach (string genre in book.Genres) {
                        genres.Add(genre);
                    }
                }
            }
            return genres.AsEnumerable();
        }

        public static IEnumerable<string> GetAllKeywords(IEnumerable<Book> books) {
            var keywords = new SortedSet<string>();
            foreach (Book book in books) {
                if (book.KeyWords != null) {
                    foreach (string keyword in book.KeyWords) {
                        keywords.Add(keyword);
                    }
                }
            }
            return keywords.AsEnumerable();
        }

        private static XmlLoadSettings GetSettings() {
            var readSettings = new XmlReaderSettings {
                DtdProcessing = DtdProcessing.Ignore
            };
            return new XmlLoadSettings(readSettings);
        }

        private static async Task<FB2File> ReadFile(string file) {
            try {
                var reader = new FB2Reader();
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                return await reader.ReadAsync(stream, GetSettings());
            }
            catch {
                return null;
            }
        }

        private static string GetAuthorName(AuthorType author) {

            var b = new StringBuilder();

            AddName(author.FirstName, b);
            AddName(author.LastName, b);

            string name = b.ToString();

            if (string.IsNullOrEmpty(name)) {
                if (HasValue(author.NickName))
                    return Condense(author.NickName.Text);
                else
                    return "Неизвестный автор";
            }
            else {
                return Condense(name);
            }
        }

        private static void AddName(TextFieldType author, StringBuilder b) {
            if (HasValue(author)) {
                b.Append(author.Text).Append(' ');
            }
        }

        private static bool HasValue(TextFieldType s) {
            return s != null && !string.IsNullOrEmpty(s.Text);
        }

        private static string[] MapSafely(TextFieldType input, Func<string, string> mapper) {
            if (input == null)
                return null;
            else
                return input.Text.Split(',').Select(mapper).ToArray();
        }

        private static B[] MapSafely<A, B>(IEnumerable<A> array, Func<A, B> mapper) {
            if (array == null)
                return null;
            else
                return array.Select(mapper).ToArray();
        }

        private static string Condense(string input) {
            return MyRegex().Replace(input.Trim(), " ").ToLowerInvariant();
        }

        private static Book CreateBook(FB2File fb2, string file) {

            if (fb2 == null)
                return null;

            var book = new Book() {
                Name = fb2.TitleInfo.BookTitle.Text,
                PathToFile = file,
                Thumbnail = Thumbnail.CreateThumbnail(fb2).ToString(),
                Authors = MapSafely(fb2.TitleInfo.BookAuthors, GetAuthorName),
                Genres = MapSafely(fb2.TitleInfo.Genres, i => i.ToString()),
                KeyWords = MapSafely(fb2.TitleInfo.Keywords, Condense)
            };

            return book;
        }

        public static async Task<Book> GetBookFromFile(string file) {

            FB2File fb2 = await ReadFile(file);
            return CreateBook(fb2, file);
        }

        [GeneratedRegex(" {2,}")]
        private static partial Regex MyRegex();
    }
}
