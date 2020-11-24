namespace DatabaseScaffold.Models
{
    public enum MotorVersion
    {
        Base,
        Core5
    }

    public class MotorFactory
    {
        public IMotor GetMotor(MotorVersion version)
        {
            return version switch
            {
                MotorVersion.Core5 => new MotorCore5(),
                _ => new BaseMotor(),
            };
        }
    }
}
