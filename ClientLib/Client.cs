using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;


namespace ClientLib
{
    public delegate void OnMessageRecived(string senderId,string message);
    public delegate void OnServerDown();
    public delegate void OnUpdateClientList(string updatedListOfClients);

    public class Client
    {

        public event OnMessageRecived newMessage;
        public event OnServerDown serverDown;
        public event OnUpdateClientList clientListEvent;

        public string Id;

        TcpClient clientSocket;
        NetworkStream serverStream;
        public string listOfOtherClients;

        public void Initialize(int port)
        {

            clientSocket = new TcpClient();

            clientSocket.Connect(IPAddress.Loopback, port);

            serverStream = clientSocket.GetStream();
            Message m1 = ReceiveFromServerStream();

            Id = m1.MessageBody;

            Thread receiverThread = new Thread(() => ReceiverThreadFunction(serverStream));
            receiverThread.Start();
        }
        private void ReceiverThreadFunction(NetworkStream stream)
        {
            while (clientSocket.Connected)
            {
                Message dataFromServer = ReceiveFromServerStream();
                if(dataFromServer == null)
                {
                    break;
                }
                else
                if (dataFromServer.SenderClientID == "Server")
                {
                    listOfOtherClients = dataFromServer.MessageBody;
                    clientListEvent.Invoke(listOfOtherClients);

                }
                else
                {
                    newMessage.Invoke(dataFromServer.SenderClientID, dataFromServer.MessageBody);
                }

            }
        }
        
        // Convert an object to a byte array
        private byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        private Object ByteArrayToObject(byte[] arrBytes)
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

        //https://stackoverflow.com/questions/7099875/sending-messages-and-files-over-networkstream
        private Message ReceiveFromServerStream()
        {
            try
            {
                //Read the length of incoming message from the server stream
                byte[] msgLengthBytes1 = new byte[sizeof(int)];
                serverStream.Read(msgLengthBytes1, 0, msgLengthBytes1.Length);
                //store the length of message as an integer
                int msgLength1 = BitConverter.ToInt32(msgLengthBytes1, 0);

                //create a buffer for incoming data of size equal to length of message
                byte[] inStream = new byte[msgLength1];
                //read that number of bytes from the server stream
                serverStream.Read(inStream, 0, msgLength1);
                //convert the byte array to message object
                Message dataFromServer = (Message)ByteArrayToObject(inStream);
                return dataFromServer;
            }
            catch(IOException)
            {
                serverDown.Invoke();
                return null;
            }
            
        }

        public void SendToServerStream(Message dataFromClient)
        {
            try
            {
                byte[] message = ObjectToByteArray(dataFromClient);
                //Get the length of message in terms of number of bytes
                int messageLength = message.Length;

                //lengthBytes are first 4 bytes in stream that contain
                //message length as integer
                byte[] lengthBytes = BitConverter.GetBytes(messageLength);
                serverStream.Write(lengthBytes, 0, lengthBytes.Length);

                //Write the message to the server stream
                byte[] outStream = message;
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            catch (Exception)
            {
                throw;
            }
        }

        ~Client()
        {
            clientSocket.Close();
        }
    }
}
