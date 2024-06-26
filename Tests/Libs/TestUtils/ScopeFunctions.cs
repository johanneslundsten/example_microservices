namespace TestUtils;

public static class ScopeFunctions
{
    public static T Also<T>(this T self, Action<T> action)
    {
        action(self);
        return self;
    }

    public static TResult Let<T, TResult>(this T self, Func<T, TResult> func) => func(self);

    public static TResult Run<TResult>(Func<TResult> func) => func();
}