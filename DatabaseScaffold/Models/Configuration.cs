namespace DatabaseScaffold.Models
{
    public class Configuration : NotifyObject
    {
        private string connectionString;
        private string dataProjectFile;
        private string contextFile;
        private string additionalParams;
        private Database database;

        public string ConnectionString { get => connectionString; set { connectionString = value; OnPropertyChanged(); } }
        public string DataProjectFile { get => dataProjectFile; set { dataProjectFile = value; OnPropertyChanged(); } }
        public string ContextFile { get => contextFile; set { contextFile = value; OnPropertyChanged(); } }
        public string AdditionalParams { get => additionalParams; set { additionalParams = value; OnPropertyChanged(); } }
        public Database Database { get => database; set { database = value; OnPropertyChanged(); } }
    }
}
