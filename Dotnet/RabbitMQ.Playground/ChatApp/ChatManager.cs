using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp
{
    public class ChatManager:IDisposable
    {
        /// <summary>
        /// Creates a Chat Manager Instance
        /// </summary>
        public ChatManager()
        {
            //Client ID to be using
            _clientID = Guid.NewGuid();
            //Creating the connection to Rabbit MQ
            _connection = CreateConnection();
            //Then the channel on top of that
            _channel = _connection.CreateModel();
            //Initializing some data structures
            ChatHistory = new ObservableCollection<ChatMessage>();
            _registeredChannelConsumers = new Dictionary<MessageChannel, EventingBasicConsumer>();
        }

        /// <summary>
        /// Exchange to be used
        /// </summary>
        private static string exchange = "chat.exchange";
        /// <summary>
        /// Queue Preffix for the Channel: All
        /// </summary>
        private static string allQueuePreffix = "chat.all.";
        /// <summary>
        /// Queue Preffix for the Channel: Business
        /// </summary>
        private static string businessQueuePreffix = "chat.business.";
        /// <summary>
        /// Queue Preffix for the Channel: Private
        /// </summary>
        private static string privateQueuePreffix = "chat.private.";

        /// <summary>
        /// Client Id(used to register the queue)
        /// </summary>
        private Guid _clientID;
        /// <summary>
        /// Connection to RabbitMQ
        /// </summary>
        private IConnection _connection;
        /// <summary>
        /// Channel to RabbitMQ
        /// </summary>
        private IModel _channel;

        /// <summary>
        /// Attached Consumers, by Channel
        /// </summary>
        private Dictionary<MessageChannel, EventingBasicConsumer> _registeredChannelConsumers;
   
        /// <summary>
        /// Starts listening on a given channel
        /// </summary>
        /// <param name="channelToListen">Channel to listen to</param>
        public void StartChannelListening(MessageChannel channelToListen)
        {
            //Declaring exchanging, if it is still not created
            _channel.ExchangeDeclare(exchange, "topic", true, false);

            //Then we create the consumer
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += MessageReceived;
            //Based on the Channel, we might declare different resources
            switch (channelToListen)
                {
                case MessageChannel.Business:
                    //First of all, we create an auto-detroyable  Queue
                    _channel.QueueDeclare(businessQueuePreffix + _clientID.ToString());
                    //Then we bind it to Business and All
                    _channel.QueueBind(businessQueuePreffix + _clientID.ToString(), exchange, "chat.business");
                    _channel.QueueBind(businessQueuePreffix + _clientID.ToString(), exchange, "chat.all");
                    //And finally attach the consumer!
                    _channel.BasicConsume(queue: businessQueuePreffix + _clientID.ToString(), autoAck: true, consumer: consumer);
                    break;
                case MessageChannel.Private:
                    //First of all, we create an auto-detroyable  Queue
                    _channel.QueueDeclare(privateQueuePreffix + _clientID.ToString());
                    //Then we bind it to PÜrivate and All
                    _channel.QueueBind(privateQueuePreffix + _clientID.ToString(), exchange, "chat.private");
                    _channel.QueueBind(privateQueuePreffix + _clientID.ToString(), exchange, "chat.all");
                    //And finally attach the consumer!
                    _channel.BasicConsume(queue: privateQueuePreffix + _clientID.ToString(), autoAck: true, consumer: consumer);
                    break;
                default:
                    //First of all, we create an auto-detroyable  Queue
                    _channel.QueueDeclare(allQueuePreffix + _clientID.ToString());
                    //Then we just Bind to anything(chat.*), in our context, that would mean All!!
                    _channel.QueueBind(allQueuePreffix + _clientID.ToString(), exchange, "chat.*");
                    //And finally attach the consumer!
                    _channel.BasicConsume(queue: allQueuePreffix + _clientID.ToString(), autoAck: true, consumer: consumer);
                    break;
            }
            //We want to keep track of registered channel consumers
            _registeredChannelConsumers.Add(channelToListen, consumer);
        }

        /// <summary>
        /// Stops Listening to a given channel
        /// </summary>
        /// <param name="channelToListen">Channel to stop listening to</param>
        public void StopChannelListening(MessageChannel channelToListen)
        {
            if (_registeredChannelConsumers.ContainsKey(channelToListen))
            {
                _channel.BasicCancel(_registeredChannelConsumers[channelToListen].ConsumerTag);
                _registeredChannelConsumers.Remove(channelToListen);
            }
            else
            {
                throw new Exception("No Channel Listener Consumer registered!");
            }            
        }

        /// <summary>
        /// Creates a RabbitMQ connection
        /// </summary>
        /// <returns>Returns the created connection</returns>
        private static IConnection CreateConnection()
        {
            //Default from the Docker Container
            var user = "guest";
            var password = "guest";
            var server = "localhost";
            var port = "5672";
            //We want to use a virtual host named GFT
            var vHost = "gft";

            //Creating the connection!
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{user}:{password}@{server}:{port}/{vHost}");
            return factory.CreateConnection();
        }

        /// <summary>
        /// Sends a chat Messahe
        /// </summary>
        /// <param name="message">Chat Message to Send</param>
        public void SendChatMessage(ChatMessage message)
        {
            //Create a Connection
            using (var connection = CreateConnection())
            {
                //Then, on top of that we create a channel
                using (var connectionChannel = connection.CreateModel())
                {
                    //Posting the Message
                    var metaData = connectionChannel.CreateBasicProperties();
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    //Note that the Routing key will be chat.*, where * is the channel name
                    //Which can be either Private,Business or All
                    connectionChannel.BasicPublish(exchange,
                                         $"chat.{message.Channel.ToString().ToLower()}",
                                         metaData,
                                         body);
                }
            }
        }


        /// <summary>
        /// Chat History, bound to the UI
        /// </summary>
        public ObservableCollection<ChatMessage> ChatHistory { get; set; }

        /// <summary>
        /// Triggered when a Message is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MessageReceived(object sender, BasicDeliverEventArgs args)
        {
            //Deserializin the Message
            var bodyString = Encoding.UTF8.GetString(args.Body);
            var message = JsonConvert.DeserializeObject<ChatMessage>(bodyString);
            //Then we add to the collection using the UI Thread, 
            //in order to refresh it accordingly
            Application.Current.Dispatcher.Invoke(new Action(() => {
                ChatHistory.Add(message);
            }));
        }

        /// <summary>
        /// When disposing, we just finish any open consumer and close channel and connection
        /// </summary>
        public void Dispose()
        {
            foreach(var item in _registeredChannelConsumers)
            {
                _channel.BasicCancel(item.Value.ConsumerTag);
            }
            _channel.Dispose();
            _connection.Close();
        }
    }
}
