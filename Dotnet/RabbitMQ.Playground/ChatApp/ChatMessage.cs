using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    public class ChatMessage
    {
        public ChatMessage(string message,MessageChannel channel, string alias="")
        {
            if (string.IsNullOrWhiteSpace(alias))
                Sender = Environment.UserName;
            else
                Sender = alias;

            TimeStamp = DateTime.Now;
            Text = message;
            Channel = channel;
        }
        public MessageChannel Channel { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
