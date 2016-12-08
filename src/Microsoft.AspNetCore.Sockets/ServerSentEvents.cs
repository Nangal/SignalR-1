// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Sockets
{
    public class ServerSentEvents : IHttpTransport
    {
        private readonly HttpConnection _channel;
        private readonly Connection _connection;

        public ServerSentEvents(Connection connection)
        {
            _connection = connection;
            _channel = (HttpConnection)connection.Transport;
        }

        public async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers["Cache-Control"] = "no-cache";
            context.Response.Headers["Content-Encoding"] = "identity";
            await context.Response.Body.FlushAsync();

            while (true)
            {
                var result = await _channel.Output.ReadAsync();
                var buffer = result.Buffer;

                if (buffer.IsEmpty && result.IsCompleted)
                {
                    break;
                }

                await Send(context, buffer);

                _channel.Output.Advance(buffer.End);
            }
        }

        private async Task Send(HttpContext context, ReadableBuffer data)
        {
            // TODO: Pooled buffers
            // 8 = 6(data: ) + 2 (\n\n)
            var buffer = new byte[8 + data.Length];
            var at = 0;
            buffer[at++] = (byte)'d';
            buffer[at++] = (byte)'a';
            buffer[at++] = (byte)'t';
            buffer[at++] = (byte)'a';
            buffer[at++] = (byte)':';
            buffer[at++] = (byte)' ';
            data.CopyTo(new Span<byte>(buffer, at, data.Length));
            at += data.Length;
            buffer[at++] = (byte)'\n';
            buffer[at++] = (byte)'\n';
            await context.Response.Body.WriteAsync(buffer, 0, at);
            await context.Response.Body.FlushAsync();
        }
    }
}
