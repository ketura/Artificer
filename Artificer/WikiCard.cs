
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Artificer
{
	public class WikiSet
	{
		public string Name { get; set; }
		public int ID { get; set; }
		public string MarketOffset { get; set; }
		public DateTime ReleaseDate { get; set; }

		public WikiSet(ValveSet set)
		{
			Name = set.set_info.name["english"];
			ID = set.set_info.set_id;
			MarketOffset = set.set_info.pack_item_def.ToString();
			ReleaseDate = new DateTime(2018, 11, 28);
		}
	}

	public class WikiCard
	{
		public int ID { get; set; }
		public int BaseID { get; set; }
		public int MarketplaceID { get; set; }
		public string Name { get; set; }
		public int SetID { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactCardType CardType { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactSubType SubType { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactRarity Rarity { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactColor Color { get; set; }
		
		public bool IsToken { get; set; }
		public bool IsCollectable { get; set; }
		public bool HasPulse { get; set; }

		public string Text { get; set; }
		public string TextRaw { get; set; }
		public string CardImage { get; set; }
		public string CardImageRaw { get; set; }
		public string CardIcon { get; set; }
		public string CardIconRaw { get; set; }

		public string Illustrator { get; set; }
		public string VoiceActor { get; set; }
		public string Lore { get; set; }
		public string LoreFormatted { get; set; }
		public string LoreRaw { get; set; }
		public Dictionary<string, string> VoiceOverLines { get; set; }
		public Dictionary<string, string> VoiceOverFiles { get; set; }

		public WikiSubCard SubCard { get; set; }

		public WikiCard()
		{
			VoiceOverLines = new Dictionary<string, string>();
			VoiceOverFiles = new Dictionary<string, string>();
		}

		public WikiCard(int setID, ValveCard card) : this()
		{
			ID = card.card_id;
			BaseID = card.base_card_id;
			MarketplaceID = card.item_def;
			Name = card.card_name["english"];
			SetID = setID;
			CardType = card.card_type;
			SubType = card.sub_type;
			Rarity = card.rarity;

			if (card.is_black)
				Color = ArtifactColor.Black;
			else if (card.is_blue)
				Color = ArtifactColor.Blue;
			else if (card.is_green)
				Color = ArtifactColor.Green;
			else if (card.is_red)
				Color = ArtifactColor.Red;
			else
				Color = ArtifactColor.None;

			IsToken = false;
			IsCollectable = false;
			HasPulse = false;
			Text = card.card_text.GetValueOrDefault("english");
			CardImage = GetImageName(this, "card");
			CardImageRaw = GetImageName(this, "cardraw");
			CardIcon = GetImageName(this, "icon");
			CardIconRaw = GetImageName(this, "iconraw");
			Illustrator = card.illustrator;
		}

		public static string ScrubString(string str)
		{
			return Regex.Replace(str, @"[^\w]+", "_");
		}

		public static string GetImageName(WikiCard card, string imageType, string language = "default", string extension = "jpg")
		{
			return $"{card.SetID.ToString("00")}_{ScrubString(card.Name)}_{card.ID}_{imageType}_{language}.{extension}";
		}

		public static string GetImageName(int setID, ValveCard card, string imageType, string language = "default", string extension = "jpg")
		{
			return $"{setID.ToString("00")}_{ScrubString(card.card_name["english"])}_{card.card_id}_{imageType}_{language}.{extension}";
		}

		public static WikiCard ParseAbility(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card)
			{
				SubCard = new WikiAbility(card)
			};
		}

		public static WikiCard ParseCreep(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card)
			{
				SubCard = new WikiCreep(card)
			};
		}

		public static WikiCard ParseHero(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card)
			{
				SubCard = new WikiHero(set.set_info.set_id, card)
			};
		}

		public static WikiCard ParseSpell(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card)
			{
				SubCard = new WikiSpell(card)
			};
		}

		public static WikiCard ParseItem(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card)
			{
				SubCard = new WikiItem(card)
			};
		}

		public static WikiCard ParseCard(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card);
		}
	}

	public abstract class WikiSubCard
	{
		public int ID { get; set; }
		// Name of the ability. For improvements/creeps the ability will be the name of the card + " : Effect" e.g. "Keenfolk Turret : Effect."
		public string Name { get; set; }
	}

	public class WikiAbility : WikiSubCard
	{
		
		public int CardID { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactAbilityType AbilityType { get; set; }
		public int Charges { get; set; }

		public WikiAbility(ValveCard card)
		{
			CardID = card.card_id;
			AbilityType = ArtifactAbilityType.None;
			Charges = card.charges;
		}
	}

	public class WikiCreep : WikiSubCard
	{
		public int ManaCost { get; set; }
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Health { get; set; }
		public Dictionary<int, ArtifactReferenceType> Abilities { get; set; }

		public WikiCreep()
		{
			Abilities = new Dictionary<int, ArtifactReferenceType>();
		}

		public WikiCreep(ValveCard creep) : this()
		{
			ManaCost = creep.mana_cost;
			Attack = creep.attack;
			Armor = creep.armor;
			Health = creep.hit_points;
			foreach(var reference in creep.references)
			{
				Abilities[reference.card_id] = reference.ref_type;
			}
		}
	}

	public class WikiHero : WikiSubCard
	{
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Health { get; set; }
		public int SignatureCardID { get; set; }
		public Dictionary<int, ArtifactReferenceType> Abilities { get; set; }
		public string HeroIcon { get; set; }
		public string HeroIconRaw { get; set; }

		public WikiHero()
		{
			Abilities = new Dictionary<int, ArtifactReferenceType>();
		}

		public WikiHero(int setID, ValveCard hero) : this()
		{
			Attack = hero.attack;
			Armor = hero.armor;
			Health = hero.hit_points;
			HeroIcon = WikiCard.GetImageName(setID, hero, "hero", extension:"png");
			HeroIconRaw = WikiCard.GetImageName(setID, hero, "heroraw", extension: "png");
			foreach (var reference in hero.references)
			{
				if (reference.count == 3)
				{
					SignatureCardID = reference.card_id;
				}
				else
				{
					Abilities[reference.card_id] = reference.ref_type;
				}
			}

		}
	}

	public class WikiSpell : WikiSubCard
	{
		public int CardSpawned { get; set; }
		public int ManaCost { get; set; }
		public int Charges { get; set; }
		public bool GrantsInitiative { get; set; }
		public bool IsCrosslane { get; set; }

		public WikiSpell(ValveCard spell)
		{
			CardSpawned = spell.references.Where(x => x.ref_type == ArtifactReferenceType.References).Select(x => x.card_id).FirstOrDefault();
			ManaCost = spell.mana_cost;
			Charges = spell.charges;
			GrantsInitiative = false;
			IsCrosslane = spell.is_crosslane;
		}
	}

	public class WikiItem : WikiSubCard
	{
		public int GoldCost { get; set; }
		public Dictionary<int, ArtifactReferenceType> Abilities { get; set; }
		public int Charges { get; set; }
		public bool GrantsInitiative { get; set; }
		public bool IsCrosslane { get; set; }

		public WikiItem()
		{
			Abilities = new Dictionary<int, ArtifactReferenceType>();
		}

		public WikiItem(ValveCard item) :this()
		{
			GoldCost = item.gold_cost;
			foreach (var reference in item.references)
			{
				Abilities[reference.card_id] = reference.ref_type;
			}
		}
	}
}
