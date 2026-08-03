using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Behaviors;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Monitoring
{
    public class LogContext
    {
        public partial class ValidationBehaviorLogContext<TRequest>(ILogger<ValidationBehavior<TRequest>> _logger) where TRequest : IRequest
        {

        }
    }
}
