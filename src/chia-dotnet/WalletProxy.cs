﻿using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace chia.dotnet
{
    /// <summary>
    /// Proxy that communicates with the wallet via the daemon
    /// </summary>
    public class WalletProxy : ServiceProxy
    {
        /// <summary>
        /// Default location for backups
        /// </summary>
        public const string DefaultBackupHost = "https://backup.chia.net";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="daemon">The <see cref="Daemon"/> to handle RPC</param>
        public WalletProxy(Daemon daemon)
            : base(daemon, ServiceNames.Wallet)
        {
        }

        /// <summary>
        /// Sets a key to active.
        /// </summary>
        /// <param name="fingerprint">The fingerprint</param>          
        /// <param name="skipImport">Indicator whether to skip the import at login</param>          
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>a key fingerprint</returns>
        public async Task<dynamic> LogIn(uint fingerprint, bool skipImport, CancellationToken cancellationToken)
        {
            dynamic data = new ExpandoObject();
            data.fingerprint = fingerprint;
            data.type = skipImport ? "skip" : "normal";
            data.host = DefaultBackupHost;

            var message = CreateMessage("log_in", data);
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.fingerprint;
        }

        /// <summary>
        /// Sets a key to active.
        /// </summary>
        /// <param name="fingerprint">The fingerprint</param>
        /// <param name="filePath">The path to the backup file</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>a key fingerprint</returns>
        public async Task<dynamic> LogInAndRestoreBackup(uint fingerprint, string filePath, CancellationToken cancellationToken)
        {
            dynamic data = new ExpandoObject();
            data.fingerprint = fingerprint;
            data.type = "restore_backup";
            data.file_path = filePath;
            data.host = DefaultBackupHost;

            var message = CreateMessage("log_in", data);
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.fingerprint;
        }

        /// <summary>
        /// Get the list of wallets
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>A list of wallets</returns>
        public async Task<IEnumerable<dynamic>> GetWallets(CancellationToken cancellationToken)
        {
            var message = CreateMessage("get_wallets");
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.wallets;
        }

        /// <summary>
        /// Get the balance of a specific wallet
        /// </summary>
        /// <param name="walletId">The numeric id of the wallet to query</param>        
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The wallet balance</returns>
        public async Task<dynamic> GetWalletBalance(uint walletId, CancellationToken cancellationToken)
        {
            dynamic data = new ExpandoObject();
            data.wallet_id = walletId;

            var message = CreateMessage("get_wallet_balance", data);
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.wallet_balance;
        }

        /// <summary>
        /// Get all root public keys accessible by the wallet
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>all root public keys accessible by the walle</returns>
        public async Task<IEnumerable<dynamic>> GetPublicKeys(CancellationToken cancellationToken)
        {
            var message = CreateMessage("get_public_keys");
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.public_key_fingerprints;
        }

        /// <summary>
        /// Get the private key accessible by the wallet
        /// </summary>
        /// <param name="fingerprint">The fingerprint</param>          
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>a private key</returns>
        public async Task<dynamic> GetPrivateKey(uint fingerprint, CancellationToken cancellationToken)
        {
            dynamic data = new ExpandoObject();
            data.fingerprint = fingerprint;

            var message = CreateMessage("get_private_key", data);
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.private_key;
        }

        /// <summary>
        /// Get the wallet's sync status
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>sync status</returns>
        public async Task<dynamic> GetSyncStatus(CancellationToken cancellationToken)
        {
            var message = CreateMessage("get_sync_status");
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data;
        }

        /// <summary>
        /// Retrieves some information about the current network
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>network name and prefix</returns>
        public async Task<dynamic> GetNetworkInfo(CancellationToken cancellationToken)
        {
            var message = CreateMessage("get_network_info");
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data;
        }

        /// <summary>
        /// Get blockchain height info
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>Current block height</returns>
        public async Task<uint> GetHeightInfo(CancellationToken cancellationToken)
        {
            var message = CreateMessage("get_height_info");
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.height;
        }

        /// <summary>
        /// Get the list of transactions
        /// </summary>
        /// <param name="walletId">The numeric id of the wallet to query</param> 
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>A list of transactions</returns>
        public async Task<IEnumerable<dynamic>> GetTransactions(uint walletId, CancellationToken cancellationToken)
        {
            dynamic data = new ExpandoObject();
            data.wallet_id = walletId;

            var message = CreateMessage("get_transactions", data);
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.transactions;
        }

        /// <summary>
        /// Get the list of transactions
        /// </summary>
        /// <param name="walletId">The numeric id of the wallet to query</param> 
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>A list of transactions</returns>
        public async Task<string> GetNextAddress(uint walletId, bool newAddress, CancellationToken cancellationToken)
        {
            dynamic data = new ExpandoObject();
            data.wallet_id = walletId;
            data.new_address = newAddress;

            var message = CreateMessage("get_next_address ", data);
            var response = await Daemon.SendMessage(message, cancellationToken);

            return response.Data.address;
        }
    }
}
