namespace DatabaseScaffold.Models
{
    using System.Threading.Tasks;

    public interface ICommandGenerator
    {
        Task Scaffold(Configuration configuration, IMotor motor);
    }
}
