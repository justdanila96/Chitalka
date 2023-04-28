using Chitalka.Models;
using Chitalka.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FB2Library;
using FB2Library.Elements;
using FB2Library.Elements.Poem;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Chitalka.ViewModels {
    internal partial class ReaderPageViewModel : ObservableObject {

        public static event Action GettingBack;
        [RelayCommand]
        private void GoToStartPage() {
            GettingBack?.Invoke();
        }

        [ObservableProperty]
        FlowDocument document;

        [ObservableProperty]
        Book selectedBook;

        [RelayCommand]
        private async Task LoadBook(Uri pageUri) {

            var idParsed = UriRegex().Match(pageUri.ToString());

            if (idParsed.Success) {
                int id = int.Parse(idParsed.Groups[1].Value);
                SelectedBook = Library.GetBookById(id);
                var fb2 = await Library.ReadFile(SelectedBook.PathToFile);
                GetImages(fb2);
                Document = ConvertToXaml(fb2);
            }
        }

        private Lazy<Dictionary<string, BitmapImage>> images = new();

        private void GetImages(FB2File fb2) {

            foreach (var imgSrc in fb2.Images) {

                var span = new ReadOnlySpan<byte>(imgSrc.Value.BinaryData);
                var img = Image.Load(span);
                var str = new MemoryStream();
                img.SaveAsJpeg(str);
                str.Seek(0, SeekOrigin.Begin);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = str;
                bmp.EndInit();
                images.Value.Add(imgSrc.Key, bmp);
            }
        }

        private FlowDocument ConvertToXaml(FB2File fb2) {
            var doc = new FlowDocument();

            foreach (var bodyItem in fb2.Bodies) {

                var body = new Section();

                foreach (var sectionItem in bodyItem.Sections) {
                    var bodySection = new Section();
                    PrepareTextItem(sectionItem, bodySection);
                    body.Blocks.Add(bodySection);
                }

                doc.Blocks.Add(body);
            }

            return doc;
        }

        private void PrepareTextItems(IEnumerable<IFb2TextItem> textItems, Section section) {

            foreach (var textItem in textItems) {

                if (textItem is IFb2TextItem item) {
                    PrepareTextItem(textItem, section);
                }
            }
        }

        private void PrepareTextItem(IFb2TextItem textItem, Section section) {
            switch (textItem) {

                case CiteItem citeItem:
                    PrepareTextItems(citeItem.CiteData, section);
                    break;

                case PoemItem poemItem:
                    if (poemItem.Title != null)
                        section.Blocks.Add(AddTitle(poemItem.Title));
                    PrepareTextItems(poemItem.Content, section);
                    break;

                case SectionItem sectionItem:
                    if (sectionItem.Title != null)
                        section.Blocks.Add(AddTitle(sectionItem.Title));
                    PrepareTextItems(sectionItem.Content, section);
                    break;

                case StanzaItem stanzaItem:
                    if (stanzaItem.Title != null)
                        section.Blocks.Add(AddTitle(stanzaItem.Title));
                    PrepareTextItems(stanzaItem.Lines, section);
                    break;

                case ParagraphItem:
                case EmptyLineItem:
                case TitleItem:
                case SimpleText:
                    section.Blocks.Add(AddTextLine(textItem.ToString()));
                    break;

                case DateItem dateItem:
                    section.Blocks.Add(AddTextLine(dateItem.DateValue.ToString()));
                    break;

                case EpigraphItem epigraphItem:
                    PrepareTextItems(epigraphItem.EpigraphData, section);
                    break;

                case ImageItem imgItem:

                    var key = imgItem.HRef.Replace("#", string.Empty);
                    if (images.Value.TryGetValue(key, out BitmapImage img)) {

                        var imgBlock = new BlockUIContainer(new System.Windows.Controls.Image { Source = img });
                        section.Blocks.Add(imgBlock);
                    }

                    break;
            }
        }

        private Section AddTitle(TitleItem titleItem) {

            var titleSection = new Section();
            foreach (var title in titleItem.TitleData) {

                var bold = new Bold();
                var run = new Run() { Text = title.ToString() };
                bold.Inlines.Add(run);

                var paragraph = new Paragraph();
                paragraph.Inlines.Add(bold);
                titleSection.Blocks.Add(paragraph);
            }

            return titleSection;
        }

        private Paragraph AddTextLine(string text) {

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(text);
            return paragraph;
        }

        [GeneratedRegex("\\?id\\=(\\d+)$")]
        private static partial Regex UriRegex();
    }
}
