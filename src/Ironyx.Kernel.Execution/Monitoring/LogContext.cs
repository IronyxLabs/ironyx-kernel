using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Behaviors;
using Ironyx.Kernel.Execution.Dispatchers;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Monitoring
{
    public partial class ValidationBehaviorLogContext<TRequest>(ILogger<ValidationBehavior<TRequest>> _logger) where TRequest : IRequest
    {
        [LoggerMessage(LogLevel.Debug, Message = "Validating {Request}")]
        public partial void LogValidating(string request);

        [LoggerMessage(LogLevel.Debug, Message = "{Request} request has been validated")]
        public partial void LogHasBeenValidated(string request);

        [LoggerMessage(LogLevel.Warning, Message = "No validator found for {Request} request")]
        public partial void LogNoValidator(string request);
    }

    public partial class CommandDispatcherLogContext(ILogger<CommandDispatcher> _logger)
    {
        [LoggerMessage(LogLevel.Debug, "Dispatching {Command}")]
        public partial void LogDispatchingCommand(string command);

        [LoggerMessage(LogLevel.Debug, "Dispatching {Command} has been finished")]
        public partial void LogDispatchingCommandFinished(string command);

        [LoggerMessage(LogLevel.Debug, "{Count} pre handler(s) found")]
        public partial void LogPreHandlersFound(int count);
    }
}
