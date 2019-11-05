using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockChain.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockChain.Api
{
    [EnableCors("MyPolicy")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BlockChainController : ControllerBase
    {
        public static CryptoCurrency blcokChain = new CryptoCurrency();


        [HttpPost("transactions/new")]
        public IActionResult NewTransaction([FromBody] Transaction transaction)
        {
            var response = blcokChain.CreateTransaction(transaction);
            return Ok(response);
        }

        [HttpGet("transactions/get")]
        public IActionResult GetTransaction()
        {
            var response = new { transactions = blcokChain.GetTransactions() };
            return Ok(response);
        }

        [HttpGet("chain")]
        public IActionResult FullChain()
        {
            var blocks = blcokChain.GetBlocks();
            var response = new
            {
                chain = blocks,
                length = blocks.Count
            };
            return Ok(response);
        }

        [HttpGet("mine")]
        public IActionResult Mine()
        {
            var block = blcokChain.Mine();
            var response = new
            {
                message = "New Block Forged",
                block_number = block.Index,
                transactions = block.Transactions.ToArray(),
                nonce = block.Proof,
                previous_hash = block.PreviousHash
            };
            return Ok(response);
        }

        [HttpPost("nodes/register")]
        public IActionResult RegisterNodes(string[] nodes)
        {
            blcokChain.RegisterNodes(nodes);

            var response = new
            {
                message = "New nodes are been added",
                total_nodes = nodes.Length // 'total_nodes' : [node for node in blockChain.nodes],
            };
            return Created("",response);
        }

        [HttpGet("nodes/resolve")]
        public IActionResult Consensus()
        {
            return Ok(blcokChain.Consensus());
        }

        [HttpGet("nodes/get")]
        public IActionResult GetNodes()
        {
            return Ok(new { nodes = blcokChain.GetNodes() });
        }

        /// <summary>
        ///  Show miner Public and Private Key
        /// </summary>
        /// <returns></returns>
        [HttpGet("wallet/miner")]
        public IActionResult GetMinerWallet()
        {
            return Ok(blcokChain.GetMinerWallet());
        }

    }
}