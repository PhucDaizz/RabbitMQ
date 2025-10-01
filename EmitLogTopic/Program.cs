using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// 1. Khai báo Exchange loại Topic
await channel.ExchangeDeclareAsync(exchange: "topic_logs", type: ExchangeType.Topic);

var messages = new (string RoutingKey, string Content)[]
{
    ("server.log.error", "Critical server crash!"),
    ("server.log.info", "Server started successfully."),
    ("client.payment.success", "User A made a payment."),
    ("client.payment.error", "Payment failure: Gateway timeout."),
    ("db.slow_query.warning", "Database query took 5s.")
};

// 2. Gửi các tin nhắn
foreach (var msg in messages)
{
    var body = Encoding.UTF8.GetBytes(msg.Content);
    await channel.BasicPublishAsync(
        exchange: "topic_logs",
        routingKey: msg.RoutingKey,
        body: body);

    Console.WriteLine($" [x] Sent '{msg.RoutingKey}' : '{msg.Content}'");
}

Console.WriteLine(" [x] All messages sent. Press [enter] to exit.");
Console.ReadLine();