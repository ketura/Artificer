
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

namespace Artificer
{
	public class WikiArticle
	{
		public string TabTemplate { get; set; }
		public string CardInfobox { get; set; }
		public string SubcardInfobox { get; set; }
		public string CardStinger { get; set; }
		public List<KeyValuePair<string, string>> Sections { get; set; }
		public List<string> Categories { get; set; }

		public void AddSection(string header, string body)
		{
			Sections.Add(new KeyValuePair<string, string>(header, body));
		}

		public string GenerateNewArticle()
		{
			return $@"{TabTemplate}
{CardInfobox}
{SubcardInfobox}

{CardStinger}

{GenerateSections(Sections)}

{GenerateCategories(Categories)}
";
		}

		protected static string GenerateSection(string header, string body)
		{
			return $"== {header} ==\n{body}\n\n";
		}

		protected static string GenerateSections(IEnumerable<KeyValuePair<string, string>> sections)
		{
			string result = "";
			foreach(var pair in sections)
			{
				result += GenerateSection(pair.Key, pair.Value);
			}

			return result;
		}

		protected static string GenerateCategories(IEnumerable<string> cats)
		{
			string result = "";
			foreach(string cat in cats)
			{
				result += $"[[Category:{cat}]] ";
			}

			return result;
		}

		public WikiArticle()
		{
			Sections = new List<KeyValuePair<string, string>>();
			Categories = new List<string>();
		}
	}

	public abstract class WikiArticleGenerator
	{
		public static Dictionary<int, WikiSet> Sets { get; set; }

		protected WikiCard Card { get; set; }
		protected ArtifactCardType CardType { get; set; }

		protected abstract void AddTabTemplate(WikiArticle article);
		protected abstract void AddCardInfobox(WikiArticle article);
		protected abstract void AddSubcardInfobox(WikiArticle article);
		protected abstract void AddCardStinger(WikiArticle article);
		protected abstract void AddSections(WikiArticle article);
		protected abstract void AddCategories(WikiArticle article);
		protected virtual string Finalize(WikiArticle article, string result) { return result.Replace("\r", ""); }

		public string GenerateArticleText()
		{
			WikiArticle article = new WikiArticle();
			AddTabTemplate(article);
			AddCardInfobox(article);
			AddSubcardInfobox(article);
			AddCardStinger(article);
			AddSections(article);
			AddCategories(article);
			string result = article.GenerateNewArticle();
			result = Finalize(article, result);
			return result;
		}

		protected WikiArticleGenerator(WikiCard card, ArtifactCardType type)
		{
			Card = card;
			CardType = type;
		}
	}

	

	public abstract class WikiArticleGenerator<T> : WikiArticleGenerator
		where T : WikiSubCard
	{
		protected T SubCard { get; set; }
		protected readonly string TypeTemplate;

		protected WikiArticleGenerator(WikiCard card, ArtifactCardType type, string template) : base(card, type)
		{
			TypeTemplate = template;

			if(card.SubCard is T t)
			{
				SubCard = t;
			}
			else
			{
				throw new ArgumentException($"Card {card.Name} is of subtype {card.SubType}, but this generator expects type {CardType}.");
			}
		}
	}
}
