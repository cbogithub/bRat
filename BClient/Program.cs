using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BClient {
    class Program {
        static Socket socket;

        static void Main(string[] args) {
            Console.ReadKey();
            try {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(new IPEndPoint(IPAddress.Loopback, 3321), new AsyncCallback(ConnectedCallback), null);
            } catch(Exception) { }

            Thread recvThread = new Thread(new ThreadStart(Receive));
            recvThread.Start();

            bool exit = false;
            while(!exit) {
                Console.Write(">>");
                string text = Console.ReadLine();
                if(text != "exit") {
                    Send(text);
                } else {
                    exit = true;
                }
            }
        }

        static byte[] buffer;

        static void Receive() {
            try {
                buffer = new byte[socket.ReceiveBufferSize];
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(DataReceivedCallback), null);
            }
            catch(Exception) { }
        }

        static void DataReceivedCallback(IAsyncResult ar) {
            try {
                int received = socket.EndReceive(ar);
                Array.Resize(ref buffer, received);
                string text = Encoding.ASCII.GetString(buffer);
                Console.WriteLine("SERVER: {0}", text);

                Receive();
            }
            catch(Exception) { }
        }

        static void ConnectedCallback(IAsyncResult ar) {
            socket.EndConnect(ar);
        }

        static void Send(string text) {
            try {
                byte[] buffer = Encoding.ASCII.GetBytes(text);
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            } catch(Exception) { }
        }

        static void SendCallback(IAsyncResult ar) {
            try {
                socket.EndSend(ar);
            } catch(Exception) { }
        }
    }
}
