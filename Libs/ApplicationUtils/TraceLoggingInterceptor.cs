using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace ApplicationUtils;

public class TraceLoggingInterceptor(ILogger<TraceLoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var traceId = context.RequestHeaders.GetValue("trace-id") ?? Guid.NewGuid().ToString();
        using (logger.BeginScope(new Dictionary<string, object> { { "TraceId", traceId } }))
        {
            context.ResponseTrailers.Add("trace-id", traceId);

            logger.LogInformation("Trace ID: {TraceId}, Received request: {Request}", traceId, request);
            var response = await continuation(request, context);
            logger.LogInformation("Trace ID: {TraceId}, Sending response: {Response}", traceId, response);

            return response;
        }
    }
    
}