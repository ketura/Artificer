
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
	public class WikiSet
	{
		public string Name { get; set; }
		public int ID { get; set; }
		public string MarketOffset { get; set; }
		public DateTime ReleaseDate { get; set; }

		public WikiSet()
		{

		}

		public WikiSet(ValveSet set)
		{
			Name = set.set_info.name["english"];
			ID = set.set_info.set_id;
			MarketOffset = set.set_info.pack_item_def.ToString();
			ReleaseDate = new DateTime(2018, 11, 28);
		}
	}
}
