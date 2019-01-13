
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
		//Timing
		DeploymentPhase, //each round
		BeforeActionPhase,
		AfterCombatPhase,
		ThisRound, //end of its next combat phase
		OtherDeath, //dies

		//Locations
		Fountain,
		AnyLane, //choose a lane
		ThisLane,
		OtherLane, //other lanes, another lane
		AllLanes,
		EmptyPosition, //empty combat position, empty position

		//Unit target types
		AlliedNeighbors,
		EnemyNeighbors,
		AlliedTower, //your tower
		EnemyTower,
		AllTowers,
		Improvements,
		MeleeCreeps,
		AlliedCreeps,
		EnemyCreeps,
		AlliedHeroes, //allied X hero, allied hero
		EnemyHeroes, //enemy hero
		BlackHeroes,
		BlueHeroes,
		GreenHeroes,
		RedHeroes,
		AnyHero, //a hero, a X hero
		AllHeroes, //all \w+ heroes
		AlliedUnits, //another ally, to allies, other allies, give allies, each ally, an ally, modify allies, random ally
		EnemyUnits, //each enemy, two enemies, random enemy
		AnyUnit, //a unit, another unit
		AllUnits,
		Items, //not "non-item"


		//Actions
		Purge,
		Taunt,
		Disarm,
		Stun,
		Silence,
		Lock,
		Condemn,
		Summon,
		DirectDamage,
		Heal,
		ChangeTarget, //combat target
		Move, //Swap
		Battle,
		Modify,
		Discard,
		Draw,

		//Attribute types
		PlusMana, //+\d mana, restore .* mana
		MinusMana, //-\d mana
		Bounty,
		Gold,
		Siege,
		Cleave,
		Retaliate,
		PlusAttack, //+X
		MinusAttack, //-X
		PlusArmor, //+X
		MinusArmor, //-X
		PlusHealth,
		MinusHealth,
		Regeneration,
		DeathShield,
		DamageImmunity,
		Pierce,
		PiercingDamage,
		RapidDeployment,
		Soulbound,

		//Mechanics
		Pulse,
		Initiative,
		Quicken,
		RandomChance, //random, chance, not "the random"

		//Passive types
		DeathEffect,
		PlayEffect,
		ContinuousEffect,
		ReactiveEffect,

		//Unused?
		LethalToCreep,
		LethalToHero,
		Hacks,
		Reveal,
	}

}
