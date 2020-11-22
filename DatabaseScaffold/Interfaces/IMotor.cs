using System.Collections.Generic;

namespace DatabaseScaffold.Interfaces
{
    public interface IMotor
    {
        string Name { get; }
        IEnumerable<IMotorOption> Options { get; }

        string GetParams();
    }
}
