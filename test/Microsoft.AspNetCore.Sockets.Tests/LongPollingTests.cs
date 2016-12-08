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
    public class LongPollingTests
    {
        [Fact]
        public async Task Set204StatusCodeWhenChannelComplete()
        {
            using (var factory = new PipelineFactory())
            {
                var connection = new Connection();
                connection.ConnectionId = Guid.NewGuid().ToString();
                var channel = new HttpConnection(factory);
                connection.Transport = channel;
                var context = new DefaultHttpContext();
                var poll = new LongPolling(connection);

                channel.Output.CompleteWriter();

                await poll.ProcessRequestAsync(context);

                Assert.Equal(204, context.Response.StatusCode);
            }
        }

        [Fact]
        public async Task NoFramingAddedWhenDataSent()
        {
            using (var factory = new PipelineFactory())
            {
                var connection = new Connection();
                connection.ConnectionId = Guid.NewGuid().ToString();
                var channel = new HttpConnection(factory);
                connection.Transport = channel;
                var context = new DefaultHttpContext();
                var ms = new MemoryStream();
                context.Response.Body = ms;
                var poll = new LongPolling(connection);

                await channel.Output.WriteAsync(Encoding.UTF8.GetBytes("Hello World"));

                channel.Output.CompleteWriter();

                await poll.ProcessRequestAsync(context);

                Assert.Equal("Hello World", Encoding.UTF8.GetString(ms.ToArray()));
            }
        }
    }
}
