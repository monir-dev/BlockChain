using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockChainC.Models;
using Newtonsoft.Json;
using RestSharp;

namespace BlockChainC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MakeTransaction()
        {
            return View();
        }

        public IActionResult ViewTransaction()
        {
            return View();
        }

        public IActionResult WalletTransaction()
        {
            return View(new List<Transaction>());
        }

        [HttpPost]
        public IActionResult WalletTransaction(string publicKey)
        {
            var url = new Uri("http://localhost:58811" + "/api/BlockChain/chain");
            var blocks = GetChain(url);
            ViewBag.publicKey = publicKey;

            return View(TransactionByAddress(publicKey, blocks));
        }

        public List<Transaction> TransactionByAddress(string ownerAddress, List<Block> chain)
        {
            List<Transaction> trns = new List<Transaction>();
            foreach (var block in chain.OrderByDescending(x => x.Index))
            {
                var ownerTransactions =
                    block.Transactions.Where(x => x.Sender == ownerAddress || x.Recipient == ownerAddress);
                trns.AddRange(ownerTransactions);
            }
            return trns;
        }

        [HttpPost]
        public IActionResult ViewTransaction(string node_url)
        {
            var url = new Uri(node_url + "/api/BlockChain/chain");
            ViewBag.Blocks = GetChain(url);
            return View();
        }

        private List<Block> GetChain(Uri url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var model = new
                {
                    chain = new List<Block>(),
                    length = 0
                };
                string json = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()).ReadToEnd();
                var data = JsonConvert.DeserializeAnonymousType(json, model);

                return data.chain;
            }

            return null;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
