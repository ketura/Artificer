
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
	public class WikiCard
	{
		// Id of the card used for matching it with signature/related cards. Currently the Id is just a random number as we don't know the card collection number.
		public int Id { get; set; }

		// IDs of related cards such as signature spells and tokens.
		public List<string> RelatedCards { get; set; }

		// Card name
		public string Name { get; set; }

		// Hero/Creep/Improvement/Spell/Item
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactCardType CardType { get; set; }

		// Consumable/Weapon/Armor/Accessory
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactItemType ItemType { get; set; }

		// Black/Blue/Green/Red/Yellow
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactColor Color { get; set; }

		// Basic/Common/Uncommon/Rare
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactRarity Rarity { get; set; }

		//The raw card text e.g. Active 1: Do something.
		public string Text { get; set; }

		// Attack
		public int Attack { get; set; }

		// Armor
		public int Armor { get; set; }

		// Health
		public int Health { get; set; }

		// The name of the card that is this card's signature card.
		public string SignatureCard { get; set; }

		// If true this card is a signature card for a hero. Use the RelatedCards to get the hero.
		public bool IsSignatureCard { get; set; }

		// How many charges a card has for its effect.
		public int Charges { get; set; }

		// Cost of buying an item.
		public int GoldCost { get; set; }

		// An array of all abilities/effects for the Hero/Creep/Improvement/Item card. For creeps and improvements their Text has been parsed into an ability so it is easier to search for abilities.
		public List<string> Abilities { get; set; }

		// Mana cost for card.
		public int ManaCost { get; set; }

		// If true this card gives player initiative. If null/false it cannot.
		public bool GetInitiative { get; set; }

		// If true this card can be cast across lanes. If null/false it cannot.
		public bool CrossLane { get; set; }

		// If true this card is a token created by another card.
		public bool Token { get; set; }

		// The name that assets files will use for this card. Just provide a path to what type of asset you want and the file extension.
		public string FileName { get; set; }

		// Artist name
		public string Artist { get; set; }

		// Lore description for the card.
		public string Lore { get; set; }
	}
}
