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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
| TokenOf = {(card.TokenParents.Count > 0 ? String.Join(",", card.TokenParents.Select(x => x.ID)) : "")}
| SignatureOf = {(card.SignatureOf.HasValue ? card.SignatureOf.ToString() : "")}
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

		public static string GetStinger(string template, WikiCard card, string setname)
		{
			string stinger = $"{{{{{template}|{card.Name}}}}} is a{(card.Color == ArtifactColor.None ? " " : $" {card.Color.ToString()} ")}[[{card.CardType}]] in the [[{setname}]] set.";
			if(card.SignatureParent != null)
			{
				stinger += $"  It is the [[Signature Card]] of [[{card.SignatureParent.Name}]].";
			}
			else if (card.TokenParents.Count > 1)
			{
				var spellParents = card.TokenParents.Where(x => x.SubCard is WikiSpell);
				var abilityParents = card.TokenParents.Where(x => x.SubCard is WikiAbility).Select(x => (x.SubCard as WikiAbility).Parent);

				if (spellParents.Count() > 0 && abilityParents.Count() > 0)
				{
					string spellList = "";
					if (spellParents.Count() > 1)
					{
						spellList = String.Join(", ", spellParents.Select(x => $"[[{x.Name}]]").ToArray(), 0, spellParents.Count() - 1);
						spellList += $" or ";
					}
					spellList += $"[[{spellParents.Last().Name}]]";

					string abilityList = "";
					if (abilityParents.Count() > 1)
					{
						abilityList = String.Join(", ", abilityParents.Select(x => $"[[{x.Name}]]").ToArray(), 0, abilityParents.Count() - 1);
						abilityList += $" or ";
					}
					abilityList += $"[[{abilityParents.Last().Name}]]";

					stinger += $"  It is [[Summon|summoned]] when {spellList} is played, or when the ability of {abilityList} is activated.";
				}
				else if (spellParents.Count() > 0 && abilityParents.Count() == 0)
				{
					string list = String.Join(", ", spellParents.Select(x => $"[[{x.Name}]]"), 0, spellParents.Count() - 1);
					list += $" or {spellParents.Last().Name}";
					stinger += $"  It is [[Summon|summoned]] when {list} is played.";
				}
				if (spellParents.Count() == 0 && abilityParents.Count() > 0)
				{
					string list = String.Join(", ", abilityParents.Select(x => $"[[{x.Name}]]"), 0, abilityParents.Count() - 1);
					list += $" or {abilityParents.Last().Name}";
					stinger += $"  It is [[Summon|summoned]] when the ability of {list} is activated.";
				}
			}
			else if (card.TokenParents.Count > 0)
			{
				var parent = card.TokenParents.First();
				if (parent.SubCard is WikiSpell spell)
				{
					stinger += $"  It is [[Summon|summoned]] when [[{parent.Name}]] is played.";
				}
				else
				{
					var ability = parent.SubCard as WikiAbility;
					if(ability.Name == ability.Parent.Name)
					{
						stinger += $"  It is [[Summon|summoned]] when the ability of [[{ability.Parent.Name}]] is activated.";
					}
					else
					{
						stinger += $"  It is [[Summon|summoned]] when the [[{ability.Name}]] ability of [[{ability.Parent.Name}]] is activated.";
					}
					
				}
				
			}
			return stinger;
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
| Charges = {ability.Charges}
| Cooldown = {ability.Cooldown}}}}}
";
			}
			return result;
		}

		public static string GetIllustrator(WikiCard card)
		{
			return $"* [[Media:{card.CardImageRaw}|Illustrated]] by [[{card.Illustrator}]].";
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
			if(Card.LoreFormatted == null)
			{
				article.AddSection("Artifact", $"This card does not have lore.");
			}
			else
			{
				string[] loreChunks = Card.LoreFormatted.Split("—");
				article.AddSection("Artifact", $@"{{{{Quote
| text = {loreChunks[0].Trim()}
| source = {loreChunks[1].Trim()}}}}}");
			}
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories.Add("Lore");
		}

		protected override string Finalize(WikiArticle article, string result)
		{
			result = result.Replace("\r", "");
			return Regex.Replace(result, @"\n\n+", "\n\n");
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
			result = result.Replace("\r", "");
			if (Card.VoiceOverLines.Count == 0)
			{
				return GetEmptyResponseArticle(Card);
			}

			return Regex.Replace(result, @"\n\n+", "\n\n");
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
| Abilities = {String.Join(",", Card.Abilities.Keys.Select(x => x.ToString()))}
| HeroIcon = {SubCard.HeroIcon}
| HeroIconRaw = {SubCard.HeroIconRaw}}}}}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			article.CardStinger = GetStinger(TypeTemplate, Card, Sets[Card.SetID].Name);
		}

		protected override void AddSections(WikiArticle article)
		{
			if (Card.Abilities.Count > 0)
			{
				article.AddSection("Ability", GetAbilityInfoboxes(Card.Abilities.Values));

				if (Card.Abilities.Any(x => x.Value.CardSpawned != null))
				{
					var cardSpawned = Card.Abilities.Where(x => x.Value.CardSpawned != null).Select(x => x.Value.CardSpawned).First();
					article.AddSection("Card Spawned", GetCardReference(cardSpawned));
				}
			}
			else
			{
				article.AddSection("Ability", $"{{{{{TypeTemplate}|{Card.Name}}}}} has no abilities.");
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
				"Heroes",
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
| Abilities = {String.Join(",", Card.Abilities.Keys.Select(x => x.ToString()))}}}}}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			article.CardStinger = GetStinger(TypeTemplate, Card, Sets[Card.SetID].Name);
		}

		protected override void AddSections(WikiArticle article)
		{
			if(!String.IsNullOrWhiteSpace(Card.TextFormatted))
			{
				article.AddSection("Card Text", Card.TextFormatted);
			}

			if(Card.Abilities.Count > 0)
			{
				article.AddSection("Ability", GetAbilityInfoboxes(Card.Abilities.Values));

				if (Card.Abilities.Any(x => x.Value.CardSpawned != null))
				{
					var cardSpawned = Card.Abilities.Where(x => x.Value.CardSpawned != null).Select(x => x.Value.CardSpawned).First();
					article.AddSection("Card Spawned", GetCardReference(cardSpawned));
				}
			}

			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				"Creeps",
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
| CardSpawned = {SubCard.CardSpawned?.ID}
| ManaCost = {SubCard.ManaCost}
| Charges = {SubCard.Charges}
| IsCrosslane = {SubCard.IsCrosslane}}}}}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			article.CardStinger = GetStinger(TypeTemplate, Card, Sets[Card.SetID].Name);
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
				"Improvements",
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
| CardSpawned = {SubCard.CardSpawned?.ID}
| ManaCost = {SubCard.ManaCost}
| Charges = {SubCard.Charges}
| IsCrosslane = {SubCard.IsCrosslane}}}}}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			article.CardStinger = GetStinger(TypeTemplate, Card, Sets[Card.SetID].Name);
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
				"Spells",
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
		private static Dictionary<ArtifactSubType, string> CategoryMapping = new Dictionary<ArtifactSubType, string>()
		{
			{ ArtifactSubType.Accessory, "Accessories" },
			{ ArtifactSubType.Armor, "Armor" },
			{ ArtifactSubType.Consumable, "Consumables" },
			{ ArtifactSubType.Deed, "Deeds" },
			{ ArtifactSubType.Weapon, "Weapons" },
		};
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
| Abilities = {String.Join(",", Card.Abilities.Keys.Select(x => x.ToString()))}}}}}";
		}

		protected override void AddCardStinger(WikiArticle article)
		{
			article.CardStinger = $"{{{{{TypeTemplate}|{Card.Name}}}}} is an [[Item]] in the [[{Sets[Card.SetID].Name}]] set.";
		}

		protected override void AddSections(WikiArticle article)
		{
			if (!String.IsNullOrWhiteSpace(Card.TextFormatted))
			{
				article.AddSection("Card Text", Card.TextFormatted);
			}

			if (Card.Abilities.Count > 0)
			{
				article.AddSection("Ability", GetAbilityInfoboxes(Card.Abilities.Values));

				if(Card.Abilities.Any(x => x.Value.CardSpawned != null))
				{
					var cardSpawned = Card.Abilities.Where(x => x.Value.CardSpawned != null).Select(x => x.Value.CardSpawned).First();
					article.AddSection("Card Spawned", GetCardReference(cardSpawned));
				}
			}

			article.AddSection("Miscellaneous", GetIllustrator(Card));
		}

		protected override void AddCategories(WikiArticle article)
		{
			article.Categories = new List<string>()
			{
				"Items",
				CategoryMapping[Card.SubType],
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
