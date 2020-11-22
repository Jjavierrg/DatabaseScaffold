namespace DatabaseScaffold.Models
{
    using DatabaseScaffold.Interfaces;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class CommandGenerator : ICommandGenerator
    {
        public event DataReceivedEventHandler DataReceivedEvent;

        public async Task Scaffold(Configuration configuration)
        {
            var workingDirectory = Path.GetDirectoryName(configuration.DataProjectFile);
            var outputDirectory = Path.GetDirectoryName(configuration.ContextFile).Replace(workingDirectory, "");
            var commandParams = configuration.Database.GetParams();

            var oldContextFileContent = await File.ReadAllTextAsync(configuration.ContextFile);
            var existingContent = GetTextBetween(oldContextFileContent, "protected override void OnModelCreating(ModelBuilder modelBuilder)", "modelBuilder.HasAnnotation");
            if (outputDirectory.StartsWith("\\"))
                outputDirectory = outputDirectory.Remove(0, 1);

            if (!string.IsNullOrEmpty(configuration.AdditionalParams))
                commandParams += $" {configuration.AdditionalParams}";

            var command = $"ef dbcontext scaffold \"{configuration.ConnectionString}\" Microsoft.EntityFrameworkCore.SqlServer -o {outputDirectory} {commandParams}";
            await RunCommand(command.Trim(), workingDirectory);

            var newContextFileContent = await File.ReadAllTextAsync(configuration.ContextFile);
            newContextFileContent = ReplaceTextBetween(newContextFileContent, "protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)", "modelBuilder.HasAnnotation", existingContent);

            await File.WriteAllTextAsync(configuration.ContextFile, newContextFileContent);
        }

        public string GetTextBetween(string text, string from, string to)
        {
            var init = text.IndexOf(from);
            int final = text.IndexOf(to);

            return text.Substring(init, final - init);
        }

        private string ReplaceTextBetween(string text, string from, string to, string replace)
        {
            var init = text.IndexOf(from);
            int final = text.IndexOf(to);

            var temp = text.Remove(init, final - init);
            return temp.Insert(init, replace);
        }



        private Task RunCommand(string command, string workingFolder)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = "dotnet", WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = workingFolder, Arguments = command, CreateNoWindow = true, RedirectStandardOutput = true },
                EnableRaisingEvents = true,
            };

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                DataReceivedEvent?.Invoke(this, e);
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            return tcs.Task;
        }
    }
}
