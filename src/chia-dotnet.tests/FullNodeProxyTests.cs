﻿using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace chia.dotnet.tests
{
    /// <summary>
    /// This class is a test harness for interation with an actual daemon instance
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class FullNodeProxyTests
    {
        private static FullNodeProxy _theFullNode;

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            using var cts = new CancellationTokenSource(15000);
            var rpcClient = Factory.CreateRpcClientFromHardcodedLocation();
            await rpcClient.Connect(cts.Token);

            var daemon = new DaemonProxy(rpcClient, "unit_tests");
            await daemon.RegisterService(cts.Token);

            _theFullNode = new FullNodeProxy(rpcClient, "unit_tests");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            _theFullNode.RpcClient?.Dispose();
        }

        [TestMethod]
        public async Task GetBlockChainState()
        {
            using var cts = new CancellationTokenSource(15000);
            var state = await _theFullNode.GetBlockchainState(cts.Token);

            Assert.IsNotNull(state);
        }

        [TestMethod]
        public async Task GetBlock()
        {
            using var cts = new CancellationTokenSource(15000);
            var block = await _theFullNode.GetBlock("0x09d2eeda4845ec7160142b4b30ae8b3998cf5abab62a999d2ded35879945abcb", cts.Token);

            Assert.IsNotNull(block);
        }

        [TestMethod]
        public async Task GetBlockRecord()
        {
            using var cts = new CancellationTokenSource(15000);
            var record = await _theFullNode.GetBlockRecord("0x09d2eeda4845ec7160142b4b30ae8b3998cf5abab62a999d2ded35879945abcb", cts.Token);

            Assert.IsNotNull(record);
        }

        [TestMethod]
        public async Task GetBlocks()
        {
            using var cts = new CancellationTokenSource(30000);
            var state = await _theFullNode.GetBlockchainState(cts.Token);
            Assert.IsNotNull(state.Peak, "peak not retreived yet");
            var blocks = await _theFullNode.GetBlocks(state.Peak.Height - 5, state.Peak.Height - 1, false, cts.Token);

            Assert.IsNotNull(blocks);
        }

        [TestMethod()]
        public async Task GetNetworkInfo()
        {
            using var cts = new CancellationTokenSource(15000);
            var info = await _theFullNode.GetNetworkInfo(cts.Token);

            Assert.IsNotNull(info);
        }

        [TestMethod]
        public async Task GetNetworkSpace()
        {
            using var cts = new CancellationTokenSource(15000);
            var space = await _theFullNode.GetNetworkSpace("0xcb5c085a1f0259ab5581ebfce219f82cac9ec288da29665ce31e21a5b5856089", "0x9edc235dfcb12a14e20e8f83b53060d067d97d217d6f9a3420fc9dbb470040fe", cts.Token);
            Assert.IsNotNull(space);
        }

        [TestMethod]
        public async Task Ping()
        {
            using var cts = new CancellationTokenSource(15000);
            await _theFullNode.Ping(cts.Token);
        }

        [TestMethod]
        public async Task GetConnections()
        {
            using var cts = new CancellationTokenSource(15000);
            var connections = await _theFullNode.GetConnections(cts.Token);
            Assert.IsNotNull(connections);
        }

        [TestMethod]
        public async Task OpenConnection()
        {
            using var cts = new CancellationTokenSource(15000);
            await _theFullNode.OpenConnection("testnet-node.chia.net", 58444, cts.Token);
        }

        [TestMethod()]
        public async Task GetBlockRecordByHeight()
        {
            using var cts = new CancellationTokenSource(30000);
            var state = await _theFullNode.GetBlockchainState(cts.Token);
            Assert.IsNotNull(state.Peak, "peak not retreived yet");
            var blockRecord = await _theFullNode.GetBlockRecordByHeight(state.Peak.Height - 1, cts.Token);
            Assert.IsNotNull(blockRecord);
        }

        [TestMethod()]
        public async Task GetBlockRecords()
        {
            using var cts = new CancellationTokenSource(30000);
            var state = await _theFullNode.GetBlockchainState(cts.Token);
            Assert.IsNotNull(state.Peak, "peak not retreived yet");
            var blockRecords = await _theFullNode.GetBlockRecords(state.Peak.Height - 5, state.Peak.Height - 1, cts.Token);
            Assert.IsNotNull(blockRecords);
        }

        [TestMethod()]
        public async Task GetUnfinishedBlockHeaders()
        {
            using var cts = new CancellationTokenSource(15000);
            var blockHeaders = await _theFullNode.GetUnfinishedBlockHeaders(cts.Token);
            Assert.IsNotNull(blockHeaders);
        }

        [TestMethod()]
        public async Task GetCoinRecordsByPuzzleHash()
        {
            using var cts = new CancellationTokenSource(15000);
            var records = await _theFullNode.GetCoinRecordsByPuzzleHash("0xb5a83c005c4ee98dc807a560ea5bc361d6d3b32d2f4d75061351d1f6d2b6085f", true, cts.Token);
            Assert.IsNotNull(records);
        }

        [TestMethod()]
        public async Task GetCoinRecordByName()
        {
            using var cts = new CancellationTokenSource(15000);
            var coinRecord = await _theFullNode.GetCoinRecordByName("0x2b83ca807d305cd142e0e91d4e7a18f8e57df0ac6b4fa403bff249d0d491c609", cts.Token);
            Assert.IsNotNull(coinRecord);
        }

        [TestMethod()]
        public async Task GetAdditionsAndRemovals()
        {
            using var cts = new CancellationTokenSource(15000);
            var additionsAndRemovals = await _theFullNode.GetAdditionsAndRemovals("68a8e65132da09549ef764c20bfb2b9e5b7e705d5e6bae78cfceab889389ff13", cts.Token);
            Assert.IsNotNull(additionsAndRemovals);
        }

        [TestMethod()]
        public async Task GetAllMempoolItems()
        {
            using var cts = new CancellationTokenSource(15000);
            var items = await _theFullNode.GetAllMempoolItems(cts.Token);
            Assert.IsNotNull(items);
        }

        [TestMethod()]
        public async Task GetAllMemmpoolTxIds()
        {
            using var cts = new CancellationTokenSource(15000);
            var ids = await _theFullNode.GetAllMemmpoolTxIds(cts.Token);
            Assert.IsNotNull(ids);
        }

        [TestMethod()]
        public async Task GetMemmpooItemByTxId()
        {
            using var cts = new CancellationTokenSource(15000);
            var ids = await _theFullNode.GetAllMemmpoolTxIds(cts.Token);
            Assert.IsNotNull(ids);
            Assert.IsTrue(ids.Count() > 0);

            var item = await _theFullNode.GetMemmpooItemByTxId((string)ids.First(), cts.Token);
            Assert.IsNotNull(item);
        }

        [TestMethod()]
        public async Task GetRecentSignagePoint()
        {
            using var cts = new CancellationTokenSource(15000);
            var sp = await _theFullNode.GetRecentSignagePoint("0xf3ca7a33ce723b38c9a72156252b3b2395ead751213eef5d8ed40c941c6a9017", cts.Token);
            Assert.IsNotNull(sp);
        }
    }
}
