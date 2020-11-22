using DatabaseScaffold.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DatabaseScaffold.Interfaces
{
    public interface ICommandGenerator
    {
        event DataReceivedEventHandler DataReceivedEvent;
        Task Scaffold(Configuration configuration);
    }
}
