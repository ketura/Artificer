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
using static Artificer.Transformers;

namespace Artificer
{
	public class TransformerGauntlet
	{
		private List<ICardTransformer> Setup { get; set; }
		private List<ICardTransformer> SingleCardTransformers { get; set; }
		private List<ICardCollectionTransformer> CardCollectionTransformers { get; set; }

		public void AddSetup(ICardTransformer trans)
		{
			Setup.Add(trans);
		}

		public void AddSingle(ICardTransformer trans)
		{
			SingleCardTransformers.Add(trans);
		}

		public void AddMultiple(ICardCollectionTransformer trans)
		{
			CardCollectionTransformers.Add(trans);
		}

		public void Execute(IDictionary<int, WikiCard> cards)
		{
			foreach (var trans in Setup)
			{
				foreach (var card in cards.Values)
				{
					trans.Transform(card);
				}
			}

			foreach (var trans in CardCollectionTransformers)
			{
				foreach (var card in cards.Values)
				{
					trans.Transform(cards, card);
				}
			}

			foreach (var trans in SingleCardTransformers)
			{
				foreach (var card in cards.Values)
				{
					trans.Transform(card);
				}
			}
		}

		public TransformerGauntlet()
		{
			Setup = new List<ICardTransformer>();
			SingleCardTransformers = new List<ICardTransformer>();
			CardCollectionTransformers = new List<ICardCollectionTransformer>();
		}

		public static TransformerGauntlet GenerateGauntlet()
		{
			var gauntlet = new TransformerGauntlet();

			gauntlet.AddSetup(new InitialTextSetTransformer());

			gauntlet.AddMultiple(new ResolveReferences());
			gauntlet.AddMultiple(new ResolveSignatures());
			gauntlet.AddMultiple(new ResolveTokens());
			gauntlet.AddMultiple(new SetIsCollectable());

			gauntlet.AddSingle(new ThisCardNameSubstitution());
			gauntlet.AddSingle(new ParentCardNameSubstitution());
			gauntlet.AddSingle(new FindAbilityCooldown());

			gauntlet.AddSingle(new SimpleTextSubstitution(@"\[g:608\[&#9633;]]", "[Pulse]"));
			gauntlet.AddSingle(new SimpleTextSubstitution(@"\[\[g:604\[&#9634; Get initiative]]", "Get initiative"));
			gauntlet.AddSingle(new SimpleTextSubstitution(@"\[g:\d+\[(.*?)]]", "$1"));
			gauntlet.AddSingle(new SimpleTextSubstitution(@"\[activatedability\[\[color:ability\[Active &#9632;(\d):]]", "[Cooldown: $1]"));
			gauntlet.AddSingle(new SimpleTextSubstitution(@"\[\[(.*?)]]", "$1"));
			gauntlet.AddSingle(new SimpleTextSubstitution(@"<BR/>\\n", "<br/>"));
			gauntlet.AddSingle(new SimpleTextSubstitution(@"<br/>", "<br/>"));

			gauntlet.AddSingle(new SimpleTextWikiSubstitution(@"\[g:608\[&#9633;]]", "[[Pulse]]"));
			gauntlet.AddSingle(new SimpleTextWikiSubstitution(@"\[\[g:604\[&#9634; Get initiative]]", "[[Get Initiative]]"));
			gauntlet.AddSingle(new SimpleTextWikiSubstitution(@"\[abilityname\[.*?]]", ""));
			gauntlet.AddSingle(new SimpleTextWikiSubstitution(@"\[g:\d+\[(.*?)]]", "[[$1]]"));
			gauntlet.AddSingle(new SimpleTextWikiSubstitution(@"<BR/>\\n", "<br/>"));
			gauntlet.AddSingle(new SimpleTextWikiSubstitution(@"<br/>", "<br/>"));

			gauntlet.AddSingle(new SimpleLoreSubstitution(@"<i>(.*?)</i>", "$1"));
			gauntlet.AddSingle(new SimpleLoreWikiSubstitution(@"<i>(.*?)</i>", "''$1''"));

			gauntlet.AddSingle(new SimpleVoiceoverSubstitution(@"<i>(.*?)</i>", "''$1''"));

			gauntlet.AddSingle(new KeywordHunter());


			return gauntlet;
		}
	}
}
