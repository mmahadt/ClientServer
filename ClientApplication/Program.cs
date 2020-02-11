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

        public Message GetInputFromUser()
        {
            Console.WriteLine("\n\n\nIs it Broadcast? Type (yes or no)");
            string inputString = Console.ReadLine();
            bool broadcast = inputString.ToLower() == "yes" || inputString.ToLower() == "y";

            Console.WriteLine("Type Messsage");
            string message = Console.ReadLine();

            string[] words = client.listOfOtherClients.Split('_');

            if (!broadcast)
            {
                string receiver = "";
                do
                {
                    Console.WriteLine("Input Receiver ID");
                    Console.WriteLine("Valid receiver vals are {0}", client.listOfOtherClients);
                    receiver = Console.ReadLine();
                    words = client.listOfOtherClients.Split('_');
                } while (!Array.Exists(words, x => x == receiver));
                return client.StringsToMessageObject(receiver, message, false);

            }
            else
            {
                return client.StringsToMessageObject(null, message, true);
            }
        }

        static void MessagePrinter(Message message)
        {
            Console.WriteLine("___________New Message____________");
            Console.WriteLine("Sender ID:\t{0}", message.SenderClientID);
            Console.WriteLine("Message:\t{0}", message.MessageBody);
            Console.WriteLine("Broadcast:\t{0}", message.Broadcast);
            Console.WriteLine("______________________________________");
        }

        static void InboxPrinter(Queue<Message> Inbox)
        {
            while (true)
            {
                if (Inbox.Count != 0)
                {

                    MessagePrinter(Inbox.Dequeue());

                }
            }

        }

        static void Main(string[] args)
        {
            ClientApplication clientApplication = new ClientApplication();

            //Read the port number from app.config file
            int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);

            clientApplication.client.Start(port);

            Console.WriteLine("Client application Id " + clientApplication.client.Id);

            Thread messagePrinterThread = new Thread(() => InboxPrinter(clientApplication.client.GetInbox()));
            messagePrinterThread.Start();

            while (true)
            {
                Message m1 = clientApplication.GetInputFromUser();
                if (m1.Broadcast)
                {
                    clientApplication.client.Broadcast(m1);
                }
                else
                {
                    clientApplication.client.Unicast(m1);
                }
            }
        }

    }
}
