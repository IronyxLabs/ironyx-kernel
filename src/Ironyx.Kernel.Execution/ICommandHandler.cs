using MediatR;

namespace Ironyx.Kernel
{
    internal interface ICommandHandler<TCommand> : IRequestHandler<TCommand>
        where TCommand : Command
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
