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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeopleConsumerView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Bind which includes all Persons received
        public ObservableCollection<PersonMessage> Persons;

        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public MainWindow()
        {
            InitializeComponent();
            Persons = new ObservableCollection<PersonMessage>();
            this.listBox.ItemsSource = Persons;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //In Order to listen, 
            //we will create an EventingBasicConsumer, 
            //which can be cancelled later(Unload Message)
            _connection = CreateConnection();
            _channel = _connection.CreateModel();
            //We will create an auto destroyable Queue to get messages
            //var queueName = $"person.{RandomString(10)}";
            //_channel.QueueDeclare(queueName);

            //Uncomment to use a load balanced strategy
            var queueName = $"person.data";
            _channel.QueueDeclare(queueName,true,false,false);

            //We will bind the key to "person.*"
            _channel.QueueBind(queueName, "gft.exchange", "person.*");

            //Creating Consumer
            _consumer = new EventingBasicConsumer(_channel);

            //Once a message arrives, this Method will be trigered
            _consumer.Received += (model, args) =>
            {
                if(_error)
                {
                    _channel.BasicNack(args.DeliveryTag, false, true);
                    return;
                }
                //First let deserialize
                var bodyString = Encoding.UTF8.GetString(args.Body);
                var person = JsonConvert.DeserializeObject<PersonMessage>(bodyString);
                //Then we add to the collection using the UI Thread, 
                //in order to refresh it
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    Persons.Add(person);
                    this.Title = $"Data ({Persons.Count})";
                }));
                //Onde we received and processed, we acknowledge
                _channel.BasicAck(args.DeliveryTag, false);

                //Uncomment to play around with Transactions...
                /* 
                if (b % 2 == 0)
                {
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                else
                {
                    _channel.BasicNack(args.DeliveryTag, false, true);
                }*/
            };
            //Here we start the Consumer(not the autoAck: false to support transaction-like concept)
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);
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

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{user}:{password}@{server}:{port}/{vHost}");
            return factory.CreateConnection();
        }

        #region Random String Util function
        private static Random random = new Random();
        /// <summary>
        /// Creates a randon string, given the lenght
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Returns a random string!</returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        /// <summary>
        /// When the window is closed, we just close the connection and kill the consumer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {            
            //Canceling consumer and disposing channel and connection...
            _channel.BasicCancel(_consumer.ConsumerTag);
            _channel.Dispose();
            _connection.Close();
        }

        /// <summary>
        /// Logic to simulate error
        /// </summary>
        bool _error = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _error = !_error;
        }
    }
}
