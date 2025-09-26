using RabbitMQ.Client;
using System.Text;

// Tạo kết nối
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// Khai báo hàng đợi
await channel.QueueDeclareAsync(queue: "task_queue", durable: true, exclusive: false,
    autoDelete: false, arguments: null);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

// Đánh dấu tin nhắn là bền vững
var properties = new BasicProperties
{
    Persistent = true
};

// Gửi tin nhắn vào hàng đợi
await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "task_queue", mandatory: true,
    basicProperties: properties, body: body);
Console.WriteLine($" [x] Sent {message}");

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}