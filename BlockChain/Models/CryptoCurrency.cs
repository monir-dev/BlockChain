using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RSA;

namespace BlockChain.Models
{
    public class CryptoCurrency
    {
        private List<Transaction> _currentTransactions { get; set; }
        private List<Block> _chain { get; set; }
        private List<Node> _Nodes { get; set; }
        private Block _LastBlock => _chain.Last();

        public string NodeId { get; set; }
        public int blockCount { get; set; }
        public decimal reward { get; set; }

        private static string minerPrivateKey = "";
        private static Wallet _minersWallet = RSA.RSA.KeyGenerate();


        public CryptoCurrency()
        {
            _currentTransactions = new List<Transaction>();
            _chain = new List<Block>();
            _Nodes = new List<Node>();

            minerPrivateKey = "L3aq7WPiSois3N&GxTr6ZSXMNdfbAZWNebiYvKK6hAUBijk95rL"; //_minersWallet.PrivateKey;
            NodeId = "18jp31DcT3n5vsYHGVhhQa3qsvEve4EUoQ"; //_minersWallet.PublicKey;

//            string sign = RSA.RSA.Sign(minerPrivateKey, "h");
//            bool b = RSA.RSA.Verify(NodeId, "h", sign);
            
            // initial transaction
            var transaction = new Transaction { Sender = "0", Recipient = NodeId, Amount = 50, Fees = 0, Signature = "" };
            _currentTransactions.Add(transaction);

            CreateNewBlock(proof: 100, previousHash: "1"); //genesis block
        }

        private void RegisterNode(string address)
        {
            _Nodes.Add(new Node { Address = new Uri(address) });
        }

        private Block CreateNewBlock(int proof, string previousHash = null)
        {
            var block = new Block
            {
                Index = _chain.Count,
                Timestamp = DateTime.UtcNow,
                Transactions = _currentTransactions.ToList(),
                Proof = proof,
                PreviousHash = previousHash ?? GetHash(_chain.Last())
            };
            
            _currentTransactions.Clear();
            _chain.Add(block);
            return block;
        }

        private string GetHash(Block block)
        {
            string blockText = JsonConvert.SerializeObject(block);
            return GetSha256(blockText);
        }
        
        private string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (var x in hash)
            {
                hashBuilder.Append($"{x:x2}");
            }

            return hashBuilder.ToString();
        }

        private int CreateProofOfWork(string previousHash)
        {
            int proof = 0;
            while (!IsValidProof(_currentTransactions, proof, previousHash))
            {
                proof++;
            }

            if (blockCount == 10)
            {
                blockCount = 0;
                reward = reward / 2;
            }
            
            var transaction = new Transaction
            {
                Sender = "0",
                Recipient = NodeId,
                Amount = reward,
                Fees = 0,
                Signature = ""
            };
            _currentTransactions.Add(transaction);
            
            //CreateTransaction(sender: "0", recipient: NodeId, amount: reward, signture: "");//reward
            blockCount++;
            return proof;
        }

        private bool IsValidProof(List<Transaction> transactions, int proof, string previoushash)
        {
            var signatures = transactions.Select(x => x.Signature).ToArray();
            string guess = $"{signatures}{proof}{previoushash}";
            string result = GetSha256(guess);
            return result.StartsWith("00");//difficulty
        }

        public bool Verify_transaction_signature(Transaction transaction, string signedMessage, string publicKey)
        {
            string originalMessage = transaction.ToString();
            bool verified = RSA.RSA.Verify(publicKey, originalMessage, signedMessage);
            return verified;
        }

        private List<Transaction> TransactionByAddress(string ownerAddress)
        {
            List<Transaction> trns = new List<Transaction>();
            foreach (var block in _chain.OrderByDescending(x => x.Index))
            {
                var ownerTransactions =
                    block.Transactions.Where(x => x.Sender == ownerAddress || x.Recipient == ownerAddress);
                trns.AddRange(ownerTransactions);
            }

            return trns;
        }

        public bool HasBalance(Transaction transaction)
        {
            var trns = TransactionByAddress(transaction.Sender);
            decimal balance = 0;
            foreach (var item in trns)
            {
                if (item.Recipient == transaction.Sender)
                {
                    balance = balance + item.Amount;
                }
                else
                {
                    balance = balance - item.Amount;
                }
            }

            return balance >= (transaction.Amount + transaction.Fees);
        }

        private void AddTransaction(Transaction transaction)
        {
            _currentTransactions.Add(transaction);

            if (transaction.Sender != NodeId)
            {
                _currentTransactions.Add(new Transaction
                {
                    Sender = transaction.Sender,
                    Recipient = NodeId,
                    Amount = transaction.Fees,
                    Signature = "",
                    Fees = 0
                });
            }
        }
    }
}