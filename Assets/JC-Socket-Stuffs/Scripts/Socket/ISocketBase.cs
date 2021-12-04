using System;

interface ISocketBase : IDisposable
{
    void RegisterRoom(IRoom listener);
    void Send(SocketMessage message);
}