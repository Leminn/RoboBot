using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace RoboBot
{
    public class AddonsCommands
    {
        private const string Srb2Root = "/root/.srb2/";
        private const string AddonsRootPath = "/root/.srb2/addons/";
        private const string AddonsCharactersPath = AddonsRootPath + "Characters";
        private const string AddonsLevelsPath = AddonsRootPath + "Levels";
        private const string AddonsLegacyPath = Srb2Root + ".srb21/" + "addons";

        private enum AddonsListChoice
        {
            Levels,
            Characters,
            Legacy
        }
        
        private const Permissions AddonsManagementPermissions = Permissions.ModerateMembers;
        
        private static HttpClient _httpClient;
        private const string MessageBoardHost = "mb.srb2.org";
        private const string AddonsUriPath = "addons";
        private const string DownloadUriPath = "download";
        
        [SlashCommandPermissions(AddonsManagementPermissions)]
        [SlashCommandGroup("AddonsCtl", "Add and delete addons used for reptogif / reptomp4")]
        [SlashRequireGuild]
        public class AddonsManagementCommands : ApplicationCommandModule
        {
            [SlashCommandGroup("Levels", "Manage level addons")]
            public class LevelsCommands : ApplicationCommandModule
            {
                [SlashCommand("del", "Delete a level addon from the bot")]
                public async Task AddonsDelete(InteractionContext ctx,
                    [Autocomplete(typeof(ServerLevelAddonsAutocompleteProvider))]
                    [Option("File", "The addon file to delete from the bot", true)]
                    string fileName)
                {
                    await AddonDelete(ctx, AddonsLevelsPath, fileName);
                }
                
                [SlashCommand("add", "Add a level addon to the bot")]
                public async Task AddonsAdd(InteractionContext ctx,
                    [Option("Url", "The addon url coming from the SRB2 Message Board")]
                    string addonUrl,
                    [Option("File", "The addon file to delete from the bot")]
                    string fileName)
                {
                    await AddonAdd(ctx, addonUrl, fileName, true);
                }
            }

            [SlashCommandGroup("Characters", "Manage characters addons")]
            public class CharactersCommands : ApplicationCommandModule
            {
                [SlashCommand("del", "Delete a character addon from the bot")]
                public async Task AddonsDelete(InteractionContext ctx,
                    [Autocomplete(typeof(ServerCharactersAddonsAutocompleteProvider))]
                    [Option("File", "The addon file to delete from the bot", true)]
                    string fileName)
                {
                    await AddonDelete(ctx, AddonsCharactersPath, fileName);
                }
                
                [SlashCommand("add", "Add a character addon to the bot")]
                public async Task AddonsAdd(InteractionContext ctx,
                    [Option("Url", "The addon url coming from the SRB2 Message Board")]
                    string addonUrl,
                    [Option("File", "The addon file to delete from the bot")]
                    string fileName)
                {
                    await AddonAdd(ctx, addonUrl, fileName, false);
                }
            }

            private static async Task AddonDelete(InteractionContext ctx, string addonsDir, string userFileName)
            {
                string filePath = Path.Combine(addonsDir, userFileName);

                if (!File.Exists(filePath))
                {
                    await ctx.CreateResponseAsync($"Addon file does not exist (\"{userFileName}\")");
                    return;
                }
                
                File.Delete(filePath);
                await ctx.CreateResponseAsync($"Addon file succesfully deleted! (\"{userFileName}\")");
            }
            
            private static async Task AddonAdd(InteractionContext ctx, string addonUrl, string suggestedFileName, bool isLevel)
            {
                if (_httpClient == default)
                    _httpClient = new HttpClient();

                if (!Uri.TryCreate(addonUrl, UriKind.Absolute, out Uri addonUri))
                {
                    await ctx.CreateResponseAsync("Bad addon link, try again");
                    return;
                }

                if (addonUri.Host != MessageBoardHost)
                {
                    await ctx.CreateResponseAsync("Addon link needs to come from the SRB2 Message Board");
                    return;
                }
                
                string[] urlParts = addonUri.PathAndQuery.Remove(0, 1).Split('/');
                if (urlParts.Length < 2 || urlParts[0] != AddonsUriPath || string.IsNullOrWhiteSpace(urlParts[1]))
                {
                    await ctx.CreateResponseAsync("Bad addon link, try again");
                    return;
                }
                
                string downloadUrl = $"https://{MessageBoardHost}/{AddonsUriPath}/{urlParts[1]}/{DownloadUriPath}";

                string fileNameNoExt = string.Empty;
                string fileExtension = string.Empty;

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder(
                        new DiscordMessageBuilder()
                        {
                            Content = "Downloading addon..."
                        }));

                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

                    fileExtension = string.Empty;

                    // Try and get the file extension from the user provided file name
                    int indexOfDot = suggestedFileName.LastIndexOf('.');

                    if (indexOfDot != -1 && suggestedFileName.Length - indexOfDot < 5) // Don't accept bogus file extensions
                    {
                        fileExtension = suggestedFileName.Substring(indexOfDot);
                        fileNameNoExt = suggestedFileName.Substring(0, indexOfDot);
                    }
                    else
                        fileNameNoExt = suggestedFileName;
                    
                    // If the user didn't provide a file extension, let's try and parse it from the response's suggested file name
                    if (fileExtension == string.Empty && response.Content.Headers.ContentDisposition?.FileName != null)
                    {
                        string tempMbSuggestedFileName = response.Content.Headers.ContentDisposition.FileName;

                        // Remove the '"' character at the end of the suggested file name
                        string mbSuggestedFileName = tempMbSuggestedFileName.Remove(tempMbSuggestedFileName.Length - 1);
                        
                        int mbIndexOfDot = mbSuggestedFileName.LastIndexOf('.');

                        if (mbIndexOfDot != -1 && mbSuggestedFileName.Length - mbIndexOfDot < 5) // Don't accept bogus file extensions
                            fileExtension = mbSuggestedFileName.Substring(mbIndexOfDot);
                    }

                    // All else failed, imply a file extension
                    if (fileExtension == string.Empty)
                        fileExtension = ".pk3";

                    string outputDirectory = isLevel ? AddonsLevelsPath : AddonsCharactersPath;

                    using (Stream networkStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = new FileStream(Path.Combine(outputDirectory, $"{fileNameNoExt}{fileExtension}"), FileMode.OpenOrCreate))
                    {
                        await networkStream.CopyToAsync(fileStream);
                        fileStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Something went wrong when downloading the addon ({downloadUrl}): {e.Message}"));
                    return;
                }
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Addon download was succesful! File name: \"{fileNameNoExt}{fileExtension}\""));
            }

            private class ServerLevelAddonsAutocompleteProvider : IAutocompleteProvider
            {
                public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
                {
                    return await ServerAddonsAutocompleteProviderHelper.Provider(ctx, true);
                }
            }
        
            private class ServerCharactersAddonsAutocompleteProvider : IAutocompleteProvider
            {
                public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
                {
                    return await ServerAddonsAutocompleteProviderHelper.Provider(ctx, false);
                }
            }

            private class ServerAddonsAutocompleteProviderHelper
            {
                public static async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx, bool isLevel)
                {
                    if (ctx.OptionValue is not string partialValue)
                        return new List<DiscordAutoCompleteChoice>();

                    IEnumerable<string> addons;

                    if (isLevel)
                        addons = Directory.GetFiles(AddonsLevelsPath).Select(Path.GetFileName)
                            .OrderBy(x => x, StringComparer.CurrentCulture);
                    else
                        addons = Directory.GetFiles(AddonsCharactersPath).Select(Path.GetFileName)
                            .OrderBy(x => x, StringComparer.CurrentCulture);

                    addons = addons.Take(25); // Discord only supports up to 25 AutoComplete results

                    if (string.IsNullOrWhiteSpace(partialValue))
                        return addons.Select(x => new DiscordAutoCompleteChoice(x, x));

                    return addons.Where(x => x.Contains(partialValue))
                        .Select(x => new DiscordAutoCompleteChoice(x, x));
                }
            }
        }

        [SlashCommandGroup("AddonsList", "List addons used for reptogif / reptomp4")]
        [SlashRequireGuild]
        public class AddonsListCommands : ApplicationCommandModule
        {
            [SlashCommand("Levels", "List level addons to use with reptogif / reptomp4")]
            public async Task LevelsList(InteractionContext ctx)
            {
                await AddonList(ctx, AddonsListChoice.Levels);
            }

            [SlashCommand("Characters", "List characters addons to use with reptogif / reptomp4")]
            public async Task CharactersList(InteractionContext ctx)
            {
                await AddonList(ctx, AddonsListChoice.Characters);
            }
            
            [SlashCommand("legacy", "List 2.1 addons to use with reptogif / reptomp4")]
            public async Task LegacyList(InteractionContext ctx)
            {
                await AddonList(ctx, AddonsListChoice.Legacy);
            }

            private static async Task AddonList(InteractionContext ctx, AddonsListChoice choice)
            {
                string MakeModList(List<string> mods)
                {
                    string modList = "";
                    foreach (var mod in mods)
                    {
                        modList += mod;
                        if (mod != mods.Last()) modList += ", ";
                    }
                    return modList;
                }
                
                List<string> addons = new List<string>();
                switch (choice)
                {
                    case AddonsListChoice.Characters:
                        addons.AddRange(Directory.GetFiles(AddonsCharactersPath)
                            .Select(Path.GetFileName));
                            break;
                    case AddonsListChoice.Levels:
                        addons.AddRange(Directory.GetFiles(AddonsLevelsPath)
                            .Select(Path.GetFileName));
                        break;
                    case AddonsListChoice.Legacy:
                        addons.AddRange(Directory.GetFiles(AddonsLegacyPath)
                            .Select(Path.GetFileName));
                        break;
                }

                if (!addons.Any())
                {
                    await ctx.CreateResponseAsync("Addons are empty here.");
                    return;
                }

                string modList = MakeModList(addons.OrderBy(x => x).ToList());
                
                var addonList = new DiscordEmbedBuilder
                {
                    Title = "Addons for ReplayToMp4 Converter",
                    Description = "Here are the addons available for use with the converter.",
                    Color = DiscordColor.Gold
                };
                addonList.AddField(choice + ":",modList);
                DiscordInteractionResponseBuilder addonsResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(addonList);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource ,addonsResponse);
            }
        }
    }
}