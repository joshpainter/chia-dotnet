using System;
using System.Net.Security;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace chia.dotnet
{
    /// <summary>
    /// Base class that handles core communication with the rpc endpoint using http(s)
    /// and synchronizes request and response messages
    /// </summary>
    public class HttpRpcClient : IDisposable, IRpcClient
    {
        private readonly SocketsHttpHandler _httpHandler = new();
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _receiveCancellationTokenSource = new();

        private bool disposedValue;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="endpoint">Details of the websocket endpoint</param>        
        public HttpRpcClient(EndpointInfo endpoint)
        {
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

            _httpHandler.SslOptions.ClientCertificates = CertLoader.GetCerts(Endpoint.CertPath, Endpoint.KeyPath);
            _httpHandler.SslOptions.RemoteCertificateValidationCallback += ValidateServerCertificate;
            _httpClient = new(_httpHandler);
            _httpClient.BaseAddress = Endpoint.Uri;
        }

        /// <summary>
        /// Details of the RPC service endpoint
        /// </summary>
        public EndpointInfo Endpoint { get; init; }

        /// <summary>
        /// Opens the websocket and starts the receive loop
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>Awaitable Task</returns>
        public async Task Connect(CancellationToken cancellationToken = default)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(WebSocketRpcClient));
            }

            // since each every rpc endpoint shares this readonly method
            // we'll use get_connections to make sure the endpoint is up and basic sanity checks
            using var response = await _httpClient.PostAsJsonAsync<string>("get_connections", "{}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            OnConnected();
        }

        /// <summary>
        /// Called after <see cref="Connect(CancellationToken)"/> completes successfully. Lets derived classess know that they can do
        /// post connection initialization 
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        /// Cancels the receive loop and closes the websocket
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        public async Task Close(CancellationToken cancellationToken = default)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(WebSocketRpcClient));
            }

            _receiveCancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Posts a <see cref="Message"/> to the websocket but does not wait for a response
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to post</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <remarks>Awaiting this method waits for the message to be sent only. It doesn't await a response.</remarks>
        /// <returns>Awaitable <see cref="Task"/></returns>
        public async Task PostMessage(Message message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(WebSocketRpcClient));
            }
        }

        /// <summary>
        /// Sends a <see cref="Message"/> to the websocket and waits for a response
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to send</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <remarks>Awaiting this method will block until a response is received from the <see cref="WebSocket"/> or the <see cref="CancellationToken"/> is cancelled</remarks>
        /// <returns>The response message</returns>
        /// <exception cref="ResponseException">Throws when <see cref="Message.IsSuccessfulResponse"/> is False</exception>
        public async Task<Message> SendMessage(Message message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(WebSocketRpcClient));
            }

            return null;
        }

        /// <summary>
        /// Event raised when a message is received from the endpoint that was either not in response to a request
        /// or was a response from a posted message (i.e. we didn't register to receive the response)
        /// Pooling state_changed messages come through this event
        /// </summary>
        public event EventHandler<Message> BroadcastMessageReceived;

        /// <summary>
        /// Raises the <see cref="BroadcastMessageReceived"/> event
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        protected virtual void OnBroadcastMessageReceived(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Debug.WriteLine("Broadcast message:");
            Debug.WriteLine(message.ToJson());

            BroadcastMessageReceived?.Invoke(this, message);
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // uncomment these checks to change remote cert validaiton requirements

            // require remote ca to be trusted on this machine
            //if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors) 
            //    return false;

            // require server name to be validated in the cert
            //if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
            //    return false;

            return !((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable);
        }

        /// <summary>
        /// Called when the instance is being disposed or finalized
        /// </summary>
        /// <param name="disposing">Invoke from <see cref="IDisposable.Dispose"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _receiveCancellationTokenSource.Cancel();
                    _httpClient.Dispose();
                    _receiveCancellationTokenSource.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
