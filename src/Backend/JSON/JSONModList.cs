using System;
using System.IO;
using System.Linq;
using System.Net;
using H3VRModInstaller.Backend.Common;
using H3VRModInstaller.Backend.JSON.Common;
using Newtonsoft.Json;

namespace H3VRModInstaller.Backend.JSON
{
    /// <summary>
    ///     Standard JSON file info for a mod
    /// </summary>
    public class ModFile
    {
        /// <summary>
        ///     Unique identifier given to each mod for use in the UI
        /// </summary>
        public string ModId { get; set; }

        /// <summary>
        ///     Name of the mod, for use in <c>help</c>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The raw filename of the mod once downloaded
        /// </summary>
        public string RawName { get; set; }

        /// <summary>
        ///     Author(s) of the mod
        /// </summary>
        public string[] Author { get; set; }

        /// <summary>
        ///     Version of the mod
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Description of the mod
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     URL of the download path, combines with <c>RawName</c> to download the mod
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Website of the mod, if there isn't any, type the Github/Bonetome page
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        ///     How the file should be handled once downloaded
        /// </summary>
        /// <example>
        ///     moveToFolder?ComradeKolbasa.zip?Mods/?ComradeKolbasa.zip
        /// </example>
        /// <remarks>
        ///     The <c>?</c> in the arguments are used as they cannot be inputted normally, and seperate the arguments
        /// </remarks>
        public string Arguments { get; set; }

        /// <summary>
        ///     All the dependencies of the mod, must be the <c>ModID</c>
        /// </summary>
        public string[] Dependencies { get; set; }


        /// <summary>
        ///     The deletion info for a mod
        /// </summary>
        /// <value>String</value>
        public string DelInfo { get; set; }
    }


    /// <summary>
    ///     Contains an array of ModFiles.
    /// </summary>
    public class ModListFormat
    {
        /// <summary>
        ///     Allows for multiple mods in 1 file to be added
        /// </summary>
        public ModFile[] Modlist { get; set; }
		public string ModListName { get; set; }
		public string ModListID { get; set; }
    }

    /// <summary>
    ///     This class manages the modlists, including downloading and loading
    /// </summary>
    public class JsonModList
    {
        private static readonly WebClient Client = new();

        /// <summary>
        ///     Mod list array to repersent all mods
        /// </summary>
        public static ModListFormat[] ModList;


        private static string[] DatabaseURLs;


        /// <summary>
        ///     Returns all ModListFormats.
        /// </summary>
        /// <param name="reload">Forces reload of files rather than grabbing from cache</param>
        /// <param name="jsonfiles">Specify the json files you would like to use, this is a path</param>
        /// <returns>ModList</returns>
        public static ModListFormat[] GetModLists(bool reload = false, string[] jsonfiles = null)
        {
            if (ModList == null || reload)
            {
                Console.WriteLine("Modlist null!");
                var flag = false;
                if (jsonfiles == null)
                {
                    Console.WriteLine("jsonfiles null!");
                    jsonfiles = GetDatabaseURLs();
                    ModInstallerCommon.DebugLog("Found " + jsonfiles.Length + " json files to read from!");
                    flag = true;
                }

                var _mlf = new ModListFormat[jsonfiles.Length];
                for (var i = 0; i < jsonfiles.Length; i++) _mlf[i] = GetDeserializedModListFormatOnline(jsonfiles[i]);
                if (flag) ModList = _mlf;
                return _mlf;
            }

            return ModList;
        }

        /// <summary>
        ///     Deserializes JSON file given JSON file name.
        /// </summary>
        /// <param name="jsontoload">Json file to load</param>
        /// <returns>ModList</returns>
        public static ModListFormat DeserializeModListFormat(string jsontoload)
        {
            var modList = new ModListFormat();
            ModInstallerCommon.DebugLog("Loading " + jsontoload);
            modList = JsonConvert.DeserializeObject<ModListFormat>(
                File.ReadAllText(ModInstallerCommon.Files.Modinstallerdir + jsontoload));
            return modList;
        }

        /// <summary>
        ///     Returns full database URLs for seeing jsonlists
        /// </summary>
        public static string[] GetDatabaseURLs()
        {
            var databaseinfo = GetDeserializedModListFormatOnline(JsonCommon.DatabaseInfo).Modlist[0].Dependencies;


            if (DatabaseURLs != null) return DatabaseURLs;

            var _return = new string[0];
            var prefix = "";
            var x = 0;
            for (var i = 0; i < databaseinfo.Length; i++)
                if (databaseinfo[i].Contains("https")) //string is prefix
                {
                    prefix = databaseinfo[i];
                }
                else //string is postfix
                {
                    Array.Resize(ref _return, _return.Length + 1); //adds item to array
                    _return[x] = prefix + databaseinfo[i]; //assembles full loc
                    Console.WriteLine(_return[x]);
                    x++;
                }

            DatabaseURLs = _return;
            Console.WriteLine(DatabaseURLs.Length);
            return DatabaseURLs;
        }

		private static WebClient client = new WebClient();

        /// <summary>
        ///     Returns ModListFormat, given a full URI or postfix
        /// </summary>
        public static ModListFormat GetDeserializedModListFormatOnline(string loc)
        {
			if (!loc.Contains("https"))
			{
				string[] v = new string[0];
				v = GetDatabaseURLs();
				for (int i = 0; i < v.Length; i++)
				{
					if (v[i].Contains(loc))
					{
						loc = v[i];
						break;
					}
				}
			}
			Console.WriteLine("Reading {0}", loc);
            var serialised = client.DownloadString(new Uri(loc));
            var modList = JsonConvert.DeserializeObject<ModListFormat>(serialised);
            return modList;
        }
    }
}