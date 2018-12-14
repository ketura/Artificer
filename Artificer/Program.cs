﻿using System;
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
			string configLocation = Path.GetFullPath("./config.json");
			Config config = LoadConfig(configLocation);

			string cardCache = "";
			Dictionary<int, WikiSet> Sets = null;
			Dictionary<int, WikiCard> Cards = null;
			ValveAPIResponseCollection ValveData = null;
			CardTextCollection GameFileInfo = null;
			Dictionary<string, int> VOMapping = null;

			try
			{
				string command = null;
				bool exit = false;
				while (!exit)
				{
					switch (command)
					{

						case "valve":
							ValveData = DownloadValveDefinitions(config.ValveAPIBaseURL, config.ValveCacheLocation);
							if(Sets == null || Cards == null)
							{
								(Sets, Cards) = ConvertValveCardsToWikiCards(ValveData);
							}
							break;

						case "save":
							if(ValveData == null)
							{
								ValveData = DownloadValveDefinitions(config.ValveAPIBaseURL, config.ValveCacheLocation);
							}

							DownloadCardImages(ValveData, config.APIImagesLocation);

							break;

						case "clear":

							break;

						case "load":
							
							if (ValveData == null)
							{
								ValveData = DownloadValveDefinitions(config.ValveAPIBaseURL, config.ValveCacheLocation);
							}
							if (Sets == null || Cards == null)
							{
								(Sets, Cards) = ConvertValveCardsToWikiCards(ValveData);
							}
							VOMapping = LoadVOMapping(config.VOMappingLocation);
							GameFileInfo = ExtractGameData(config.ArtifactBaseDir, Sets.Keys.ToList(), VOMapping);
							break;

						case "merge":
							if (ValveData == null)
							{
								ValveData = DownloadValveDefinitions(config.ValveAPIBaseURL, config.ValveCacheLocation);
							}
							if (Sets == null || Cards == null)
							{
								(Sets, Cards) = ConvertValveCardsToWikiCards(ValveData);
							}
							if(VOMapping == null)
							{
								VOMapping = LoadVOMapping(config.VOMappingLocation);
							}
							if(GameFileInfo == null)
							{
								GameFileInfo = ExtractGameData(config.ArtifactBaseDir, Sets.Keys.ToList(), VOMapping);
							}
							MergeAPIWithGameFiles(Sets, Cards, GameFileInfo);
							break;

						case "extract":
							ExtractRawCardImages(config.ArtifactBaseDir, config.GameImagesLocation, config.GameImageFormat);
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

					Console.WriteLine("\n\n\nPlease enter one of the following options:\n\n");
					Console.WriteLine("valve - retrieve complete card definitions from the official Valve API.");
					Console.WriteLine("save - retrieve card images from the official Valve API that are not cached.");
					Console.WriteLine("clear - delete all cached card API images.");
					Console.WriteLine("load - read card/lore/voiceover data from game files at the configured Artifact game path.");
					Console.WriteLine("merge - combine card info from the game data at the configured Artifact game path with official API data.");
					Console.WriteLine("extract - [WARNING: MEMORY HEAVY] extract card images from the game data at the configured Artifact game path.");
					Console.WriteLine("upload - push all extracted game images and cached API card images to the configured wiki.");
					Console.WriteLine("backup - download all existing wiki card articles prior to overwriting them.");
					Console.WriteLine("update - edit or create all card articles with the latest and greatest card info.");
					Console.WriteLine("exit - exit\n");
					command = Console.ReadLine().ToLower();
				}
			}
			catch(Exception e)
			{
				Console.WriteLine($"\n\n\n\n\nERROR\n\nAn exception was encountered while running Artificer.  Please provide the following to the maintainer of the bot:\n\n{e.ToString()}");
			}

			Console.WriteLine("\n\n\n\n\nPress Enter to continue...");
			Console.Read();
		}

		public static Config LoadConfig(string configLocation)
		{
			configLocation = Path.GetFullPath(configLocation);
			Config config = null;
			Console.WriteLine($"Loading config from {configLocation}...");
			if (File.Exists(configLocation))
			{
				config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));
				Console.WriteLine("Done.");
			}
			else
			{
				Console.WriteLine($"Config not found.  Generating blank config and saving to {configLocation}...");
				config = Config.GetDefaultConfig();
				File.WriteAllText(configLocation, JsonConvert.SerializeObject(config));
				Console.WriteLine("Done.");
			}

			return config;
		}

		public static void MergeAPIWithGameFiles(Dictionary<int, WikiSet> Sets, Dictionary<int, WikiCard> Cards, CardTextCollection GameFileInfo)
		{
			foreach(var card in Cards.Values)
			{
				(var text, var lore, var voiceovers) = GameFileInfo.GetGameFileData(card.ID);
				card.TextRaw = text?.RawText;
				card.LoreRaw = lore?.RawText;
				foreach(var vo in voiceovers)
				{
					card.VoiceOverLines[vo.ResponseTrigger] = vo.RawText;
				}
			}
		}

		public static Dictionary<string, int> LoadVOMapping(string VOMappingLocation)
		{
			return JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(VOMappingLocation));
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

		public static (Dictionary<int, WikiSet> Sets, Dictionary<int, WikiCard> Cards) ConvertValveCardsToWikiCards(ValveAPIResponseCollection api)
		{
			var sets = new Dictionary<int, WikiSet>();
			var cards = new Dictionary<int, WikiCard>();
			foreach (var pair in api.Responses)
			{
				var set = pair.Value.SetDefinition;
				sets[pair.Key] = new WikiSet(set);
				foreach (var card in set.card_list)
				{
					WikiCard newCard = null;
					switch (card.card_type)
					{
						case ArtifactCardType.Hero:
							newCard = WikiCard.ParseHero(set, card);
							break;
						case ArtifactCardType.Creep:
							newCard = WikiCard.ParseCreep(set, card);
							break;
						case ArtifactCardType.Improvement:
						case ArtifactCardType.Spell:
							newCard = WikiCard.ParseSpell(set, card);
							break;
						case ArtifactCardType.Item:
							newCard = WikiCard.ParseItem(set, card);
							break;
						case ArtifactCardType.Ability:
						case ArtifactCardType.PassiveAbility:
							newCard = WikiCard.ParseAbility(set, card);
							break;
						case ArtifactCardType.Stronghold:
						case ArtifactCardType.Pathing:
						default:
							newCard = WikiCard.ParseCard(set, card);
							break;
					}

					cards[card.card_id] = newCard;
				}
			}

			return (sets, cards);
		}

		public static CardTextCollection ExtractGameData(string ArtifactDir, List<int> sets, Dictionary<string, int> VOMapping)
		{
			List<string> LoreFilenames = new List<string>();
			List<string> VOFilenames = new List<string>();
			List<string> CardFilenames = new List<string>();

			foreach(int set in sets)
			{
				string setNum = set.ToString("00");
				LoreFilenames.Add(Path.Combine(ArtifactDir, $"game/dcg/panorama/localization/dcg_lore_set_{setNum}_english.txt"));
				CardFilenames.Add(Path.Combine(ArtifactDir, $"game/dcg/resource/card_set_{setNum}_english.txt"));

				if(set != 0)
				{
					VOFilenames.Add(Path.Combine(ArtifactDir, $"game/dcg/panorama/localization/dcg_vo_set_{setNum}_english.txt"));
				}
			}

			CardTextCollection collection = new CardTextCollection();

			foreach (string filename in LoreFilenames)
			{
				if (File.Exists(filename))
				{
					Console.WriteLine($"Loading {filename}...");
					string text = File.ReadAllText(filename);
					Console.WriteLine("Loading complete.  Parsing...");
					collection.ParseLoreSet(text);
					Console.WriteLine("Done.");
				}
				else
				{
					Console.WriteLine($"Filename {filename} doesn't exist!");
				}
			}

			foreach (string filename in VOFilenames)
			{
				if (File.Exists(filename))
				{
					Console.WriteLine($"Loading {filename}...");
					string text = File.ReadAllText(filename);
					Console.WriteLine("Loading complete.  Parsing...");
					collection.ParseVOSet(text, VOMapping);
					Console.WriteLine("Done.");
				}
				else
				{
					Console.WriteLine($"Filename {filename} doesn't exist!");
				}
			}

			foreach (string filename in CardFilenames)
			{
				if (File.Exists(filename))
				{
					Console.WriteLine($"Loading {filename}...");
					string text = File.ReadAllText(filename);
					Console.WriteLine("Loading complete.  Parsing...");
					collection.ParseCardSet(text);
					Console.WriteLine("Done.");
				}
				else
				{
					Console.WriteLine($"Filename {filename} doesn't exist!");
				}
			}

			return collection;
		}

		public static void ExtractRawCardImages(string ArtifactDir, string destDir, string formatName)
		{
			string pakname = "game/dcg/pak01_dir.vpk";
			string pakloc = Path.Combine(ArtifactDir, pakname);
			SKEncodedImageFormat format = SKEncodedImageFormat.Png;
			int quality = 100;
			if(formatName.ToLower() == "jpg" || formatName.ToLower() == "jpeg")
			{
				format = SKEncodedImageFormat.Jpeg;
				quality = 85;
			}



			if (!File.Exists(pakloc))
			{
				Console.WriteLine($"File is missing from the expected location of {pakloc}!  Please check that the Artifact installation directory has been correctly configured.");
				return;
			}

			using (var package = new Package())
			{
				package.Read(pakloc);
				Console.WriteLine($"{pakname} loaded successfully.");
				Console.WriteLine($"{pakname} contains {package.Entries.Count} different file types.");

				Dictionary<string, byte[]> images = new Dictionary<string, byte[]>();

				foreach(var image in package.Entries["vtex_c"])
				{
					if (!image.ToString().StartsWith("panorama/images/card_art"))
						continue;

					package.ReadEntry(image, out byte[] entry);
					string name = $"{image.DirectoryName}/{image.FileName}.{image.TypeName}";
					Console.WriteLine($"Extracting {name}...");
					images[name] = entry;
				}

				Console.WriteLine("\nAll files extracted from VPK.");

				foreach(var pair in images)
				{
					string filename = pair.Key.Replace("vtex_c", formatName);

					var resource = new Resource();
					resource.Read(new MemoryStream(pair.Value));

					var bitmap = ((Texture)resource.Blocks[BlockType.DATA]).GenerateBitmap();
					var image = SKImage.FromBitmap(bitmap);

					string fullFilename = Path.Combine(destDir, filename);
					if(!Directory.Exists(Path.GetDirectoryName(fullFilename)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fullFilename));
					}

					string combined = Path.Combine(destDir, filename);

					Console.WriteLine($"Writing image to file: {combined}...");

					using (FileStream fs = File.Create(combined))
					{
						using (var imageData = image.Encode(format, quality))
						{
							if (imageData == null)
							{
								Console.WriteLine($"Warning! {filename} could not be encoded to {formatName}.  Skipping.");
								File.WriteAllText(combined + ".txt", $"Failed to encode to {formatName}!");
							}
							else
							{
								imageData.SaveTo(fs);
							}
						}
					}
				}

				Console.WriteLine($"\nAll files converted to {formatName}.");

			}

		}

		public static void DownloadCardImages(ValveAPIResponseCollection sets, string imageLocation)
		{
			if(!Directory.Exists(imageLocation))
			{
				Directory.CreateDirectory(imageLocation);
			}

			foreach(var pair in sets.Responses)
			{
				string set = pair.Key.ToString("00");

				foreach(var card in pair.Value.SetDefinition.card_list)
				{
					foreach (var language in card.large_image.Keys)
					{
						string langDir = Path.Combine(imageLocation, "cards", language);
						Directory.CreateDirectory(langDir);
						string cardFilename = Path.Combine(langDir, $"Artifact_card_{set}_{WikiCard.ScrubString(card.card_name["english"])}_{card.card_id}_{language}.png");

						Console.WriteLine($"Working on {cardFilename}...");

						if (File.Exists(cardFilename))
						{
							Console.WriteLine("\tSkipped.");
							continue;
						}

						using (WebClient client = new WebClient())
						{
							client.DownloadFile(card.large_image[language], cardFilename);
							Console.WriteLine("\tDone.");
						}
					}


					foreach (var language in card.mini_image.Keys)
					{
						string langDir = Path.Combine(imageLocation, "icons", language);
						Directory.CreateDirectory(langDir);
						string abilityFileName = Path.Combine(langDir, $"Artifact_icon_{set}_{WikiCard.ScrubString(card.card_name["english"])}_{card.card_id}_{language}.png");

						Console.WriteLine($"Working on {abilityFileName}...");

						if (File.Exists(abilityFileName))
						{
							Console.WriteLine("\tSkipped.");
							continue;
						}

						using (WebClient client = new WebClient())
						{
							client.DownloadFile(card.mini_image[language], abilityFileName);
							Console.WriteLine("\tDone.");
						}
					}

					foreach (var language in card.ingame_image.Keys)
					{
						string langDir = Path.Combine(imageLocation, "hero_icons", language);
						Directory.CreateDirectory(langDir);
						string ingameFileName = Path.Combine(langDir, $"Artifact_heroicon_{set}_{WikiCard.ScrubString(card.card_name["english"])}_{card.card_id}_{language}.png");

						Console.WriteLine($"Working on {ingameFileName}...");

						if (File.Exists(ingameFileName))
						{
							Console.WriteLine("\tSkipped.");
							continue;
						}

						using (WebClient client = new WebClient())
						{
							client.DownloadFile(card.ingame_image[language], ingameFileName);
							Console.WriteLine("\tDone.");
						}
					}
				}

			}
		}

		

		public static ValveAPIResponseCollection DownloadValveDefinitions(string URL, string cacheLocation)
		{
			Console.Clear();
			Console.WriteLine($"Pulling Valve API data.");

			JsonSerializerSettings settings = new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Error,

			};

			ValveAPIResponseCollection responses = null;
			if(File.Exists(cacheLocation))
			{
				Console.WriteLine($"Cached Valve data found at {cacheLocation}.  Using this data.");
				responses = JsonConvert.DeserializeObject<ValveAPIResponseCollection>(File.ReadAllText(cacheLocation), settings);
				if(responses.Responses.Any(x => x.Value.IsExpired))
				{
					Console.WriteLine("This data is stale.  Deleting and refreshing.");
					responses = new ValveAPIResponseCollection();
				}
			}
			else
			{
				Console.WriteLine($"No cached Valve data found.");
				responses = new ValveAPIResponseCollection();
			}

			string baseLocation = Path.GetDirectoryName(cacheLocation);

			for(int i = 0; i < 100; i++)
			{
				string index = i.ToString("00");
				string path = Path.Combine(baseLocation, $"ValveResponse_Set{index}.json");
				if (responses.ContainsKey(i))
				{
					Console.WriteLine($"Loading set {i} from {path}...");
					File.WriteAllText(path, responses[i].JSONResult);
					responses[i].ParseJSON();
				}
				else
				{
					string setURL = $"{URL}{index}/";
					Console.WriteLine($"Connecting to cdn router for set {i} at {setURL}...");

					//Have to use HttpWebRequest instead of WebClient because this is a URL GET rather than a specific resource link that DownloadString can grab.
					HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(setURL);
					request.Method = "GET";
					string result = "";

					try
					{
						using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
						{
							Stream dataStream = response.GetResponseStream();
							StreamReader reader = new StreamReader(dataStream);
							result = reader.ReadToEnd();
							reader.Close();
							dataStream.Close();
						}
					}
					catch(WebException e)
					{
						if(e.Message.Contains("(400)"))
						{
							Console.WriteLine($"Set {i} does not exist. Stopping here.");
							//We've hit a set that doesn't exist
							break;
						}
					}



					var cdnResponse = JsonConvert.DeserializeObject<ValveAPIResponse>(result, settings);
					Console.WriteLine($"Successfully pulled from cdn router. Set {i} can be found at {cdnResponse.FullURL}.  Downloading card data...");
					using (WebClient client = new WebClient())
					{
						result = client.DownloadString(cdnResponse.FullURL);
					}
					cdnResponse.JSONResult = result;
					cdnResponse.ParseJSON();
					cdnResponse.RetrievalDate = DateTime.Now;
					Console.WriteLine($"Successfully pulled from Valve API.  Caching card data to {path}...");
					File.WriteAllText(path, result);
					Console.WriteLine("Done.");

					responses[i] = cdnResponse;
				}

				File.WriteAllText(cacheLocation, JsonConvert.SerializeObject(responses));
			}

			return responses;
		}

		public static string LoadCache(string cacheLocation, string URL)
		{
			Console.Clear();
			Console.WriteLine($"Loading card cache from {cacheLocation}...");
			string cache = "";
			if (File.Exists(cacheLocation))
			{
				cache = File.ReadAllText(cacheLocation);
				Console.WriteLine("Done.");
			}
			else
			{
				Console.WriteLine($"Cache not found.  Retrieving data from {URL} and saving to {cacheLocation}...");
				//cache = DownloadCardDefinitions(URL, cacheLocation);
				//File.WriteAllText(cacheLocation, cache);
				Console.WriteLine("Done.");
			}

			return cache;
		}

	}
}
