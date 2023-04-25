using FB2Library;
using FB2Library.Elements;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chitalka.Tools {
    internal static class Thumbnail {

        private static byte[] GetBinaryData(Dictionary<string, BinaryItem> thumbnails) {

            KeyValuePair<string, BinaryItem> cover = thumbnails.FirstOrDefault(i => {
                string key = i.Key.ToUpperInvariant();
                return key.Contains("COVER");
            });


            return cover.Key switch {
                null => null,
                _ => cover.Value.BinaryData
            };

        }

        private static Image ConvertToImage(byte[] binData) {

            if (binData == null)
                return null;

            try {
                var span = new ReadOnlySpan<byte>(binData);
                Image thumbnail = Image.Load(span);
                thumbnail.Mutate(i => i.Resize(160, 200));
                return thumbnail;
            }
            catch {
                return null;
            }
        }

        private static Guid? SaveImage(Image img) {

            if (img == null)
                return null;

            try {
                var guid = Guid.NewGuid();
                string fullPath = $@"{Properties.Settings.Default.PathToCache}\{guid}.jpg";
                img.SaveAsJpeg(fullPath);
                return guid;
            }
            catch {
                return null;
            }
            finally {
                img.Dispose();
            }
        }

        public static Guid? CreateThumbnail(FB2File fb2) {

            return SaveImage(ConvertToImage(GetBinaryData(fb2.Images)));
        }
    }
}
