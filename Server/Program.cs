using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ClientLib;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ServiceProcess;
using Server;

namespace ServerApp
{
    public class Program

    {
        //A list of strings to contain client Ids
        public static List<handleClinet> listOfClients = new List<handleClinet>();
        public static List<string> clientsList = new List<string>();
        public static Queue<Message> Outbox = new Queue<Message>();

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
        private static void MessageSender()
        {
            while (true)
            {
                if (Outbox.Count != 0)
                {
                    Message message = Outbox.Peek();
                    if (message.Broadcast)
                    {
                        Console.WriteLine(">> Broadcast message from client\t" + message.MessageBody);
                        Broadcast(message, message.SenderClientID);
                        Outbox.Dequeue();
                    }
                    else
                    {
                        Console.WriteLine(">> Unicast message from client\t" + message.MessageBody);
                        Unicast(message, message.ReceiverClientID);
                        Outbox.Dequeue();
                    }
                }
            }
        }

        public static void Unicast(Message msg, string receiverId)
        {
            foreach (handleClinet client in listOfClients)
            {
                if (client.clNo == receiverId && ClList.Contains(client.clNo))
                //send message to intended recipient only
                {
                    handleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        public static void Broadcast(Message msg, string senderId)
        {
            foreach (handleClinet client in listOfClients)
            {
                if (client.clNo != senderId && ClList.Contains(client.clNo)) //send the message to all 
                                                                             //clients except the sender
                {
                    handleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        public void StartServer()
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

            Thread senderThread = new Thread(MessageSender);
            senderThread.Start();

            while (true)
            {
                try
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();
                    Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " connected");

                    handleClinet client = new handleClinet();
                    //Make a list of clients
                    listOfClients.Add(client);
                    clientsList.Add(Convert.ToString(counter));
                    client.startClient(clientSocket, Convert.ToString(counter));

                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    break;
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
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
    //Class to handle each client request separatly
    public class handleClinet
    {
        public TcpClient clientSocket;
        public string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Message m1 = new Message()
            {
                Broadcast = false,
                SenderClientID = null,
                ReceiverClientID = Convert.ToString(clineNo),
                MessageBody = Convert.ToString(clineNo)
            };
            SendOverNetworkStream(m1, clientSocket.GetStream());

            string clientListString = string.Join("_", Program.clientsList);

            Message m2 = new Message()
            {
                Broadcast = false,
                SenderClientID = null,
                ReceiverClientID = Convert.ToString(clineNo),
                MessageBody = clientListString
            };
            SendOverNetworkStream(m2, clientSocket.GetStream());
            Program.ClList.Add(clNo);
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            //byte[] bytesFrom = new byte[10025];
            Message dataFromClient = null;
            //Byte[] sendBytes = null;
            //string serverResponse = null;
            //string rCount = null;
            //requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount += 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    while (clientSocket.Connected)
                    {
                        if (networkStream.CanRead)
                        {
                            dataFromClient = ReadFromNetworkStream(networkStream);
                            Program.Outbox.Enqueue(dataFromClient);
                        }
                        else
                        {
                            networkStream.Close();
                            return;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Client {0} disconnected.", clNo);
                    break;
                }
                catch (System.IO.IOException)
                {
                    Program.ClList.Remove(clNo);
                    Console.WriteLine("Client {0} disconnected.", clNo);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
        }

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        public static void SendOverNetworkStream(Message dataFromClient, NetworkStream networkStream)
        {
            byte[] message = ObjectToByteArray(dataFromClient);
            //Get the length of message in terms of number of bytes
            int messageLength = message.Length;

            //lengthBytes are first 4 bytes in stream that contain
            //message length as integer
            byte[] lengthBytes = BitConverter.GetBytes(messageLength);
            networkStream.Write(lengthBytes, 0, lengthBytes.Length);

            //Write the message to the server stream
            byte[] outStream = message;
            networkStream.Write(outStream, 0, outStream.Length);
            networkStream.Flush();
        }

        public static Message ReadFromNetworkStream(NetworkStream networkStream)
        {
            //Read the length of incoming message from the server stream
            byte[] msgLengthBytes1 = new byte[sizeof(int)];
            networkStream.Read(msgLengthBytes1, 0, msgLengthBytes1.Length);
            //store the length of message as an integer
            int msgLength1 = BitConverter.ToInt32(msgLengthBytes1, 0);

            //create a buffer for incoming data of size equal to length of message
            byte[] inStream = new byte[msgLength1];
            //read that number of bytes from the server stream
            networkStream.Read(inStream, 0, msgLength1);
            //convert the byte array to message string
            //string dataFromServer = Encoding.ASCII.GetString(inStream);

            //Console.WriteLine(dataFromServer);
            Message dataFromServer = (Message)ByteArrayToObject(inStream);
            return dataFromServer;
        }

    }
}
