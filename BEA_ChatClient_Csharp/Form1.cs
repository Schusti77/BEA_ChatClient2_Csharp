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
        static IPEndPoint ClientEP;
        static IPEndPoint ServerEP;
        static int port = 11000;
        static Thread Listener;
        static bool working = false;
        static String IDS;
        static String username;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Chatverlauf.Items.Add("[INFO]: Verbindungsversuch zu Server: " + txtHost.Text.Trim());
            username = txtUsername.Text.Trim();
            btnConnect.Enabled = false;
            IPHostEntry hostEntryClient = Dns.GetHostEntry(Dns.GetHostName());
            ClientEP = null;
            IPHostEntry hostEntryServer = Dns.GetHostEntry(txtHost.Text.Trim());
            ServerEP = null;
            //serveradresse suchen, wenn eine gefunden wurde, clientadresse von gleichem typ suchen
            //nach einer IPv4 Adresse suchen
            foreach (IPAddress ip_s in hostEntryServer.AddressList)
            {
                if (ip_s.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerEP = new IPEndPoint(ip_s, port);
                    //Server hat eine IPv4 Adresse, dem Client auch eine suchen
                    foreach (IPAddress ip_c in hostEntryClient.AddressList)
                    {
                        if (ip_c.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ClientEP = new IPEndPoint(ip_c, port);
                            //Server hat eine IPv4 Adresse, dem Client auch eine suchen
                            break;
                        }
                    }
                    break;
                }
            }

            if (ClientEP == null || ServerEP == null)
            {
                //Server-Rechner hat keine IP-Adresse
                //entweder keine aktive Netzwerkkarte oder irgendwas faul
                Console.ForegroundColor = ConsoleColor.Red;
                Chatverlauf.Items.Add("[Fehler]:Dieser Rechner hat keine Verbindung zum Server");
                btnConnect.Enabled = true;
                return; //Funktion beenden
            }
            Chatverlauf.Items.Add("[INFO]: Client aktiv auf IP: " + ClientEP.Address + ":" + ClientEP.Port);
            //Netzwerk auf Servernachrichten abhören
            IDS = null;
            working = true;
            this.Refresh();
            Listener = new Thread(listen);
            Listener.IsBackground = true;
            Listener.Start();

            //strings mit leerzeichen auffüllen und zusammensetzen zur nachricht, die übertragen wird
            String msg = "A" + txtUsername.Text.PadRight(32) + "".PadRight(256);
            SendToServer(msg);
            Chatverlauf.Items.Add("[INFO]: Anmeldeinformationen an Server übertragen");
            Chatverlauf.Items.Add("[INFO]: Warte auf Antwort vom Server (timeout 5s)");
            //warten auf die Serverantwort
            DateTime starttime = DateTime.Now;
            TimeSpan timediff = new TimeSpan(0, 0, 5);
            while (IDS == null)
            {
                if (DateTime.Now - starttime > timediff)
                {
                    working = false;
                    break;
                }
            }
            if (IDS == null)
            {
                Chatverlauf.Items.Add("[Fehler]:Could not connect to server");
                btnConnect.Enabled = true;
            }
            else
            {
                Chatverlauf.Items.Add("[OK]:Connection established");
                Chatverlauf.Enabled = true;
                txtSend.Enabled = true;
                btnSend.Enabled = true;
                btnConnect.Enabled = false;
                txtHost.Enabled = false;
                txtUsername.Enabled = false;
            }
        }

        public void SendToServer(String message)
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

        private void listen(Object obj)
        {
            while (working)
            {
                //Console.WriteLine("listener running");
                Socket s = new Socket(ClientEP.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);
                s.ReceiveTimeout = 100;

                // Creates an IpEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                // Binding is required with ReceiveFrom calls.
                try { 
                s.Bind(ClientEP);
                }
                catch
                {
                    Console.WriteLine("Socket gerade belegt. Nächste Runde wieder versuchen.");
                    s.Close();
                    continue;
                }
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

        void ProcessMessage(object obj)
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
                        if (!Argument1.Equals(username))
                            IDS = Argument1.Trim();
                        //ist das Argument1 = dem username, dann hat der Client seine eigene MSG empfangen
                        //da als Server wohl der localhost ausgewählt wurde. An andere Varianten wollen wir mal nicht denken
                        break;
                    }
                case "T":
                    {
                        //Textnachricht empfangen
                        //Argument1 = Benutzername
                        //Argument2 = Textnachricht
                        if (IDS != null)
                            Chatverlauf.Items.Add("["+ Argument1+"]: " + Argument2);
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
