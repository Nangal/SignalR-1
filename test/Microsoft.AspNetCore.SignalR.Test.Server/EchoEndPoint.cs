using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Sockets;

namespace Microsoft.AspNetCore.SignalR.Test.Server
{
    public class EchoEndPoint : StreamingEndPoint
    {
        public async override Task OnConnectedAsync(StreamingConnection connection)
        {
            await connection.Transport.Input.CopyToAsync(connection.Transport.Output);
        }
    }
}
