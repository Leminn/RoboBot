using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace RoboBot
{
    public static class CommandHelpers
    {
        public static async Task<bool> CheckPermissions(CommandContext ctx, Permissions requiredPermissions)
        {
            DiscordMember member = (DiscordMember)ctx.User;

            if (!member.PermissionsIn(ctx.Channel).HasPermission(requiredPermissions))
            {
                await ctx.RespondAsync($"You don't have the required permissions to execute this command\nYou need {requiredPermissions.ToPermissionString()}");
                return false;
            }

            return true;
        }

        public static async Task<DiscordMessage> GetMessageFromUrl(CommandContext ctx, string messageUrl)
        {
            if (!Uri.TryCreate(messageUrl, UriKind.Absolute, out Uri messageUri))
            {
                await ctx.RespondAsync("Bad message link, try again");
                return null;
            }
            
            string[] urlParts = messageUri.PathAndQuery.Remove(0, 1).Split('/');
            if (urlParts.Length != 4)
            {
                await ctx.RespondAsync("Bad message link, try again");
                return null;
            }
            
            if(!ulong.TryParse(urlParts[2], out ulong channelId) || !ulong.TryParse(urlParts[3], out ulong messageId))
            {
                await ctx.RespondAsync("Bad message link, try again");
                return null;
            }

            DiscordMessage message;

            try
            {
                message = await ctx.Guild.Channels[channelId].GetMessageAsync(messageId);
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.ToString());
                return null;
            }
            
            if (message is null)
            {
                await ctx.RespondAsync("Could not get the message, make sure it is in the same server and that the bot has access to it");
                return null;
            }

            return message;
        }
    }
}