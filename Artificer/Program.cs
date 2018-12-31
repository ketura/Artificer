using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using Newtonsoft.Json;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;
using SkiaSharp;

namespace Artificer
{
	class Program
	{
		static void Main(string[] args)
		{
			Artificer artificer = new Artificer();

			try
			{
				string command = null;
				bool exit = false;
				while (!exit)
				{
					switch (command)
					{
						case "extract_art":
							artificer.ExtractRawCardImages();
							break;

						case "extract_audio":
							artificer.ExtractVoiceoverAudio();
							break;


						case "api":
							artificer.DownloadValveDefinitions();
							break;

						case "images":
							artificer.DownloadCardImages();
							break;

						case "clear":
							artificer.ClearDownloadedData();
							break;

						case "parse":
							artificer.ParseGameData();
							break;

						case "merge":
							artificer.MergeAPIWithGameFiles();
							break;


						case "articles":
							artificer.DownloadCardArticles();
							break;

						case "generate":
							artificer.GenerateWikiArticles();
							break;

						case "combine":
							artificer.CombineWikiArticles();
							break;

						case "upload_art":
							artificer.MassUploadImageFilesToWiki();
							break;

						case "upload_audio":
							artificer.MassUploadAudioFilesToWiki();
							break;

						case "update":

							break;

						case "revert":

							break;


						case "exit":
							exit = true;
							break;

						case null:
							break;

						default:
							Console.WriteLine("Command not recognized.  Please try again.");
							break;
					}

					if(!exit)
					{
						Console.WriteLine("\n\n\nPlease enter one of the following options:\n\n");
						Console.WriteLine("  extract_art - [WARNING: MEMORY HEAVY] extract card background art from the configured Artifact game path.");
						Console.WriteLine("  extract_audio - [WARNING: MEMORY HEAVY] extract VO / music audio from the configured Artifact game path.");
						Console.WriteLine(" ");
						Console.WriteLine("  api - retrieve / update complete card definitions from the official Valve API.");
						Console.WriteLine("  images - retrieve card images from the official Valve API.");
						Console.WriteLine("  clear - delete all extracted art/audio and downloaded card art.");
						Console.WriteLine("  parse - read card/lore/voiceover data from game files at the configured Artifact game path.");
						Console.WriteLine("  merge - combine card info from the game data at the configured Artifact game path with official API data.");
						Console.WriteLine(" ");
						Console.WriteLine("  articles - download all card articles and export them to disk for comparison.");
						Console.WriteLine("  generate - create new card articles from all card definitions.");
						Console.WriteLine("  combine - take the downloaded card articles and incorporate updated card data into them.");
						Console.WriteLine("  upload_art - push all extracted game images and cached API card images to the configured wiki.");
						Console.WriteLine("  upload_audio - push all extracted game voiceover files to the configured wiki.");
						Console.WriteLine("  update - push or update all card articles (as configured) to the wiki as needed.");
						Console.WriteLine("  revert - revert all cards (that are configured) to a configured date");
						Console.WriteLine(" ");
						Console.WriteLine(" exit - exit\n");
						command = Console.ReadLine().ToLower();
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine($"\n\n\n\n\nERROR\n\nAn exception was encountered while running Artificer.  Please provide the following to the maintainer of the bot:\n\n{e.ToString()}");
			}

			Console.WriteLine("\n\n\n\n\nPress Enter to continue...");
			Console.Read();
		}

		//Left here for future possible use when updating the VOMapping json
		public static Dictionary<string, int> MapCardIDsToVONames(string ArtifactDir, List<int> sets)
		{
			string pakname = "game/dcg/pak01_dir.vpk";
			string pakloc = Path.Combine(ArtifactDir, pakname);
			Dictionary<string, int> nameMap = new Dictionary<string, int>();

			if (!File.Exists(pakloc))
			{
				Console.WriteLine($"File is missing from the expected location of {pakloc}!  Please check that the Artifact installation directory has been correctly configured.");
				return null;
			}

			using (var package = new Package())
			{
				package.Read(pakloc);
				Console.WriteLine($"{pakname} loaded successfully.");
				Console.WriteLine($"{pakname} contains {package.Entries.Count} different file types.");

				Dictionary<string, string> files = new Dictionary<string, string>();

				foreach (var textFile in package.Entries["txt"])
				{
					if (!textFile.ToString().StartsWith("scripts/talker/set_"))
						continue;

					package.ReadEntry(textFile, out byte[] entry);
					string name = $"{textFile.DirectoryName}/{textFile.FileName}.{textFile.TypeName}";
					Console.WriteLine($"Extracting {name}...");
					files[name] = System.Text.Encoding.Default.GetString(entry);
				}

				Console.WriteLine("\nAll files extracted from VPK.");

				foreach (var pair in files)
				{
					Match match = Regex.Match(pair.Key, @"(\w+)\.txt");
					string name = match.Groups[1].Value;
					match = Regex.Match(pair.Value, @"Whitelist Card (\d+)");
					int id = Int32.Parse(match.Groups[1].Value);

					nameMap[name] = id;
				}

			}

			return nameMap;
		}

	}
}
