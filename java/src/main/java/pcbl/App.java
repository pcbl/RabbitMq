package pcbl;
import com.rabbitmq.client.Channel;
import com.rabbitmq.client.Connection;
import com.rabbitmq.client.ConnectionFactory;
import com.rabbitmq.client.DeliverCallback;

public class App 
{
    public static void main( String[] args ) throws Exception
    {
        //Creating Connection and Channel for gft Virtual Host
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost("localhost");
        factory.setVirtualHost("gft");
        Connection connection = factory.newConnection();
        Channel channel = connection.createChannel();

        //If needed we could declare the queue
        //channel.queueDeclare("java.queue", false, false, false, null);

        //Creating the Consumer
        DeliverCallback deliverCallback = (consumerTag, delivery) -> {
            String message = new String(delivery.getBody(), "UTF-8");
            //Uncomment to test transactions
            //channel.basicNack(delivery.getEnvelope().getDeliveryTag(),false,true );
            channel.basicAck(delivery.getEnvelope().getDeliveryTag(),false);
            System.out.println(" [x] Received '" + message + "'");
        };
        channel.basicConsume("java.queue", false, deliverCallback, consumerTag -> { });
    }
}
