using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLib
{
    [Serializable]
    public class Message : IMessage
    {
        public string SenderClientID
        {
            get;
            set;
        }
        public string ReceiverClientID
        {
            get;
            set;
        }
        public string MessageBody
        {
            get;
            set;
        }

        public bool Broadcast
        {
            get;
            set;
        }
    }
}
