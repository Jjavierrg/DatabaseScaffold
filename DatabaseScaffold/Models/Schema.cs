namespace DatabaseScaffold.Models
{
    public class Schema : DatabaseItem
    {
        public override string Type => "Esquema";

        protected override string GetParam() => $"--schema {Name}";
    }
}
