namespace DatabaseScaffold.Controls
{
    using DatabaseScaffold.Core;
    using DatabaseScaffold.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class CheckedExpander : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(CheckedExpander), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DisplayFieldProperty = DependencyProperty.Register("DisplayField", typeof(string), typeof(CheckedExpander), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IDatabaseItem>), typeof(CheckedExpander), new PropertyMetadata(new List<IDatabaseItem>()));

        private RelayCommand _markAllCommand;
        private RelayCommand _markNoneCommand;
        private RelayCommand _markInvertCommand;
        public RelayCommand MarkAllCommand => _markAllCommand ??= new RelayCommand((_) => MarkAll(true));
        public RelayCommand MarkNoneCommand => _markNoneCommand ??= new RelayCommand((_) => MarkAll(false));
        public RelayCommand MarkInvertCommand => _markInvertCommand ??= new RelayCommand((_) => MarkAll(null));

        public IEnumerable<IDatabaseItem> ItemsSource
        {
            get { return (IEnumerable<IDatabaseItem>)GetValue(ItemsSourceProperty); }
            set => SetValue(ItemsSourceProperty, value);
        }

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set => SetValue(HeaderTextProperty, value);
        }

        public string DisplayField
        {
            get { return (string)GetValue(DisplayFieldProperty); }
            set => SetValue(DisplayFieldProperty, value);
        }

        public void MarkAll(bool? value) => ItemsSource.ToList().ForEach(x => x.Selected = value ?? !x.Selected);
    }
}
