using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using ClientLib;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ServiceProcess;
using Server;

namespace ServerApp
{
    public class Program

    {
        public static List<HandleClinet> listOfClients = new List<HandleClinet>();
               
        // Create a new dictionary of Sockets, with clientIdStrings as keys
        // and it will help to send messages to the appropriate clients
        public static Dictionary<string, HandleClinet> clientMapping =
            new Dictionary<string, HandleClinet>();
        
        public static ObservableCollection<string> ClList { get; set; }

        public void Clear()
        {
            ClList.Clear();
        }

        private static void OnListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // react to list changed
            string clientListString = string.Join("_", Program.ClList);

            Message m2 = new Message()
            {
                Broadcast = false,
                SenderClientID = "Server",
                ReceiverClientID = null,
                MessageBody = clientListString
            };
            Broadcast(m2, m2.SenderClientID);
        }


        public static void Unicast(Message msg, string receiverId)
        {
            HandleClinet client = clientMapping[receiverId];
            HandleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
        }

        public static void Broadcast(Message msg, string senderId)
        {
            foreach (HandleClinet client in listOfClients)
            {
                if (client.clNo != senderId && ClList.Contains(client.clNo)) 
                    //send the message to all clients except the sender
                {
                    HandleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        public void StartServer()
        {
            try
            {
                ClList = new ObservableCollection<string>();
                ClList.CollectionChanged += Program.OnListChanged;

                //Read the port number from app.config file
                int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);

                TcpListener serverSocket = new TcpListener(IPAddress.Loopback, port);

                TcpClient clientSocket = default(TcpClient);
                int counter = 0;

                serverSocket.Start();
                Console.WriteLine(" >> " + "Server Started");

                counter = 0;


                while (true)
                {
                    try
                    {
                        counter += 1;
                        clientSocket = serverSocket.AcceptTcpClient();
                        Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " connected");

                        HandleClinet client = new HandleClinet();
                        //Make a list of clients
                        listOfClients.Add(client);
                        clientMapping.Add(Convert.ToString(counter), client);
                        client.StartClient(clientSocket, Convert.ToString(counter));

                    }
                    catch (Exception)
                    {
                        //Console.WriteLine(" >> " + ex.ToString());
                        break;
                    }
                }

                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine(" >> " + "exit");
                Console.ReadLine();
            }
            catch (System.FormatException)
            {
                Console.WriteLine("Please input a valid port number in App.config file.");
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");
                Console.Read();
            }
        }

        static void Main(string[] args)
        {
            if ((!Environment.UserInteractive))
            {
                Program.RunAsAService();
            }
            else
            {
                if (args != null && args.Length > 0)
                {
                    if (args[0].Equals("-i", StringComparison.OrdinalIgnoreCase))
                    {
                        SelfInstaller.InstallMe();
                    }
                    else
                    {
                        if (args[0].Equals("-u", StringComparison.OrdinalIgnoreCase))
                        {
                            SelfInstaller.UninstallMe();
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument!");
                        }
                    }
                }
                else
                {
                    Program.RunAsAConsole();
                }
            }
        }

        static void RunAsAConsole()
        {
            Program Server = new Program();
            Server.StartServer();
        }

        static void RunAsAService()
        {
            ServiceBase[] servicesToRun = new ServiceBase[]
           {
                new HybridSvxService()
           };
            ServiceBase.Run(servicesToRun);
        }

    }
}
