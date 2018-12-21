
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
using System.Runtime.Serialization;
using System.Text;

namespace Artificer
{
	public enum ArtifactCardType
	{
		Hero,
		Creep,
		Improvement,
		Spell,
		Item,
		Stronghold,
		Pathing,
		Ability,
		[EnumMember(Value = "Passive Ability")]
		PassiveAbility
	}

	public enum ArtifactSubType { None, Consumable, Weapon, Armor, Accessory, Deed }
	public enum ArtifactColor { None, Black, Blue, Green, Red }
	public enum ArtifactRarity
	{
		[EnumMember(Value = "")]
		Basic,
		Common,
		Uncommon,
		Rare
	}
	public enum ArtifactAbilityType { None, Active, Passive }
	public enum ArtifactPassiveAbilityType { None, Continuous, Play, Death, Reactive }

	public enum ArtifactReferenceType
	{
		[EnumMember(Value = "includes")]
		Signature,
		[EnumMember(Value = "references")]
		References,
		[EnumMember(Value = "passive_ability")]
		PassiveAbility,
		[EnumMember(Value = "active_ability")]
		ActiveAbility
	}

	public enum ArtifactKeyword
	{
		BeforeActionPhase,
		AfterCombatPhase,
		Fountain,
		ModifyAlly,
		ModifyEnemy,
		AlliedNeighbors,
		EnemyNeighbors,
		Purge,
		Taunt,
		Disarm,
		Stun,
		Silence,
		Lock,
		Condemn,
		Summon,
		Mana,
		Bounty,
		Gold,
		Siege,
		Cleave,
		Retaliate,
		Attack,
		Armor,
		Health,
		Regeneration,
		DeathShield,
		DamageImmunity,
		Pierce,
		PiercingDamage,
		Damage,
		Heal,
		Soulbound,
		LethalToCreep,
		LethalToHero,
		Hacks,
		Reveal,
		Pulse,
		GainsInitiative,
		RapidDeployment,
		DeathEffect,
		PlayEffect,
		ContinuousEffect,
		ReactiveEffect
	}

}
