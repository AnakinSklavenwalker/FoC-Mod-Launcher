namespace PetroGlyph.Games.EawFoc.Services.Mods
{
    internal class WorkshopNameResolver
    {
        public string GetName(HtmlDocument htmlDocument, string workshopId)
        {
            if (htmlDocument == null)
                return workshopId;
            var node = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
            if (node is null)
                return workshopId;
            return node.InnerHtml;
        }
    }
}
