using System;

namespace Chitalka.Models {
    internal class Book {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastOpened { get; set; }
        public string PathToFile { get; set; }
        public string Thumbnail { get; set; }
        public double Progress { get; set; }
        public string[] Authors { get; set; }
        public string[] Genres { get; set; }
        public string[] KeyWords { get; set; }
    }
}
