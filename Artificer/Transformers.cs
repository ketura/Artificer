
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
	public class Transformers
	{
		public class KeywordHunter : ICardTransformer
		{
			protected Dictionary<ArtifactKeyword, List<string>> KeywordTriggers { get; set; } = new Dictionary<ArtifactKeyword, List<string>>()
			{
				{ ArtifactKeyword.BeforeActionPhase, new List<string>() { "Before the action phase", "before the action phase" } },
				{ ArtifactKeyword.AfterCombatPhase, new List<string>() { "After the combat phase", "after the combat phase" } },
				{ ArtifactKeyword.Fountain, new List<string>() { "the Fountain" } },
				{ ArtifactKeyword.ModifyAlly, new List<string>() { "Modify allies", "Modify allied", "modify allies", "modify allied" } },
				{ ArtifactKeyword.ModifyEnemy, new List<string>() { "Modify enemy", "modify enemy" } },
				{ ArtifactKeyword.AlliedNeighbors, new List<string>() { "Allied neighbors", "allied neighbors" } },
				{ ArtifactKeyword.EnemyNeighbors, new List<string>() { "Enemy neighbors", "enemy neighbors" } },
				{ ArtifactKeyword.Purge, new List<string>() { "Purge", "purge" } },
				{ ArtifactKeyword.Taunt, new List<string>() { "Taunt", "taunt" } },
				{ ArtifactKeyword.Disarm, new List<string>() { "Disarm", "disarm" } },
				{ ArtifactKeyword.Stun, new List<string>() { "Stun", "stun" } },
				{ ArtifactKeyword.Silence, new List<string>() { "Silence", "silence" } },
				{ ArtifactKeyword.Lock, new List<string>() { "Lock", "lock" } },
				{ ArtifactKeyword.Condemn, new List<string>() { "Condemn", "condemn" } },
				{ ArtifactKeyword.Summon, new List<string>() { "Summon", "summon" } },
				{ ArtifactKeyword.Mana, new List<string>() { "Mana", "mana" } },
				{ ArtifactKeyword.Bounty, new List<string>() { "Bounty" } },
				{ ArtifactKeyword.Gold, new List<string>() { "Gold", "gold" } },
				{ ArtifactKeyword.Siege, new List<string>() { "Siege", "siege" } },
				{ ArtifactKeyword.Cleave, new List<string>() { "Cleave", "cleave" } },
				{ ArtifactKeyword.Retaliate, new List<string>() { "Retaliate", "retaliate" } },
				{ ArtifactKeyword.Attack, new List<string>() { "Attack", "attack" } },
				{ ArtifactKeyword.Armor, new List<string>() { "Armor", "armor" } },
				{ ArtifactKeyword.Health, new List<string>() { "Health", "health" } },
				{ ArtifactKeyword.Regeneration, new List<string>() { "Regeneration" } },
				{ ArtifactKeyword.DeathShield, new List<string>() { "Death Shield" } },
				{ ArtifactKeyword.DamageImmunity, new List<string>() { "Damage Immunity" } },
				{ ArtifactKeyword.Pierce, new List<string>() { "Pierce" } },
				{ ArtifactKeyword.PiercingDamage, new List<string>() { "Piercing damage", "piercing damage" } },
				{ ArtifactKeyword.Damage, new List<string>() { "Damage", "damage" } },
				{ ArtifactKeyword.Heal, new List<string>() { "Heal", "heal" } },
				{ ArtifactKeyword.Soulbound, new List<string>() { "Soulbound" } },
				{ ArtifactKeyword.LethalToCreep, new List<string>() { "LethalToCreep" } },
				{ ArtifactKeyword.LethalToHero, new List<string>() { "LethalToHero" } },
				{ ArtifactKeyword.Hacks, new List<string>() { "Hacks" } },
				{ ArtifactKeyword.Reveal, new List<string>() { "Reveal" } },
				{ ArtifactKeyword.Pulse, new List<string>() { "Pulse" } },
				{ ArtifactKeyword.GainsInitiative, new List<string>() { "Get initiative", "Get Initiative" } },
				{ ArtifactKeyword.RapidDeployment, new List<string>() { "Rapid Deployment" } },
				{ ArtifactKeyword.DeathEffect, new List<string>() { "Death Effect:" } },
				{ ArtifactKeyword.PlayEffect, new List<string>() { "Play Effect:" } },
				{ ArtifactKeyword.ContinuousEffect, new List<string>() { "" } },
				{ ArtifactKeyword.ReactiveEffect, new List<string>() { "" } }
			};

			public void Transform(WikiCard card)
			{
				foreach(var pair in KeywordTriggers)
				{
					foreach(string trigger in pair.Value)
					{
						if (String.IsNullOrWhiteSpace(trigger))
							continue;

						var match = Regex.Match(card.Text, trigger);
						if(match.Success)
						{
							card.Keywords[pair.Key] = match.Value;
						}
					}
				}

				//if(card.Keywords.ContainsKey(ArtifactKeyword.DeathEffect))
				//{
				//	card.
				//}
			}
		}


		public class SimpleTextSubstitution : ICardTransformer
		{
			public string RegexString { get; protected set; }
			public string Replacement { get; protected set; }

			public virtual void Transform(WikiCard card)
			{
				card.Text = Regex.Replace(card.Text, RegexString, Replacement);
			}

			public SimpleTextSubstitution(string regex, string replace)
			{
				RegexString = regex;
				Replacement = replace;
			}
		}

		public class SimpleTextWikiSubstitution : SimpleTextSubstitution
		{
			public override void Transform(WikiCard card)
			{
				if (card.TextFormatted == null)
					return;

				card.TextFormatted = Regex.Replace(card.TextFormatted, RegexString, Replacement);
			}
			public SimpleTextWikiSubstitution(string regex, string replace) : base(regex, replace) { }
		}

		public class SimpleLoreSubstitution : SimpleTextSubstitution
		{
			public override void Transform(WikiCard card)
			{
				if (card.Lore == null)
					return;

				card.Lore = Regex.Replace(card.Lore, RegexString, Replacement);
			}
			public SimpleLoreSubstitution(string regex, string replace) : base(regex, replace) { }
		}

		public class SimpleLoreWikiSubstitution : SimpleTextSubstitution
		{
			public override void Transform(WikiCard card)
			{
				if (card.LoreFormatted == null)
					return;

				card.LoreFormatted = Regex.Replace(card.LoreFormatted, RegexString, Replacement);
			}
			public SimpleLoreWikiSubstitution(string regex, string replace) : base(regex, replace) { }
		}

		public class SimpleVoiceoverSubstitution : SimpleTextSubstitution
		{
			public override void Transform(WikiCard card)
			{
				Dictionary<string, string> newVO = new Dictionary<string, string>();
				foreach(var pair in card.VoiceOverLines)
				{
					newVO[pair.Key] = Regex.Replace(pair.Value, RegexString, Replacement);
				}

				card.VoiceOverLines = newVO;
			}
			public SimpleVoiceoverSubstitution(string regex, string replace) : base(regex, replace) { }
		}

		public class ThisCardNameSubstitution : SimpleTextSubstitution
		{
			public override void Transform(WikiCard card)
			{
				if (card.Text != null)
				{
					card.Text = Regex.Replace(card.Text, RegexString, card.Name);
				}

				if (card.TextFormatted != null)
				{
					card.TextFormatted = Regex.Replace(card.TextFormatted, RegexString, card.Name);
				}
			}
			public ThisCardNameSubstitution() : base(@"\{s:thisCardName\}", null) { }
		}

		public class ParentCardNameSubstitution : SimpleTextSubstitution
		{
			public override void Transform(WikiCard card)
			{
				if (!Regex.IsMatch(card.Text, RegexString) && !Regex.IsMatch(card.TextFormatted, RegexString))
					return;

				string parent = "";
				if (card.CardType == ArtifactCardType.Ability || card.CardType == ArtifactCardType.PassiveAbility)
				{
					parent = (card.SubCard as WikiAbility).Parent.Name;
				}
				
				//if(card.TokenParents.Count == 0)
				//{
				//	parent = card.TokenParent.Name;
				//}

				if(card.SignatureParent != null)
				{
					parent = card.SignatureParent.Name;
				}

				if (String.IsNullOrWhiteSpace(parent))
					throw new Exception("Couldn't find the parent name for some reason.");

				card.Text = Regex.Replace(card.Text, RegexString, parent);
				card.TextFormatted = Regex.Replace(card.TextFormatted, RegexString, parent);
			}
			public ParentCardNameSubstitution() : base(@"\{s:parentCardName\}", null) { }
		}

		public class InitialTextSetTransformer : ICardTransformer
		{
			public void Transform(WikiCard card)
			{
				card.Text = card.TextRaw;
				card.TextFormatted = card.TextRaw;
				card.Lore = card.LoreRaw;
				card.LoreFormatted = card.LoreRaw;

				foreach (var pair in card.VoiceOverLinesRaw)
				{
					card.VoiceOverLines[pair.Key] = pair.Value;
				}
			}
		}

		public class FindAbilityCooldown : ICardTransformer
		{
			public void Transform(WikiCard card)
			{

				var match = Regex.Match(card.TextRaw, @"\[activatedability\[\[color:ability\[Active &#9632;(\d):]]");
				if(match.Captures.Count > 0)
				{
					foreach(var ability in card.Abilities.Values)
					{
						if(ability.AbilityType == ArtifactAbilityType.Active)
						{
							ability.Cooldown = Int32.Parse(match.Groups[1].Value);
						}
					}
				}

			}
		}

		public class ResolveReferences : CardCollectionTransformer
		{
			protected override void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card)
			{
				foreach (var pair in card.References)
				{
					pair.Value.Card = cards[pair.Key];

					if (pair.Value.Card.CardType == ArtifactCardType.Ability || pair.Value.Card.CardType == ArtifactCardType.PassiveAbility)
					{
						var ability = pair.Value.Card.SubCard as WikiAbility;
						ability.ParentID = card.ID;
						ability.Parent = card;
						ability.Name = ability.Name ?? card.Name;
						card.Abilities[ability.ID] = ability;
					}

					if (card.Text.Contains($"ummon a {pair.Value.Card.Name}"))
					{
						if (card.CardType == ArtifactCardType.Ability || card.CardType == ArtifactCardType.PassiveAbility)
						{
							var ability = card.SubCard as WikiAbility;

							ability.CardSpawned = pair.Value.Card;
							ability.CardSpawnedID = pair.Key;
						}
						else if(card.CardType == ArtifactCardType.Spell || card.CardType == ArtifactCardType.Improvement)
						{
							var spell = card.SubCard as WikiSpell;
							spell.CardSpawned = pair.Value.Card;
						}
						
					}

					
				}
			}

			protected override void TransformSingle(WikiCard card) { }
		}

		public class ResolveSignatures : CardCollectionTransformer
		{
			protected override void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card)
			{
				if (card.CardType == ArtifactCardType.Hero)
				{
					var hero = card.SubCard as WikiHero;
					var sig = cards[hero.SignatureCardID];
					hero.SignatureCard = sig;
					sig.SignatureOf = card.ID;
					sig.SignatureParent = card;
				}
			}

			protected override void TransformSingle(WikiCard card) { }
		}

		public class ResolveTokens : CardCollectionTransformer
		{
			protected override void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card)
			{
				if (card.CardType == ArtifactCardType.Ability ||
						card.CardType == ArtifactCardType.PassiveAbility)
				{
					
					var ability = card.SubCard as WikiAbility;
					if (ability.CardSpawned != null)
					{
						var spawned = cards[ability.CardSpawnedID];
						spawned.TokenParents.Add(card);
					}
				}
				else if (card.CardType == ArtifactCardType.Spell ||
						card.CardType == ArtifactCardType.Improvement)
				{
					var spell = card.SubCard as WikiSpell;
					if (spell.CardSpawned != null)
					{
						var spawned = cards[spell.CardSpawnedID];
						spawned.TokenParents.Add(card);
					}
				}
			}

			protected override void TransformSingle(WikiCard card) { }
		}

		public class SetIsCollectable : CardCollectionTransformer
		{
			protected override void TransformSingle(WikiCard card)
			{
				if (card.Rarity == ArtifactRarity.Basic ||
						card.CardType == ArtifactCardType.Stronghold ||
						card.CardType == ArtifactCardType.Pathing ||
						card.CardType == ArtifactCardType.Ability ||
						card.CardType == ArtifactCardType.PassiveAbility)
				{
					card.IsCollectable = false;
				}
			}

			protected override void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card)
			{
				foreach(var pair in card.References)
				{
					cards[pair.Key].IsCollectable = false;
				}
			}
		}


	}
}
