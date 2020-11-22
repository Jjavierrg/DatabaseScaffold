namespace DatabaseScaffold.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class NotifyObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) => OnPropertyChanged(this, name);

        protected void OnPropertyChanged(object sender, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
        }
    }
}
