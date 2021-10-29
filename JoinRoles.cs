using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace RoboBot
{
    public class SerializedJoinRoles
    {
        [JsonIgnore] private static DiscordClient _client;

        public static void SetDiscordClient(DiscordClient client) => _client = client;

        public ulong GuildId;

        public List<SerializedJoinRole> Roles;

        public SerializedJoinRoles(JoinRoles joinRoles)
        {
            GuildId = joinRoles.Guild.Id;
            Roles = new List<SerializedJoinRole>();
            
            foreach (JoinRole joinRole in joinRoles.Roles)
            {
                Roles.Add(new SerializedJoinRole(joinRole));
            }
        }

        [JsonConstructor]
        public SerializedJoinRoles(ulong guildId, List<SerializedJoinRole> roles)
        {
            GuildId = guildId;
            Roles = roles;
        }

        public JoinRoles ToJoinRoles()
        {
            if (!_client.Guilds.TryGetValue(GuildId, out DiscordGuild guild))
                return null;

            List<JoinRole> joinRoleList = new List<JoinRole>();

            foreach (SerializedJoinRole serializedJoinRole in Roles)
            {
                if (!guild.Roles.TryGetValue(serializedJoinRole.RoleId, out DiscordRole role))
                {
                    GuildEventLogger.Instance.LogWarning(guild, $"Could not get the role {serializedJoinRole.RoleId}. Has it been removed? (Skipping this role)");
                    continue;
                }
                
                joinRoleList.Add(new JoinRole(role));
            }

            return new JoinRoles(guild, joinRoleList);
        }
    }

    public class JoinRoles
    {
        public DiscordGuild Guild;

        public List<JoinRole> Roles;

        public JoinRoles(DiscordGuild guild, List<JoinRole> roles)
        {
            Guild = guild;
            Roles = roles;
        }

        public override string ToString()
        {
            string rolesString = String.Empty;
            foreach (JoinRole joinRole in Roles)
            {
                rolesString += $"\t{joinRole.Role.Mention}\n";
            }

            return $"Guild: {Guild.Name}\nRoles:\n{rolesString}";
        }
    }

    public class SerializedJoinRole
    {
        public ulong RoleId;

        public SerializedJoinRole(JoinRole joinRole)
        {
            RoleId = joinRole.Role.Id;
        }

        [JsonConstructor]
        public SerializedJoinRole(ulong roleId)
        {
            RoleId = roleId;
        }
    }
    
    public class JoinRole
    {
        public DiscordRole Role;

        public JoinRole(DiscordRole role)
        {
            Role = role;
        }

        public override bool Equals(object obj)
        {
            return Role == ((JoinRole)obj).Role;
        }
    }
}