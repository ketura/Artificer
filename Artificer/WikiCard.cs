
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
using System.Diagnostics;
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

	public class WikiCardReference
	{
		public int ID { get; set; }
		public WikiCard Card { get; set; }
		public int Count { get; set; }
		public ArtifactReferenceType ReferenceType { get; set; }

		public WikiCardReference(ValveCardReference refe)
		{
			ID = refe.card_id;
			Count = refe.count;
			ReferenceType = refe.ref_type;
		}
	}

	[DebuggerDisplay("WikiCard {ID}({Name}), {CardType}/{SubType}")]
	public class WikiCard
	{
		public static List<int> NonCollectableIDs { get; } = new List<int>()
		{

		};

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
		
		public List<WikiCard> TokenParents { get; set; }
		public int? SignatureOf { get; set; }
		public WikiCard SignatureParent { get; set; }
		public bool IsCollectable { get; set; }

		public string Text { get; set; }
		public string TextAPI { get; set; }
		public string TextFormatted { get; set; }
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
		public Dictionary<string, string> VoiceOverLinesRaw { get; set; }
		public Dictionary<string, string> VoiceOverFiles { get; set; }

		public Dictionary<int, WikiCardReference> References { get; set; }
		public Dictionary<int, WikiAbility> Abilities { get; set; }
		public Dictionary<ArtifactKeyword, string> Keywords { get; set; }

		public WikiSubCard SubCard { get; set; }

		public WikiCard()
		{
			VoiceOverLines = new Dictionary<string, string>();
			VoiceOverLinesRaw = new Dictionary<string, string>();
			VoiceOverFiles = new Dictionary<string, string>();
			References = new Dictionary<int, WikiCardReference>();
			Abilities = new Dictionary<int, WikiAbility>();
			Keywords = new Dictionary<ArtifactKeyword, string>();
			TokenParents = new List<WikiCard>();
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

			SignatureOf = null;
			//We manually set all signatures, tokens, and abilities to false
			IsCollectable = true;

			TextAPI = card.card_text.GetValueOrDefault("english");
			CardImage = GetImageName(this, "card");
			CardImageRaw = GetImageName(this, "cardraw");
			CardIcon = GetImageName(this, "icon");
			CardIconRaw = GetImageName(this, "iconraw");
			Illustrator = card.illustrator;

			foreach(var reference in card.references)
			{
				References[reference.card_id] = new WikiCardReference(reference);
			}

		}

		public static string ScrubString(string str)
		{
			return Regex.Replace(str, @"[^\w]+", "_");
		}

		public static string GetAudioName(WikiCard card, string audioType, string audioName, string extension="mp3")
		{
			return $"{card.SetID.ToString("00")}_{ScrubString(card.Name)}_{card.ID}_{audioType}_{ScrubString(audioName)}.{extension}";
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
			var wcard = new WikiCard(set.set_info.set_id, card);
			wcard.SubCard = new WikiAbility(card);
			return wcard;
		}

		public static WikiCard ParseCreep(ValveSet set, ValveCard card)
		{
			var wcard = new WikiCard(set.set_info.set_id, card);
			wcard.SubCard = new WikiCreep(wcard.Abilities, card);
			return wcard;
		}

		public static WikiCard ParseHero(ValveSet set, ValveCard card)
		{
			var wcard = new WikiCard(set.set_info.set_id, card);
			wcard.SubCard = new WikiHero(wcard.Abilities, set.set_info.set_id, card);
			return wcard;
		}

		public static WikiCard ParseSpell(ValveSet set, ValveCard card)
		{
			var wcard = new WikiCard(set.set_info.set_id, card);
			wcard.SubCard = new WikiSpell(wcard.Abilities, card);
			return wcard;
		}

		public static WikiCard ParseItem(ValveSet set, ValveCard card)
		{
			var wcard = new WikiCard(set.set_info.set_id, card);
			wcard.SubCard = new WikiItem(wcard.Abilities, card);
			return wcard;
		}

		public static WikiCard ParseCard(ValveSet set, ValveCard card)
		{
			return new WikiCard(set.set_info.set_id, card);
		}
	}

	public abstract class WikiSubCard
	{
		public int ID { get; set; }
		// Name of the card or ability. For improvements/creeps the ability will be the name of the card + ":Effect" e.g. "Keenfolk Turret:Effect."
		public string Name { get; set; }
		public WikiSubCard(int id, string name)
		{
			ID = id;
			Name = name;
		}
	}

	public class WikiAbility : WikiSubCard
	{
		//CardID in the db
		public int ParentID { get; set; }
		public WikiCard Parent { get; set; }
		public int CardSpawnedID { get; set; }
		public WikiCard CardSpawned { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactAbilityType AbilityType { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactPassiveAbilityType PassiveAbilityType { get; set; }
		public int Cooldown { get; set; }
		public int Charges { get; set; }

		public WikiAbility(ValveCard card) : base(card.card_id, card.card_name["english"] + (card.card_type != ArtifactCardType.Ability && card.card_type != ArtifactCardType.PassiveAbility ? ":Effect" : ""))
		{
			switch (card.card_type)
			{
				case ArtifactCardType.Ability:
					AbilityType = ArtifactAbilityType.Active;
					break;
				case ArtifactCardType.PassiveAbility:
					AbilityType = ArtifactAbilityType.Passive;
					break;
				default:
					AbilityType = ArtifactAbilityType.None;
					break;
			}

			PassiveAbilityType = ArtifactPassiveAbilityType.None;
			Charges = card.charges;
		}

		public WikiAbility(ValveCardReference card, string name) : base(card.card_id, name)
		{
			if (card.ref_type == ArtifactReferenceType.ActiveAbility)
			{
				AbilityType = ArtifactAbilityType.Active;
			}
			else
			{
				AbilityType = ArtifactAbilityType.Passive;
				PassiveAbilityType = ArtifactPassiveAbilityType.None;
			}
		}
	}

	public class WikiCreep : WikiSubCard
	{
		public int ManaCost { get; set; }
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Health { get; set; }

		public WikiCreep(Dictionary<int, WikiAbility> abilities, ValveCard creep) : base(creep.card_id, creep.card_name["english"])
		{
			ManaCost = creep.mana_cost;
			Attack = creep.attack;
			Armor = creep.armor;
			Health = creep.hit_points;
			foreach(var reference in creep.references)
			{
				abilities[reference.card_id] = new WikiAbility(reference, null);
			}
		}
	}

	public class WikiHero : WikiSubCard
	{
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Health { get; set; }
		public WikiCard SignatureCard { get; set; }
		public int SignatureCardID { get; set; }
		public string HeroIcon { get; set; }
		public string HeroIconRaw { get; set; }

		public WikiHero(Dictionary<int, WikiAbility> abilities, int setID, ValveCard hero) : base(hero.card_id, hero.card_name["english"])
		{
			Attack = hero.attack;
			Armor = hero.armor;
			Health = hero.hit_points;
			HeroIcon = WikiCard.GetImageName(setID, hero, "hero", extension:"png");
			HeroIconRaw = WikiCard.GetImageName(setID, hero, "heroraw", extension: "png");
			foreach (var reference in hero.references)
			{
				if (reference.ref_type == ArtifactReferenceType.Signature)
				{
					SignatureCardID = reference.card_id;
				}
				else
				{
					abilities[reference.card_id] = new WikiAbility(reference, null);
				}
			}

		}
	}

	public class WikiSpell : WikiSubCard
	{
		public int CardSpawnedID { get; set; }
		public WikiCard CardSpawned { get; set; }
		public int ManaCost { get; set; }
		public int Charges { get; set; }
		public bool IsCrosslane { get; set; }

		public WikiSpell(Dictionary<int, WikiAbility> abilities, ValveCard spell) : base(spell.card_id, spell.card_name["english"])
		{
			CardSpawnedID = spell.references.Where(x => x.ref_type == ArtifactReferenceType.References).Select(x => x.card_id).FirstOrDefault();
			CardSpawned = null;
			ManaCost = spell.mana_cost;
			Charges = spell.charges;
			IsCrosslane = spell.is_crosslane;
		}
	}

	public class WikiItem : WikiSubCard
	{
		public int GoldCost { get; set; }
		public int Charges { get; set; }

		public WikiItem(Dictionary<int, WikiAbility> abilities, ValveCard item) : base(item.card_id, item.card_name["english"])
		{
			GoldCost = item.gold_cost;
			foreach (var reference in item.references)
			{
				abilities[reference.card_id] = new WikiAbility(reference, null);
			}
		}
	}
}
