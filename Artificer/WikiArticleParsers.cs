
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

namespace Artificer
{
	public abstract class ExistingArticleParser : WikiArticleGenerator
	{
		public string ArticleName { get; protected set; }
		public string OriginalArticle { get; protected set; }
		public string CurrentArticle { get; protected set; }

		public ExistingArticleParser(WikiCard card, string articleName, string article) : base(card, card.CardType)
		{
			ArticleName = articleName;
			OriginalArticle = article.Replace("\r", "");
			CurrentArticle = article.Replace("\r", "");
		}
	}

	public class ExistingLoreArticleParser : ExistingArticleParser
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, @"\{\{Tabs.*\n+");
			if (match.Success)
			{
				article.TabTemplate = match.Value.Trim();
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"\{\{unreleased content\}\}\n+");
			if (match.Success)
			{
				article.TabTemplate += $"\n{match.Value.Trim()}";
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
		}

		protected override void AddCardInfobox(WikiArticle article)
		{

		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{

		}

		protected override void AddCardStinger(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, $@"(?s).*?{Card.Name}\}}?\}}? (is|are) .*?\n+(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.CardStinger = match.Value.Trim();
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
		}

		protected override void AddSections(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, @"(?s)==+\s*Artifact\s*==+\s*\n+((\{\{.*?\}\}\n*)+)");
			if (match.Success)
			{
				article.AddSection("Artifact", match.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
			else
			{
				match = Regex.Match(CurrentArticle, @"(?s)==+\s*Artifact\s*==+\s*\n*");
				if (match.Success)
				{
					article.AddSection("Artifact", "");
					CurrentArticle = CurrentArticle.Replace(match.Value, "");
				}
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*Dota 2\s*==+\s*\n+(\{\{.*?\}\})");
			if (match.Success)
			{
				article.AddSection("Dota 2", match.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*Related Lore\s*==+\s*\n+((\{\{.*?\}\}\n*)+)");
			if (match.Success)
			{
				article.AddSection("Related Lore", match.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
		}

		protected override void AddCategories(WikiArticle article)
		{

		}

		public ExistingLoreArticleParser(WikiCard card, string articleName, string article) : base(card, articleName, article) { }
	}

	public class ExistingResponseArticleParser : ExistingArticleParser
	{
		protected override void AddTabTemplate(WikiArticle article)
		{

		}

		protected override void AddCardInfobox(WikiArticle article)
		{

		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{

		}

		protected override void AddCardStinger(WikiArticle article)
		{

		}

		protected override void AddSections(WikiArticle article)
		{

		}

		protected override void AddCategories(WikiArticle article)
		{

		}

		public ExistingResponseArticleParser(WikiCard card, string articleName, string article) : base(card, articleName, article) { }
	}

	public class OldExistingArticleParser : ExistingArticleParser
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, @"\{\{Tabs.*\n+");
			if (match.Success)
			{
				article.TabTemplate = match.Value.Trim();
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"\{\{unreleased content\}\}\n+");
			if (match.Success)
			{
				article.TabTemplate += $"\n{match.Value.Trim()}";
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
		}

		protected override void AddCardInfobox(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, @"^(?s)\s*\{\{\w+ Infobox.*?\}\}\n+");
			if (!match.Success)
				return;

			article.CardInfobox = match.Value.Trim();
			CurrentArticle = CurrentArticle.Replace(match.Value, "");
		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{

		}

		protected override void AddCardStinger(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, $@"(?s)\{{?\{{?\w+\|{Card.Name}\}}?\}}? (is|are) .*?\n+(?=(\[\[Category|==))");
			if (!match.Success)
				return;

			article.CardStinger = match.Value.Trim();
			CurrentArticle = CurrentArticle.Replace(match.Value, "");
		}

		protected override void AddSections(WikiArticle article)
		{
			var match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Ability|Abilities|Active)\s*==+\s*\n+(((\{\{Ability Infobox.*?\}\})\n+)+)");
			if (match.Success)
			{
				article.AddSection("Ability", match.Groups[2].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
			else
			{
				match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Ability|Abilities|Active)\s*==+\s*\n+((.*?no.*?(abilities|ability|active).*?)|N/A)\n+");
				if(match.Success)
				{
					article.AddSection("Ability", match.Groups[2].Value);
					CurrentArticle = CurrentArticle.Replace(match.Value, "");
				}
				else
				{
					match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Ability|Abilities|Active)\s*==+\n(?=(\[\[Category|==))");
					if (match.Success)
					{
						article.AddSection("Ability", match.Groups[2].Value);
						CurrentArticle = CurrentArticle.Replace(match.Value, "");
					}
				}
				
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Description|Effect|Effects|Play effect|Play Effect|Active|Ability)\s*==+\s*\n+(.*?)\n(\s*)?(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.AddSection("Card Text", match.Groups[2].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Premier Card|Signature Card|Spell|Card)\s*==+\s*\n+(\{\{(Card|Deck).*?\}\})\n+");
			if (match.Success)
			{
				article.AddSection("Signature Card", match.Groups[2].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*Notes\s*==+\s*\n+(.*?)\n(\s+)?(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.AddSection("Notes", match.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Related Cards|Related cards)\s*==+\s*\n+(.*?)\n(\s+)?(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.AddSection("Related Cards", match.Groups[2].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*Changelog\s*==+\s*\n+(.*?)\n(\s+)?(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.AddSection("Changelog", match.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*Trivia\s*==+\s*\n+(.*?)\n(\s+)?(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.AddSection("Trivia", match.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}

			match = Regex.Match(CurrentArticle, @"(?s)==+\s*(Misc|Miscellaneous)\s*==+\s*\n+(.*?)\n(\s+)?(?=(\[\[Category|==))");
			if (match.Success)
			{
				article.AddSection("Miscellaneous", match.Groups[2].Value);
				CurrentArticle = CurrentArticle.Replace(match.Value, "");
			}
		}

		protected override void AddCategories(WikiArticle article)
		{
			var matches = Regex.Matches(CurrentArticle, @"\[\[Category:(.*?)]]");
			foreach(Match cat in matches)
			{
				article.Categories.Add(cat.Groups[1].Value);
				CurrentArticle = CurrentArticle.Replace(cat.Value, "");
			}

		}

		public OldExistingArticleParser(WikiCard card, string articleName, string article) : base(card, articleName, article) { }
	}

}
