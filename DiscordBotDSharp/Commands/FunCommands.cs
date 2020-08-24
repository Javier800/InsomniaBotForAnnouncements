using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Linq;
using System.Drawing;

namespace DiscordBotDSharp.Commands {

    class FunCommands {

        [Command("ping")]
        [Description("Returns pong")]
        public async Task Ping(CommandContext ctx) {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("add")]
        [Description("Add two numbers together")]
        public async Task Add(
            CommandContext ctx, 
            [Description("First number")] int numberOne,
            [Description("Second number")] int numberTwo) {
            await ctx.Channel.
                SendMessageAsync((numberOne+numberTwo).ToString()).
                ConfigureAwait(false);
        }

        [Command("respondmessage")]
        public async Task RespondMessage(CommandContext ctx) {
            var interactivity = ctx.Client.GetInteractivityModule();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author.Id == ctx.Member.Id).ConfigureAwait(false);


            await ctx.Channel.SendMessageAsync(message.Message.Content);
        }
        
        [Command("join")]
        public async Task Join(CommandContext ctx) {
            var joinEmbed = new DiscordEmbedBuilder {
                Title = "Would you like to join?",
                Color = DiscordColor.Red
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivityModule();

            var reactionResult = await interactivity.WaitForReactionAsync(
                x => x.Name == thumbsUpEmoji.Name 
                || x.Name == thumbsDownEmoji.Name
                ).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(reactionResult.Message.Content);

            if (reactionResult.Emoji == thumbsUpEmoji && reactionResult.User.Id == ctx.User.Id){
                var role = ctx.Guild.GetRole(746698267592622140);
                await ctx.Channel.SendMessageAsync(ctx.User.Username.ToString() + role);
                await ctx.Guild.GrantRoleAsync(ctx.Member, role).ConfigureAwait(false);
            }
            else if (reactionResult.Emoji == thumbsDownEmoji && reactionResult.User.Id == ctx.User.Id) {
                var role = ctx.Guild.GetRole(746698295556046868);
                await ctx.Channel.SendMessageAsync(ctx.User.Username.ToString() + role);
                await ctx.Guild.GrantRoleAsync(ctx.Member, role).ConfigureAwait(false);
            }            
        }

        [Command("copymessage")]
        public async Task CopyMessageToChannel(CommandContext ctx) {
            var interactivity = ctx.Client.GetInteractivityModule();

            await ctx.Channel.SendMessageAsync("Título del anuncio");
            var title = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author.Id == ctx.Member.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("Descripción del anuncio");
            var description = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author.Id == ctx.Member.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("Canal para mensaje");
            await DisplayChannels(ctx);
            var channelName = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author.Id == ctx.Member.Id).ConfigureAwait(false);
            var channel = ctx.Guild.Channels.Where(x => x.Name == channelName.Message.Content.ToString()).First().Id;

            await ctx.Channel.SendMessageAsync("Color del mensaje");
            await ColorLink(ctx);
            await DisplayColor(ctx);
            var colorMessage = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author.Id == ctx.Member.Id).ConfigureAwait(false);
            var color = new DiscordColor(colorMessage.Message.Content);            

            await ctx.Channel.SendMessageAsync("Icono: url");
            var thumbnailImageURL = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author.Id == ctx.Member.Id).ConfigureAwait(false);

            var messageToCopy = new DiscordEmbedBuilder {
                Title = title.Message.Content,
                Description = description.Message.Content,
                Color = color,
                ThumbnailUrl = thumbnailImageURL.Message.Content
            };

            await ctx.Guild.GetChannel(channel).SendMessageAsync(embed: messageToCopy);

            await ctx.Channel.SendMessageAsync(embed: messageToCopy);
        }

        [Command("colormaker")]
        public async Task ColorLink(CommandContext ctx) {
            await ctx.Channel.SendMessageAsync("https://html-color-codes.info/codigos-de-colores-hexadecimales/");
        }

        [Command("colors")]
        public async Task DisplayColor(CommandContext ctx) {
            Console.WriteLine("hola?");
            string colorDescription = new string($"Blanco:  {DiscordColor.White}   {DiscordEmoji.FromUnicode(ctx.Client, ":white_large_square:").Name} \n" +
                $"Negro:     {DiscordColor.Black}   {DiscordEmoji.FromUnicode(ctx.Client, ":black_large_square:").Name} \n" +
                $"Rojo:      {DiscordColor.Red}   {DiscordEmoji.FromUnicode(ctx.Client, ":red_square:").Name} \n" +
                $"Verde:     {DiscordColor.Green}   {DiscordEmoji.FromUnicode(ctx.Client, ":green_square:").Name} \n" +
                $"Azul:      {DiscordColor.Blue}   {DiscordEmoji.FromUnicode(ctx.Client, ":blue_square:").Name} \n" +
                $"Amarillo:  {DiscordColor.Yellow}   {DiscordEmoji.FromUnicode(ctx.Client, ":yellow_square:").Name} \n" +
                $"Naranja:   {DiscordColor.Orange}   {DiscordEmoji.FromUnicode(ctx.Client, ":orange_square:").Name} \n" +
                $"Morado:    {DiscordColor.Purple}   {DiscordEmoji.FromUnicode(ctx.Client, ":purple_square:").Name} \n" +
                $"\n **No pongas el #**");

            Console.WriteLine(colorDescription);

            var colors = new DiscordEmbedBuilder {
                Title = "Algunos colores",
                Description = colorDescription
            };

            await ctx.Channel.SendMessageAsync(embed: colors);
        }

        [Command("channels")]
        public async Task Channels(CommandContext ctx) {
            //var channels = new DiscordEmbedBuilder {
            //    Title = "Canales disponibles",
            //    Description = DisplayChannels(ctx),
            //};
            await DisplayChannels(ctx);
        }


        public async Task DisplayChannels(CommandContext ctx) {
            var channels = ctx.Guild.Channels;
            StringBuilder description = new StringBuilder();

            foreach(DiscordChannel channel in channels) {
                string newChannelName = channel.Name.ToString();

                if (!char.IsUpper(newChannelName[0])) {
                    description.Append(newChannelName);
                    description.Append("\n");
                }                
            }            

            var embed = new DiscordEmbedBuilder {
                Title = "Canales disponibles",
                Description = description.ToString()
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
        }
    }
}
