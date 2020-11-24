namespace DatabaseScaffold.Models
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class CommandGenerator : ICommandGenerator
    {
        private readonly IConsole _console;

        public CommandGenerator(IConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public async Task Scaffold(Configuration configuration, IMotor motor)
        {
            var workingDirectory = Path.GetDirectoryName(configuration.DataProjectFile);
            var outputDirectory = Path.GetDirectoryName(configuration.ContextFile).Replace(workingDirectory, "");
            var commandParams = configuration.Database.GetParams();
            var motorParams = motor.GetParams();

            var oldContextFileContent = await File.ReadAllTextAsync(configuration.ContextFile);
            var existingContent = GetTextBetween(oldContextFileContent, "protected override void OnModelCreating(ModelBuilder modelBuilder)", "modelBuilder.HasAnnotation");
            if (outputDirectory.StartsWith("\\"))
                outputDirectory = outputDirectory.Remove(0, 1);

            if (!string.IsNullOrEmpty(motorParams))
                commandParams += $" {motorParams}";

            if (!string.IsNullOrEmpty(configuration.AdditionalParams))
                commandParams += $" {configuration.AdditionalParams}";

            var command = $"ef dbcontext scaffold \"{configuration.ConnectionString}\" Microsoft.EntityFrameworkCore.SqlServer -o {outputDirectory} {commandParams}";
            var processExitCode = await _console.RunCommandAsync("dotnet", command.Trim(), workingDirectory);

            if (processExitCode != 0)
                throw new InvalidDataException("Error during command execution");

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
    }
}
