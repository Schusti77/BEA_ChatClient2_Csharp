using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace BEA_ChatClient_Csharp
{
    public partial class Form1 : Form
    {
        static IPEndPoint endPoint;
        static int port = 11000;
        static Thread Listener;
        static bool working = false;
        static String IDS;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            endPoint = null;

            //nach einer IPv4 Adresse suchen
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    endPoint = new IPEndPoint(ip, port);
                    break;
                }
            }

            //nach einer IPv6 Adresse suchen
            if (endPoint == null)
            {
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        endPoint = new IPEndPoint(ip, port);
                        break;
                    }
                }
            }

            if (endPoint == null)
            {
                //Server-Rechner hat keine IP-Adresse
                //entweder keine aktive Netzwerkkarte oder irgendwas faul
                Console.ForegroundColor = ConsoleColor.Red;
                Chatverlauf.Items.Add("[Fehler]:Dieser Rechner hat keine Verbindung zu einem Netzwerk");
            }

            //Netzwerk auf Servernachrichten abhören
            IDS = null;
            working = true;
            Listener = new Thread(listen);
            Listener.IsBackground = true;
            Listener.Start();

            //strings mit leerzeichen auffüllen und zusammensetzen zur nachricht, die übertragen wird
            String msg = "A" + txtUsername.Text.PadRight(32) + "".PadRight(256);
            SendToServer(msg);

            //warten auf die Serverantwort
            DateTime starttime = DateTime.Now;
            TimeSpan timediff = new TimeSpan(0, 0, 5);
            while (IDS == null)
            {
                if (DateTime.Now - starttime > timediff)
                    break;
            }
            if (IDS == null)
                Chatverlauf.Items.Add("[Fehler]:Could not connect to server");
            else
            {
                Chatverlauf.Items.Add("[OK]:Connection established");
            }
        }

        public static void SendToServer(String message)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[2], 11000);

            Socket s = new Socket(endPoint.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);

            byte[] msg = Encoding.ASCII.GetBytes(message);
            //Console.WriteLine("Sending data.");
            // This call blocks. 
            s.SendTo(msg, endPoint);
            s.Close();
        }

        private static void listen(Object obj)
        {
            while (working)
            {
                Console.WriteLine("listener running");
                Socket s = new Socket(endPoint.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);
                s.ReceiveTimeout = 1000;

                // Creates an IpEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                // Binding is required with ReceiveFrom calls.
                s.Bind(endPoint);
                byte[] msg = new Byte[1 + 32 + 256];
                //Console.WriteLine("Waiting to receive datagrams from client...");
                // This call blocks.  
                try
                {
                    s.ReceiveFrom(msg, 0, msg.Length, SocketFlags.None, ref senderRemote);

                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0'));
                    MsgToProcess MTP = new MsgToProcess();
                    MTP.Message = System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0');
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), MTP);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    //innerhalb des Socket.ReceiveTimeout keine Nachricht empfangen
                    //nichts machen, einfach noch einen Loop in der While-Schleife
                }
                finally
                {
                    //socket immer schließen
                    s.Close();
                }
            }
        }

        static void ProcessMessage(object obj)
        {
            MsgToProcess MTP = obj as MsgToProcess;
            Console.WriteLine("Nachricht verarbeiten: {0}", MTP.Message);
            String Nachrichtentyp = MTP.Message.Substring(0, 1);
            String Argument1 = MTP.Message.Substring(1, 32);
            String Argument2 = MTP.Message.Substring(33, 256);

            switch (Nachrichtentyp)
            {
                case "A":
                    {
                        //Client anmelden
                        //Argument1 = Benutzername
                        //Argument2 = leer
                        IDS = Argument1.Trim();
                        break;
                    }
                case "T":
                    {
                        //Textnachricht empfangen
                        break;
                    }
                default:
                    {
                        //unbekannte Nachricht empfangen
                        //verwerfen
                        break;
                    }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //strings mit leerzeichen auffüllen und zusammensetzen zur nachricht, die übertragen wird
            String msg = "T" + "".PadRight(32) + txtSend.Text.PadRight(256);
            SendToServer(msg);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //strings mit leerzeichen auffüllen und zusammensetzen zur nachricht, die übertragen wird
            String msg = "Q" + "".PadRight(32) + txtSend.Text.PadRight(256);
            SendToServer(msg);
            working = false;
        }
    }

    class MsgToProcess
    {
        public String Message { get; set; }
    }
}
