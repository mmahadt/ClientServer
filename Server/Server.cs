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
using ServerService;

namespace ServerApp
{
    public class Server

    {
        public static List<HandleClient> listOfClients = new List<HandleClient>();
               
        // Create a new dictionary of Sockets, with clientIdStrings as keys
        // and it will help to send messages to the appropriate clients
        public static Dictionary<string, HandleClient> clientMapping =
            new Dictionary<string, HandleClient>();
        
        public static ObservableCollection<string> ClList { get; set; }

        public void Clear()
        {
            ClList.Clear();
        }

        private static void OnListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // react to list changed
            string clientListString = string.Join("_", Server.ClList);

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
            HandleClient client = clientMapping[receiverId];
            client.SendOverNetworkStream(msg);
        }

        public static void Broadcast(Message msg, string senderId)
        {
            foreach (HandleClient client in listOfClients)
            {
                if (client.clNo != senderId && ClList.Contains(client.clNo)) 
                    //send the message to all clients except the sender
                {
                    client.SendOverNetworkStream(msg);
                }
            }
        }

        public void StartServer()
        {
            try
            {
                ClList = new ObservableCollection<string>();
                ClList.CollectionChanged += Server.OnListChanged;

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

                        HandleClient client = new HandleClient();
                        //Make a list of clients
                        listOfClients.Add(client);
                        clientMapping.Add(Convert.ToString(counter), client);
                        client.StartClient(clientSocket, Convert.ToString(counter));

                    }
                    catch (Exception ex)
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
                Server.RunAsAService();
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
                    Server.RunAsAConsole();
                }
            }
        }

        static void RunAsAConsole()
        {
            Server Server = new Server();
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
