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

						case "api":
							ValveData = DownloadValveDefinitions(config.ValveAPIBaseURL, config.ValveCacheLocation);
							if(Sets == null || Cards == null)
							{
								(Sets, Cards) = ConvertValveCardsToWikiCards(ValveData);
							}
							break;

						case "download":
							if(ValveData == null)
							{
								ValveData = DownloadValveDefinitions(config.ValveAPIBaseURL, config.ValveCacheLocation);
							}

							DownloadCardImages(ValveData, config.APIImagesLocation, config.APILanguage);

							break;

						case "clear":
							if(Directory.Exists(config.APIImagesLocation))
							{
								Console.WriteLine($"Deleting {config.APIImagesLocation}...");
								Directory.Delete(config.APIImagesLocation, true);
							}
							if (Directory.Exists(config.GameImagesLocation))
							{
								Console.WriteLine($"Deleting {config.GameImagesLocation}...");
								Directory.Delete(config.GameImagesLocation, true);
							}
							if (Directory.Exists(config.GameAudioLocation))
							{
								Console.WriteLine($"Deleting {config.GameAudioLocation}...");
								Directory.Delete(config.GameAudioLocation, true);
							}
							Console.WriteLine("Done.");
							break;

						case "parse":
							
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

						case "extract_art":
							ExtractRawCardImages(config.ArtifactBaseDir, config.GameImagesLocation);
							break;

						case "extract_audio":
							ExtractVoiceoverAudio(config.ArtifactBaseDir, config.GameAudioLocation);
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
					Console.WriteLine("  extract_art - [WARNING: MEMORY HEAVY] extract card background art from the configured Artifact game path.");
					Console.WriteLine("  extract_audio - [WARNING: MEMORY HEAVY] extract VO / music audio from the configured Artifact game path.");
					Console.WriteLine(" ");
					Console.WriteLine("  api - retrieve / update complete card definitions from the official Valve API.");
					Console.WriteLine("  download - retrieve card images from the official Valve API.");
					Console.WriteLine("  clear - delete all extracted art/audio and downloaded card art.");
					Console.WriteLine("  parse - read card/lore/voiceover data from game files at the configured Artifact game path.");
					Console.WriteLine("  merge - combine card info from the game data at the configured Artifact game path with official API data.");
					Console.WriteLine(" ");
					Console.WriteLine("  upload - push all extracted game images and cached API card images to the configured wiki.");
					Console.WriteLine("  backup - download all existing wiki card articles prior to overwriting them.");
					Console.WriteLine("  update - edit or create all card articles with the latest and greatest card info.");
					Console.WriteLine(" ");
					Console.WriteLine(" exit - exit\n");
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

		public static void ExtractVoiceoverAudio(string ArtifactDir, string destDir)
		{
			string pakname = "game/dcg/pak01_dir.vpk";
			string pakloc = Path.Combine(ArtifactDir, pakname);

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

				foreach (var image in package.Entries["vsnd_c"])
				{
					if (!image.ToString().StartsWith("sounds/responses") && !image.ToString().StartsWith("sounds/music"))
						continue;

					package.ReadEntry(image, out byte[] entry);
					string name = $"{image.DirectoryName}/{image.FileName}.{image.TypeName}";
					Console.WriteLine($"Extracting {name}...");
					images[name] = entry;
				}

				Console.WriteLine("\nAll files extracted from VPK.");

				foreach (var pair in images)
				{

					string filename = Path.Combine(destDir, pair.Key.Replace("vsnd_c", "mp3"));

					var resource = new Resource();
					resource.Read(new MemoryStream(pair.Value));
					var sound = ((Sound)resource.Blocks[BlockType.DATA]).GetSound();

					if (!Directory.Exists(Path.GetDirectoryName(filename)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(filename));
					}

					//Writing MP3
					if (File.Exists(filename))
					{
						Console.WriteLine($"\tSkipping {filename}; it already exists.");
					}
					else
					{
						Console.WriteLine($"Writing mp3 to file: {filename}...");

						using (FileStream fs = File.Create(filename))
						{
							fs.Write(sound, 0, sound.Length);
						}
					}

					
				}

				Console.WriteLine($"\nAll files extracted.");
			}
		}

		public static void ExtractRawCardImages(string ArtifactDir, string destDir)
		{
			string pakname = "game/dcg/pak01_dir.vpk";
			string pakloc = Path.Combine(ArtifactDir, pakname);
			SKEncodedImageFormat pngFormat = SKEncodedImageFormat.Png;
			SKEncodedImageFormat jpgFormat = SKEncodedImageFormat.Jpeg;
			int jpgQuality = 85;

			string pngDir = Path.Combine(destDir, "png");
			string jpgDir = Path.Combine(destDir, "jpg");


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
					
					string pngFilename = Path.Combine(pngDir, pair.Key.Replace("vtex_c", ".png"));
					string jpgFilename = Path.Combine(jpgDir, pair.Key.Replace("vtex_c", ".jpg"));

					var resource = new Resource();
					resource.Read(new MemoryStream(pair.Value));

					var bitmap = ((Texture)resource.Blocks[BlockType.DATA]).GenerateBitmap();
					var image = SKImage.FromBitmap(bitmap);
					Console.WriteLine("Checking for white border...");
					image = CropBorder(image);

					if (!Directory.Exists(Path.GetDirectoryName(pngFilename)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(pngFilename));
					}
					if (!Directory.Exists(Path.GetDirectoryName(jpgFilename)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(jpgFilename));
					}

					//Writing PNG
					if (File.Exists(pngFilename))
					{
						Console.WriteLine($"\tSkipping {pngFilename}; it already exists.");
					}
					else
					{
						Console.WriteLine($"Writing image to file: {pngFilename}...");

						using (FileStream fs = File.Create(pngFilename))
						{
							using (var imageData = image.Encode(pngFormat, 100))
							{
								if (imageData == null)
								{
									Console.WriteLine($"Warning! {pngFilename} could not be encoded to {pngFilename}.  Skipping.");
									File.WriteAllText(pngFilename + ".txt", $"Failed to encode to {pngFilename}!");
								}
								else
								{
									imageData.SaveTo(fs);
								}
							}
						}
					}

					//Writing JPG
					if (File.Exists(jpgFilename))
					{
						Console.WriteLine($"\tSkipping {jpgFilename}; it already exists.");
					}
					else
					{
						Console.WriteLine($"Writing image to file: {jpgFilename}...");

						using (FileStream fs = File.Create(jpgFilename))
						{
							using (var imageData = image.Encode(jpgFormat, jpgQuality))
							{
								if (imageData == null)
								{
									Console.WriteLine($"Warning! {jpgFilename} could not be encoded to {jpgFilename}.  Skipping.");
									File.WriteAllText(jpgFilename + ".txt", $"Failed to encode to {jpgFilename}!");
								}
								else
								{
									imageData.SaveTo(fs);
								}
							}
						}
					}
				}

				Console.WriteLine($"\nAll files extracted.");
			}
		}

		private static SKImage CropBorder(SKImage original)
		{
			var bitmap = SKBitmap.FromImage(original);
			int sampleCount = 4;

			int top = 0;
			int bottom = bitmap.Height;
			int left = 0;
			int right = bitmap.Width;

			int increment = (bitmap.Width - 1) / sampleCount;
			int x = 0;
			int y = 0;
			SKColor pureWhite = new SKColor(255,255,255,255);

			//top border
			y = 0;
			do
			{
				for (x = 0; x < sampleCount; x++)
				{
					SKColor pixel = bitmap.GetPixel(x, y);
					if (pixel != pureWhite)
					{
						//Console.WriteLine($"Found end of blank top margin at [{x}, {y}].");
						break;
					}
				}
				if (x < sampleCount)
				{
					top = y;
					break;
				}
				else
				{
					y++;
				}
			} while (y < bitmap.Height / 3);


			//bottom border
			y = bottom - 1;
			do
			{
				for (x = 0; x < sampleCount; x++)
				{
					SKColor pixel = bitmap.GetPixel(x, y);
					if (pixel != pureWhite)
					{
						//Console.WriteLine($"Found end of blank bottom margin at [{x}, {y}].");
						break;
					}
				}
				if (x < sampleCount)
				{
					bottom = y - 1;
					break;
				}
				else
				{
					y--;
				}
			} while (y > bitmap.Height * 0.66);


			//left border
			x = 0;
			do
			{
				for (y = 0; y < sampleCount; y++)
				{
					SKColor pixel = bitmap.GetPixel(x, y);
					if (pixel != pureWhite)
					{
						//Console.WriteLine($"Found end of blank top margin at [{x}, {y}].");
						break;
					}
				}
				if (y < sampleCount)
				{
					left = x;
					break;
				}
				else
				{
					x++;
				}
			} while (x < bitmap.Width / 3);


			//right border
			x = right - 1;
			do
			{
				for (y = 0; y < sampleCount; y++)
				{
					SKColor pixel = bitmap.GetPixel(x, y);
					if (pixel != pureWhite)
					{
						//Console.WriteLine($"Found end of blank bottom margin at [{x}, {y}].");
						break;
					}
				}
				if (y < sampleCount)
				{
					right = x - 1;
					break;
				}
				else
				{
					x--;
				}
			} while (x > bitmap.Width * 0.66);

			return original.Subset(new SKRectI(left, top, right, bottom));

		}

		public static void DownloadCardImages(ValveAPIResponseCollection sets, string imageLocation, string language)
		{
			if(language.ToLower() == "english")
			{
				language = "default";
			}

			string pngCardDir = Path.Combine(imageLocation, "png", "cards", language);
			string jpgCardDir = Path.Combine(imageLocation, "jpg", "cards", language);
			string pngIconDir = Path.Combine(imageLocation, "png", "icons", language);
			string jpgIconDir = Path.Combine(imageLocation, "jpg", "icons", language);
			string pngHeroDir = Path.Combine(imageLocation, "png", "hero_icons", language);
			string jpgHeroDir = Path.Combine(imageLocation, "jpg", "hero_icons", language);
			Directory.CreateDirectory(pngCardDir);
			Directory.CreateDirectory(jpgCardDir);
			Directory.CreateDirectory(pngIconDir);
			Directory.CreateDirectory(jpgIconDir);
			Directory.CreateDirectory(pngHeroDir);
			Directory.CreateDirectory(jpgHeroDir);

			if (!Directory.Exists(imageLocation))
			{
				Directory.CreateDirectory(imageLocation);
			}

			foreach (var pair in sets.Responses)
			{
				string set = pair.Key.ToString("00");

				foreach (var card in pair.Value.SetDefinition.card_list)
				{
					if (!card.large_image.Keys.Contains(language))
					{
						Console.WriteLine($"Configured language {language} not found!");
						continue;
					}

					string scrubbedName = WikiCard.ScrubString(card.card_name["english"]);

					Console.WriteLine($"Working on {scrubbedName}...");


					//Getting main card images
					string pngCardFilename = Path.Combine(pngCardDir, $"{set}_{scrubbedName}_{card.card_id}_card_{language}.png");
					string jpgCardFilename = Path.Combine(jpgCardDir, $"{set}_{scrubbedName}_{card.card_id}_card_{language}.jpg");
					DownloadAndConvertCard(card.large_image[language], pngCardFilename, jpgCardFilename);

					//Getting card icons
					string pngAbilityFileName = Path.Combine(pngIconDir, $"{set}_{scrubbedName}_{card.card_id}_icon_{language}.png");
					string jpgAbilityFileName = Path.Combine(jpgIconDir, $"{set}_{scrubbedName}_{card.card_id}_icon_{language}.jpg");
					DownloadAndConvertCard(card.mini_image[language], pngAbilityFileName, jpgAbilityFileName);

					//Getting hero icons
					string pngIngameFileName = Path.Combine(pngHeroDir, $"{set}_{scrubbedName}_{card.card_id}_hero_{language}.png");
					string jpgIngameFileName = Path.Combine(jpgHeroDir, $"{set}_{scrubbedName}_{card.card_id}_hero_{language}.jpg");
					if(card.ingame_image.Count > 0)
					{
						DownloadAndConvertCard(card.ingame_image[language], pngIngameFileName, jpgIngameFileName);
					}

					Console.WriteLine("Done.");
				}

			}
		}

		private static void DownloadAndConvertCard(string cardURL, string pngFilename, string jpgFilename)
		{
			if (File.Exists(pngFilename))
			{
				Console.WriteLine($"\tSkipped download, png already exists.");
			}
			else
			{
				int MaxAttempts = 4;

				for (int attempt = 1; attempt <= MaxAttempts; attempt++)
				{
					try
					{
						HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cardURL);
						request.Timeout = 10000;
						request.ReadWriteTimeout = 10000;
						var response = (HttpWebResponse)request.GetResponse();
						using (var fs = File.OpenWrite(pngFilename))
						{
							response.GetResponseStream().CopyTo(fs);
						}
						Console.WriteLine($"Download complete.");
					}
					catch (WebException)
					{
						if (attempt == MaxAttempts)
							throw;

						Console.WriteLine("\nConnection Error.  Retrying...");
					}
				}
			}

			//convert downloaded png to jpg
			if (File.Exists(jpgFilename))
			{
				Console.WriteLine($"\tSkipped conversion, jpg already exists.");
			}
			else
			{
				using (FileStream inputFS = File.OpenRead(pngFilename))
				{
					//byte[] bytes = inputFS.read
					var bitmap = SKBitmap.Decode(inputFS);
					SKImage image = SKImage.FromBitmap(bitmap);

					using (FileStream outputFS = File.Create(jpgFilename))
					{
						using (var imageData = image.Encode(SKEncodedImageFormat.Jpeg, 85))
						{
							if (imageData == null)
							{
								Console.WriteLine($"Warning! {jpgFilename} could not be encoded to jpg.  Skipping.");
								File.WriteAllText(jpgFilename + ".txt", $"Failed to encode to jpg!");
							}
							else
							{
								imageData.SaveTo(outputFS);
							}
						}
					}
				}
				Console.WriteLine($"Conversion complete.");
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
