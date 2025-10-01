using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "topic_logs", type: ExchangeType.Topic);

QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
string queueName = queueDeclareResult.QueueName;

// **********************************************
// Định nghĩa Binding Key CỐ ĐỊNH, thể hiện logic
// **********************************************
// Binding Key: #.error -> Khớp mọi tin nhắn kết thúc bằng '.error' (0 hoặc nhiều từ trước đó)
string bindingKey = "#.error";
await channel.QueueBindAsync(queue: queueName, exchange: "topic_logs", routingKey: bindingKey);

Console.WriteLine($" [*] Consumer 1 (Errors Handler) subscribed to pattern: '{bindingKey}'. Waiting for messages.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($" [x] Received ERROR: '{ea.RoutingKey}' -> '{message}'");
    Console.ResetColor();
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
Console.ReadLine();