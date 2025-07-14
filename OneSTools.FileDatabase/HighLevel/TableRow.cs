using System;

namespace OneSTools.FileDatabase.HighLevel;

public class TableRow
{
    private readonly Table _table;
    
    public object[] Values { get; }
    
    internal TableRow(Table table, object[] values)
    {
        _table = table;
        Values = values;
    }
    
    public object this[string name]
    {
        get
        {
            if (_table.FieldNameNumberCache.TryGetValue(name, out var index))
                return Values[index];

            throw new Exception("failed to get field index");
        }
    }
}