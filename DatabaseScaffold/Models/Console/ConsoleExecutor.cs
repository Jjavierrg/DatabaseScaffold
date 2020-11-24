namespace DatabaseScaffold.Models
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ConsoleExecutor : IConsole
    {
        public event DataReceivedEventHandler OutputDataReceived;
        public event DataReceivedEventHandler ErrorDataReceived;

        public Task<int> RunCommandAsync(string command, string parameters, string workingFolder)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = command, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = workingFolder, Arguments = parameters, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true },
                EnableRaisingEvents = true,
            };

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => OutputDataReceived?.Invoke(this, e);
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => ErrorDataReceived?.Invoke(this, e);

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
