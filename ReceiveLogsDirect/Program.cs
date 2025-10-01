using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "direct_logs", type: ExchangeType.Direct);

var queueDeclareResult = await channel.QueueDeclareAsync(queue: "", exclusive: true);
var queueName = queueDeclareResult.QueueName;

// **********************************************
// Định nghĩa Binding Key CỐ ĐỊNH, thể hiện logic
// **********************************************
var severitiesToReceive = new string[] { "error", "warning" };

Console.WriteLine($" [*] Consumer 1 (Critical Handler) subscribed to: {string.Join(", ", severitiesToReceive)}.");

// --- TẠO BINDING (LIÊN KẾT) TỪ EXCHANGE TỚI QUEUE ---
foreach (var severity in severitiesToReceive)
{
    // Chỉ nhận tin nhắn có routingKey KHỚP CHÍNH XÁC với 'error' hoặc 'warning'
    await channel.QueueBindAsync(queue: queueName,
                                 exchange: "direct_logs",
                                 routingKey: severity);
    Console.WriteLine($" -> Bound to routing key: '{severity}'");
}

Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

// --- TẠO VÀ CHẠY CONSUMER ---
var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($" [x] Received CRITICAL LOG '{ea.RoutingKey}':'{message}'");
    Console.ResetColor();
    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
};

await channel.BasicConsumeAsync(queue: queueName,
                                autoAck: false,
                                consumer: consumer);

Console.ReadLine();