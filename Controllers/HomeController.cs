using MayerTest.Models; 
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks; 
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace MayerTest.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index(string SearchString ="", int page = 1)
		{ 
			HttpContext.Session.SetString("login", SearchString);  
			List<Item> accounts = new List<Item>();
			try
			{
				if (accounts.Count() <= 0 && String.IsNullOrEmpty(SearchString))
				{
					var str = HttpContext.Session.GetString("account");
					if (!String.IsNullOrEmpty(str))
					{
						var obj = JsonConvert.DeserializeObject<List<Item>>(str);
						accounts = obj;
					}
				}
				else
				{

					accounts = getUserGithub(SearchString).items.ToList();

					HttpContext.Session.SetString("account", JsonConvert.SerializeObject(accounts));
				}
			}
			catch (Exception ex)
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
		public IActionResult Repository(string login, int page = 1)
		{
			this.ViewBag.login = HttpContext.Session.GetString("login").ToString();
			this.ViewBag.account = login;
			List<Account> repos = new List<Account>();
			try
			{
				if (repos.Count() <= 0 && String.IsNullOrEmpty(login))
				{
					var str = HttpContext.Session.GetString("repos");
					if (!String.IsNullOrEmpty(str))
					{
						var obj = JsonConvert.DeserializeObject<List<Account>>(str);
						repos = obj;
					}
				}
				else
				{

					repos = getData(login).ToList();

					HttpContext.Session.SetString("repos", JsonConvert.SerializeObject(repos));
				} 
				const int pageSize = 16;
				if (page < 1)
				{
					page = 1;
				}

				int recsCount = repos.Count();
				var pager = new Pager(recsCount, page, pageSize);
				int recSkip = (page - 1) * pageSize;
				var data = repos.Skip(recSkip).Take(pager.PageSize).ToList();
				this.ViewBag.pager = pager;
				return View(data); 
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
		public UserGithub getUserGithub(string SearchString)
		{
			UserGithub members = null;
			try
			{
				using (var client = new HttpClient())
				{

					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(
						new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
					client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
					var responseTask = client.GetAsync("https://api.github.com/search/users?q=" + SearchString);
					responseTask.Wait();

					//To store result of web api response.   
					var result = responseTask.Result;


					//If success received   
					if (result.IsSuccessStatusCode)
					{ 
						var readTask = result.Content.ReadAsAsync<UserGithub>();
						readTask.Wait();

						 members = readTask.Result;
					}
					else
					{
						//Error response received   
						members = new UserGithub();
						ModelState.AddModelError(string.Empty, "Server error try after some time.");
					}
				}



			}
			catch (Exception ex)
			{
				string s = ex.ToString();
			}
			return members;
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
