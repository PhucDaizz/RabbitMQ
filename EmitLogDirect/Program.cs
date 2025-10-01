using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// 1. Khai báo Exchange loại Direct
await channel.ExchangeDeclareAsync(exchange: "direct_logs", type: ExchangeType.Direct);

// Định nghĩa một loạt các tin nhắn và Routing Key cố định để kiểm tra
var logMessages = new (string Severity, string Content)[]
{
    ("error", "Cần hành động ngay: Database connection failed!"),
    ("warning", "Sắp đầy: Dung lượng ổ đĩa còn 10%."),
    ("info", "User A đã đăng nhập thành công."),
    ("error", "Lỗi nghiêm trọng: Server treo."),
    ("debug", "Debug message - sẽ bị bỏ qua nếu không ai bind tới 'debug'") // Key không được bind
};

Console.WriteLine(" [x] Starting to publish log messages...");

// 2. Gửi các tin nhắn
foreach (var log in logMessages)
{
    var body = Encoding.UTF8.GetBytes(log.Content);
    await channel.BasicPublishAsync(
        exchange: "direct_logs",
        routingKey: log.Severity, // routingKey phải KHỚP CHÍNH XÁC với bindingKey
        body: body);

    Console.WriteLine($" [x] Sent '{log.Severity}' : '{log.Content}'");
    await Task.Delay(100); // Tạm dừng để dễ quan sát
}

Console.WriteLine("\n [x] All messages sent. Press [enter] to exit.");
Console.ReadLine();