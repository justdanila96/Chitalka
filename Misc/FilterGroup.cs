using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Chitalka.Misc {
    internal partial class FilterGroup : ObservableObject {
        [ObservableProperty]
        string name;

        [ObservableProperty]
        ObservableCollection<string> options;

        public FilterGroup(char name, IEnumerable<string> opt) {
            Name = name.ToString();
            Options = new ObservableCollection<string>(opt);
        }
    }
}
