namespace DatabaseScaffold.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;

    public class Database : DatabaseItem
    {
        public override string Type => "Base de datos";

        protected override string GetParam() => "";

        [JsonIgnore]
        public IEnumerable<Table> Tables => Children.SelectMany(x => x.Children).OfType<Table>().OrderBy(x => x.FullName);
    }
}
