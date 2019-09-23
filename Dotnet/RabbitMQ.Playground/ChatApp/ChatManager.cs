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
        static string user = "guest";
        static string password = "guest";
        static string server = "localhost";
        static string port = "5672";
        static string vHost = "gft";
        static string exchange = "chat.exchange";
        static string allQueue = "chat.all.";
        static string businessQueue = "chat.business.";
        static string privateQueue = "chat.private.";

        private Guid _clientID;
        private IConnection _connection;
        private IModel _channel;
        private Dictionary<MessageChannel, EventingBasicConsumer> _registeredChannelConsumers;
   
        public ChatManager()
        {
            _clientID = Guid.NewGuid();
            _connection = CreateConnection();
            _channel = _connection.CreateModel();
            ChatHistory = new ObservableCollection<ChatMessage>();
            _registeredChannelConsumers = new Dictionary<MessageChannel, EventingBasicConsumer>();
        }

        public void ListenChannel(MessageChannel channelToListen)
        {
            //Declaring exchanging, if it is still not created
            _channel.ExchangeDeclare(exchange, "topic", true, false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += MessageReceived;
            switch (channelToListen)
                {
                case MessageChannel.Business:
                    _channel.QueueDeclare(businessQueue + _clientID.ToString());
                    _channel.QueueBind(businessQueue + _clientID.ToString(), exchange, "chat.business");
                    _channel.QueueBind(businessQueue + _clientID.ToString(), exchange, "chat.all");
                    _channel.BasicConsume(queue: businessQueue + _clientID.ToString(), autoAck: true, consumer: consumer);
                    break;
                case MessageChannel.Private:
                    _channel.QueueDeclare(privateQueue + _clientID.ToString());
                    _channel.QueueBind(privateQueue + _clientID.ToString(), exchange, "chat.private");
                    _channel.QueueBind(privateQueue + _clientID.ToString(), exchange, "chat.all");
                    _channel.BasicConsume(queue: privateQueue + _clientID.ToString(), autoAck: true, consumer: consumer);
                    break;
                default:
                    _channel.QueueDeclare(allQueue + _clientID.ToString());
                    _channel.QueueBind(allQueue + _clientID.ToString(), exchange, "chat.*");
                    _channel.BasicConsume(queue: allQueue + _clientID.ToString(), autoAck: true, consumer: consumer);
                    break;
            }
            _registeredChannelConsumers.Add(channelToListen, consumer);
        }

        public void StopChannel(MessageChannel channelToListen)
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

        private static IConnection CreateConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{user}:{password}@{server}:{port}/{vHost}");
            return factory.CreateConnection();
        }

        public void Send(ChatMessage message)
        {
            using (var connection = CreateConnection())
            {
                using (var connectionChannel = connection.CreateModel())
                {
                    var metaData = connectionChannel.CreateBasicProperties();
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    connectionChannel.BasicPublish(exchange,
                                         $"chat.{message.Channel.ToString().ToLower()}",
                                         metaData,
                                         body);
                }
            }
        }

        public ObservableCollection<ChatMessage> ChatHistory { get; set; }

        private void MessageReceived(object sender, BasicDeliverEventArgs args)
        {
            var bodyString = Encoding.UTF8.GetString(args.Body);
            var message = JsonConvert.DeserializeObject<ChatMessage>(bodyString);
            //Running on UI therad!!!!
            Application.Current.Dispatcher.Invoke(new Action(() => {
                ChatHistory.Add(message);
            }));
        }

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
