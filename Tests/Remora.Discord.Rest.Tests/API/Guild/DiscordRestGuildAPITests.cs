//
//  DiscordRestGuildAPITests.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Rest.API;
using Remora.Discord.Rest.Tests.Extensions;
using Remora.Discord.Rest.Tests.TestBases;
using Remora.Discord.Tests;
using RichardSzalay.MockHttp;
using Xunit;

namespace Remora.Discord.Rest.Tests.API.Guild
{
    /// <summary>
    /// Tests the <see cref="DiscordRestGuildAPI"/> class.
    /// </summary>
    public class DiscordRestGuildAPITests
    {
        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.CreateGuildAsync"/> method.
        /// </summary>
        public class CreateGuildAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var name = "brr";
                var region = "aa";

                // Create a dummy PNG image
                await using var icon = new MemoryStream();
                await using var binaryWriter = new BinaryWriter(icon);
                binaryWriter.Write(9894494448401390090);
                icon.Position = 0;

                var verificationLevel = VerificationLevel.High;
                var defaultMessageNotifications = MessageNotificationLevel.AllMessages;
                var explicitContentFilter = ExplicitContentFilterLevel.Disabled;
                var roles = new List<IRole>();
                var channels = new List<IPartialChannel>();
                var afkChannelID = new Snowflake(0);
                var afkTimeout = TimeSpan.FromSeconds(10);
                var systemChannelID = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                .WithProperty("name", p => p.Is(name))
                                .WithProperty("region", p => p.Is(region))
                                .WithProperty("icon", p => p.IsString())
                                .WithProperty("verification_level", p => p.Is((int)verificationLevel))
                                .WithProperty
                                (
                                    "default_message_notifications",
                                    p => p.Is((int)defaultMessageNotifications)
                                )
                                .WithProperty("explicit_content_filter", p => p.Is((int)explicitContentFilter))
                                .WithProperty("roles", p => p.IsArray(a => a.WithCount(0)))
                                .WithProperty("channels", p => p.IsArray(a => a.WithCount(0)))
                                .WithProperty("afk_channel_id", p => p.Is(afkChannelID.ToString()))
                                .WithProperty("afk_timeout", p => p.Is(afkTimeout.TotalSeconds))
                                .WithProperty("system_channel_id", p => p.Is(systemChannelID.ToString()))
                            )
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.CreateGuildAsync
                (
                    name,
                    region,
                    icon,
                    verificationLevel,
                    defaultMessageNotifications,
                    explicitContentFilter,
                    roles,
                    channels,
                    afkChannelID,
                    afkTimeout,
                    systemChannelID
                );

                ResultAssert.Successful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task ReturnsErrorIfNameIsTooShort()
            {
                var name = new string('b', 1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.CreateGuildAsync
                (
                    name
                );

                ResultAssert.Unsuccessful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task ReturnsErrorIfNameIsTooLong()
            {
                var name = new string('b', 101);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.CreateGuildAsync
                (
                    name
                );

                ResultAssert.Unsuccessful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task ReturnsErrorIfImageIsUnknownType()
            {
                var name = "aaa";

                // Create a dummy PNG image
                await using var icon = new MemoryStream();
                await using var binaryWriter = new BinaryWriter(icon);
                binaryWriter.Write(0x00000000);
                icon.Position = 0;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.CreateGuildAsync
                (
                    name,
                    icon: icon
                );

                ResultAssert.Unsuccessful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildAsync"/> method.
        /// </summary>
        public class GetGuildAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var withCounts = true;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}")
                        .WithQueryString("with_counts", withCounts.ToString())
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.GetGuildAsync(guildId, withCounts);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildPreviewAsync"/> method.
        /// </summary>
        public class GetGuildPreviewAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/preview")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuildPreview)])
                );

                var result = await api.GetGuildPreviewAsync(guildId);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyGuildAsync"/> method.
        /// </summary>
        public class ModifyGuildAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var name = "brr";
                var region = "aa";

                // Create a dummy PNG image
                await using var icon = new MemoryStream();
                await using var binaryWriter = new BinaryWriter(icon);
                binaryWriter.Write(9894494448401390090);
                icon.Position = 0;

                var verificationLevel = VerificationLevel.High;
                var defaultMessageNotifications = MessageNotificationLevel.AllMessages;
                var explicitContentFilter = ExplicitContentFilterLevel.Disabled;
                var afkChannelID = new Snowflake(0);
                var afkTimeout = TimeSpan.FromSeconds(10);
                var systemChannelID = new Snowflake(1);
                var ownerId = new Snowflake(2);

                await using var splash = new MemoryStream();
                await using var splashBinaryWriter = new BinaryWriter(splash);
                splashBinaryWriter.Write(9894494448401390090);
                splash.Position = 0;

                await using var banner = new MemoryStream();
                await using var bannerBinaryWriter = new BinaryWriter(banner);
                bannerBinaryWriter.Write(9894494448401390090);
                banner.Position = 0;

                var rulesChannelId = new Snowflake(3);
                var publicUpdatesChannelID = new Snowflake(4);
                var preferredLocale = "dd";

                var api = CreateAPI
                (
                    b => b
                    .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}")
                    .WithJson
                    (
                        j => j.IsObject
                        (
                            o => o
                            .WithProperty("name", p => p.Is(name))
                            .WithProperty("region", p => p.Is(region))
                            .WithProperty("verification_level", p => p.Is((int)verificationLevel))
                            .WithProperty
                            (
                                "default_message_notifications",
                                p => p.Is((int)defaultMessageNotifications)
                            )
                            .WithProperty("explicit_content_filter", p => p.Is((int)explicitContentFilter))
                            .WithProperty("afk_channel_id", p => p.Is(afkChannelID.ToString()))
                            .WithProperty("afk_timeout", p => p.Is(afkTimeout.TotalSeconds))
                            .WithProperty("icon", p => p.IsString())
                            .WithProperty("owner_id", p => p.Is(ownerId.ToString()))
                            .WithProperty("splash", p => p.IsString())
                            .WithProperty("banner", p => p.IsString())
                            .WithProperty("system_channel_id", p => p.Is(systemChannelID.ToString()))
                            .WithProperty("rules_channel_id", p => p.Is(rulesChannelId.ToString()))
                            .WithProperty("public_updates_channel_id", p => p.Is(publicUpdatesChannelID.ToString()))
                            .WithProperty("preferred_locale", p => p.Is(preferredLocale))
                        )
                    )
                    .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.ModifyGuildAsync
                (
                    guildId,
                    name,
                    region,
                    verificationLevel,
                    defaultMessageNotifications,
                    explicitContentFilter,
                    afkChannelID,
                    afkTimeout,
                    icon,
                    ownerId,
                    splash,
                    banner,
                    systemChannelID,
                    rulesChannelId,
                    publicUpdatesChannelID,
                    preferredLocale
                );

                ResultAssert.Successful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsNullableRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var name = "brr";

                var api = CreateAPI
                (
                    b => b
                    .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}")
                    .WithJson
                    (
                        j => j.IsObject
                        (
                            o => o
                            .WithProperty("icon", p => p.IsNull())
                            .WithProperty("splash", p => p.IsNull())
                            .WithProperty("banner", p => p.IsNull())
                        )
                    )
                    .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.ModifyGuildAsync
                (
                    guildId,
                    name,
                    icon: null,
                    banner: null,
                    splash: null
                );

                ResultAssert.Successful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task ReturnsErrorIfIconIsUnknownFormat()
            {
                var guildId = new Snowflake(0);
                var name = "brr";

                await using var icon = new MemoryStream();
                await using var binaryWriter = new BinaryWriter(icon);
                binaryWriter.Write(0x00000000);
                icon.Position = 0;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.ModifyGuildAsync
                (
                    guildId,
                    name,
                    icon: icon
                );

                ResultAssert.Unsuccessful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task ReturnsErrorIfBannerIsUnknownFormat()
            {
                var guildId = new Snowflake(0);
                var name = "brr";

                await using var banner = new MemoryStream();
                await using var binaryWriter = new BinaryWriter(banner);
                binaryWriter.Write(0x00000000);
                banner.Position = 0;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.ModifyGuildAsync
                (
                    guildId,
                    name,
                    banner: banner
                );

                ResultAssert.Unsuccessful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task ReturnsErrorIfSplashIsUnknownFormat()
            {
                var guildId = new Snowflake(0);
                var name = "brr";

                await using var splash = new MemoryStream();
                await using var binaryWriter = new BinaryWriter(splash);
                binaryWriter.Write(0x00000000);
                splash.Position = 0;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuild)])
                );

                var result = await api.ModifyGuildAsync
                (
                    guildId,
                    name,
                    splash: splash
                );

                ResultAssert.Unsuccessful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.DeleteGuildAsync"/> method.
        /// </summary>
        public class DeleteGuildAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Delete, $"{Constants.BaseURL}guilds/{guildId}")
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.DeleteGuildAsync(guildId);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildChannelsAsync"/> method.
        /// </summary>
        public class GetGuildChannelsAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/channels")
                        .Respond("application/json", "[ ]")
                );

                var result = await api.GetGuildChannelsAsync(guildId);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.CreateGuildChannelAsync"/> method.
        /// </summary>
        public class CreateGuildChannelAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsTextRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var name = "dd";
                var type = ChannelType.GuildText;
                var topic = "ggg";
                var rateLimitPerUser = 10;
                var position = 1;
                var permissionOverwrites = new List<IPermissionOverwrite>();
                var parentId = new Snowflake(1);
                var nsfw = true;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds/{guildId}/channels")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                .WithProperty("name", p => p.Is(name))
                                .WithProperty("type", p => p.Is((int)type))
                                .WithProperty("topic", p => p.Is(topic))
                                .WithProperty("rate_limit_per_user", p => p.Is(rateLimitPerUser))
                                .WithProperty("position", p => p.Is(position))
                                .WithProperty("permission_overwrites", p => p.IsArray(a => a.WithCount(0)))
                                .WithProperty("parent_id", p => p.Is(parentId.ToString()))
                                .WithProperty("nsfw", p => p.Is(nsfw))
                            )
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IChannel)])
                );

                var result = await api.CreateGuildChannelAsync
                (
                    guildId,
                    name,
                    type,
                    topic,
                    rateLimitPerUser: rateLimitPerUser,
                    position: position,
                    permissionOverwrites: permissionOverwrites,
                    parentID: parentId,
                    isNsfw: nsfw
                );

                ResultAssert.Successful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsVoiceRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var name = "dd";
                var type = ChannelType.GuildVoice;
                var topic = "ggg";
                var bitrate = 4600;
                var userLimit = 10;
                var position = 1;
                var permissionOverwrites = new List<IPermissionOverwrite>();
                var parentId = new Snowflake(1);
                var nsfw = true;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds/{guildId}/channels")
                        .Respond("application/json", SampleRepository.Samples[typeof(IChannel)])
                );

                var result = await api.CreateGuildChannelAsync
                (
                    guildId,
                    name,
                    type,
                    topic,
                    bitrate,
                    userLimit,
                    position: position,
                    permissionOverwrites: permissionOverwrites,
                    parentID: parentId,
                    isNsfw: nsfw
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyGuildChannelPositionsAsync"/> method.
        /// </summary>
        public class ModifyGuildChannelPositionsAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var swaps = new List<(Snowflake Channel, int? Position)>
                {
                    (new Snowflake(1), 1),
                    (new Snowflake(2), 2),
                    (new Snowflake(3), 3),
                    (new Snowflake(4), 4)
                };

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/channels")
                        .WithJson
                        (
                            j => j.IsArray
                            (
                                a => a
                                    .WithCount(swaps.Count)
                                    .WithElement
                                    (
                                        0,
                                        e => e.IsObject
                                        (
                                            o => o
                                            .WithProperty("id", p => p.Is(1.ToString()))
                                            .WithProperty("position", p => p.Is(1))
                                        )
                                    )
                                    .WithElement
                                    (
                                        1,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(2.ToString()))
                                                .WithProperty("position", p => p.Is(2))
                                        )
                                    )
                                    .WithElement
                                    (
                                        2,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(3.ToString()))
                                                .WithProperty("position", p => p.Is(3))
                                        )
                                    )
                                    .WithElement
                                    (
                                        3,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(4.ToString()))
                                                .WithProperty("position", p => p.Is(4))
                                        )
                                    )
                            )
                        )
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.ModifyGuildChannelPositionsAsync(guildId, swaps);

                ResultAssert.Successful(result);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsNullableRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var swaps = new List<(Snowflake Channel, int? Position)>
                {
                    (new Snowflake(1), null),
                    (new Snowflake(2), null),
                    (new Snowflake(3), null),
                    (new Snowflake(4), null)
                };

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/channels")
                        .WithJson
                        (
                            j => j.IsArray
                            (
                                a => a
                                    .WithCount(swaps.Count)
                                    .WithElement
                                    (
                                        0,
                                        e => e.IsObject
                                        (
                                            o => o
                                            .WithProperty("id", p => p.Is(1.ToString()))
                                            .WithProperty("position", p => p.IsNull())
                                        )
                                    )
                                    .WithElement
                                    (
                                        1,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(2.ToString()))
                                                .WithProperty("position", p => p.IsNull())
                                        )
                                    )
                                    .WithElement
                                    (
                                        2,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(3.ToString()))
                                                .WithProperty("position", p => p.IsNull())
                                        )
                                    )
                                    .WithElement
                                    (
                                        3,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(4.ToString()))
                                                .WithProperty("position", p => p.IsNull())
                                        )
                                    )
                            )
                        )
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.ModifyGuildChannelPositionsAsync(guildId, swaps);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildMemberAsync"/> method.
        /// </summary>
        public class GetGuildMemberAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuildMember)])
                );

                var result = await api.GetGuildMemberAsync(guildId, userId);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ListGuildMembersAsync"/> method.
        /// </summary>
        public class ListGuildMembersAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var limit = 10;
                var after = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/members")
                        .WithQueryString
                        (
                            new[]
                            {
                                new KeyValuePair<string, string>("limit", limit.ToString()),
                                new KeyValuePair<string, string>("after", after.ToString())
                            }
                        )
                        .Respond("application/json", "[ ]")
                );

                var result = await api.ListGuildMembersAsync(guildId, limit, after);

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.AddGuildMemberAsync"/> method.
        /// </summary>
        public class AddGuildMemberAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsExistingMemberRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);
                var accessToken = "aa";
                var nick = "cdd";
                var roles = new List<Snowflake>();
                var mute = true;
                var deaf = true;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Put, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("access_token", p => p.Is(accessToken))
                                    .WithProperty("nick", p => p.Is(nick))
                                    .WithProperty("roles", p => p.IsArray(a => a.WithCount(0)))
                                    .WithProperty("mute", p => p.Is(mute))
                                    .WithProperty("deaf", p => p.Is(deaf))
                            )
                        )
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.AddGuildMemberAsync
                (
                    guildId,
                    userId,
                    accessToken,
                    nick,
                    roles,
                    mute,
                    deaf
                );

                ResultAssert.Successful(result);
                Assert.Null(result.Entity);
            }

            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsNewMemberRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);
                var accessToken = "aa";
                var nick = "cdd";
                var roles = new List<Snowflake>();
                var mute = true;
                var deaf = true;

                var api = CreateAPI
                (
                    b => b
                    .Expect(HttpMethod.Put, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}")
                    .WithJson
                    (
                        j => j.IsObject
                        (
                            o => o
                            .WithProperty("access_token", p => p.Is(accessToken))
                            .WithProperty("nick", p => p.Is(nick))
                            .WithProperty("roles", p => p.IsArray(a => a.WithCount(0)))
                            .WithProperty("mute", p => p.Is(mute))
                            .WithProperty("deaf", p => p.Is(deaf))
                        )
                    )
                    .Respond("application/json", SampleRepository.Samples[typeof(IGuildMember)])
                );

                var result = await api.AddGuildMemberAsync
                (
                    guildId,
                    userId,
                    accessToken,
                    nick,
                    roles,
                    mute,
                    deaf
                );

                ResultAssert.Successful(result);
                Assert.NotNull(result.Entity);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyGuildMemberAsync"/> method.
        /// </summary>
        public class ModifyGuildMemberAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);

                var nick = "cdd";
                var roles = new List<Snowflake>();
                var mute = true;
                var deaf = true;
                var channelId = new Snowflake(2);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("nick", p => p.Is(nick))
                                    .WithProperty("roles", p => p.IsArray(a => a.WithCount(0)))
                                    .WithProperty("mute", p => p.Is(mute))
                                    .WithProperty("deaf", p => p.Is(deaf))
                                    .WithProperty("channel_id", p => p.Is(channelId.ToString()))
                            )
                        )
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.ModifyGuildMemberAsync
                (
                    guildId,
                    userId,
                    nick,
                    roles,
                    mute,
                    deaf,
                    channelId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyCurrentUserNickAsync"/> method.
        /// </summary>
        public class ModifyCurrentUserNickAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var nick = "cdd";
                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/members/@me/nick")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("nick", p => p.Is(nick))
                            )
                        )
                        .Respond("application/text", "\"cdd\"")
                );

                var result = await api.ModifyCurrentUserNickAsync
                (
                    guildId,
                    nick
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.AddGuildMemberRoleAsync"/> method.
        /// </summary>
        public class AddGuildMemberRoleAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);
                var roleId = new Snowflake(2);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Put, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}/roles/{roleId}")
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.AddGuildMemberRoleAsync
                (
                    guildId,
                    userId,
                    roleId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.RemoveGuildMemberRoleAsync"/> method.
        /// </summary>
        public class RemoveGuildMemberRoleAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);
                var roleId = new Snowflake(2);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Delete, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}/roles/{roleId}")
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.RemoveGuildMemberRoleAsync
                (
                    guildId,
                    userId,
                    roleId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.RemoveGuildMemberAsync"/> method.
        /// </summary>
        public class RemoveGuildMemberAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Delete, $"{Constants.BaseURL}guilds/{guildId}/members/{userId}")
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.RemoveGuildMemberAsync
                (
                    guildId,
                    userId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildBansAsync"/> method.
        /// </summary>
        public class GetGuildBansAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/bans")
                        .Respond("application/json", "[ ]")
                );

                var result = await api.GetGuildBansAsync
                (
                    guildId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildBanAsync"/> method.
        /// </summary>
        public class GetGuildBanAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/bans/{userId}")
                        .Respond("application/json", SampleRepository.Samples[typeof(IBan)])
                );

                var result = await api.GetGuildBanAsync
                (
                    guildId,
                    userId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.CreateGuildBanAsync"/> method.
        /// </summary>
        public class CreateGuildBanAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);
                var deleteMessageDays = 10;
                var reason = "ddd";

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Put, $"{Constants.BaseURL}guilds/{guildId}/bans/{userId}")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("delete_message_days", p => p.Is(deleteMessageDays))
                                    .WithProperty("reason", p => p.Is(reason))
                            )
                        )
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.CreateGuildBanAsync
                (
                    guildId,
                    userId,
                    deleteMessageDays,
                    reason
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.RemoveGuildBanAsync"/> method.
        /// </summary>
        public class RemoveGuildBanAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var userId = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Delete, $"{Constants.BaseURL}guilds/{guildId}/bans/{userId}")
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.RemoveGuildBanAsync
                (
                    guildId,
                    userId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildRolesAsync"/> method.
        /// </summary>
        public class GetGuildRolesAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/roles")
                        .Respond("application/json", "[ ]")
                );

                var result = await api.GetGuildRolesAsync
                (
                    guildId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.CreateGuildRoleAsync"/> method.
        /// </summary>
        public class CreateGuildRoleAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var name = "brr";
                var permissions = new DiscordPermissionSet(DiscordPermission.Administrator);
                var color = Color.Aqua;
                var hoist = true;
                var mentionable = true;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds/{guildId}/roles")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("name", p => p.Is(name))
                                    .WithProperty("permissions", p => p.Is(permissions.Value.ToString()))
                                    .WithProperty("color", p => p.Is((uint)(color.ToArgb() & 0x00FFFFFF)))
                                    .WithProperty("hoist", p => p.Is(hoist))
                                    .WithProperty("mentionable", p => p.Is(mentionable))
                            )
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IRole)])
                );

                var result = await api.CreateGuildRoleAsync
                (
                    guildId,
                    name,
                    permissions,
                    color,
                    hoist,
                    mentionable
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyGuildRolePositionsAsync"/> method.
        /// </summary>
        public class ModifyGuildRolePositionsAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var swaps = new List<(Snowflake ID, Optional<int?> Position)>
                {
                    (new Snowflake(1), 1),
                    (new Snowflake(2), 2),
                    (new Snowflake(3), 3),
                    (new Snowflake(4), 4)
                };

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/roles")
                        .WithJson
                        (
                            j => j.IsArray
                            (
                                a => a
                                    .WithCount(swaps.Count)
                                    .WithElement
                                    (
                                        0,
                                        e => e.IsObject
                                        (
                                            o => o
                                            .WithProperty("id", p => p.Is(1.ToString()))
                                            .WithProperty("position", p => p.Is(1))
                                        )
                                    )
                                    .WithElement
                                    (
                                        1,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(2.ToString()))
                                                .WithProperty("position", p => p.Is(2))
                                        )
                                    )
                                    .WithElement
                                    (
                                        2,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(3.ToString()))
                                                .WithProperty("position", p => p.Is(3))
                                        )
                                    )
                                    .WithElement
                                    (
                                        3,
                                        e => e.IsObject
                                        (
                                            o => o
                                                .WithProperty("id", p => p.Is(4.ToString()))
                                                .WithProperty("position", p => p.Is(4))
                                        )
                                    )
                            )
                        )
                        .Respond("application/json", "[ ]")
                );

                var result = await api.ModifyGuildRolePositionsAsync
                (
                    guildId,
                    swaps
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyGuildRoleAsync"/> method.
        /// </summary>
        public class ModifyGuildRoleAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var roleId = new Snowflake(1);
                var name = "ff";
                var permissions = new DiscordPermissionSet(DiscordPermission.Administrator);
                var color = Color.Aqua;
                var hoist = true;
                var mentionable = true;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/roles/{roleId}")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("name", p => p.Is(name))
                                    .WithProperty("permissions", p => p.Is(permissions.Value.ToString()))
                                    .WithProperty("color", p => p.Is(color.ToArgb() & 0x00FFFFFF))
                                    .WithProperty("hoist", p => p.Is(hoist))
                                    .WithProperty("mentionable", p => p.Is(mentionable))
                            )
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IRole)])
                );

                var result = await api.ModifyGuildRoleAsync
                (
                    guildId,
                    roleId,
                    name,
                    permissions,
                    color,
                    hoist,
                    mentionable
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.DeleteGuildRoleAsync"/> method.
        /// </summary>
        public class DeleteGuildRoleAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var roleId = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Delete, $"{Constants.BaseURL}guilds/{guildId}/roles/{roleId}")
                        .Respond(HttpStatusCode.NoContent)
                );

                var result = await api.DeleteGuildRoleAsync
                (
                    guildId,
                    roleId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildPruneCountAsync"/> method.
        /// </summary>
        public class GetGuildPruneCountAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var days = 2;
                var includeRoles = new List<Snowflake>
                {
                    new(1),
                    new(2),
                    new(3)
                };

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/prune")
                        .WithQueryString
                        (
                            new[]
                            {
                                new KeyValuePair<string, string>("days", days.ToString()),
                                new KeyValuePair<string, string>("include_roles", string.Join(',', includeRoles))
                            }
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IPruneCount)])
                );

                var result = await api.GetGuildPruneCountAsync
                (
                    guildId,
                    days,
                    includeRoles
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.BeginGuildPruneAsync"/> method.
        /// </summary>
        public class BeginGuildPruneAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var days = 3;
                var computePruneCount = true;
                var includeRoles = new List<Snowflake>();

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Post, $"{Constants.BaseURL}guilds/{guildId}/prune")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("days", p => p.Is(days))
                                    .WithProperty("compute_prune_count", p => p.Is(computePruneCount))
                                    .WithProperty("include_roles", p => p.IsArray(a => a.WithCount(0)))
                            )
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IPruneCount)])
                );

                var result = await api.BeginGuildPruneAsync
                (
                    guildId,
                    days,
                    computePruneCount,
                    includeRoles
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildVoiceRegionsAsync"/> method.
        /// </summary>
        public class GetGuildVoiceRegionsAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/regions")
                        .Respond("application/json", "[ ]")
                );

                var result = await api.GetGuildVoiceRegionsAsync
                (
                    guildId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildInvitesAsync"/> method.
        /// </summary>
        public class GetGuildInvitesAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/invites")
                        .Respond("application/json", "[ ]")
                );

                var result = await api.GetGuildInvitesAsync
                (
                    guildId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildIntegrationsAsync"/> method.
        /// </summary>
        public class GetGuildIntegrationsAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/integrations")
                        .WithQueryString("include_applications", "true")
                        .Respond("application/json", "[ ]")
                );

                var result = await api.GetGuildIntegrationsAsync
                (
                    guildId,
                    true
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildWidgetSettingsAsync"/> method.
        /// </summary>
        public class GetGuildWidgetSettingsAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/widget")
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuildWidget)])
                );

                var result = await api.GetGuildWidgetSettingsAsync
                (
                    guildId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.ModifyGuildWidgetAsync"/> method.
        /// </summary>
        public class ModifyGuildWidgetAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var enabled = true;
                var channelId = new Snowflake(1);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Patch, $"{Constants.BaseURL}guilds/{guildId}/widget")
                        .WithJson
                        (
                            j => j.IsObject
                            (
                                o => o
                                    .WithProperty("enabled", p => p.Is(enabled))
                                    .WithProperty("channel_id", p => p.Is(channelId.ToString()))
                            )
                        )
                        .Respond("application/json", SampleRepository.Samples[typeof(IGuildWidget)])
                );

                var result = await api.ModifyGuildWidgetAsync
                (
                    guildId,
                    enabled,
                    channelId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildVanityUrlAsync"/> method.
        /// </summary>
        public class GetGuildVanityUrlAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/vanity-url")
                        .Respond("application/json", SampleRepository.Samples[typeof(IInvite)])
                );

                var result = await api.GetGuildVanityUrlAsync
                (
                    guildId
                );

                ResultAssert.Successful(result);
            }
        }

        /// <summary>
        /// Tests the <see cref="DiscordRestGuildAPI.GetGuildWidgetImageAsync"/> method.
        /// </summary>
        public class GetGuildWidgetImageAsync : RestAPITestBase<IDiscordRestGuildAPI>
        {
            /// <summary>
            /// Tests whether the API method performs its request correctly.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
            [Fact]
            public async Task PerformsRequestCorrectly()
            {
                var guildId = new Snowflake(0);
                var widgetStyle = WidgetImageStyle.Banner1;

                var api = CreateAPI
                (
                    b => b
                        .Expect(HttpMethod.Get, $"{Constants.BaseURL}guilds/{guildId}/widget.png")
                        .WithQueryString("style", widgetStyle.ToString().ToLowerInvariant())
                        .Respond("image/png", new MemoryStream())
                );

                var result = await api.GetGuildWidgetImageAsync
                (
                    guildId,
                    widgetStyle
                );

                ResultAssert.Successful(result);
            }
        }
    }
}
