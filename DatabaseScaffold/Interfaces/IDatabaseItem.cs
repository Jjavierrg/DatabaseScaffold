
namespace DatabaseScaffold.Interfaces
{
    using System.Collections.Generic;
    public interface IDatabaseItem
    {
        string Name { get; set; }
        bool Selected { get; set; }
        IDatabaseItem Parent { get; set; }
        IEnumerable<IDatabaseItem> Children { get; }
        string FullName { get; }

        string GetParams();
    }
}
