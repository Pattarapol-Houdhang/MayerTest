using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MayerTest.Models
{
	public class UserGithub
	{

        public List<Item> items { get; set; }

    } 
    public class Item
    {
        public string login { get; set; } 
        public string avatar_url { get; set; } 
        public double score { get; set; }
    } 
}
