using System;
using System.IO.Abstractions;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Utilities;

namespace PetroGlyph.Games.EawFoc.Services
{
    public class SteamGameHelpers
    {
        private const string SteamWorkshopsBaseUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=";

        public static IDirectoryInfo GetWorkshopsLocation(IGame game)
        {
            if (game.Platform != GamePlatform.SteamGold)
                throw new PetroglyphGameException("Unable to get workshops location for non-Steam game.");

            if (PathUtilities.IsAbsolute(game.Directory.FullName))
                throw new InvalidOperationException("Game path must be absolute");

            var gameDir = game.Directory;

            var commonParent = gameDir.Parent?.Parent?.Parent;
            if (commonParent is null)
                throw new PetroglyphGameException("Unable to find workshops location");

            var fs = game.Directory.FileSystem;
            var workshopDirPath = fs.Path.Combine(commonParent.FullName, "workshop/content/32470");
            return fs.DirectoryInfo.FromDirectoryName(workshopDirPath);
        }

        public static bool TryGetWorkshopsLocation(IGame game, out IDirectoryInfo? workshopsLocation)
        {
            workshopsLocation = null;
            try
            {
                workshopsLocation = GetWorkshopsLocation(game);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ToSteamWorkshopsId(string input, out ulong steamId)
        {
            return ulong.TryParse(input, out steamId);
        }

        public static async Task<HtmlDocument?> GetSteamWorkshopsPageHtmlAsync(ulong workshopId)
        {
            try
            {
                var address = $"{SteamWorkshopsBaseUrl}{workshopId}";
                var client = new WebClient();
                var reply = await client.DownloadStringTaskAsync(address);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(reply);
                return htmlDocument;
            }
            catch
            {
                return null;
            }
        }
    }
}
