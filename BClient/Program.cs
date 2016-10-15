using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BClient {
    class Program {
        static Socket socket;

        static void Main(string[] args) {
            Console.ReadKey();
            try {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(new IPEndPoint(IPAddress.Loopback, 3321), new AsyncCallback(ConnectedCallback), null);
            } catch(Exception) { }

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

        static void ConnectedCallback(IAsyncResult ar) {
            socket.EndConnect(ar);
        }

        static void Send(string text) {
            try {
                byte[] buffer = Encoding.ASCII.GetBytes(text);
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SentCallback), null);
                Console.WriteLine("hii");
            } catch(Exception) { }
        }

        static void SentCallback(IAsyncResult ar) {
            try {
                socket.EndSend(ar);
            } catch(Exception) { }
        }
    }
}
