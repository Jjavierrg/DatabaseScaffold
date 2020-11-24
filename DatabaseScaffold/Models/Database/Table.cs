namespace DatabaseScaffold.Models
{
    public class Table : DatabaseItem
    {
        public override string Type => "Tabla";
        protected override string GetParam() => $"-t {base.Name}";
    }
}
