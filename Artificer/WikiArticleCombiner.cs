
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
using System.Text.RegularExpressions;
using System.Linq;

namespace Artificer
{
	public abstract class WikiArticleCombiner
	{
		public WikiCard Card { get; private set; }

		protected abstract void CombineTabTemplate(WikiArticle parsed, WikiArticle generated, WikiArticle combined);
		protected abstract void CombineCardInfobox(WikiArticle parsed, WikiArticle generated, WikiArticle combined);
		protected abstract void CombineSubcardInfobox(WikiArticle parsed, WikiArticle generated, WikiArticle combined);
		protected abstract void CombineCardStinger(WikiArticle parsed, WikiArticle generated, WikiArticle combined);
		protected abstract void CombineSections(WikiArticle parsed, WikiArticle generated, WikiArticle combined);
		protected abstract void CombineCategories(WikiArticle parsed, WikiArticle generated, WikiArticle combined);

		public WikiArticle CombineArticles(WikiArticle parsed, WikiArticle generated)
		{
			if (parsed == null)
				return generated;

			WikiArticle combined = new WikiArticle();
			CombineTabTemplate(parsed, generated, combined);
			CombineCardInfobox(parsed, generated, combined);
			CombineSubcardInfobox(parsed, generated, combined);
			CombineCardStinger(parsed, generated, combined);
			CombineSections(parsed, generated, combined);
			CombineCategories(parsed, generated, combined);
			return combined;
		}

		public WikiArticleCombiner(WikiCard card)
		{
			Card = card;
		}
	}

	public class OldArticleCombiner : WikiArticleCombiner
	{
		public static readonly Dictionary<string, string> SectionMapping = new Dictionary<string, string>()
		{
			{ "Card Text", "Card Text" },
			{ "Ability", "Ability" },
			{ "Signature Card", "Signature Card" },
			{ "Summoned By", "Summoned By" },
			{ "Card Summoned", "Card Summoned" },
			{ "Notes", "Notes" },
			{ "Strategy", "Strategy" },
			{ "Changelog", "Changelog" },
			{ "Miscellaneous", "Miscellaneous" },

			{ "Related Cards", "" },
			{ "Trivia", "Miscellaneous" },
			{ "Misc", "Miscellaneous" }
		};

		public static readonly List<string> MandatorySections = new List<string>()
		{
			"Notes",
			"Miscellaneous"
		};

		public static readonly List<string> SkippedSections = new List<string>()
		{
			"Card Text",
			"Ability",
			"Signature Card"
		};

		protected static Dictionary<string, string> GetFreshSections()
		{
			return new Dictionary<string, string>()
			{
				{ "Card Text", "" },
				{ "Ability", "" },
				{ "Signature Card", "" },
				{ "Summoned By", "" },
				{ "Card Summoned", "" },
				{ "Notes", "" },
				{ "Strategy", "" },
				{ "Changelog", "" },
				{ "Miscellaneous", "" }
			};
		}




		protected override void CombineTabTemplate(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			combined.TabTemplate = "{{AutomaticallyGenerated}}\n" + generated.TabTemplate;
		}

		protected override void CombineCardInfobox(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			combined.CardInfobox = generated.CardInfobox;
		}

		protected override void CombineSubcardInfobox(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			combined.SubcardInfobox = generated.SubcardInfobox + "\n__TOC__";
		}

		protected override void CombineCardStinger(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			string parsedStinger = parsed.CardStinger ?? "";
			parsedStinger = Regex.Replace(parsedStinger, @"[^\w]+", "").ToLower();

			string generatedStinger = generated.CardStinger;
			generatedStinger = Regex.Replace(generatedStinger, @"[^\w]+", "").ToLower();

			string shortStinger = $"{Card.Name} is a {Card.Color.ToString()} {Card.CardType}".ToLower();

			if (generatedStinger.Contains(parsedStinger))
			{
				combined.CardStinger = generated.CardStinger;
			}
			else
			{
				combined.CardStinger = $"{generated.CardStinger}\n\n{parsed.CardStinger}";
			}

		}

		protected override void CombineSections(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			var sections = GetFreshSections();

			foreach (var pair in generated.Sections)
			{
				sections[pair.Key] += pair.Value + "\n";
			}

			foreach (var pair in parsed.Sections)
			{
				string key = SectionMapping[pair.Key];
				if (SkippedSections.Contains(key) || String.IsNullOrWhiteSpace(key))
					continue;

				if (key == "Miscellaneous")
				{
					foreach (string line in pair.Value.Split("\n"))
					{
						if (line.ToLower().Contains("illustrator") || line.ToLower().Contains("illustrated"))
							continue;
						if (line.ToLower().Contains("source needed"))
							continue;

						if (line.Contains("More information has been revealed on"))
						{
							sections[key] += line.Replace("More information has been revealed on", "This card was included in") + "\n";
							continue;
						}

						sections[key] += line + "\n";
					}

					sections[key] += "\n";
					continue;
				}

				sections[key] += pair.Value + "\n";
			}

			foreach (var pair in sections)
			{
				if (String.IsNullOrWhiteSpace(pair.Value) && !MandatorySections.Contains(pair.Key))
					continue;

				combined.AddSection(pair.Key, pair.Value);
			}
		}

		protected override void CombineCategories(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			foreach (string cat in generated.Categories)
			{
				combined.Categories.Add(cat);
			}

			foreach (string cat in parsed.Categories)
			{
				if (cat == "Unreleased")
					continue;

				bool found = false;
				foreach (string gcat in generated.Categories)
				{
					if (gcat.Contains(cat) || cat.Contains(gcat))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					combined.Categories.Add(cat);
				}
			}
		}


		public OldArticleCombiner(WikiCard card) : base(card) { }
	}

	public class OldLoreArticleCombiner : WikiArticleCombiner
	{
		public static readonly Dictionary<string, string> SectionMapping = new Dictionary<string, string>()
		{
			{ "Artifact", "Artifact" },
			{ "Dota 2", "Dota 2" },
			{ "Related Lore", "" },
		};

		public static readonly List<string> MandatorySections = new List<string>()
		{
			"Artifact",
		};

		public static readonly List<string> SkippedSections = new List<string>()
		{
			"Related Lore",
			"Artifact"
		};

		protected static Dictionary<string, string> GetFreshSections()
		{
			return new Dictionary<string, string>()
			{
				{ "Artifact", "" },
				{ "Dota 2", "" }
			};
		}

		protected override void CombineTabTemplate(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			//combined.TabTemplate = "{{AutomaticallyGenerated}}\n" + generated.TabTemplate;
			combined.TabTemplate = generated.TabTemplate;
		}

		protected override void CombineCardInfobox(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{

		}

		protected override void CombineSubcardInfobox(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{

		}

		protected override void CombineCardStinger(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{

		}

		protected override void CombineSections(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			var sections = GetFreshSections();

			foreach (var pair in generated.Sections)
			{
				sections[pair.Key] += pair.Value + "\n";
			}

			foreach (var pair in parsed.Sections)
			{
				string key = SectionMapping[pair.Key];
				if (SkippedSections.Contains(key) || String.IsNullOrWhiteSpace(key))
					continue;

				sections[key] += pair.Value + "\n";
			}

			foreach (var pair in sections)
			{
				if (String.IsNullOrWhiteSpace(pair.Value) && !MandatorySections.Contains(pair.Key))
					continue;

				combined.AddSection(pair.Key, pair.Value);
			}
		}

		protected override void CombineCategories(WikiArticle parsed, WikiArticle generated, WikiArticle combined)
		{
			combined.Categories = generated.Categories;
		}


		public OldLoreArticleCombiner(WikiCard card) : base(card) { }
	}
}
