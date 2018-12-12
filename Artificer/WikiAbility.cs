
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
	public class WikiAbility
	{
		// Name of the ability. For improvements/creeps the ability will be the name of the card + " : Effect" e.g. "Keenfolk Turret : Effect."
		public string Name { get; set; }

		public string Icon { get; set; }

		// Active/Continuous/Play/Death/Reactive
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactAbilityType Type { get; set; }

		// The description of the effect. For improvement/creep/item abilities it will remove the prefix e.g. "Active 1: Do something." will become "Do something.".
		public string Text { get; set; }

		// Active affect cooldown.
		public int Cooldown { get; set; }
	}
}
