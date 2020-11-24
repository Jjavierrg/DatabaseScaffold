namespace DatabaseScaffold.Models
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public interface IConsole
    {
        event DataReceivedEventHandler OutputDataReceived;
        event DataReceivedEventHandler ErrorDataReceived;
        Task<int> RunCommandAsync(string command, string parameters, string workingFolder);
    }
}
