﻿using ClientLib;
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
            clientApplication.Run();
        }

        private void Run()
        {
            getInputFromUser = true;

            client.newMessage += New_message;//subscribe to client newMessage event
            client.serverDown += Server_down;//subscribe to serverdown event
            client.clientListEvent += Client_list_update;//subscribe to client list change event

            try
            {
                //Read the port number from app.config file
                int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);
                client.Initialize(port);

                Console.WriteLine("Client application Id " + client.Id);

                while (getInputFromUser)
                {
                    Message m1 = GetInputFromUser();
                    if (getInputFromUser)
                    {
                        client.SendToServerStream(m1);
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

        private void New_message(string senderId, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Sender Id\t" + senderId + "\tMessage\t\t" + message);
            Console.ResetColor();
        }

        private void Server_down()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Server Down.");
            getInputFromUser = false;
            Console.ResetColor();
        }

        private void Client_list_update(string updatedListOfClients)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n>>>Updated list of clients {0}", updatedListOfClients.Replace("_", ", "));
            Console.ResetColor();
        }

        public Message StringsToMessageObject(string receiver, string message, bool broadcast)
        {
            if (!broadcast)
            {
                Message m1 = new Message()
                {
                    Broadcast = false,
                    SenderClientID = client.Id,
                    ReceiverClientID = receiver,
                    MessageBody = message
                };
                return m1;
            }
            else
            {
                Message m1 = new Message()
                {
                    Broadcast = true,
                    SenderClientID = client.Id,
                    ReceiverClientID = receiver,
                    MessageBody = message
                };
                return m1;
            }
        }


        public Message GetInputFromUser()
        {
            Console.WriteLine("\n\n\nType Messsage");
            string message = Console.ReadLine();

            Console.WriteLine("Is it Broadcast? Type (yes or no)");
            string inputString = Console.ReadLine();
            bool broadcast = inputString.ToLower() == "yes" || inputString.ToLower() == "y";

            string[] clients = client.listOfOtherClients.Split('_');
            List<string> validReceivers = clients.ToList();
            validReceivers.Remove(client.Id);

            if (!broadcast)
            {
                string receiver = "";
                while (!validReceivers.Contains(receiver) && getInputFromUser)
                {
                    if (string.Join(", ", validReceivers) == "")
                    {
                        Console.WriteLine("No valid receivers. Wait for new clients to connect.");
                        break;//break out of this loop if no valid receivers 
                    }
                    Console.WriteLine("Input Receiver ID");
                    Console.WriteLine("Valid receiver vals are {0}", string.Join(", ", validReceivers));
                    receiver = Console.ReadLine();
                    validReceivers = client.listOfOtherClients.Split('_').ToList();
                    validReceivers.Remove(client.Id);
                } 
                
                return StringsToMessageObject(receiver, message, false);

            }
            else
            {
                if (string.Join(", ", validReceivers) == "")
                {
                    Console.WriteLine("No valid receivers. Wait for new clients to connect.");
                }
                return StringsToMessageObject(null, message, true);
            }
        }

    }
}
