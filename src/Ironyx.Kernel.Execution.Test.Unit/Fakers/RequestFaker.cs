using System.Text.Json;

namespace Ironyx.Kernel.Execution.Test.Unit.Fakers
{
    internal class RequestFaker
    {
        //private string? _type;
        //private string? _content;

        //public Request Generate()
        //{
        //    return new Request { Type = _type, Content = _content };
        //}

        //public RequestFaker WithoutRequestType<TCommand>(TCommand command)
        //    where TCommand : Command
        //{
        //    _content = command.Serialize();

        //    return this;
        //}

        //public RequestFaker WithType(string type)
        //{
        //    _type = type;
        //    _content = "";

        //    return this;
        //}

        //public RequestFaker With<TCommand>(TCommand command)
        //    where TCommand : Command
        //{
        //    _type = command.GetRequestType();
        //    _content = command.Serialize();

        //    return this;
        //}
    }

    file static class RequestFakerExtensions
    {
        public static string Serialize<TCommand>(this TCommand command)
            where TCommand : Command
        {
            return JsonSerializer.Serialize(command);
        }

        public static string GetRequestType<TCommand>(this TCommand command)
            where TCommand : Command
        {
            return $"{command.GetType().FullName}, {command.GetType().Assembly.GetName().Name}";
        }
    }
}