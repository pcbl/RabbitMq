package pcbl;
import com.rabbitmq.client.Channel;
import com.rabbitmq.client.Connection;
import com.rabbitmq.client.ConnectionFactory;
import com.rabbitmq.client.DeliverCallback;

public class App 
{
    public static void main( String[] args ) throws Exception
    {
        System.out.println( "Hello World!" );

        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost("localhost");
        factory.setVirtualHost("gft");
        Connection connection = factory.newConnection();
        Channel channel = connection.createChannel();

//        channel.queueDeclare("java.queue", false, false, false, null);
 //       System.out.println(" [*] Waiting for messages. To exit press CTRL+C");

        DeliverCallback deliverCallback = (consumerTag, delivery) -> {
            String message = new String(delivery.getBody(), "UTF-8");
           // channel.basicNack(delivery.getEnvelope().getDeliveryTag(),false,true );
            channel.basicAck(delivery.getEnvelope().getDeliveryTag(),false);
            System.out.println(" [x] Received '" + message + "'");
        };
        channel.basicConsume("java.queue", false, deliverCallback, consumerTag -> { });

    }
}
