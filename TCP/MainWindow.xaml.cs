using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace TCP
{
    public class ClientsInfo
    {
        public Socket ClientSocket { get; set; }

        public string Login { get; set; }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread OpenServerThread = new Thread(OpenServer);
            OpenServerThread.Start();
        }

        public void OpenServer()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 13400);
            Socket listner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listner.Bind(ipPoint);
                listner.Listen(100);
                while(true)
                {
                    Socket handler = listner.Accept();
                    var CI = new List<ClientsInfo>();
                    
                    Thread NewClientThread = new Thread(ClientThread);
                    NewClientThread.Start(handler);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

        }
        List<ClientsInfo> CI = new List<ClientsInfo>();
        public void ClientThread(object obj)
        {
            try
            {
                Socket handler = (Socket)obj;
                MessageBox.Show("Connect: " + handler.RemoteEndPoint.ToString());
                int bytes  = 0;
                byte[] data = new byte[256];
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available < 0);

                    if (builder.ToString().StartsWith("@"))
                    {
                        string login = builder.ToString();
                        login = login.Remove(0, 1);
                        CI.Add(new ClientsInfo
                        {
                            ClientSocket = handler,
                            Login = login
                        });
                    }
                    else
                    {
                        string[] MsgArray = builder.ToString().Split(new char[] { '#' });

                        string Message = MsgArray[0];
                        string From = MsgArray[1];
                        string To = MsgArray[2];

                        SendToClient(From, To, Message, CI);

                        if (builder.ToString() == "STOP")
                        {
                            CLoseServer(handler);
                            break;
                        }
                    }
                    
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        public void CLoseServer(Socket handler)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        public void SendToClient(string From, string To, string message, List<ClientsInfo> CI)
        {
            var clients = CI.FirstOrDefault(c => c.Login == To);
            Socket handler = clients.ClientSocket;
            byte[] data = Encoding.Unicode.GetBytes(From + "#" + To + "#" + message);
            handler.Send(data);
        }
    }
}
