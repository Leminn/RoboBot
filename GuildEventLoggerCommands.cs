using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace RoboBot
{
    public class GuildEventLoggerCommands : BaseCommandModule
    {
        private static class CommandNames
        {
            public const string SetLoggingChannel = "logsetchan";
            public const string UnsetLoggingChannel = "logunsetchan";
            public const string TestLogging = "logtest";
            public const string LoggingInfo = "loginfo";
        }
        
        private const Permissions RequiredPermissions = Permissions.Administrator;

        [RequireGuild]
        [Command(CommandNames.SetLoggingChannel)]
        public async Task SetLoggingChannel(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            GuildEventLogger.Instance.SetLogChannel(ctx.Guild, ctx.Channel);

            await ctx.RespondAsync("Set this channel as the bot log channel");
        }
        
        [RequireGuild]
        [Command(CommandNames.UnsetLoggingChannel)]
        public async Task UnsetLoggingChannel(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            GuildEventLogger.Instance.UnsetLogChannel(ctx.Guild);

            await ctx.RespondAsync("Unset this channel as the bot log channel");
        }
        
        [RequireGuild]
        [Command(CommandNames.TestLogging)]
        public async Task TestLogging(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            await ctx.RespondAsync($"Running test... if nothing happens try setting the log channel again with {ctx.Prefix}{CommandNames.SetLoggingChannel}");
            await GuildEventLogger.Instance.LogInfo(ctx.Guild, $"This is the test of the event logger as requested per [this message]({ctx.Message.JumpLink})");
        }

        [RequireGuild]
        [Command(CommandNames.LoggingInfo)]
        public async Task LoggingInfo(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            GuildEventLogger.GuildLogInfoResult result = GuildEventLogger.Instance.GetGuildLogInfo(ctx.Guild);

            string response = $"Is event logging set : {result.IsSet}";

            if(result.IsSet)
                response += $"\nChannel where events are logged : {result.LogChannel.Mention}";

            await ctx.RespondAsync(response);
        }
    }
}