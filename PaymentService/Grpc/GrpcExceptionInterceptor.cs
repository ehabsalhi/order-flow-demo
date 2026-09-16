using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using PaymentService.Exceptions;

namespace PaymentService.Grpc;

public class GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exception) when (exception is not RpcException)
        {
            throw ToRpcException(exception);
        }
    }

    private RpcException ToRpcException(Exception exception)
    {
        switch (exception)
        {
            case ValidationException validation:
                var detail = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
                return new RpcException(new Status(StatusCode.InvalidArgument, detail));

            case NotFoundException notFound:
                return new RpcException(new Status(StatusCode.NotFound, notFound.Message));

            case ConflictException conflict:
                return new RpcException(new Status(StatusCode.AlreadyExists, conflict.Message));

            default:
                logger.LogError(exception, "Unhandled gRPC exception occurred.");
                return new RpcException(
                    new Status(StatusCode.Internal, "An unexpected error occurred.")
                );
        }
    }
}
