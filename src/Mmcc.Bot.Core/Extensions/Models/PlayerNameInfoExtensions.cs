﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mmcc.Bot.Core.Models.MojangApi;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Core.Extensions.Models
{
    public static class PlayerNameInfoExtensions
    {
        public static EmbedField ToEmbedField(this IEnumerable<IPlayerNameInfo> playerNameInfos)
        {
            var nameInfos = playerNameInfos.ToList();
            var fieldValue = new StringBuilder();

            for (var i = 0; i < nameInfos.Count; i++)
            {
                if (nameInfos[i] is null) break;
                fieldValue.AppendLine(nameInfos[i].ChangedToAt is not null
                    ? $"**▸ #{i + 1}**\n {nameInfos[i].Name} (Changed at: {DateTimeOffset.FromUnixTimeMilliseconds(nameInfos[i].ChangedToAt!.Value).UtcDateTime})"
                    : $"**▸ #{i + 1}**\n {nameInfos[i].Name}");
            }

            return new EmbedField(":information_source: Name history", fieldValue.ToString(), false);
        }
    }
}