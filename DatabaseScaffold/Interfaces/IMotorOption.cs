namespace DatabaseScaffold.Interfaces
{
    public interface IMotorOption
    {
        string Parameter { get; }
        string Name { get; }
        string Description { get; }
        bool Apply { get; set; }
        bool HasParams { get; }
        string Params { get; set; }
        bool HasParamsAndSelected { get; }
    }
}
