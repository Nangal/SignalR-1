// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Sockets.Tests
{
    public class ServerSentEventsTests
    {
        [Fact]
        public async Task SSESetsContentType()
        {
            using (var factory = new PipelineFactory())
            {
                var connection = new Connection();
                connection.ConnectionId = Guid.NewGuid().ToString();
                var httpConnection = new HttpConnection(factory);
                connection.Transport = httpConnection;
                var sse = new ServerSentEvents(connection);
                var context = new DefaultHttpContext();

                httpConnection.Output.CompleteWriter();

                await sse.ProcessRequestAsync(context);

                Assert.Equal("text/event-stream", context.Response.ContentType);
                Assert.Equal("no-cache", context.Response.Headers["Cache-Control"]);
            }
        }

        [Fact]
        public async Task SSEAddsAppropriateFraming()
        {
            using (var factory = new PipelineFactory())
            {
                var connection = new Connection();
                connection.ConnectionId = Guid.NewGuid().ToString();
                var httpConnection = new HttpConnection(factory);
                connection.Transport = httpConnection;
                var sse = new ServerSentEvents(connection);
                var context = new DefaultHttpContext();
                var ms = new MemoryStream();
                context.Response.Body = ms;

                await httpConnection.Output.WriteAsync(Encoding.UTF8.GetBytes("Hello World"));

                httpConnection.Output.CompleteWriter();

                await sse.ProcessRequestAsync(context);

                var expected = "data: Hello World\n\n";
                Assert.Equal(expected, Encoding.UTF8.GetString(ms.ToArray()));
            }
        }
    }
}
