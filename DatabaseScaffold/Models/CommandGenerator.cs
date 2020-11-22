namespace DatabaseScaffold.Models
{
    using DatabaseScaffold.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class CommandGenerator : ICommandGenerator
    {
        public event DataReceivedEventHandler DataReceivedEvent;
        private List<string> errors;
        private List<string> messages;

        public async Task Scaffold(Configuration configuration)
        {
            var workingDirectory = Path.GetDirectoryName(configuration.DataProjectFile);
            var outputDirectory = Path.GetDirectoryName(configuration.ContextFile).Replace(workingDirectory, "");
            var commandParams = configuration.Database.GetParams();
            var motorParams = configuration.Motor.GetParams();

            var oldContextFileContent = await File.ReadAllTextAsync(configuration.ContextFile);
            var existingContent = GetTextBetween(oldContextFileContent, "protected override void OnModelCreating(ModelBuilder modelBuilder)", "modelBuilder.HasAnnotation");
            if (outputDirectory.StartsWith("\\"))
                outputDirectory = outputDirectory.Remove(0, 1);

            if (!string.IsNullOrEmpty(motorParams))
                commandParams += $" {motorParams}";

            if (!string.IsNullOrEmpty(configuration.AdditionalParams))
                commandParams += $" {configuration.AdditionalParams}";

            errors = new List<string>();
            messages = new List<string>();

            var command = $"ef dbcontext scaffold \"{configuration.ConnectionString}\" Microsoft.EntityFrameworkCore.SqlServer -o {outputDirectory} {commandParams}";
            var processExitCode = await RunCommand(command.Trim(), workingDirectory);

            if (errors.Count > 0)
                throw new InvalidDataException(string.Join(Environment.NewLine, errors));

            if (processExitCode != 0)
                throw new InvalidDataException(messages.Count > 0 ? messages[^1] : "Error during command execution");

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

        private Task<int> RunCommand(string command, string workingFolder)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = "dotnet", WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = workingFolder, Arguments = command, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true },
                EnableRaisingEvents = true,
            };

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (!string.IsNullOrEmpty(e?.Data))
                    messages.Add(e.Data);

                DataReceivedEvent?.Invoke(this, e);
            };

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (!string.IsNullOrEmpty(e?.Data))
                    errors.Add(e.Data);
            };
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }
    }
}
