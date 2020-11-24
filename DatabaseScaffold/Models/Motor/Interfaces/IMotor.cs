namespace DatabaseScaffold.Models
{
    using System.Collections.Generic;
    public interface IMotor
    {
        string Name { get; }
        IEnumerable<IMotorOption> Options { get; }
        string GetParams();
    }
}
