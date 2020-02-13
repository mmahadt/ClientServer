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
            
            try
            {
                //Read the port number from app.config file
                int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);
                clientApplication.client.Initialize(port);

                Console.WriteLine("Client application Id " + clientApplication.client.Id);

                while (getInputFromUser)
                {
                    Message m1 = clientApplication.GetInputFromUser();
                    if (m1.Broadcast && getInputFromUser)
                    {
                        clientApplication.client.SendToServerStream(m1);
                    }
                    
                }              
            }
            catch (System.FormatException)
            {
                Console.WriteLine("Please input a valid port number in App.config file.");
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("Server is not connected.");
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");
                Console.Read();
            }
        }

        private static void new_message(string senderId, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Sender Id\t" + senderId + "\tMessage\t\t" + message);
            Console.ResetColor();
        }

        private static void server_down()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Server Down.");
            Console.ResetColor();
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

        

        

    }
}
