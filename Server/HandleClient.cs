using System;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ClientLib;

namespace ServerApp
{
    //Class to handle each client request separatly
    public class HandleClient
    {
        public TcpClient clientSocket;
        NetworkStream networkStream;
        public string clNo;

        ~HandleClient()
        {
            clientSocket.Close();
        }

        public void StartClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            networkStream = clientSocket.GetStream();
            this.clNo = clineNo;
            Message m1 = new Message()
            {
                Broadcast = false,
                SenderClientID = null,
                ReceiverClientID = Convert.ToString(clineNo),
                MessageBody = Convert.ToString(clineNo)
            };
            SendOverNetworkStream(m1);

            Server.ClList.Add(clNo);
            Thread ctThread = new Thread(DoChat);
            ctThread.Start();
        }

        private void DoChat()
        {
            Message dataFromClient = null;

            try
            {
                while (clientSocket.Connected)
                {
                    if (networkStream.CanRead)
                    {
                        dataFromClient = ReadFromNetworkStream();

                        if (dataFromClient.Broadcast)
                        {
                            Server.Broadcast(dataFromClient, dataFromClient.SenderClientID);
                        }
                        else
                        {
                            Server.Unicast(dataFromClient, dataFromClient.ReceiverClientID);
                        }

                    }

                }
            }
            //catch (InvalidOperationException)
            //{
            //    Console.WriteLine("", clNo);

            //}
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                Console.WriteLine("No valid receivers at this moment.");
            }
            catch (System.IO.IOException ex)
            {
                Server.ClList.Remove(clNo);
                Console.WriteLine("Client {0} disconnected.", clNo);
                Console.WriteLine("{0}",ex.ToString());
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" >> " + ex.ToString());
            }

        }

        // Convert an object to a byte array
        public byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public Object ByteArrayToObject(byte[] arrBytes)
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

        public void SendOverNetworkStream(Message dataFromClient)
        {
            try
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
            catch (IOException)
            {
                Console.WriteLine("Client you are sending message to is closed.");
                //throw;
            }
        }

        public Message ReadFromNetworkStream()
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

            //convert the byte array to message object
            Message dataFromServer = (Message)ByteArrayToObject(inStream);
            return dataFromServer;
        }

    }
}
