using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    /// <summary>
    /// Chat Message
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Creates a Chat Message
        /// </summary>
        /// <param name="message">Message Text</param>
        /// <param name="channel">Channel to Post</param>
        /// <param name="sender">Message Sender</param>
        public ChatMessage(string message, MessageChannel channel, string sender = "")
        {
            if (string.IsNullOrWhiteSpace(sender))
                Sender = Environment.UserName;
            else
                Sender = sender;

            TimeStamp = DateTime.Now;
            Text = message;
            Channel = channel;
        }
        /// <summary>
        /// Channel tos end the message to
        /// </summary>
        public MessageChannel Channel { get; set; }
        /// <summary>
        /// Message Sender
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// Message Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Timestam, when the message was created
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
