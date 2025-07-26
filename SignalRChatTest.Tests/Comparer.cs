using System.Diagnostics.CodeAnalysis;
using SignalRChatTest.Models;

namespace SignalRChatTest.Tests;

public class Comparer
{
    public static Comparer<U> Get<U>(Func<U, U, bool> func)
    {
        return new Comparer<U>(func);
    }
}

public class Comparer<T> : Comparer, IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _comparisonFunction;

    public Comparer(Func<T, T, bool> comparisonFunction)
    { 
        _comparisonFunction = comparisonFunction;
    }

    public bool Equals(T? x, T? y)
    {
        return _comparisonFunction(x!, y!);
    }

    public int GetHashCode(T obj)
    {
        return obj!.GetHashCode();
    }
}