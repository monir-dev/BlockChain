using System;
using System.Text;
using System.Threading;
using NBitcoin;
using NBitcoin.Protocol;
using Newtonsoft.Json;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace _01Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //var network = Network.TestNet;
            //var privateKey = new Key();
            //var bitcoinPrivateKey = privateKey.GetWif(network);
            //var address = bitcoinPrivateKey.GetAddress();
            //Console.WriteLine(bitcoinPrivateKey);
            //Console.WriteLine(address);



            var bitcoinPrivateKey = new BitcoinSecret("cN5YQMWV8y19ntovbsZSaeBxXaVPaK4n7vapp4V56CKx5LhrK2RS");
            //var bitcoinPrivateKey = new BitcoinSecret("cNtGg1k1PoyoMnZwBvqkWHFKT3R2nWhMnW5Mw1gULuqS2iURxh8E"); // Testnet
            //var bitcoinPrivateKey = new BitcoinSecret("KwJaEZnTZPZ1wSsFdzMrcCvfCh5bp6CyqJrep4ERT84WJ5pAvyTE"); // Mainnet
            var network = bitcoinPrivateKey.Network;
            var address = bitcoinPrivateKey.GetAddress();
            
            var client = new QBitNinjaClient(network);
            //var transactionId = uint256.Parse("0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a");
            var transactionId = uint256.Parse("481e4ba7cc989970052b669693a1b74bc75d7910a3279a09335c6052bc18bcbb");
            var transactionResponse = client.GetTransaction(transactionId).Result;

            var receivedCoins = transactionResponse.ReceivedCoins;
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            Console.WriteLine("We want to spend {0}. outpoint:", outPointToSpend.N + 1);



            var transaction = Transaction.Create(network);
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });


            var hallOfTheMakersAddress = BitcoinAddress.Create("mzp4No5cmCXjZUpf112B1XWsvWBfws5bbB", network);

            var hallOfTheMakersAmount = new Money(0.0004m, MoneyUnit.BTC);
            var minerFee = new Money(0.00007m, MoneyUnit.BTC);

            var txInAmount = (Money)receivedCoins[(int)outPointToSpend.N].Amount;
            var changeAmount = txInAmount - hallOfTheMakersAmount - minerFee;
            TxOut hallOfTheMakersTxOut = new TxOut()
            {
                Value = hallOfTheMakersAmount,
                ScriptPubKey = hallOfTheMakersAddress.ScriptPubKey
            };

            TxOut changeTxOut = new TxOut()
            {
                Value = changeAmount,
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };
            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeTxOut);

            var message = "Long live test";
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });

            transaction.Inputs[0].ScriptSig = bitcoinPrivateKey.ScriptPubKey;

            
            transaction.Sign(bitcoinPrivateKey, receivedCoins.ToArray());

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (!broadcastResponse.Success)
            {
                Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                Console.WriteLine(transaction.GetHash());
            }

        }
    }
}
