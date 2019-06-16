using System;

namespace drawing.Messaging
{
    public enum Command
    {
        ServerInfoRequest,
        CleanScreen,
        GetPic
    }
    [Serializable]//метаданные, подлежит сериализации
    public class ServiceMessage
    {
        public Command Command { get; }

        public ServiceMessage (Command command)//сообщение, которое собираемся передавать
        {
            Command = command;
        }
    }
}
