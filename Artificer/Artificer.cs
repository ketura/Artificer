﻿
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////
////                                                                               ////
////    Copyright 2017-2018 Christian 'ketura' McCarty                             ////
////                                                                               ////
////    Licensed under the Apache License, Version 2.0 (the "License");            ////
////    you may not use this file except in compliance with the License.           ////
////    You may obtain a copy of the License at                                    ////
////                                                                               ////
////                http://www.apache.org/licenses/LICENSE-2.0                     ////
////                                                                               ////
////    Unless required by applicable law or agreed to in writing, software        ////
////    distributed under the License is distributed on an "AS IS" BASIS,          ////
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   ////
////    See the License for the specific language governing permissions and        ////
////    limitations under the License.                                             ////
////                                                                               ////
///////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using SkiaSharp;
using SteamDatabase.ValvePak;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

namespace Artificer
{
	public class Artificer
	{
		public readonly string ConfigLocation = Path.GetFullPath("./config.json");


		public ValveAPIResponseCollection ValveData { get; set; }
		public Dictionary<int, WikiSet> Sets { get; set; }
		public Dictionary<int, WikiCard> Cards { get; set; }
		
		public CardTextCollection GameFileInfo { get; set; }
		public Dictionary<string, int> VOMapping { get; set; }

		private Config _config { get; set; }

		public Artificer()
		{
			_config = Config.LoadConfig(ConfigLocation);
		}

		private void AssertValveData()
		{
			if (ValveData == null)
			{
				DownloadValveDefinitions();
			}
		}

		private void AssertVOMapping()
		{
			if(VOMapping == null)
			{
				LoadVOMapping();
			}
		}

		private void AssertSets()
		{
			AssertValveData();

			if (Sets == null || Cards == null)
			{
				ConvertValveCardsToWikiCards();
			}
		}

		private void AssertGameFileInfo()
		{
			AssertSets();
			AssertVOMapping();
			if (GameFileInfo == null)
			{
				ParseGameData();
			}
		}

		public void DownloadValveDefinitions()
		{
			DownloadValveDefinitions(_config.ValveAPIBaseURL, _config.ValveCacheLocation);
		}
		public void DownloadValveDefinitions(string URL, string cacheLocation)
		{
			Console.Clear();
			Console.WriteLine($"Pulling Valve API data.");

			JsonSerializerSettings settings = new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Error,

			};

			ValveAPIResponseCollection responses = null;
			if (File.Exists(cacheLocation))
			{
				Console.WriteLine($"Cached Valve data found at {cacheLocation}.  Using this data.");
				responses = JsonConvert.DeserializeObject<ValveAPIResponseCollection>(File.ReadAllText(cacheLocation), settings);
				if (responses.Responses.Any(x => x.Value.IsExpired))
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

			for (int i = 0; i < 100; i++)
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
					catch (WebException e)
					{
						if (e.Message.Contains("(400)"))
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

			ValveData = responses;
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

		public void DownloadCardImages()
		{
			DownloadCardImages(_config.APIImagesLocation, _config.APILanguage);
		}
		public void DownloadCardImages(string imageLocation, string language)
		{
			AssertValveData();

			if (language.ToLower() == "english")
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

			foreach (var pair in ValveData.Responses)
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
					if (card.ingame_image.Count > 0)
					{
						DownloadAndConvertCard(card.ingame_image[language], pngIngameFileName, jpgIngameFileName);
					}

					Console.WriteLine("Done.");
				}

			}
		}

		public void ClearDownloadedData()
		{
			if (Directory.Exists(_config.APIImagesLocation))
			{
				Console.WriteLine($"Deleting {_config.APIImagesLocation}...");
				Directory.Delete(_config.APIImagesLocation, true);
			}
			if (Directory.Exists(_config.GameImagesLocation))
			{
				Console.WriteLine($"Deleting {_config.GameImagesLocation}...");
				Directory.Delete(_config.GameImagesLocation, true);
			}
			if (Directory.Exists(_config.GameAudioLocation))
			{
				Console.WriteLine($"Deleting {_config.GameAudioLocation}...");
				Directory.Delete(_config.GameAudioLocation, true);
			}
			Console.WriteLine("Done.");
		}

		public void ConvertValveCardsToWikiCards()
		{
			Sets = new Dictionary<int, WikiSet>();
			Cards = new Dictionary<int, WikiCard>();
			foreach (var pair in ValveData.Responses)
			{
				var set = pair.Value.SetDefinition;
				Sets[pair.Key] = new WikiSet(set);
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

					Cards[card.card_id] = newCard;
				}
			}
		}

		public void LoadVOMapping()
		{ 
			VOMapping = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(_config.VOMappingLocation));
		}

		public void ParseGameData()
		{
			ParseGameData(_config.ArtifactBaseDir);
		}
		public void ParseGameData(string ArtifactDir)
		{
			AssertSets();
			AssertVOMapping();

			List<string> LoreFilenames = new List<string>();
			List<string> VOFilenames = new List<string>();
			List<string> CardFilenames = new List<string>();

			foreach (int set in Sets.Keys.ToList())
			{
				string setNum = set.ToString("00");
				LoreFilenames.Add(Path.Combine(ArtifactDir, $"game/dcg/panorama/localization/dcg_lore_set_{setNum}_english.txt"));
				CardFilenames.Add(Path.Combine(ArtifactDir, $"game/dcg/resource/card_set_{setNum}_english.txt"));

				if (set != 0)
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

			GameFileInfo = collection;
		}

		public void MergeAPIWithGameFiles()
		{
			AssertGameFileInfo();

			foreach (var card in Cards.Values)
			{
				(var text, var lore, var voiceovers) = GameFileInfo.GetGameFileData(card.ID);
				card.TextRaw = text?.RawText;
				card.LoreRaw = lore?.RawText;
				foreach (var vo in voiceovers)
				{
					card.VoiceOverLinesRaw[vo.ResponseTrigger] = vo.RawText;
				}
			}

			var gauntlet = TransformerGauntlet.GenerateGauntlet();
			gauntlet.Execute(Cards);
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
			SKColor pureWhite = new SKColor(255, 255, 255, 255);

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

		public void ExtractRawCardImages()
		{
			ExtractRawCardImages(_config.ArtifactBaseDir, _config.GameImagesLocation);
		}
		public void ExtractRawCardImages(string ArtifactDir, string destDir)
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

				foreach (var image in package.Entries["vtex_c"])
				{
					if (!image.ToString().StartsWith("panorama/images/card_art"))
						continue;

					package.ReadEntry(image, out byte[] entry);
					string name = $"{image.DirectoryName}/{image.FileName}.{image.TypeName}";
					Console.WriteLine($"Extracting {name}...");
					images[name] = entry;
				}

				Console.WriteLine("\nAll files extracted from VPK.");

				foreach (var pair in images)
				{

					string pngFilename = Path.Combine(pngDir, pair.Key.Replace("vtex_c", "png"));
					string jpgFilename = Path.Combine(jpgDir, pair.Key.Replace("vtex_c", "jpg"));

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

		public void ExtractVoiceoverAudio()
		{
			ExtractVoiceoverAudio(_config.ArtifactBaseDir, _config.GameAudioLocation);
		}
		public void ExtractVoiceoverAudio(string ArtifactDir, string destDir)
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

		public void MassUploadImageFilesToWiki()
		{
			MassUploadImageFilesToWiki(_config.GameImagesLocation, _config.APIImagesLocation, _config.WikiURL, _config.WikiUsername, _config.WikiPassword);
		}
		public void MassUploadImageFilesToWiki(string GameImageLocation, string APIImageLocation, string wikiurl, string wikiuser, string wikipass)
		{
			AssertGameFileInfo();

			ArtifactWikiBot bot = new ArtifactWikiBot(wikiurl, wikiuser, wikipass);
			bot.Initialize();
			foreach (var card in Cards.Values)
			{
				//Main Card Image
				bot.UploadFile(Path.Combine(APIImageLocation, "jpg/cards/default/", card.CardImage), card.CardImage);
				//Card Icon
				bot.UploadFile(Path.Combine(APIImageLocation, "jpg/icons/default/", card.CardIcon), card.CardIcon);
				//Raw Background Image
				bot.UploadFile(Path.Combine(GameImageLocation, $"jpg/panorama/images/card_art/set{card.SetID.ToString("00")}/full_art", $"{card.ID}_psd.jpg"), card.CardImageRaw);
				//Unfiltered Card Icon
				bot.UploadFile(Path.Combine(GameImageLocation, $"jpg/panorama/images/card_art/set{card.SetID.ToString("00")}/mini_icons", $"{card.ID}_psd.jpg"), card.CardIconRaw);

				if (card.CardType == ArtifactCardType.Hero)
				{
					var hero = card.SubCard as WikiHero;
					//API Hero Icon
					bot.UploadFile(Path.Combine(APIImageLocation, "png/hero_icons/default/", hero.HeroIcon), hero.HeroIcon);
					//Raw Pixel Art Hero Icon
					bot.UploadFile(Path.Combine(GameImageLocation, $"png/panorama/images/card_art/set{card.SetID.ToString("00")}/hero_icons", $"{card.ID}_png.png"), hero.HeroIconRaw);
				}
			}

			bot.End();
		}

		public void DownloadCardArticles()
		{
			DownloadCardArticles(_config.ArticleLocation, _config.WikiURL, _config.WikiUsername, _config.WikiPassword);
		}
		public void DownloadCardArticles(string fileLocation, string wikiurl, string wikiuser, string wikipass)
		{
			AssertGameFileInfo();

			ArtifactWikiBot bot = new ArtifactWikiBot(wikiurl, wikiuser, wikipass);
			bot.Initialize();

			var titles = Cards.Where(x => x.Value.CardType != ArtifactCardType.Ability 
																 && x.Value.CardType != ArtifactCardType.PassiveAbility 
																 && x.Value.CardType != ArtifactCardType.Stronghold
																 && x.Value.CardType != ArtifactCardType.Pathing)
				.Select(x => x.Value.Name);
			var pages = bot.DownloadArticles(titles);

			var basepath = Path.Combine(fileLocation, "Existing_Articles");
			Directory.CreateDirectory(basepath);

			string results = "";

			foreach (var page in pages)
			{
				if(!page.Exists)
				{
					results += $"{page.Title} does not exist.\n";
					continue;
				}

				string path = Path.Combine(basepath, page.Title);
				File.WriteAllText($"{path}.txt", page.Content);
			}

			File.WriteAllText(Path.Combine(basepath, "results.txt"), results);

			bot.End();
		}

		public void GenerateWikiArticles()
		{
			GenerateWikiArticles(_config.ArticleLocation);
		}

		public void GenerateWikiArticles(string fileLocation)
		{
			AssertGameFileInfo();

			var validCards = Cards.Where(x => x.Value.CardType != ArtifactCardType.Ability
																 && x.Value.CardType != ArtifactCardType.PassiveAbility
																 && x.Value.CardType != ArtifactCardType.Stronghold
																 && x.Value.CardType != ArtifactCardType.Pathing)
														.Select(x => x.Value);

			var basepath = Path.Combine(fileLocation, "New_Articles");
			Directory.CreateDirectory(basepath);

			foreach (var card in validCards)
			{
				GenerateArticle(card, Path.Combine(basepath, card.Name));				
			}
		}

		public Dictionary<string, string> GenerateArticle(WikiCard card, string basefile)
		{
			string tabType = null;
			switch (card.CardType)
			{
				case ArtifactCardType.Hero:
					tabType = "Hero";
					break;
				case ArtifactCardType.Creep:
					tabType = "Creep";
					break;
				case ArtifactCardType.Improvement:
					tabType = "Improvement";
					break;
				case ArtifactCardType.Spell:
					tabType = "Spell";
					break;
				case ArtifactCardType.Item:
					tabType = "Item";
					break;
				case ArtifactCardType.Stronghold:
				case ArtifactCardType.Pathing:
				case ArtifactCardType.Ability:
				case ArtifactCardType.PassiveAbility:
				default:
					tabType = null;
					break;
			}

			string mainPage = "";
			mainPage += $"{{{{Tabs/{tabType}}}}}\n";
			mainPage += $@"{{{{Card Infobox
| ID = {card.ID}
| BaseID = {card.BaseID}
| MarketplaceID = {card.MarketplaceID}
| Name = {card.Name}
| Aliases = 
| SetID = {card.SetID}
| CardType = {card.CardType}
| SubType = {card.SubType}
| Rarity = {card.Rarity}
| Color = {card.Color}
| TokenOf = {card.TokenOf ?? 0}
| SignatureOf = {card.SignatureOf ?? 0}
| IsCollectable = {card.IsCollectable}
| Text = {card.Text}
| TextRaw = {card.TextRaw}
| TextFormatted = {card.TextFormatted}
| CardImage = {card.CardImage}
| CardImageRaw = {card.CardImageRaw}
| CardIcon = {card.CardIcon}
| Illustrator = {card.Illustrator}
| VoiceActor = {card.VoiceActor}
| Lore = {card.Lore}
| LoreFormatted = {card.LoreFormatted}
| Keywords = {String.Join(",", card.Keywords)}}}}}\n";

			switch (card.CardType)
			{
				case ArtifactCardType.Hero:
					var hero = card.SubCard as WikiHero;
					mainPage += $@"{{{{Hero Infobox
| ID = {card.ID}
| Name = {card.Name}
| Attack = {hero.Attack}
| Armor = {hero.Armor}
| Health = {hero.Health}
| SignatureCardID = {hero.SignatureCardID}
| Abilities = {String.Join(",", hero.Abilities.Keys.Select(x => x.ToString()))}
| HeroIcon = {hero.HeroIcon}
| HeroIconRaw = {hero.HeroIconRaw}}}}}\n\n";
					break;
				case ArtifactCardType.Creep:
					var creep = card.SubCard as WikiCreep;
					mainPage += $@"{{{{{tabType} Infobox
| ID = {card.ID}
| Name = {card.Name}
| ManaCost = {creep.ManaCost}
| Attack = {creep.Attack}
| Armor = {creep.Armor}
| Health = {creep.Health}
| Abilities = {String.Join(",", creep.Abilities.Keys.Select(x => x.ToString()))}}}}}\n\n";
					break;
				case ArtifactCardType.Improvement:
				case ArtifactCardType.Spell:
					var spell = card.SubCard as WikiSpell;
					mainPage += $@"{{{{{tabType} Infobox
| ID = {card.ID}
| Name = {card.Name}
| CardSpawned = {spell.CardSpawned}
| ManaCost = {spell.ManaCost}
| Charges = {spell.Charges}
| IsCrosslane = {spell.IsCrosslane}}}}}\n\n";
					break;
				case ArtifactCardType.Item:
					var item = card.SubCard as WikiItem;
					mainPage += $@"{{{{Item Infobox
| ID = {card.ID}
| Name = {card.Name}
| GoldCost = {item.GoldCost}
| Abilities = {String.Join(",", item.Abilities.Keys.Select(x => x.ToString()))}}}}}\n\n";
					break;
				default:
					break;
			}

			if (card.CardType == ArtifactCardType.Item)
			{
				mainPage += $"{{{{{tabType.ToUpper()[0]}|{card.Name}}}}} is an [[{tabType}]] in the [[{Sets[card.SetID].Name}]] set.\n\n";
			}
			else if (card.CardType == ArtifactCardType.Improvement)
			{
				mainPage += $"{{{{Im|{card.Name}}}}} is a {card.Color} [[{tabType}]] in the [[{Sets[card.SetID].Name}]] set.\n\n";
			}
			else
			{
				mainPage += $"{{{{{tabType.ToUpper()[0]}|{card.Name}}}}} is a {card.Color} [[{tabType}]] in the [[{Sets[card.SetID].Name}]] set.\n\n";
			}

			if(card.CardType == ArtifactCardType.Hero)
			{

			}
			else
			{
				mainPage += "== Card Text ==\n";
				mainPage += $"{card.TextFormatted}\n\n";

				mainPage += "== Miscellaneous ==\n";
				mainPage += $"* [{card.CardImageRaw} | Illustrated ] by [[{card.Illustrator}]].\n\n";

				mainPage += $"[[Category:{tabType}]] [[Category:{card.Color}]] [[Category:{card.Rarity}]] [[Category:{Sets[card.SetID].Name}]]";
			}


			File.WriteAllText($"{basefile}.txt", mainPage);

			// - items no color, items need subtype in categories
			// - items have no text?? might be subsumed in abilities

			return null;
		}

	}
}
