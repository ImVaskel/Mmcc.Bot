﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Infrastructure.Requests.Generic;
using Mmcc.Bot.Protos;
using MoreLinq;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat
{
    public class HandleCommandResult
    {
        public class Handler : AsyncRequestHandler<TcpRequest<GenericCommandResult>>
        {
            private readonly IDiscordRestChannelAPI _channelApi;
            private readonly ILogger<HandleCommandResult> _logger;

            public Handler(IDiscordRestChannelAPI channelApi, ILogger<HandleCommandResult> logger)
            {
                _channelApi = channelApi;
                _logger = logger;
            }

            protected override async Task Handle(TcpRequest<GenericCommandResult> request, CancellationToken cancellationToken)
            {
                var msg = request.Message;
                // we want an exception if failed, hence Parse instead of TryParse;
                var parsedId = ulong.Parse(request.Message.DiscordChannelId);
                var channelSnowflake = new Snowflake(parsedId);
                var getChannelResult = await _channelApi.GetChannelAsync(channelSnowflake, cancellationToken);

                if (!getChannelResult.IsSuccess || getChannelResult.Entity is null)
                {
                    throw new Exception(getChannelResult.Error?.Message ?? $"Could not get channel with ID {parsedId}");
                }
                
                var embeds = new List<Embed>
                {
                    new()
                    {
                        Title = $"Command {msg.Command} executed!",
                        Fields = new List<EmbedField>
                        {
                            new("Server", msg.ServerId, false)
                        }
                    }
                };
                var output = msg.CommandOutput
                    .Batch(1024)
                    .Select(charArr => charArr.ToString())
                    .ToList();
                embeds
                    .AddRange(output
                        .Select((str, i) => new Embed
                        {
                            Title = $"[{i + 1}/{output.Count}] Command {msg.Command} output",
                            Fields = new List<EmbedField>
                            {
                                new("Output message", string.IsNullOrEmpty(str) ? "*No output*" : str!, false)
                            }
                        }));

                foreach (var embed in embeds)
                {
                    var sendMessageResult = await _channelApi.CreateMessageAsync(
                        channelSnowflake,
                        embed: embed,
                        ct: cancellationToken);

                    if (!sendMessageResult.IsSuccess)
                    {
                        _logger.LogError("Error while sending command output embed: {error}",
                            sendMessageResult.Error.Message);
                    }
                }
            }
        }
    }
}