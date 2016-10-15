using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BRat {

    delegate void ClientConnectedHandler(object sender, ClientEventArgs args);
    delegate void DataReceivedHandler(object sender, DataReceivedEventArgs args);
    delegate void ClientDisconnectedHandler(object sender, ClientEventArgs args);

    struct SocketInfo {
        public Socket socket;
        public bool isOnline;

        public SocketInfo(Socket socket, bool isOnline = false) {
            this.socket = socket;
            this.isOnline = isOnline;
        }
    }

    /// <summary>
    /// Simple server class. It can Accept new clients,
    /// Receives data from client and returns string data from client.
    /// </summary>
    class Server {

        public event ClientConnectedHandler OnClientConnected; // You guess it right!
        public event DataReceivedHandler OnDataReceived; // Yup.
        public event ClientDisconnectedHandler OnClientDisconnected; // Hehe

        private int port; // Server socket port
        private Socket socket; // Server socket
        private List<SocketInfo> connectedSockets; // Connected clients
        private Socket selectedSocket; // Current selected client
        private byte[] buffer; // Buffer for receiving data from client

        private Thread listenThread; // Thread for listening
        private Thread checkThread; // Thread for check sockets. ie. If socket is disconnected etc.

        /// <summary>
        /// Returns connected clients enumerator for looping through.
        /// </summary>
        /// <returns>Connected clients enumerator.</returns>
        public IEnumerator GetClientsEnumerator() {
            return connectedSockets.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets selected socket.
        /// </summary>
        public Socket SelectedSocket {
            get {
                return selectedSocket;
            } set {
                if(connectedSockets.FindIndex(x => x.socket == value) >= 0) {
                    selectedSocket = value;
                }
            }
        }

        /// <summary>
        /// Constructor for server.
        /// </summary>
        /// <param name="_port"></param>
        public Server(int _port) {
            port = _port;
            connectedSockets = new List<SocketInfo>();
        }

        /// <summary>
        /// Simply this function is starts server.
        /// Binds socket with port and Starts listening for clients.
        /// </summary>
        public void StartServer() {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
            checkThread = new Thread(new ThreadStart(CheckSockets));
            checkThread.Start();
        }

        /// <summary>
        /// Listens for client. If client trys to connect
        /// ClientConnectedCallback runs.
        /// </summary>
        private void Listen() {
            socket.Listen(0);
            socket.BeginAccept(new AsyncCallback(ClientConnectedCallback), null);
        }

        /// <summary>
        /// Gets data from selectedSocket.
        /// When data received, it calls DataReceivedCallback function.
        /// </summary>
        private void Receive() {
            try {
                buffer = new byte[selectedSocket.ReceiveBufferSize];
                selectedSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(DataReceivedCallback), null);
            }
            catch(Exception) { }
        }

        /// <summary>
        /// Checks all connected sockets. If a socket is disconnected fires OnClientDisconnected event.
        /// </summary>
        private void CheckSockets() {
            try {
                IEnumerator enumerator = GetClientsEnumerator();
                while(enumerator.MoveNext()) {
                    SocketInfo current = (SocketInfo)enumerator.Current;
                    if(!IsConnected(current.socket)) {
                        SocketInfo info = connectedSockets.Find(x => x.socket == current.socket);
                        info.isOnline = false;
                        if(selectedSocket == current.socket) {
                            selectedSocket = null;
                        }
                        OnClientDisconnected(this, new ClientEventArgs(current.socket));
                    }
                }
                Thread.Sleep(100);
                CheckSockets();
            } catch(Exception) { }
        }

        /// <summary>
        /// Runs when a client connected.
        /// Adds connected client to connectedSockets.
        /// Receives data from client and Listens for new clients.
        /// </summary>
        /// <param name="ar"></param>
        private void ClientConnectedCallback(IAsyncResult ar) {
            Socket clientSocket = socket.EndAccept(ar);

            if(connectedSockets.Count == 0 || selectedSocket == null) {
                selectedSocket = clientSocket;
            }
            connectedSockets.Add(new SocketInfo(clientSocket, true));

            OnClientConnected(this, new ClientEventArgs(clientSocket));

            Listen();
            Receive();
        }

        /// <summary>
        /// When new data received, converts data from buffer,
        /// sets current data to received data and receives again.
        /// </summary>
        /// <param name="ar"></param>
        private void DataReceivedCallback(IAsyncResult ar) {
            try {
                int received = selectedSocket.EndReceive(ar);
                string text = Encoding.ASCII.GetString(buffer);
                Array.Resize(ref buffer, received);

                OnDataReceived(this, new DataReceivedEventArgs(buffer));

                Array.Resize(ref buffer, selectedSocket.ReceiveBufferSize);
                Receive();
            } catch(Exception) { }
        }

        /// <summary>
        /// Checks socket is online or offline.
        /// </summary>
        /// <param name="sock"></param>
        /// <returns></returns>
        public bool IsConnected(Socket sock) {
            return !((sock.Poll(1000, SelectMode.SelectRead) && (sock.Available == 0)) || !sock.Connected);
        }
    }

    class ClientEventArgs : EventArgs {
        private Socket socket;
        public ClientEventArgs(Socket sock) {
            this.socket = sock;
        }

        public Socket Socket {
            get {
                return socket;
            }
        }
    }

    class DataReceivedEventArgs : EventArgs {
        private byte[] buffer;
        public DataReceivedEventArgs(byte[] buffer) {
            this.buffer = buffer;
        }

        public String Data {
            get {
                return Encoding.ASCII.GetString(buffer);
            }
        }
    }
}