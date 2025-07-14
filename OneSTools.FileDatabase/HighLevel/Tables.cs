using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneSTools.FileDatabase.HighLevel;

public class Tables(IList<Table> list) : ReadOnlyCollection<Table>(list)
{
    public Table this[string name]
        => this.FirstOrDefault(c => c.Name == name);
}