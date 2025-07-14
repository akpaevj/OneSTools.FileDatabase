using System;

namespace OneSTools.FileDatabase.Extensions;

public static class ObjectExtensions
{
    public static Guid AsGuid(this object obj)
    {
        if (obj is byte[] { Length: 16 } bytes)
            return new Guid(bytes);

        throw new Exception("failed to convert object to GUID");
    }
    
    public static string AsString(this object obj)
    {
        if (obj is string value)
            return value;

        throw new Exception("failed to convert object to string");
    }
}