
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Artificer
{
	public class CardTextCollection
	{
		public Dictionary<int, CardText> Text { get; set; }
		public Dictionary<int, CardLore> Lore { get; set; }
		public Dictionary<int, List<CardVoiceOver>> Voiceover { get; set; }
		public Dictionary<int, string> SimpleMapping { get; set; }

		public CardTextCollection()
		{
			Text = new Dictionary<int, CardText>();
			Lore = new Dictionary<int, CardLore>();
			Voiceover = new Dictionary<int, List<CardVoiceOver>>();
			SimpleMapping = new Dictionary<int, string>();
		}

		public void ParseCardSet(string text)
		{
			int nameCount = Regex.Matches(text, "\"CardName").Count;
			int textCount = Regex.Matches(text, "CardText").Count;
			int effectCount = Regex.Matches(text, "CardEffect").Count;
			int modCount = Regex.Matches(text, "CardModification").Count;

			Regex cardRegex = new Regex(@"""Card(.*?)_(\d+)(_\d+)?""\s+""(.*)""");
			var result = cardRegex.Matches(text);

			Console.WriteLine($"\tCardName count: {nameCount}\n\tCardText count: {textCount}\n\tCardEffect count: {effectCount}\n\tCardModification count: {modCount}");
			Console.WriteLine($"Total: {nameCount + textCount + effectCount + modCount}; found {result.Count} full matches");

			foreach (Match match in result)
			{
				int id = int.Parse(match.Groups[2].Value);
				CardText cardText = Text.GetValueOrDefault(id, null);
				if(cardText == null)
				{
					cardText = new CardText()
					{
						ID = id
					};

					Text[id] = cardText;
				}

				switch (match.Groups[1].Value)
				{
					case "Name":
						cardText.Name = match.Groups[4].Value;
						break;

					case "Text":
						cardText.RawText = match.Groups[4].Value;
						break;

					case "Effect":
						cardText.Effect = match.Groups[4].Value;
						cardText.Suffix = match.Groups[3].Value;
						break;

					case "Modification":
						cardText.Modification = match.Groups[4].Value;
						break;

					default:
						throw new FormatException($"Unrecognized card tag \"{match.Groups[1].Value}\"! Please inform the maintainer to maintain his shit.");
						break;
				}

			}
		}

		public void ParseLoreSet(string text)
		{
			int loreCount = Regex.Matches(text, "CardLore").Count;

			Regex loreRegex = new Regex(@"""CardLore_(\d+)""\s+""(.*)""");
			var result = loreRegex.Matches(text);

			Console.WriteLine($"Lore count of {loreCount}; found {result.Count} full matches.");

			foreach(Match match in result)
			{
				CardLore lore = new CardLore
				{
					ID = int.Parse(match.Groups[1].Value),
					RawText = match.Groups[2].Value
				};

				Lore[lore.ID] = lore;
			}
		}

		public void ParseVOSet(string text, Dictionary<string, int> VOMapping)
		{
			int missingID = 0;
			int voCount = Regex.Matches(text, "DCG_VO").Count;

			Regex voRegex = new Regex(@"""DCG_VO_(.*?)""\s+""(.*?)""");
			var result = voRegex.Matches(text);

			Console.WriteLine($"VO count of {voCount}; found {result.Count} full matches.");

			foreach (Match match in result)
			{
				string fullname = match.Groups[1].Value;
				string name = VOMapping.Keys.Where(x => fullname.StartsWith(x)).FirstOrDefault();
				int id = 0;
				if(name == null)
				{
					Console.WriteLine($"{fullname} not found in the VO mapping!");
					id = --missingID;
				}
				else
				{
					id = VOMapping[name];
				}

				CardVoiceOver vo = new CardVoiceOver()
				{
					ID = id,
					Name = fullname,
					CharacterName = name,
					ResponseTrigger = fullname.Replace($"{name}_", ""),
					RawText = match.Groups[2].Value
				};

				if(!Voiceover.ContainsKey(id))
				{
					Voiceover[id] = new List<CardVoiceOver>();
				}

				Voiceover[id].Add(vo);
			}
		}

		public (CardText, CardLore, List<CardVoiceOver>) GetGameFileData(int id)
		{
			return (Text.GetValueOrDefault(id), Lore.GetValueOrDefault(id), Voiceover.GetValueOrDefault(id, new List<CardVoiceOver>()));
		}
	}

	public class CardText
	{
		public string Name { get; set; }
		public string Suffix { get; set; }
		public int ID { get; set; }
		public string Effect { get; set; }
		public string Modification { get; set; }
		public string RawText { get; set; }
		public string Text { get; set; }
		public string WikiText { get; set; }
	}

	public class CardLore
	{
		public int ID { get; set; }
		public string RawText { get; set; }
		public string Text { get; set; }
		public string WikiText { get; set; }
	}

	public class CardVoiceOver
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string CharacterName { get; set; }
		public string ResponseTrigger { get; set; }
		public string RawText { get; set; }
		public string Text { get; set; }
		public string WikiText { get; set; }
	}

}
