using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestUtils;

public class XUnitLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ITestOutputHelper _output;
    private IDisposable _currentScope;

    public XUnitLogger(string categoryName, ITestOutputHelper output)
    {
        _categoryName = categoryName;
        _output = output;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        _currentScope = new Scope(state, _output);
        return _currentScope;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var traceId = (_currentScope as Scope)?.TraceId;
        try
        {
            _output.WriteLine($"{logLevel}: {_categoryName} [TraceId: {traceId}] {message}");
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e);
        }
    }

    private class Scope : IDisposable
    {
        private readonly ITestOutputHelper _output;
        public object State { get; }
        public string? TraceId => ((State as Dictionary<string, Object>))?.GetValueOrDefault("TraceId", null)?.ToString();
        
        public Scope(object state, ITestOutputHelper output)
        {
            State = state;
            _output = output;
            _output.WriteLine($"Begin scope: {state}");
        }

        public void Dispose()
        {
            _output.WriteLine($"End scope: {State}");
        }
    }
}

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public XUnitLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(categoryName, _output);
    }

    public void Dispose() { }
}