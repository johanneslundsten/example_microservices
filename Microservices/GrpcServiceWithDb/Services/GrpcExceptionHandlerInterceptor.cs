namespace GrpcServiceWithDb.Services;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class GrpcExceptionHandlerInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionHandlerInterceptor> _logger;

    public GrpcExceptionHandlerInterceptor(ILogger<GrpcExceptionHandlerInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            return HandleException<TResponse>(ex, context);
        }
    }

    private TResponse HandleException<TResponse>(Exception exception, ServerCallContext context)
    {
        _logger.LogError(exception, "An unhandled exception occurred during the gRPC request.");

        var statusCode = StatusCode.Unknown;
        var message = "An unexpected error occurred.";

        switch (exception)
        {
            case RpcException rpcException:
                statusCode = rpcException.StatusCode;
                message = rpcException.Status.Detail;
                break;
            case ArgumentException _:
                statusCode = StatusCode.InvalidArgument;
                message = exception.Message;
                break;
            case UnauthorizedAccessException _:
                statusCode = StatusCode.PermissionDenied;
                message = exception.Message;
                break;
        }

        throw new RpcException(new Status(statusCode, message));
    }
}
