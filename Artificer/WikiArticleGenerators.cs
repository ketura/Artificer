
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
using System.Linq;
using System.Text;

using static Artificer.SharedArticleFunctions;

namespace Artificer
{
	public static class SharedArticleFunctions
	{
		public static string GetTabTemplate(ArtifactCardType type)
		{
			return $"{{{{Tabs/{type.ToString()}}}}}";
		}

		public static string GetCardInfobox(WikiCard card)
		{
			return $@"{{{{Card Infobox
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
| Keywords = {String.Join(",", card.Keywords.Keys)}}}}}
";
		}

		public static string GetStinger(string template, string name, ArtifactColor color, ArtifactCardType type, string setname)
		{
			return $"{{{{{template}|{name}}}}} is a {(color == ArtifactColor.None ? " " : color.ToString())}[[{type}]] in the [[{setname}]] set.\n\n";
		}

		public static string GetAbilityInfoboxes(IEnumerable<WikiAbility> abilities)
		{
			string result = "";
			foreach(var ability in abilities)
			{
				result += $@"{{{{Card Infobox
| ID = {ability.ID}
| Name = {ability.Name}
| CardID = {ability.ParentID}
| AbilityType = {ability.AbilityType}
| Charges = {ability.Charges}}}}}
";
			}
			return result;
		}

		public static string GetIllustrator(WikiCard card)
		{
			return $"* [[{card.CardImageRaw}|Illustrated]] by [[{card.Illustrator}]].";
		}

		public static string GetCardReference(WikiCard card)
		{
			return $"{{{{Card/{card.CardType}|{card.Name}}}}}";
		}
	}


	public class LoreArticleGenerator : WikiArticleGenerator
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article) { }
		protected override void AddSubcardInfobox(WikiArticle article) { }
		protected override void AddCardStinger(WikiArticle article) { }

		protected override void AddSections(WikiArticle article)
		{
			string[] loreChunks = Card.LoreFormatted.Split("—");
			article.AddSection("Artifact", $@"{{{{Quote
| text = {loreChunks[0].Trim()}
| source = [[{loreChunks[1].Trim()}]]}}}}");
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories.Add("Lore");
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			return base.Finalize(article, result);
		}

		public LoreArticleGenerator(WikiCard card) : base(card, card.CardType) { }
	}

	public class ResponseArticleGenerator : WikiArticleGenerator
	{
		protected static string GetEmptyResponseArticle(WikiCard card)
		{
			return $"{GetTabTemplate(card.CardType)}\n'Card has no responses.'\n[Category:Responses]";
		}
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article) { }
		protected override void AddSubcardInfobox(WikiArticle article) { }
		protected override void AddCardStinger(WikiArticle article) { }

		protected override void AddSections(WikiArticle article)
		{
			string combined = "";
			foreach(var pair in Card.VoiceOverFiles)
			{
				combined += $"* [[{pair.Value}|File]]: {Card.VoiceOverLines[pair.Key]}\n";
			}
			article.AddSection("Uncategorized", combined);
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories.Add("Responses");
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			if(Card.VoiceOverLines.Count == 0)
			{
				return GetEmptyResponseArticle(Card);
			}

			return result;
		}

		public ResponseArticleGenerator(WikiCard card) : base(card, card.CardType) { }
	}

	public class HeroArticleGenerator : WikiArticleGenerator<WikiHero>
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article)
		{
			article.CardInfobox = GetCardInfobox(Card);
		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{
			article.SubcardInfobox = $@"{{{{Hero Infobox
| ID = {Card.ID}
| Name = {Card.Name}
| Attack = {SubCard.Attack}
| Armor = {SubCard.Armor}
| Health = {SubCard.Health}
| SignatureCardID = {SubCard.SignatureCardID}
| Abilities = {String.Join(",", SubCard.Abilities.Keys.Select(x => x.ToString()))}
| HeroIcon = {SubCard.HeroIcon}
| HeroIconRaw = {SubCard.HeroIconRaw}}}}}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			GetStinger(TypeTemplate, Card.Name, Card.Color, Card.CardType, Sets[Card.SetID].Name);
		}

		protected override void AddSections(WikiArticle article)
		{
			if (SubCard.Abilities.Count > 0)
			{
				article.AddSection("Ability", GetAbilityInfoboxes(SubCard.Abilities.Values));

				if (SubCard.Abilities.Any(x => x.Value.CardSpawned != null))
				{
					var cardSpawned = SubCard.Abilities.Where(x => x.Value.CardSpawned != null).Select(x => x.Value.CardSpawned).First();
					article.AddSection("Card Spawned", GetCardReference(cardSpawned));
				}
			}
			if (SubCard.SignatureCard != null)
			{
				article.AddSection("Signature Card", GetCardReference(SubCard.SignatureCard));
			}
			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				Card.CardType.ToString(),
				Card.Color.ToString(),
				Card.Rarity.ToString(),
				Sets[Card.SetID].Name
			};
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			return base.Finalize(article, result);
		}

		public HeroArticleGenerator(WikiCard card) : base(card, ArtifactCardType.Hero, "H") { }
	}

	public class CreepArticleGenerator : WikiArticleGenerator<WikiCreep>
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article)
		{
			article.CardInfobox = GetCardInfobox(Card);
		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{
			article.SubcardInfobox += $@"{{{{Creep Infobox
| ID = {Card.ID}
| Name = {Card.Name}
| ManaCost = {SubCard.ManaCost}
| Attack = {SubCard.Attack}
| Armor = {SubCard.Armor}
| Health = {SubCard.Health}
| Abilities = {String.Join(",", SubCard.Abilities.Keys.Select(x => x.ToString()))}}}}}{"\n\n"}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			GetStinger(TypeTemplate, Card.Name, Card.Color, Card.CardType, Sets[Card.SetID].Name);
		}

		protected override void AddSections(WikiArticle article)
		{
			if(!String.IsNullOrWhiteSpace(Card.TextFormatted))
			{
				article.AddSection("Card Text", Card.TextFormatted);
			}

			if(SubCard.Abilities.Count > 0)
			{
				article.AddSection("Ability", GetAbilityInfoboxes(SubCard.Abilities.Values));

				if (SubCard.Abilities.Any(x => x.Value.CardSpawned != null))
				{
					var cardSpawned = SubCard.Abilities.Where(x => x.Value.CardSpawned != null).Select(x => x.Value.CardSpawned).First();
					article.AddSection("Card Spawned", GetCardReference(cardSpawned));
				}
			}

			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				Card.CardType.ToString(),
				Card.Color.ToString(),
				Card.Rarity.ToString(),
				Sets[Card.SetID].Name
			};
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			return base.Finalize(article, result);
		}

		public CreepArticleGenerator(WikiCard card) : base(card, ArtifactCardType.Creep, "C") { }
	}

	public class ImprovementArticleGenerator : WikiArticleGenerator<WikiSpell>
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article)
		{
			article.CardInfobox = GetCardInfobox(Card);
		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{
			article.SubcardInfobox += $@"{{{{Improvement Infobox
| ID = {Card.ID}
| Name = {Card.Name}
| CardSpawned = {SubCard.CardSpawned}
| ManaCost = {SubCard.ManaCost}
| Charges = {SubCard.Charges}
| IsCrosslane = {SubCard.IsCrosslane}}}}}\n\n";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			GetStinger(TypeTemplate, Card.Name, Card.Color, Card.CardType, Sets[Card.SetID].Name);
		}

		protected override void AddSections(WikiArticle article)
		{
			if (!String.IsNullOrWhiteSpace(Card.TextFormatted))
			{
				article.AddSection("Card Text", Card.TextFormatted);
			}
			if (SubCard.CardSpawned != null)
			{
				article.AddSection("Card Spawned", GetCardReference(SubCard.CardSpawned));
			}
			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				Card.CardType.ToString(),
				Card.Color.ToString(),
				Card.Rarity.ToString(),
				Sets[Card.SetID].Name
			};
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			return base.Finalize(article, result);
		}

		public ImprovementArticleGenerator(WikiCard card) : base(card, ArtifactCardType.Improvement, "Im") { }
	}

	public class SpellArticleGenerator : WikiArticleGenerator<WikiSpell>
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article)
		{
			article.CardInfobox = GetCardInfobox(Card);
		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{
			article.SubcardInfobox += $@"{{{{Spell Infobox
| ID = {Card.ID}
| Name = {Card.Name}
| CardSpawned = {SubCard.CardSpawned}
| ManaCost = {SubCard.ManaCost}
| Charges = {SubCard.Charges}
| IsCrosslane = {SubCard.IsCrosslane}}}}}\n\n";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			GetStinger(TypeTemplate, Card.Name, Card.Color, Card.CardType, Sets[Card.SetID].Name);
		}

		protected override void AddSections(WikiArticle article)
		{
			if (!String.IsNullOrWhiteSpace(Card.TextFormatted))
			{
				article.AddSection("Card Text", Card.TextFormatted);
			}
			if (SubCard.CardSpawned != null)
			{
				article.AddSection("Card Spawned", GetCardReference(SubCard.CardSpawned));
			}
			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				Card.CardType.ToString(),
				Card.Color.ToString(),
				Card.Rarity.ToString(),
				Sets[Card.SetID].Name
			};
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			return base.Finalize(article, result);
		}

		public SpellArticleGenerator(WikiCard card) : base(card, ArtifactCardType.Spell, "S") { }
	}

	public class ItemArticleGenerator : WikiArticleGenerator<WikiItem>
	{
		protected override void AddTabTemplate(WikiArticle article)
		{
			article.TabTemplate = GetTabTemplate(Card.CardType);
		}

		protected override void AddCardInfobox(WikiArticle article)
		{
			article.CardInfobox = GetCardInfobox(Card);
		}

		protected override void AddSubcardInfobox(WikiArticle article)
		{
			article.SubcardInfobox += $@"{{{{Item Infobox
| ID = {Card.ID}
| Name = {Card.Name}
| GoldCost = {SubCard.GoldCost}
| Abilities = {String.Join(",", SubCard.Abilities.Keys.Select(x => x.ToString()))}}}}}{"\n\n"}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			article.CardStinger = $"{{{{{TypeTemplate}|{Card.Name}}}}} is an [[Item]] in the [[{Sets[Card.SetID].Name}]] set.\n\n";
		}

		protected override void AddSections(WikiArticle article)
		{
			if (!String.IsNullOrWhiteSpace(Card.TextFormatted))
			{
				article.AddSection("Card Text", Card.TextFormatted);
			}

			if (SubCard.Abilities.Count > 0)
			{
				article.AddSection("Ability", GetAbilityInfoboxes(SubCard.Abilities.Values));

				if(SubCard.Abilities.Any(x => x.Value.CardSpawned != null))
				{
					var cardSpawned = SubCard.Abilities.Where(x => x.Value.CardSpawned != null).Select(x => x.Value.CardSpawned).First();
					article.AddSection("Card Spawned", GetCardReference(cardSpawned));
				}
			}

			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				Card.CardType.ToString(),
				Card.Color.ToString(),
				Card.Rarity.ToString(),
				Sets[Card.SetID].Name
			};
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			return base.Finalize(article, result);
		}

		public ItemArticleGenerator(WikiCard card) : base(card, ArtifactCardType.Item, "I") { }
	}

}
