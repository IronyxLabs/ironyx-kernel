using System.Text.Json;

namespace Ironyx.Kernel.Execution.Test.Unit.Fakers
{
    internal static class RequestFaker
    {
        public static Request Create<TCommand>(TCommand command)
            where TCommand : Command
        {
            return new Request { Body = command.Serialize() };
        }
    }

    file static class RequestFakerExtensions
    {
        public static string Serialize<TCommand>(this TCommand command)
            where TCommand : Command
        {
            return JsonSerializer.Serialize(command);
        }
    }
}