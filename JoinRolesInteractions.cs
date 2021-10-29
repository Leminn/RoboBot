using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace RoboBot
{
    public class JoinRolesInteractions
    {
        private readonly string JoinRolesLocalPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "join-roles.json");

        private DiscordClient _client;

        public List<JoinRoles> JoinRoles;

        public JoinRolesInteractions(DiscordClient client)
        {
            _client = client;
            
            SerializedJoinRoles.SetDiscordClient(_client);
            JoinRolesSetupCommands.SetJoinRoleInteractions(this);
            
            LoadJoinRoles();
            
            client.GuildMemberAdded += ClientOnGuildMemberAdded;
        }

        private Task ClientOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            return Task.Run(() =>
            {
                JoinRoles associatedRoles = JoinRoles.FirstOrDefault(x => x.Guild == e.Guild);

                if (associatedRoles == null || e.Member.Equals(_client.CurrentUser))
                    return;
                
                foreach (JoinRole joinRole in associatedRoles.Roles)
                {
                    e.Member.GrantRoleAsync(joinRole.Role).Wait();
                }
                
                GuildEventLogger.Instance
                    .LogInfo(e.Guild, $"{e.Member.Mention} was granted the default roles by joining the server").Wait();
            });
        }
        
        public void SaveToFile()
        {
            List<SerializedJoinRoles> listToSerialize = new List<SerializedJoinRoles>();

            foreach (JoinRoles joinRoles in JoinRoles)
            {
                listToSerialize.Add(new SerializedJoinRoles(joinRoles));
            }

            string json = JsonConvert.SerializeObject(listToSerialize, Formatting.Indented);
            File.WriteAllText(JoinRolesLocalPath, json);
        }

        public void LoadJoinRoles()
        {
            JoinRoles = new List<JoinRoles>();
            if (!File.Exists(JoinRolesLocalPath))
            {
                return;
            }
            
            string json = File.ReadAllText(JoinRolesLocalPath);
            List<SerializedJoinRoles> serializedJoinRolesList =
                JsonConvert.DeserializeObject<List<SerializedJoinRoles>>(json);

            foreach (SerializedJoinRoles serializedJoinRoles in serializedJoinRolesList)
            {
                JoinRoles joinRoles = serializedJoinRoles.ToJoinRoles();

                if (joinRoles != null)
                    JoinRoles.Add(joinRoles);
            }
        }
    }
}