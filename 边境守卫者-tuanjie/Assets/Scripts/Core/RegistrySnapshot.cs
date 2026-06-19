using System.Collections.Generic;

public static class RegistrySnapshot
{
    public static IReadOnlyList<T> Copy<T>(List<T> source, List<T> bufferA, List<T> bufferB, ref int version)
    {
        version++;
        var buffer = (version & 1) == 0 ? bufferA : bufferB;
        buffer.Clear();
        buffer.AddRange(source);
        return buffer;
    }
}
