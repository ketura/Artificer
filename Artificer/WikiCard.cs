
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
		public Dictionary<string, string> VoiceOverLines { get; set; }
		public Dictionary<string, string> VoiceOverFiles { get; set; }

		public WikiCard()
		{
			VoiceOverLines = new Dictionary<string, string>();
			VoiceOverFiles = new Dictionary<string, string>();
		}

		public WikiCard(ValveCard card) : this()
		{

		}
	}

	public class WikiAbility
	{
		public int ID { get; set; }
		// Name of the ability. For improvements/creeps the ability will be the name of the card + " : Effect" e.g. "Keenfolk Turret : Effect."
		public string Name { get; set; }
		public int CardID { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactAbilityType AbilityType { get; set; }
		public int Charges { get; set; }

		public WikiAbility(ValveCard card)
		{

		}
	}

	public class WikiCreep
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int ManaCost { get; set; }
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Health { get; set; }
		public List<int> Abilities { get; set; }

		public WikiCreep()
		{
			Abilities = new List<int>();
		}

		public WikiCreep(ValveCard creep) : this()
		{
			
		}
	}

	public class WikiHero
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Health { get; set; }
		public int SignatureCardID { get; set; }
		public List<int> Abilities { get; set; }
		public string HeroIcon { get; set; }
		public string HeroIconRaw { get; set; }

		public WikiHero()
		{
			Abilities = new List<int>();
		}

		public WikiHero(ValveCard hero) : this()
		{
			
		}
	}

	public class WikiSpell
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int CardSpawned { get; set; }
		public int ManaCost { get; set; }
		public int Charges { get; set; }
		public bool GrantsInitiative { get; set; }
		public bool IsCrosslane { get; set; }

		public WikiSpell(ValveCard spell)
		{

		}
	}
}
