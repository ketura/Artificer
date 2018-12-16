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
using System.Text;

namespace Artificer
{
	public class Config
	{
		public string CardCacheLocation { get; set; }
		public string ValveAPIBaseURL { get; set; }
		public string ValveCacheLocation { get; set; }
		public string APIImagesLocation { get; set; }
		public string APILanguage { get; set; }
		public string ArtifactBaseDir { get; set; }
		public string GameImagesLocation { get; set; }
		public string GameAudioLocation { get; set; }
		public string VOMappingLocation { get; set; }
		public string WikiURL { get; set; }
		public string WikiUsername { get; set; }
		public string WikiPassword { get; set; }

		public static Config GetDefaultConfig()
		{
			return new Config()
			{
				CardCacheLocation = "./CardCache.json",
				ValveAPIBaseURL = "https://playartifact.com/cardset/",
				ValveCacheLocation = "./ValveAPIResponses.json",
				APIImagesLocation = "./CardImages",
				APILanguage = "default",
				ArtifactBaseDir = "C:/Program Files/Steam/steamapps/common/Artifact",
				GameImagesLocation = "./RawCardImages",
				GameAudioLocation = "./GameAudio",
				VOMappingLocation = "./data/VoiceoverMapping.json"

			};
		}
	}
}
