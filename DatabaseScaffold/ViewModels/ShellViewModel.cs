namespace DatabaseScaffold.ViewModels
{
    using Caliburn.Micro;
    using DatabaseScaffold.Core;
    using DatabaseScaffold.Interfaces;
    using DatabaseScaffold.Models;
    using MahApps.Metro.Controls.Dialogs;
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using System;
    using System.Collections.ObjectModel;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class ShellViewModel : Screen, IShell
    {
        private readonly ICommandGenerator _commandGenerator;
        private readonly IDialogCoordinator _dialogCoordinator;
        private string _busyText;
        private RelayCommand _generateScaffoldCommand;
        private bool _isBusy;
        private RelayCommand _loadSchemeCommand;
        private Configuration configuration;

        public ShellViewModel(IDialogCoordinator dialogCoordinator, ICommandGenerator commandGenerator)
        {
            _dialogCoordinator = dialogCoordinator ?? throw new ArgumentNullException(nameof(dialogCoordinator));
            _commandGenerator = commandGenerator ?? throw new ArgumentNullException(nameof(commandGenerator));
            _commandGenerator.DataReceivedEvent += (object sender, DataReceivedEventArgs e) => BusyText = e.Data;

            var lastSettings = Properties.Settings.Default.lastSettings;
            LoadConfigFromSerialized(lastSettings);
        }

        public RelayCommand LoadSchemaCommand => _loadSchemeCommand ??= new RelayCommand(async (sender) => await LoadSchema(), (sender) => CanLoadSchema());
        public RelayCommand GenerateScaffoldCommand => _generateScaffoldCommand ??= new RelayCommand(async (sender) => await GenerateScaffold(), (sender) => CanGenerateScaffold());
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; NotifyOfPropertyChange(); }
        }
        public string BusyText
        {
            get => _busyText;
            set { _busyText = value; NotifyOfPropertyChange(); }
        }
        public Configuration Configuration
        {
            get => configuration;
            protected set { configuration = value; NotifyOfPropertyChange(); }
        }
        public void FindContext()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Context Files (*Context.cs) | *Context.cs|C# Files (*.cs) | *.cs"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            Configuration.ContextFile = openFileDialog.FileName;
        }

        public void FindDataProject()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Project File (*.csproj) | *.csproj"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            Configuration.DataProjectFile = openFileDialog.FileName;
        }

        public async Task GenerateScaffold()
        {
            try
            {
                IsBusy = true;
                BusyText = "Generando scaffold";
                await _commandGenerator.Scaffold(Configuration);

                Properties.Settings.Default.lastSettings = SerializeConfig(Configuration);
                Properties.Settings.Default.Save();

                await _dialogCoordinator.ShowMessageAsync(this, "Proceso completado", "El proceso de scaffolding ha concluído con éxito", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", ex.Message, settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadConfigAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Scaffolding Configuration File (*.scf) | *.scf"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            try
            {
                var fileContent = await File.ReadAllTextAsync(openFileDialog.FileName);
                if (!LoadConfigFromSerialized(fileContent)) throw new FileFormatException("El archivo no es válido");
                await _dialogCoordinator.ShowMessageAsync(this, "Finalizado", "Archivo cargado con éxito", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", ex.Message, settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
        }

        public async Task LoadSchema()
        {
            try
            {
                IsBusy = true;
                Configuration.Database = await GetSchemaAsync(Configuration.ConnectionString);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", ex.Message, settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SaveConfigAsync()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Scaffolding Configuration File (*.scf) | *.scf",
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            try
            {
                await File.WriteAllTextAsync(saveFileDialog.FileName, SerializeConfig(Configuration));
                await _dialogCoordinator.ShowMessageAsync(this, "Finalizado", "Archivo guardado con éxito", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", ex.Message, settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            }
        }

        private bool CanLoadSchema() => !string.IsNullOrEmpty(Configuration?.ConnectionString);
        private bool CanGenerateScaffold() => !string.IsNullOrEmpty(Configuration?.ConnectionString) && (Configuration?.Database?.Tables?.Any(x => x.Selected) ?? false);
        private Configuration GetConfigFromSerialized(string serialized) => JsonConvert.DeserializeObject<Configuration>(serialized, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto });

        private async Task<Database> GetSchemaAsync(string connectionString)
        {
            BusyText = "Estableciendo conexión";
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var database = new Database();

            BusyText = "Obteniendo Esquema";
            var table = connection.GetSchema("Tables");
            var rows = table?.Rows?.Cast<System.Data.DataRow>().Where(x => x["TABLE_TYPE"].ToString().Contains("TABLE"));

            if (!(rows?.Any() ?? false))
                return database;

            var esquemas = rows.GroupBy(x => (string)x["TABLE_SCHEMA"], x => (string)x["TABLE_NAME"]).Select(x =>
            {
                var esquema = new Schema { Parent = database, Name = x.Key, Selected = true };
                esquema.Children = new ObservableCollection<Table>(x.Select(table => new Table { Parent = esquema, Name = table, Selected = true }));
                return esquema;
            });

            database.Children = new ObservableCollection<Schema>(esquemas.OrderBy(x => x.Name));
            return database;
        }

        private bool LoadConfigFromSerialized(string serialized)
        {
            bool result;
            try
            {
                Configuration = string.IsNullOrEmpty(serialized) ? new Configuration() : GetConfigFromSerialized(serialized);
                result = true;
            }
            catch (Exception)
            {
                Configuration = new Configuration();
                result = false;
            }

            return result;
        }

        private string SerializeConfig(Configuration configuration) => JsonConvert.SerializeObject(configuration, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto });
    }
}
