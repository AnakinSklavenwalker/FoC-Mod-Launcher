using System;
using System.Globalization;
using EawModinfo.Spec;
using HtmlAgilityPack;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    public class OnlineWorkshopNameResolver : ModNameResolverBase
    {
        public OnlineWorkshopNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        
        protected override string ResolveCore(IModReference modReference, CultureInfo culture)
        {
            if (modReference.Type != ModType.Workshops)
                throw new InvalidOperationException("Can only resolve for Steam Workshop mods!");
            if (!SteamGameHelpers.ToSteamWorkshopsId(modReference.Identifier, out var modId))
                throw new InvalidOperationException($"Cannot get SteamID from workshops object {modReference.Identifier}");
            
            var modsWorkshopWebpage = SteamGameHelpers.GetSteamWorkshopsPageHtmlAsync(modId).GetAwaiter().GetResult();
            if (modsWorkshopWebpage is null)
                throw new InvalidCastException("Unable to get the mod's workshop web page.");
            return GetName(modsWorkshopWebpage);
        }

        private static string GetName(HtmlDocument htmlDocument)
        {
            Requires.NotNull(htmlDocument, nameof(htmlDocument));
            var node = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
            if (node is null)
                throw new InvalidOperationException("Unable to get name form Workshop's web page. Mussing 'workshopItemTitle' node.");
            return node.InnerHtml;
        }
    }
}