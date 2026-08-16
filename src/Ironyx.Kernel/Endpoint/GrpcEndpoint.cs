using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Execution.Exceptions;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Monitoring;
using Ironyx.Kernel.Options;
using Ironyx.Kernel.Serializers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ironyx.Kernel.Receivers
{
    [ExcludeFromCodeCoverage]
    public partial class GrpcEndpoint : GenericAPI.GenericAPIBase
    {
        private readonly IRequestDeserializer _deserializer;
        private readonly IExtractor _extractor;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IOptionsMonitor<ServiceOptions> _serviceOptions;
        private readonly LogContext.GrpEndpointLogContext _logger;

        public GrpcEndpoint(IRequestDeserializer deserilaizer, IExtractor extractor, IRequestContextAccessor requestContext, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, IOptionsMonitor<ServiceOptions> serviceOptions, ILogger<GrpcEndpoint> logger)
        {
            _deserializer = deserilaizer;
            _extractor = extractor;
            _requestContext = requestContext;
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _serviceOptions = serviceOptions;
            _logger = new LogContext.GrpEndpointLogContext(logger);
        }

        public override async Task<Reply> SendAsync(Envelop envelop, ServerCallContext context)
        {
            _logger.ReceivingCommand();
            await _extractor.ExtractAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.SetLogContext(_requestContext.CorrelationId, _requestContext.CausationId, _requestContext.RequestId);

            try
            {
                await _commandDispatcher.DispatchAsync(await _deserializer.DeserializeAsync(envelop, context.CancellationToken), context.CancellationToken);
            }
            catch (BusinessRuleException exception)
            {
                _logger.Error(exception);
                throw exception.BusinessRuleViolation()
                                    .ErrorInfo(_serviceOptions.CurrentValue.Name, "BUSINESS_RULE_VIOLATION", _requestContext.CorrelationId)
                                    .ResourceInfo(_serviceOptions.CurrentValue.Name, exception)
                                    .ToRpcException();
            }
            catch (ConflictException exception)
            {
                _logger.Error(exception);
                throw exception.Conflict()
                                    .ErrorInfo(_serviceOptions.CurrentValue.Name, "CONFLICT", _requestContext.CorrelationId)
                                    .ResourceInfo(_serviceOptions.CurrentValue.Name, exception)
                                    .ToRpcException();
            }
            catch (NotFoundException exception)
            {
                _logger.Error(exception);
                throw exception.NotFound()
                                    .ErrorInfo(_serviceOptions.CurrentValue.Name, "RESOURCE_NOT_FOUND", _requestContext.CorrelationId)
                                    .ResourceInfo(_serviceOptions.CurrentValue.Name, exception)
                                    .ToRpcException();
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                throw exception.InternalError()
                                    .ErrorInfo(_serviceOptions.CurrentValue.Name, "INTERNAL_SERVER_ERROR", _requestContext.CorrelationId)
                                    .ToRpcException();
            }

            _logger.CommandAccepted();
            return new Reply();
        }

        public override async Task<Reply> GetAsync(Envelop envelop, ServerCallContext context)
        {
            _logger.ReceivingQuery();
            var query = await _deserializer.DeserializeAsync(envelop, context.CancellationToken);

            await _extractor.ExtractAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.SetLogContext(_requestContext.CorrelationId, _requestContext.CausationId, _requestContext.RequestId);

            var result = await _queryDispatcher.DispatchAsync<dynamic>(query, context.CancellationToken);

            _logger.QueryExecuted();
            return new Reply()
            {
                Data = JsonSerializer.Serialize(result)
            };
        }
    }

    file static class GrpcEndpointExtensions
    {
        public static Google.Rpc.Status BusinessRuleViolation(this BusinessRuleException exception)
        {
            var status = new Google.Rpc.Status()
            {
                Code = (int)StatusCode.FailedPrecondition,
                Message = exception.Message
            };

            var failure = new PreconditionFailure();
            failure.Violations.Add(new PreconditionFailure.Types.Violation
            {
                Type = exception.ErrorCode,
                Subject = exception.Subject,
                Description = exception.Message
            });

            status.Details.Add(Any.Pack(failure));

            return status;
        }
        public static Google.Rpc.Status Conflict(this ConflictException exception)
        {
            return new Google.Rpc.Status()
            {
                Code = (int)StatusCode.AlreadyExists,
                Message = exception.Message
            };
        }
        public static Google.Rpc.Status NotFound(this NotFoundException exception)
        {
            return new Google.Rpc.Status()
            {
                Code = (int)StatusCode.NotFound,
                Message = exception.Message
            };
        }

        public static Google.Rpc.Status InternalError(this Exception exception)
        {
            return new Google.Rpc.Status()
            {
                Code = (int)StatusCode.Internal,
                Message = "An internal server error occured"
            };
        }

        public static Google.Rpc.Status ErrorInfo(this Google.Rpc.Status status, string domain, string reason, Ulid correlationId)
        {
            var errorInfo = new ErrorInfo()
            {
                Domain = domain,
                Reason = reason
            };
            errorInfo.Metadata.Add(ErrorInfoConstants.CorrelationId, correlationId.ToString());

            status.Details.Add(Any.Pack(errorInfo));

            return status;
        }

        public static Google.Rpc.Status ResourceInfo(this Google.Rpc.Status status, string owner, ResourceException exception)
        {
            var resourceInfo = new ResourceInfo()
            {
                Owner = owner,
                ResourceType = exception.ResourceType,
                ResourceName = exception.ResourceName,
                Description = exception.Message
            };

            status.Details.Add(Any.Pack(resourceInfo));

            return status;
        }
    }
}
