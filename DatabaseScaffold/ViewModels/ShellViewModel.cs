namespace DatabaseScaffold.ViewModels
{
    using DatabaseScaffold.Core;
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
    using static DatabaseScaffold.Models.MotorFactory;

    public class ShellViewModel : NotifyObject
    {
        private readonly ICommandGenerator _commandGenerator;
        private readonly IConsole _console;
        private readonly MotorFactory _motorFactory;
        private readonly IDialogCoordinator _dialogCoordinator;

        private Configuration configuration;
        private IMotor _motor;

        private string _busyText;
        private bool _isBusy;

        private RelayCommand _generateScaffoldCommand;
        private RelayCommand _loadSchemeCommand;
        private RelayCommand _loadConfigCommand;
        private RelayCommand _saveConfigCommand;
        private RelayCommand _uninstallMotorCommand;
        private RelayCommand<string> _installMotorCommand;
        private RelayCommand<MotorVersion> _useMotorCommand;
        private RelayCommand _findDataProjectCommand;
        private RelayCommand _findContextCommand;

        public ShellViewModel(IDialogCoordinator dialogCoordinator, ICommandGenerator commandGenerator, IConsole console, MotorFactory motorFactory)
        {
            _dialogCoordinator = dialogCoordinator ?? throw new ArgumentNullException(nameof(dialogCoordinator));
            _commandGenerator = commandGenerator ?? throw new ArgumentNullException(nameof(commandGenerator));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _motorFactory = motorFactory ?? throw new ArgumentNullException(nameof(motorFactory));
            _console.OutputDataReceived += (object sender, DataReceivedEventArgs e) => BusyText = e.Data;

            var lastSettings = Properties.Settings.Default.lastSettings;
            LoadConfigFromSerialized(lastSettings);
        }

        public RelayCommand LoadSchemaCommand => _loadSchemeCommand ??= new RelayCommand(async (_) => await LoadSchema(), (_) => CanLoadSchema());
        public RelayCommand GenerateScaffoldCommand => _generateScaffoldCommand ??= new RelayCommand(async (_) => await GenerateScaffold(), (_) => CanGenerateScaffold());
        public RelayCommand LoadConfigCommand => _loadConfigCommand ??= new RelayCommand(async (_) => await LoadConfigAsync());
        public RelayCommand SaveConfigCommand => _saveConfigCommand ??= new RelayCommand(async (_) => await SaveConfigAsync());
        public RelayCommand<string> InstallMotorCommand => _installMotorCommand ??= new RelayCommand<string>(async (version) => await InstallMotorAsync(version));
        public RelayCommand UninstallMotorCommand => _uninstallMotorCommand ??= new RelayCommand(async (_) => await UninstallMotorAsync());
        public RelayCommand<MotorVersion> UseMotorCommand => _useMotorCommand ??= new RelayCommand<MotorVersion>((version) => Motor = _motorFactory.GetMotor(version));
        public RelayCommand FindDataProjectCommand => _findDataProjectCommand ??= new RelayCommand((_) => FindDataProject());
        public RelayCommand FindContextCommand => _findContextCommand ??= new RelayCommand((_) => FindContext());

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }
        public string BusyText
        {
            get => _busyText;
            set { _busyText = value; OnPropertyChanged(); }
        }
        public Configuration Configuration
        {
            get => configuration;
            protected set { configuration = value; OnPropertyChanged(); }
        }
        public IMotor Motor
        {
            get => _motor;
            protected set { _motor = value; OnPropertyChanged(); }
        }
        //public void SetMotorV3() => Motor = IoC.Get<IMotor>(Constants.KEY_MOTOR_BASE);
        //public void SetMotorV5() => Motor = IoC.Get<IMotor>(Constants.KEY_MOTOR_V5);

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
                await _commandGenerator.Scaffold(Configuration, Motor);

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

        public async Task InstallMotorAsync(string version)
        {
            var proc = Process.Start("dotnet", $"tool install --global dotnet-ef --version {version}");
            proc.WaitForExit();

            if (proc.ExitCode == 0)
                await _dialogCoordinator.ShowMessageAsync(this, "Motor instalado", "Motor instalado con éxito", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            else
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "No se ha podido instalar el motor seleccionado. Pruebe a desinstalar el motor primero", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
        }

        public async Task UninstallMotorAsync()
        {
            var proc = Process.Start("dotnet", $"tool uninstall --global dotnet-ef");
            proc.WaitForExit();

            if (proc.ExitCode == 0)
                await _dialogCoordinator.ShowMessageAsync(this, "Motor desinstalado", "Motor desinstalado con éxito", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
            else
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "No se ha podido desinstalar el motor. Asegúrese de tener algún motor instalado", settings: new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented });
        }

        private bool CanLoadSchema() => true;
        private bool CanGenerateScaffold()
        {
            if (string.IsNullOrEmpty(Configuration?.ConnectionString)) return false;
            if (string.IsNullOrEmpty(Configuration?.DataProjectFile) || !File.Exists(Configuration.DataProjectFile)) return false;
            if (string.IsNullOrEmpty(Configuration?.ContextFile) || !File.Exists(Configuration.ContextFile)) return false;

            return Configuration?.Database?.Tables?.Any(x => x.Selected) ?? false;
        }
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

            await connection.CloseAsync();
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
