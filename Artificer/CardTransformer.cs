
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
using System.Linq;

namespace Artificer
{
	public interface ICardTransformer
	{
		void Transform(WikiCard card);
	}

	public interface ICardCollectionTransformer
	{
		void Transform(IDictionary<int, WikiCard> cards, WikiCard card);
	}

	public abstract class CardCollectionTransformer : ICardCollectionTransformer, ICardTransformer
	{
		public void Transform(IDictionary<int, WikiCard> cards, WikiCard card)
		{
			TransformSingle(card);
			TransformChildren(cards, card);
		}

		public void Transform(WikiCard card)
		{
			TransformSingle(card);
		}

		protected abstract void TransformSingle(WikiCard card);
		protected abstract void TransformChildren(IDictionary<int, WikiCard> cards, WikiCard card);
	}	
}
