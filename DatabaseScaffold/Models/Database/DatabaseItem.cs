namespace DatabaseScaffold.Models
{
    using DatabaseScaffold.Core;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public abstract class DatabaseItem : NotifyObject, IDatabaseItem
    {
        private IEnumerable<IDatabaseItem> children = new ObservableCollection<IDatabaseItem>();
        private bool selected;
        public string Name { get; set; }
        [JsonIgnore]
        public string FullName => $"{Parent?.FullName}{(string.IsNullOrEmpty(Parent?.FullName) ? "" : ".")}{Name}";
        public abstract string Type { get; }
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                Children?.ToList().ForEach(x => x.Selected = value);
                OnPropertyChanged();

                if (Parent is DatabaseItem par)
                    par.ForceSelected(par.Children.All(x => x.Selected));
            }
        }
        public IDatabaseItem Parent { get; set; }
        public IEnumerable<IDatabaseItem> Children
        {
            get => children; set { children = value; OnPropertyChanged(); }
        }
        public string GetParams()
        {
            var param = selected ? GetParam() : string.Empty;

            var childParams = string.Empty;
            var selectedItems = Children?.Where(x => x.Selected);

            if (selectedItems?.Any() ?? false)
                childParams = string.Join(" ", selectedItems.Select(x => x.GetParams()));

            return $"{param} {childParams}".Trim();
        }

        protected abstract string GetParam();

        private void ForceSelected(bool value)
        {
            selected = value;
            OnPropertyChanged("Selected");
        }
    }
}
