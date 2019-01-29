
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
				//Timing
				//DeploymentPhase //each round
				{ ArtifactKeyword.DeploymentPhase, new List<string>() { "[Dd]eployment [Pp]hase", "[Ee]ach [Rr]ound" } },
				{ ArtifactKeyword.BeforeActionPhase, new List<string>() { "[Bb]efore the action phase" } },
				{ ArtifactKeyword.AfterCombatPhase, new List<string>() { "[Aa]fter the combat phase" } },
				{ ArtifactKeyword.ThisRound, new List<string>() { "[Tt]his [Rr]ound", "end of its next combat phase" } },
				{ ArtifactKeyword.OtherDeath, new List<string>() { @"\bdies\b" } },

				//Locations
				{ ArtifactKeyword.Fountain, new List<string>() { "[Tt]he Fountain" } },
				{ ArtifactKeyword.AnyLane, new List<string>() { "[Aa]ny [Ll]ane", "[Cc]hoose a [Ll]ane" } },
				{ ArtifactKeyword.ThisLane, new List<string>() { "[Tt]his [Ll]ane" } },
				{ ArtifactKeyword.OtherLane, new List<string>() { "[Oo]ther [Ll]anes?", "[Aa]nother [Ll]ane" } },
				{ ArtifactKeyword.AllLanes, new List<string>() { "[Aa]ll [Ll]anes" } },
				{ ArtifactKeyword.EmptyPosition, new List<string>() { "[Ee]mpty ([Cc]ombat )?[Pp]osition" } },

				//Unit target types
				{ ArtifactKeyword.AlliedNeighbors, new List<string>() { "[Aa]llied neighbors?" } },
				{ ArtifactKeyword.EnemyNeighbors, new List<string>() { "[Ee]nemy neighbors?" } },
				{ ArtifactKeyword.AlliedTower, new List<string>() { "[Aa]llied [Tt]ower", "[Yy]our [Tt]ower" } },
				{ ArtifactKeyword.EnemyTower, new List<string>() { "[Ee]nemy [Tt]owers?", "[Bb]oth [Tt]owers?" } },
				{ ArtifactKeyword.AllTowers, new List<string>() { "[Aa]ll [Tt]ower" } },
				{ ArtifactKeyword.Improvements, new List<string>() { "[Ii]mprovements?" } },
				{ ArtifactKeyword.MeleeCreeps, new List<string>() { "[Mm]elee [Cc]reeps?" } },
				{ ArtifactKeyword.AlliedCreeps, new List<string>() { "[Aa]llied [Cc]reeps?" } },
				{ ArtifactKeyword.EnemyCreeps, new List<string>() { "[Ee]nemy [Cc]reeps?" } },
				{ ArtifactKeyword.AlliedHeroes, new List<string>() { "[Aa]llied [Hh]eroe?s?", "[Aa]llied ([Bb]lack|[Bb]lue|[Gg]reen|[Rr]ed) [Hh]eroe?s?" } },
				{ ArtifactKeyword.EnemyHeroes, new List<string>() { "[Ee]nemy [Hh]eroe?s?" } },
				{ ArtifactKeyword.BlackHeroes, new List<string>() { "[Bb]lack [Hh]eroe?s?" } },
				{ ArtifactKeyword.BlueHeroes, new List<string>() { "[Bb]lue [Hh]eroe?s?" } },
				{ ArtifactKeyword.GreenHeroes, new List<string>() { "[Gg]reen [Hh]eroe?s?" } },
				{ ArtifactKeyword.RedHeroes, new List<string>() { "[Rr]ed [Hh]eroe?s?" } },
				{ ArtifactKeyword.AnyHero, new List<string>() { "[Aa] [Hh]eroe?s?", "[Aa] ([Bb]lack|[Bb]lue|[Gg]reen|[Rr]ed) [Hh]eroe?s?" } },
				{ ArtifactKeyword.AllHeroes, new List<string>() { "[Aa]ll [Hh]eroe?s?", "[Aa]ll ([Bb]lack|[Bb]lue|[Gg]reen|[Rr]ed) [Hh]eroe?s?" } },
				{ ArtifactKeyword.AlliedUnits, new List<string>() { "[Aa]llied [Uu]nits?", "[Aa]nother [Aa]lly", "(to )?[Aa]llies", "[Oo]ther [Aa]llies", "[Gg]ive [Aa]llies", "[Ee]ach [Aa]lly", "[Aa]n [Aa]lly", "[Mm]odify [Aa]llies", "[Rr]andom [Aa]lly" } },
				{ ArtifactKeyword.EnemyUnits, new List<string>() { "[Ee]nemy [Uu]nits?", "[Aa]nother [Ee]nemy [Uu]nits?", "to [Ee]nem(y|ies) [Uu]nits?", "[Oo]ther [Ee]nem(y|ies) [Uu]nits?", "[Gg]ive [Ee]nem(y|ies) [Uu]nits?", "[Ee]ach (other )?[Ee]nem(y|ies)( [Uu]nits?)?", "[Aa]n [Ee]nem(y|ies) [Uu]nits?", "[Mm]odify [Ee]nem(y|ies) [Uu]nits?", "[Rr]andom [Ee]nem(y|ies) [Uu]nits?", "two [Ee]nem(y|ies)" } },
				{ ArtifactKeyword.AnyUnit, new List<string>() { "[Aa] [Uu]nit", "[Aa]nother [Uu]nit" } },
				{ ArtifactKeyword.AllUnits, new List<string>() { "[Aa]ll [Uu]nits" } },
				{ ArtifactKeyword.Items, new List<string>() { "(?<!non-)[Ii]tems?" } },

				//Actions
				{ ArtifactKeyword.Purge, new List<string>() { "[Pp]urge" } },
				{ ArtifactKeyword.Taunt, new List<string>() { "[Tt]aunt" } },
				{ ArtifactKeyword.Disarm, new List<string>() { "[Dd]isarm" } },
				{ ArtifactKeyword.Stun, new List<string>() { "[Ss]tun" } },
				{ ArtifactKeyword.Silence, new List<string>() { "[Ss]ilence" } },
				{ ArtifactKeyword.Lock, new List<string>() { @"\b[Ll]ock\b" } },
				{ ArtifactKeyword.Condemn, new List<string>() { "[Cc]ondemn" } },
				{ ArtifactKeyword.Summon, new List<string>() { "[Ss]ummon" } },
				{ ArtifactKeyword.DirectDamage, new List<string>() { @"\d ([Pp]iercing)? damage", @"[Dd]eal (\d+ )?damage" } },
				{ ArtifactKeyword.Heal, new List<string>() { @"\b[Hh]eal\b" } },
				{ ArtifactKeyword.ChangeTarget, new List<string>() { "[Cc]hange [Tt]arget", "[Cc]ombat [Tt]arget" } },
				{ ArtifactKeyword.Move, new List<string>() { @"\b[Mm]ove\b", @"[Ss]wap\b" } },
				{ ArtifactKeyword.Battle, new List<string>() { "[Tt]hey [Bb]attle", "[Ii]t [Bb]attle" } },
				{ ArtifactKeyword.Modify, new List<string>() { "[Mm]odify", "[Mm]odifies" } },
				{ ArtifactKeyword.Discard, new List<string>() { "[Dd]iscard" } },
				{ ArtifactKeyword.Draw, new List<string>() { @"\b[Dd]raw\b" } },

				//Attribute types
				{ ArtifactKeyword.PlusMana, new List<string>() { @"\+\d+ [Mm]ana", @"\+X [Mm]ana", @"restore your [Tt]ower's [Mm]ana" } },
				{ ArtifactKeyword.MinusMana, new List<string>() { @"-\d+ [Mm]ana", @"-X [Mm]ana" } },
				{ ArtifactKeyword.Bounty, new List<string>() { "[Bb]ounty" } },
				{ ArtifactKeyword.Gold, new List<string>() { "[Gg]old" } },
				{ ArtifactKeyword.Siege, new List<string>() { "[Ss]iege" } },
				{ ArtifactKeyword.Cleave, new List<string>() { @"[Cc]leave\b" } },
				{ ArtifactKeyword.Retaliate, new List<string>() { "[Rr]etaliate" } },
				{ ArtifactKeyword.PlusAttack, new List<string>() { @"\+\d+ [Aa]ttack\b", @"\+X [Aa]ttack\b" } },
				{ ArtifactKeyword.MinusAttack, new List<string>() { @"-\d+ [Aa]ttack\b", @"-X [Aa]ttack\b" } },
				{ ArtifactKeyword.PlusArmor, new List<string>() { @"\+\d+ [Aa]rmor", @"\+X [Aa]rmor" } },
				{ ArtifactKeyword.MinusArmor, new List<string>() { @"-\d+ [Aa]rmor", @"-X [Aa]rmor" } },
				{ ArtifactKeyword.PlusHealth, new List<string>() { @"\+\d+ [Hh]ealth", @"\+X [Hh]ealth" } },
				{ ArtifactKeyword.MinusHealth, new List<string>() { @"-\d+ [Hh]ealth", @"-X [Hh]ealth" } },
				{ ArtifactKeyword.Regeneration, new List<string>() { "[Rr]egeneration" } },
				{ ArtifactKeyword.DeathShield, new List<string>() { "[Dd]eath [Ss]hield" } },
				{ ArtifactKeyword.DamageImmunity, new List<string>() { "[Dd]amage [Ii]mmunity" } },
				{ ArtifactKeyword.Pierce, new List<string>() { "[Pp]ierce" } },
				{ ArtifactKeyword.PiercingDamage, new List<string>() { "[Pp]iercing damage" } },
				{ ArtifactKeyword.RapidDeployment, new List<string>() { "[Rr]apid [Dd]eployment" } },
				{ ArtifactKeyword.Soulbound, new List<string>() { "[Ss]oulbound" } },

				//Mechanics
				{ ArtifactKeyword.Pulse, new List<string>() { "[Pp]ulse" } },
				{ ArtifactKeyword.Initiative, new List<string>() { "[Gg]et [Ii]nitiative" } },
				{ ArtifactKeyword.Quicken, new List<string>() { "[Qq]uicken" } },
				{ ArtifactKeyword.RandomChance, new List<string>() { "(?<!the )[Rr]andom", "[Cc]hance" } },

				//Passive Types
				{ ArtifactKeyword.DeathEffect, new List<string>() { "Death Effect:" } },
				{ ArtifactKeyword.PlayEffect, new List<string>() { "Play Effect:", "Equip Effect:"} },
				{ ArtifactKeyword.ContinuousEffect, new List<string>() { "" } },
				{ ArtifactKeyword.ReactiveEffect, new List<string>() { "[aA]fter", "[bB]efore", "[wW]hen(ever)?", @"[Ee]ach \w+ phase"} },

				//Unused?
				{ ArtifactKeyword.LethalToCreep, new List<string>() { "LethalToCreep" } },
				{ ArtifactKeyword.LethalToHero, new List<string>() { "LethalToHero" } },
				{ ArtifactKeyword.Hacks, new List<string>() { "Hacks" } },
				{ ArtifactKeyword.Reveal, new List<string>() { "Reveal" } },
			};

			public void Transform(WikiCard card)
			{
				foreach (var pair in KeywordTriggers)
				{
					foreach (string trigger in pair.Value)
					{
						if (String.IsNullOrWhiteSpace(trigger) || String.IsNullOrWhiteSpace(card.Text))
							continue;

						var match = Regex.Match(card.Text, trigger);
						if (match.Success)
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

		public class AbilitySubtypeProcessing : ICardTransformer
		{

			public void Transform(WikiCard card)
			{
				foreach(var ability in card.Abilities.Values)
				{
					if(ability.AbilityType == ArtifactAbilityType.Active)
					{
						if (card.CardType == ArtifactCardType.Creep)
						{
							ability.AbilityCardParent.CardIcon = "Creep_ability_active_icon.png";
						}
					}
					else if (card.Keywords.ContainsKey(ArtifactKeyword.DeathEffect))
					{
						ability.PassiveAbilityType = ArtifactPassiveAbilityType.Death;

						if (card.CardType == ArtifactCardType.Creep)
						{
							ability.AbilityCardParent.CardIcon = "Creep_ability_death_icon.png";
						}
					}
					else if (card.Keywords.ContainsKey(ArtifactKeyword.ReactiveEffect))
					{
						ability.PassiveAbilityType = ArtifactPassiveAbilityType.Reactive;
						if (card.CardType == ArtifactCardType.Creep)
						{
							ability.AbilityCardParent.CardIcon = "Creep_ability_reactive_icon.png";
						}
					}
					else if (card.Keywords.ContainsKey(ArtifactKeyword.PlayEffect))
					{
						ability.PassiveAbilityType = ArtifactPassiveAbilityType.Play;
						if (card.CardType == ArtifactCardType.Creep)
						{
							ability.AbilityCardParent.CardIcon = "Creep_ability_reactive_icon.png";
						}
					}
					else
					{
						ability.PassiveAbilityType = ArtifactPassiveAbilityType.Continuous;
						card.Keywords.Add(ArtifactKeyword.ContinuousEffect, "process of elimination");
						if (card.CardType == ArtifactCardType.Creep)
						{
							ability.AbilityCardParent.CardIcon = "Creep_ability_continuous_icon.png";
						}
					}

					if(card.CardType == ArtifactCardType.Item || card.CardType == ArtifactCardType.Improvement)
					{
						ability.AbilityCardParent.CardIcon = card.CardIcon;
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
				if (card.Text == null)
				{
					Console.WriteLine($"Card {card.Name} has no text!");
					return;
				}
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
				if (card.TextRaw == null)
				{
					return;
				}
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
				if(card.TextRaw == null)
				{
					Console.WriteLine($"Card {card.Name} has no text!");
					return;
				}
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
						ability.AbilityCardParent = pair.Value.Card;
						card.Abilities[ability.ID] = ability;
					}

					if (card.TextRaw == null)
					{
						continue;
					}

					if (Regex.IsMatch(card.Text, $@"ummon \w+ {pair.Value.Card.Name}"))
					{
						if (card.CardType == ArtifactCardType.Ability || card.CardType == ArtifactCardType.PassiveAbility)
						{
							var ability = card.SubCard as WikiAbility;

							ability.CardSpawned = pair.Value.Card;
							ability.CardSpawnedID = pair.Key;
						}
						else if (card.CardType == ArtifactCardType.Spell || card.CardType == ArtifactCardType.Improvement)
						{
							var spell = card.SubCard as WikiSpell;
							spell.CardSpawned = pair.Value.Card;
						}

					}
				}
			}

			protected override void TransformSingle(WikiCard card) { }
		}

		public class CorrectAbilities : CardCollectionTransformer
		{
			protected override void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card)
			{
				//Abilities are initialized by taking the references of a card, but this is inaccurate for many cases. 
				// This finds all non-ability abilities and purges them.
				foreach (var pair in card.Abilities.ToList())
				{
					var abilityCard = cards[pair.Key];

					if (abilityCard.CardType != ArtifactCardType.Ability && abilityCard.CardType != ArtifactCardType.PassiveAbility)
					{
						card.Abilities.Remove(pair.Key);
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
					sig.Rarity = card.Rarity;
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

		public class SetAbilityText : CardCollectionTransformer
		{
			protected override void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card) { }

			protected override void TransformSingle(WikiCard card)
			{
				if(card.CardType == ArtifactCardType.Ability ||
						card.CardType == ArtifactCardType.PassiveAbility)
				{
					var ability = card.SubCard as WikiAbility;
					ability.Text = card.Text;
					ability.TextFormatted = card.TextFormatted;
				}
			}
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
