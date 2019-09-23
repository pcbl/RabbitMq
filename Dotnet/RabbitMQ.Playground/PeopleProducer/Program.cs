
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            var randon = new Random();
            //Let´s remain on the loop until we leave it with X
            while (true)
            {
                Console.Clear();
                Console.WriteLine("PRODUCER@LOCAL - How many Messages to pubblish? (X to quit)");
                var input = Console.ReadLine();
                //Quit! when X is typed!
                if (input.ToUpper().Equals("X"))
                {
                    break;
                }
                //Parsing
                if (int.TryParse(input, out int itemsToCreate))
                {
                    for (int i = 0; i < itemsToCreate; i++)
                    {
                        //Age between 1 and 120
                        var person = new { Age = randon.Next(1,120) , Name = $"Person{i}" };
                        //Sending Message
                        SendMessage(person);
                        Console.WriteLine($"SENT:{person.Name},{person.Age}...");
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="message">message to send</param>
        private static void SendMessage(object message)
        {
            //Create a Connection
            using (var connection = CreateConnection())
            {
                //Then, on top of that we create a channel
                using (var channel = connection.CreateModel())
                {
                    //We want to use some meta data, would be used on Headers Exchange!
                    var metaData = channel.CreateBasicProperties();
                    metaData.Headers = new Dictionary<string, object>();
                    metaData.Headers.Add("Company", "GFT");
                    metaData.Headers.Add("Domain", "CONSULTING");
                    //Serializing to JSON!
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    //Finally Publishing the message
                    channel.BasicPublish("gft.exchange",
                                         "person.data",
                                         metaData,
                                         body);
                }
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



    }
}
