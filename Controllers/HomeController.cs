using MayerTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MayerTest.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		List<Account> accounts = new List<Account>();
		public IActionResult Index(string SearchString, int page = 1)
		{
			try
			{
				if (accounts.Count() <= 0 && String.IsNullOrEmpty(SearchString))
				{
					var str = HttpContext.Session.GetString("account");
					if (!String.IsNullOrEmpty(str))
					{
						var obj = JsonConvert.DeserializeObject<List<Account>>(str);
						accounts = obj;
					}
				}
				else
				{

					accounts = getData(SearchString).ToList();

					HttpContext.Session.SetString("account", JsonConvert.SerializeObject(accounts));
				}
			}
			catch (Exception)
			{
			}
			try
			{
				//if (accounts.Count() <= 0)
				//{

				//	accounts = getData(ViewData["SearchString"] as string).ToList();
				//}
				const int pageSize = 16;
				if (page < 1)
				{
					page = 1;
				}

				int recsCount = accounts.Count();
				var pager = new Pager(recsCount, page, pageSize);
				int recSkip = (page - 1) * pageSize;
				var data = accounts.Skip(recSkip).Take(pager.PageSize).ToList();
				this.ViewBag.pager = pager;
				return View(data);
				//return View(accounts);
			}
			catch (Exception)
			{

				return View();
			}
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IEnumerable<Account> getData(string dev_name)
		{
			IEnumerable<Account> members = null;

			using (var client = new HttpClient())
			{

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
				client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

				client.BaseAddress = new Uri("http://api.github.com/users/" + dev_name + "/");


				var responseTask = client.GetAsync("repos");
				responseTask.Wait();

				//To store result of web api response.   
				var result = responseTask.Result;

				//If success received   
				if (result.IsSuccessStatusCode)
				{
					var readTask = result.Content.ReadAsAsync<IList<Account>>();
					readTask.Wait();

					members = readTask.Result;
				}
				else
				{
					//Error response received   
					members = Enumerable.Empty<Account>();
					ModelState.AddModelError(string.Empty, "Server error try after some time.");
				}
			}
			return members;
		}
	}
}
