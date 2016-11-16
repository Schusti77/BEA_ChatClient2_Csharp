using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;

namespace BEA_ChatClient_Csharp
{
    public partial class Form1 : Form
    {
        static IPEndPoint ClientEP;
        static IPEndPoint ServerEP;
        static int sendport = 11000;
        static int recport = 11001;
        static Thread Listener;
        static bool working = false;
        static String IDS;
        static String username;
        Socket s;

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
                    ServerEP = new IPEndPoint(ip_s, sendport);
                    //Server hat eine IPv4 Adresse, dem Client auch eine suchen
                    foreach (IPAddress ip_c in hostEntryClient.AddressList)
                    {
                        if (ip_c.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ClientEP = new IPEndPoint(ip_c, recport);
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
            Chatverlauf.Items.Add("[INFO]: Server IP ermittelt: " + ServerEP.Address + ":" + ServerEP.Port);

            s = new Socket(ClientEP.Address.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);
            s.Bind(ClientEP);

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
                    //working = false;
                    break;
                }
            }
            if (IDS == null)
            {
                Chatverlauf.Items.Add("[Fehler]:Could not connect to server");
                working = false;
                //Listener.Start();
                btnConnect.Enabled = true;
                s.Close();
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

        public bool SendToServer(String message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);

            try
            {
                s.SendTo(msg, ServerEP);
                return true;
            }
            catch(SocketException er)
            {
                Chatverlauf.Items.Add("Fehler beim Senden aufgetreten: " + er.ErrorCode);
                return false;
            }
        }

        private void listen(Object obj)
        {
            byte[] msg = new Byte[1 + 32 + 256];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            while (working)
            {
                s.ReceiveFrom(msg, 0, msg.Length, SocketFlags.None, ref senderRemote);

                Console.WriteLine(System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0'));
                MsgToProcess MTP = new MsgToProcess();
                MTP.Message = System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0');
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(msg).TrimEnd('\0'));
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessage), MTP);
            }
        }

        void ProcessMessage(object obj)
        {
            MsgToProcess MTP = obj as MsgToProcess;
            Console.WriteLine("Nachricht verarbeiten: {0}", MTP.Message);
            String Nachrichtentyp = MTP.Message.Substring(0, 1);
            String Argument1 = MTP.Message.Substring(1, 32).Trim();
            String Argument2 = MTP.Message.Substring(33, 256).Trim();

            switch (Nachrichtentyp)
            {
                case "A":
                    {
                        //Client anmelden
                        //Argument1 = Benutzername
                        //Argument2 = leer
                        if (!Argument1.Equals(username))
                        {
                            IDS = Argument1.Trim();
                            Console.WriteLine("IDS gesetzt");
                        }
                        break;
                    }
                case "T":
                    {
                        //Textnachricht empfangen
                        //Argument1 = Benutzername
                        //Argument2 = Textnachricht
                        if (IDS != null)
                            addLineToChat("[" + Argument1 + "]: " + Argument2);
                        break;
                    }
                case "S":
                    {
                        //Statusabfrage empfangen
                        //Argument1 = IDS
                        //Argument2 = Leer
                        if (IDS != null)
                        {
                            if (Argument1 == IDS)
                                //Msg unverändert zurücksenden
                                SendToServer(MTP.Message);
                            UInt16 anzClients;
                            if (UInt16.TryParse(Argument2, out anzClients))
                            {
                                if (anzClients != Benutzerliste.Items.Count)
                                {   //anzahl der Benutzer stimmt nicht mehr
                                    Benutzerliste.Invoke(new Action(() =>  Benutzerliste.Items.Clear()));
                                    SendToServer("U" + IDS.PadRight(32) + "".PadRight(256));
                                }
                            }
                        }
                        break;
                    }
                case "U":
                    {
                        //Nutzerverzeichnis empfangen
                        //Argument1 = Benutzername
                        //Argument2 = Leer
                        if (IDS != null)
                            addLineToUserList(Argument1);
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

        //Threadsave auf das Formular zugreifen
        delegate void SetTextCallback(string text);
        delegate void DelTextCallback();

        private void addLineToChat(String line)
        {
            if (this.Chatverlauf.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLineToChat);
                this.Invoke(d, new object[] { line });
            }
            else
            {
                this.Chatverlauf.Items.Add(line);
            }
        }

        private void addLineToUserList(String line)
        {
            if (this.Benutzerliste.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLineToUserList);
                this.Invoke(d, new object[] { line });
            }
            else
            {
                this.Benutzerliste.Items.Add(line);
            }
        }

        //private void ClearUserList()
        //{
        //    if (this.Benutzerliste.InvokeRequired)
        //    {
        //        DelTextCallback d = new DelTextCallback();
        //        this.Invoke(d, new object[] {  });
        //    }
        //    else
        //    {
        //        this.Benutzerliste.Items.Clear();
        //    }
        //}
        //Threadsave auf das Formular zugreifen - Ende

        private void btnSend_Click(object sender, EventArgs e)
        {
            //strings mit leerzeichen auffüllen und zusammensetzen zur nachricht, die übertragen wird
            String msg = "T" + IDS.PadRight(32) + txtSend.Text.PadRight(256);
            SendToServer(msg);
            txtSend.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            working = false; //listener stoppen
            if (IDS != null)
            {
                //strings mit leerzeichen auffüllen und zusammensetzen zur nachricht, die übertragen wird
                String msg = "Q" + IDS.PadRight(32) + txtSend.Text.PadRight(256);
                SendToServer(msg);
                s.Close(); //hier kann es zu einer exception kommen, wenn receivefrom noch wartet
            }
        }

        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                btnSend_Click(sender, new EventArgs());
        }
    }

    class MsgToProcess
    {
        public String Message { get; set; }
    }
}
