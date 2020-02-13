using ClientLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClientApplication
{
    public class ClientApplication
    {
        Client client = new Client();
        static bool getInputFromUser;
        static void Main(string[] args)
        {

            ClientApplication clientApplication = new ClientApplication();
            getInputFromUser = true;

            clientApplication.client.newMessage += new_message;//subscribe to client newMessage event
            clientApplication.client.serverDown += server_down;//subscribe to serverdown event
            clientApplication.client.clientListEvent += client_list_update;//subscribe to client list change event

            //Read the port number from app.config file
            int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);

          
                clientApplication.client.Initialize(port);
        

            Console.WriteLine("Client application Id " + clientApplication.client.Id);

            //Thread messagePrinterThread = new Thread(() => clientApplication.InboxPrinter(clientApplication.client.GetInbox()));
            //messagePrinterThread.Start();

            while (getInputFromUser)
            {
                Message m1 = clientApplication.GetInputFromUser();
                if (m1.Broadcast && getInputFromUser)
                {
                    clientApplication.client.Broadcast(m1);
                }
                else
                {
                    clientApplication.client.Unicast(m1);
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        private static void new_message(string senderId, string message)
        {
            Console.WriteLine("Sender Id\t" + senderId + "\tMessage\t" + message);
        }

        private static void server_down()
        {
            Console.WriteLine("Server Down.");
        }

        private static void client_list_update(string updatedListOfClients)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n>>>Updated list of clients {0}", updatedListOfClients.Replace("_", ", "));
            Console.ResetColor();
        }

        public Message GetInputFromUser()
        {
            Console.WriteLine("\n\n\nType Messsage");
            string message = Console.ReadLine();

            Console.WriteLine("Is it Broadcast? Type (yes or no)");
            string inputString = Console.ReadLine();
            bool broadcast = inputString.ToLower() == "yes" || inputString.ToLower() == "y";

            string[] clients = client.listOfOtherClients.Split('_');

            if (!broadcast)
            {
                string receiver = "";
                do
                {
                    Console.WriteLine("Input Receiver ID");
                    Console.WriteLine("Valid receiver vals are {0}", client.listOfOtherClients.Replace("_",", "));
                    receiver = Console.ReadLine();
                    clients = client.listOfOtherClients.Split('_');
                } while (!Array.Exists(clients, x => x == receiver));
                return client.StringsToMessageObject(receiver, message, false);

            }
            else
            {
                return client.StringsToMessageObject(null, message, true);
            }
        }

        //public void MessagePrinter(Message message)
        //{
            
        //    if (message.SenderClientID == "Server")
        //    {
        //        //Console.ForegroundColor = ConsoleColor.Cyan;
        //        //Console.WriteLine("\n>>>Updated list of clients {0}", client.listOfOtherClients.Replace("_", ", "));
        //        //Console.ResetColor();
        //    }
        //    else
        //    {
        //        Console.WriteLine("___________New Message____________");
        //        Console.WriteLine("Sender ID:\t{0}", message.SenderClientID);
        //        Console.WriteLine("Message:\t{0}", message.MessageBody);
        //        Console.WriteLine("Broadcast:\t{0}", message.Broadcast);
        //        Console.WriteLine("______________________________________");
        //    }          
        //}

        //public void InboxPrinter(Queue<Message> Inbox)
        //{
        //    while (true)
        //    {
        //        if (Inbox.Count != 0)
        //        {
        //            if (Inbox.Peek() == null)
        //            {
        //                Console.WriteLine("Server Down.");
        //                getInputFromUser = false;
        //                return;
        //            }
        //            else
        //            {
        //                MessagePrinter(Inbox.Dequeue());
        //            }
   
        //        }
                
        //    }
            
        //}

        

    }
}
