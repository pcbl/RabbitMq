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
        public MainWindow()
        {
            InitializeComponent();
            Persons = new ObservableCollection<Person>();
            this.listBox.ItemsSource = Persons;
        }




        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public ObservableCollection<Person> Persons;

        static string user = "guest";
        static string password = "guest";
        static string server = "localhost";
        static string port = "5672";
        static string vHost = "gft";
        static string exchange = "gft.exchange";
        static string queuePreffix = "person.";

        private static IConnection CreateConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{user}:{password}@{server}:{port}/{vHost}");
            return factory.CreateConnection();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            //In Order to listen, we will create an EventingBasicConsumer, which can be cancelled later(via TiBusListener.ShutDown())
            var connection = CreateConnection();
            var channel = connection.CreateModel();
            var queueName = $"{queuePreffix}{RandomString(10)}";

            channel.QueueDeclare(queueName);
            channel.QueueBind(queueName, exchange, $"{queuePreffix}*");

            var consumer = new EventingBasicConsumer(channel);
            int b = 0;
            //Once a message arrives, we trigger our Callback
            consumer.Received += (model, args) =>
            {
                var bodyString = Encoding.UTF8.GetString(args.Body);
                var person = JsonConvert.DeserializeObject<Person>(bodyString);
                //Running on UI therad!!!!
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    Persons.Add(person);
                }));

                channel.BasicAck(args.DeliveryTag, false);

                /*        if (b % 2 == 0)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                        }
                        else
                        {
                            channel.BasicNack(args.DeliveryTag, false, true);
                        }*/
                b++;


            };
            //Here we start the Consumer(not the autoAck: false to support transaction-like concept)
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        }


        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
