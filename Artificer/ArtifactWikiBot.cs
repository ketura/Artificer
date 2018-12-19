
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
////                                         aaa                                      ////
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
using System.Threading.Tasks;

using WikiClientLibrary.Client;
using WikiClientLibrary.Sites;
using WikiClientLibrary.Pages;
using WikiClientLibrary;
using System.IO;
using WikiClientLibrary.Files;
using System.Linq;

namespace Artificer
{
	//uses https://github.com/CXuesong/WikiClientLibrary/wiki/%5BMediaWiki%5D-Getting-started
	public class ArtifactWikiBot
	{
		public string WikiBaseURL { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }

		private WikiClient Client { get; set; }
		private WikiSite Site { get; set; }

		public ArtifactWikiBot(string url, string user, string pass)
		{
			WikiBaseURL = url;
			Username = user;
			Password = pass;
		}

		public void Initialize()
		{
			ConnectAsync().Wait();
		}

		public void End()
		{
			DisconnectAsync().Wait();
		}

		public void UploadFile(string filename, string localName)
		{
			UploadFileAsync(filename, localName).Wait();
		}

		public IEnumerable<WikiPage> DownloadArticles(IEnumerable<string> titles)
		{
			List<List<string>> batches = new List<List<string>>();
			int batchCount = titles.Count() / 50;
			if(titles.Count() % 50 != 0)
			{
				batchCount++;
			}
			for(int i = 1; i <= batchCount; i++)
			{
				int start = (i - 1) * 50;
				batches.Add(new List<string>(titles.Skip(start).Take(50)));
			}

			List<WikiPage> result = new List<WikiPage>();

			foreach(var batch in batches)
			{
				var task = GetPagesAsync(batch);
				task.Wait();
				result.AddRange(task.Result);
			}

			return result;			
		}

		private async Task ConnectAsync()
		{
			// A WikiClient has its own CookieContainer.
			Client = new WikiClient
			{
				ClientUserAgent = "WCLQuickStart/1.0 (your user name or contact information here)"
			};
			// You can create multiple WikiSite instances on the same WikiClient to share the state.
			Site = new WikiSite(Client, $"{WikiBaseURL}/api.php");
			// Wait for initialization to complete.
			// Throws error if any.
			await Site.Initialization;
			try
			{
				await Site.LoginAsync(Username, Password);
			}
			catch (WikiClientException ex)
			{
				Console.WriteLine(ex.Message);
				// Add your exception handler for failed login attempt.
			}

			// Do what you want
			Console.WriteLine(Site.SiteInfo.SiteName);
			Console.WriteLine(Site.AccountInfo);
			Console.WriteLine("{0} extensions", Site.Extensions.Count);
			Console.WriteLine("{0} interwikis", Site.InterwikiMap.Count);
			Console.WriteLine("{0} namespaces", Site.Namespaces.Count);
		}

		private async Task DisconnectAsync()
		{
			// We're done here
			await Site.LogoutAsync();
			Client.Dispose();        // Or you may use `using` statement.
		}



		private async Task<IEnumerable<WikiPage>> GetPagesAsync(IEnumerable<string> titles)
		{
			if (Client == null)
				throw new InvalidOperationException("Wiki client has not been initialized!  Call Initialize() first.");

			var pages = titles.Select(title => new WikiPage(Site, title)).ToArray();

			await pages.RefreshAsync(PageQueryOptions.FetchContent | PageQueryOptions.ResolveRedirects);
			return pages;
		}

		private async Task UploadFileAsync(string filename, string localName)
		{
			if (Client == null)
				throw new InvalidOperationException("Wiki client has not been initialized!  Call Initialize() first.");

			if(!File.Exists(filename))
			{
				Console.WriteLine($"{filename} does not exist!  Cannot upload.");
				return;
			}

			using (var s = File.OpenRead(filename))
			{
				var source = new StreamUploadSource(s);
				var suppressWarnings = false;
				try
				{
					var result = await Site.UploadAsync($"File:{localName}", source, "bot upload test", suppressWarnings);
					if (result.ResultCode == UploadResultCode.Warning)
					{
						if(result.Warnings.TitleExists && result.Warnings.DuplicateVersions.Count == 0)
						{
							//source = new StreamUploadSource(s);
							s.Seek(0, SeekOrigin.Begin);
							result = await Site.UploadAsync($"File:{localName}", source, "bot upload test", true);
						}
						Console.WriteLine(result.Warnings.ToString());
					}
				}
				catch (OperationFailedException ex)
				{
					// Since MW 1.31, if you are uploading the exactly same content to the same title with ignoreWarnings set to true, you will reveive this exception with ErrorCode set to fileexists-no-change.
					// See https://gerrit.wikimedia.org/r/378702.
					Console.WriteLine(ex.Message);
				}
			}
		}


	}
}
