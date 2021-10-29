using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace RoboBot
{
    public class JoinRolesSetupCommands : BaseCommandModule
    {
        private class CommandNames
        {
            public const string Enable = "joinroleenable";
            public const string Disable = "joinroledisable";

            public const string AddRole = "joinroleadd";
            public const string RemoveRole = "joinroleremove";
            
            public const string ListRoles = "joinrolelist";
        }
        
        private const Permissions RequiredPermissions = Permissions.Administrator;

        private static JoinRolesInteractions _joinRolesInteractions;

        public static void SetJoinRoleInteractions(JoinRolesInteractions joinRolesInteractions) =>
            _joinRolesInteractions = joinRolesInteractions;

        [RequireGuild]
        [Command(CommandNames.Enable)]
        public async Task Enable(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            if (_joinRolesInteractions.JoinRoles.FirstOrDefault(x => x.Guild == ctx.Guild) != null)
            {
                await ctx.RespondAsync($"JoinRoles are already enabled for this guild");    
                return;
            }
            
            _joinRolesInteractions.JoinRoles.Add(new JoinRoles(ctx.Guild, new List<JoinRole>()));
            await ctx.RespondAsync(
                $"JoinRoles are now enabled for this guild, add roles with {ctx.Prefix}{CommandNames.AddRole}");
            _joinRolesInteractions.SaveToFile();
        }
        
        [RequireGuild]
        [Command(CommandNames.Disable)]
        public async Task Disable(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            JoinRoles associatedJoinRoles =
                _joinRolesInteractions.JoinRoles.FirstOrDefault(joinRoles => joinRoles.Guild == ctx.Guild);

            if (associatedJoinRoles == null)
            {
                await ctx.RespondAsync("JoinRoles are not enabled for this guild");
                return;
            }

            _joinRolesInteractions.JoinRoles.Remove(associatedJoinRoles);
            await ctx.RespondAsync("Disabled JoinRoles for this guild");
            
            _joinRolesInteractions.SaveToFile();
        }
        
        [RequireGuild]
        [Command(CommandNames.AddRole)]
        public async Task AddRole(CommandContext ctx, DiscordRole role)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            JoinRoles associatedJoinRoles =
                _joinRolesInteractions.JoinRoles.FirstOrDefault(joinRoles => joinRoles.Guild == ctx.Guild);

            if (associatedJoinRoles == null)
            {
                await ctx.RespondAsync("JoinRoles are not enabled for this guild");
                return;
            }
            
            JoinRole joinRole = new JoinRole(role);

            if (associatedJoinRoles.Roles.Contains(joinRole))
            {
                await ctx.RespondAsync("This role was already added");
                return;
            }
            
            associatedJoinRoles.Roles.Add(joinRole);
            await ctx.RespondAsync($"Added {role.Name} to the roles to add when a person joins");
            _joinRolesInteractions.SaveToFile();
        }
        
        [RequireGuild]
        [Command(CommandNames.AddRole)]
        public async Task AddRole(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide a mention to the role for this command");
        }
        
        [RequireGuild]
        [Command(CommandNames.RemoveRole)]
        public async Task RemoveRole(CommandContext ctx, DiscordRole role)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            JoinRoles associatedJoinRoles =
                _joinRolesInteractions.JoinRoles.FirstOrDefault(joinRoles => joinRoles.Guild == ctx.Guild);

            if (associatedJoinRoles == null)
            {
                await ctx.RespondAsync("JoinRoles are not enabled for this guild");
                return;
            }
            
            JoinRole joinRole = new JoinRole(role);

            if (!associatedJoinRoles.Roles.Contains(joinRole))
            {
                await ctx.RespondAsync("This role was not added in the first place");
                return;
            }
            
            associatedJoinRoles.Roles.Remove(joinRole);
            await ctx.RespondAsync($"Removed {role.Name} to the roles to add when a person joins");
            _joinRolesInteractions.SaveToFile();
        }
        
        [RequireGuild]
        [Command(CommandNames.RemoveRole)]
        public async Task RemoveRole(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide a mention to the role for this command");
        }
        
        [RequireGuild]
        [Command(CommandNames.ListRoles)]
        public async Task ListRoles(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            JoinRoles associatedJoinRoles =
                _joinRolesInteractions.JoinRoles.FirstOrDefault(joinRoles => joinRoles.Guild == ctx.Guild);

            if (associatedJoinRoles == null)
            {
                await ctx.RespondAsync("JoinRoles are not enabled for this guild");
                return;
            }
            
            await ctx.RespondAsync(associatedJoinRoles.ToString());
        }
    }
}