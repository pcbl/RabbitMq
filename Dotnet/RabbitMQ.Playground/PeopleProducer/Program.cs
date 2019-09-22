
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace PeopleProducer
{
    class Program
    {
        static string user = "guest";
        static string password = "guest";
        static string server = "localhost";
        static string port = "5672";
        static string vHost = "gft";

        static string exchange = "gft.exchange";
        static string key = "person.data";

        private static IConnection CreateConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{user}:{password}@{server}:{port}/{vHost}");
            return factory.CreateConnection();
        }
        private static void Send(object message)
        {
            using (var connection = CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var metaData = channel.CreateBasicProperties();
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    channel.BasicPublish(exchange,
                                                  key,
                                                  metaData,
                                                  body);
                }
            }
        }


        static void Main(string[] args)
        {
            int tmpSequenceId = 1;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("PRODUCER@LOCAL - How many Messages to pubblish? (X to quit)");
                var tmpInput = Console.ReadLine();
                //Quit!
                if (tmpInput.ToUpper().Equals("X"))
                {
                    break;
                }
                if (int.TryParse(tmpInput, out int tmpNumber))
                {
                    for (int i = 1; i <= tmpNumber; i++, tmpSequenceId++)
                    {
                        var person = new { Age = i, Name = $"Person{i}" };
                        Send(new { Age = i, Name = $"Person{i}" });
                        Console.WriteLine($"SENT:{person.Name},{person.Age}...");
                    }
                }
            }
        }
    }
}
