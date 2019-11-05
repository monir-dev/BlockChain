using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlockChain.Api;
using Microsoft.AspNetCore.Mvc;
using BlockChain.Models;

namespace BlockChain.Controllers
{
    public class HomeController : Controller
    {
        private static CryptoCurrency blockChain = BlockChainController.blcokChain; // new CryptoCurrency();

        public IActionResult Index()
        {
            List<Transaction> transactions = blockChain.GetTransactions();
            ViewBag.Transactions = transactions;

            List<Block> blocks = blockChain.GetBlocks();
            ViewBag.Blocks = blocks;

            return View();
        }

        public IActionResult Mine()
        {
            blockChain.Mine();
            return RedirectToAction("Index");
        }

        public IActionResult Configure()
        {
            return View(blockChain.GetNodes());
        }


        [HttpPost]
        public IActionResult RegisterNodes(string nodes)
        {
            string[] node = nodes.Split(",");
            blockChain.RegisterNodes(node);
            return RedirectToAction("Index");
        }

        public IActionResult CoinBase()
        {
            List<Block> blocks = blockChain.GetBlocks();
            ViewBag.Blocks = blocks;

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}