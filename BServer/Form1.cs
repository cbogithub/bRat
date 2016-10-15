using System;
using System.Linq;
using System.Windows.Forms;

namespace BRat {
    public partial class Form1 : Form {

        Server server;

        public Form1() {
            InitializeComponent();
        }

        private void btn_startServer_Click(object sender, EventArgs e) {
            int port;

            if(!int.TryParse(tb_port.Text, out port)) {
                MessageBox.Show("Please enter an integer value.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btn_startServer.Enabled = false;

            server = new Server(port);
            server.StartServer();

            server.OnClientConnected += ClientConnected;
            server.OnDataReceived += DataReceived;
            server.OnClientDisconnected += ClientDisconnected;
        }

        private void ClientConnected(object sender, ClientEventArgs args) {
            string[] data = { args.Socket.LocalEndPoint.ToString(), "yup!" };
            RunControlMethodFromAnotherThread(listView1, new MethodInvoker(delegate {
                ListViewItem i = listView1.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Text == data[0]);
                if(i == null) {
                    listView1.Items.Add(new ListViewItem(data));
                } else {
                    listView1.Items[i.Index].SubItems[1].Text = "yup!";
                }
            }));
        }

        private void ClientDisconnected(object sender, ClientEventArgs args) {
            RunControlMethodFromAnotherThread(listView1, new MethodInvoker(delegate {
                ListViewItem i = listView1.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Text == args.Socket.LocalEndPoint.ToString());
                listView1.Items[i.Index].SubItems[1].Text = "nope!";
            }));
        }

        private void DataReceived(object sender, DataReceivedEventArgs args) {
            MessageBox.Show(args.Data);
        }

        private void RunControlMethodFromAnotherThread(Control control, Delegate del, params object[] obj) {
            if(control.InvokeRequired) {
                control.Invoke(del, obj);
            } else {
                del.DynamicInvoke(obj);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            server.Send("Hi from serv xd");
        }
    }
}
