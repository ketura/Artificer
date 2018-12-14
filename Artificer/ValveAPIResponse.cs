
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
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Artificer
{
	public class ValveAPIResponseCollection
	{
		public Dictionary<int, ValveAPIResponse> Responses { get; set; }

		public ValveAPIResponseCollection()
		{
			Responses = new Dictionary<int, ValveAPIResponse>();
		}

		public ValveAPIResponse this[int i]
		{
			get { return Responses[i]; }
			set { Responses[i] = value; }
		}

		public bool ContainsKey(int i)
		{
			return Responses.ContainsKey(i);
		}
	}

	public class ValveAPIResponse
	{
		public int SetID { get; set; }
		public string cdn_root { get; set; }
		public string url { get; set; }
		public long expire_time { get; set; }
		public DateTime RetrievalDate { get; set; }
		public string JSONResult { get; set; }

		[JsonIgnore]
		public string FullURL
		{
			get { return $"{cdn_root}{url}"; }	
		}

		[JsonIgnore]
		private DateTime? _expiration { get; set; } = null;

		[JsonIgnore]
		public DateTime ExpirationDate
		{
			get
			{
				if(!_expiration.HasValue)
				{
					DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
					_expiration = epoch.AddSeconds(expire_time).ToLocalTime();
				}

				return _expiration.Value;
			}
		}

		[JsonIgnore]
		public bool IsExpired
		{
			get { return ExpirationDate < DateTime.Now; }
		}

		[JsonIgnore]
		public ValveSet SetDefinition { get; set; }

		public void ParseJSON()
		{
			if(SetDefinition == null)
			{
				JsonSerializerSettings settings = new JsonSerializerSettings()
				{
					MissingMemberHandling = MissingMemberHandling.Error,

				};
				SetDefinition = JsonConvert.DeserializeObject<ValveSetWrapper>(JSONResult, settings).card_set;
			}
		}
	}

	public class ValveSetWrapper
	{
		public ValveSet card_set { get; set; }
	}


	public class ValveSet
	{
		public List<ValveCard> card_list { get; set; }
		public ValveSetInfo set_info { get; set; }
		public int version { get; set; }
	}

	[DebuggerDisplay("{card_name[\"english\"]}")]
	public class ValveCard
	{
		public int base_card_id { get; set; }
		public int card_id { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactCardType card_type { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactSubType sub_type { get; set; }
		public bool is_black { get; set; }
		public bool is_blue { get; set; }
		public bool is_green { get; set; }
		public bool is_red { get; set; }

		public bool is_crosslane { get; set; }
		public bool is_quick { get; set; }
		public int charges { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactRarity rarity { get; set; }
		public int item_def { get; set; }
		public Dictionary<string, string> card_name { get; set; }
		public Dictionary<string, string> card_text { get; set; }
		public int attack { get; set; }
		public int armor { get; set; }
		public int hit_points { get; set; }
		public int mana_cost { get; set; }
		public int gold_cost { get; set; }

		public List<ValveCardReference> references { get; set; }

		public Dictionary<string, string> ingame_image { get; set; }
		public Dictionary<string, string> large_image { get; set; }
		public Dictionary<string, string> mini_image { get; set; }

		public string illustrator { get; set; }


		public ValveCard()
		{
			card_name = new Dictionary<string, string>();
			card_text = new Dictionary<string, string>();
			ingame_image = new Dictionary<string, string>();
			large_image = new Dictionary<string, string>();
			mini_image = new Dictionary<string, string>();
			references = new List<ValveCardReference>();
		}
	}

	public class ValveCardReference
	{
		public int card_id { get; set; }
		public int count { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public ArtifactReferenceType ref_type { get; set; }
	}


	public class ValveSetInfo
	{
		public Dictionary<string, string> name { get; set; }
		public int pack_item_def { get; set; }
		public int set_id { get; set; }

		public ValveSetInfo()
		{
			name = new Dictionary<string, string>();
		}
	}


}
