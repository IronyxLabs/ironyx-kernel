using AutoBogus;
using Grpc.Core;

namespace Ironyx.Kernel.Test.Unit.Kernel.Fakers
{
    public class UnaryServerMethodFaker
    {
        public UnaryServerMethod<Envelop, Reply> Throw<TException>(TException exception)
            where TException : Exception
        {
            return (_, __) => throw exception;
        }

        public UnaryServerMethod<Envelop, Reply> InternalServerError() => Throw(new AutoFaker<Exception>().Generate());
        public UnaryServerMethod<Envelop, Reply> NotFound(NotFoundException exception) => Throw(exception);
        public UnaryServerMethod<Envelop, Reply> Conflict(ConflictException exception) => Throw(exception);
        public UnaryServerMethod<Envelop, Reply> BusinessRule(BusinessRuleException exception) => Throw(exception);
    }
}
