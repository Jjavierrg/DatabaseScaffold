namespace DatabaseScaffold.Controls
{
    using DatabaseScaffold.Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class CheckedExpander : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(CheckedExpander), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DisplayFieldProperty = DependencyProperty.Register("DisplayField", typeof(string), typeof(CheckedExpander), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IDatabaseItem>), typeof(CheckedExpander), new PropertyMetadata(new List<IDatabaseItem>()));

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

        public void MarkAll(bool? value) => ItemsSource.ToList().ForEach(x => x.Selected = value.HasValue ? value.Value : !x.Selected);
    }
}
