// Consumer/Program.cs

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

// --- KIỂM TRA THAM SỐ ĐẦU VÀO ---
if (args.Length < 1)
{
    Console.Error.WriteLine("Sử dụng: dotnet run -- [info] [warning] [error]");
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.Exit(1);
    return;
}

// --- KẾT NỐI TỚI RABBITMQ ---
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// --- KHAI BÁO EXCHANGE (phải giống hệt bên Producer) ---
await channel.ExchangeDeclareAsync(exchange: "direct_logs", type: ExchangeType.Direct);

// --- KHAI BÁO MỘT QUEUE TẠM THỜI ---
// queue: "" -> RabbitMQ sẽ tự tạo tên ngẫu nhiên
// exclusive: true -> Queue này sẽ bị xóa khi kết nối của consumer đóng lại.
var queueDeclareResult = await channel.QueueDeclareAsync(queue: "", exclusive: true);
var queueName = queueDeclareResult.QueueName;

Console.WriteLine($" [*] Queue '{queueName}' is waiting for messages.");

// --- TẠO BINDING (LIÊN KẾT) TỪ EXCHANGE TỚI QUEUE ---
// Lặp qua các severity (VD: "info", "warning") mà consumer này muốn nhận
foreach (var severity in args)
{
    // Yêu cầu exchange 'direct_logs' gửi message có routingKey khớp với 'severity' tới queue của chúng ta.
    await channel.QueueBindAsync(queue: queueName,
                      exchange: "direct_logs",
                      routingKey: severity);
    Console.WriteLine($" -> Bound to routing key: '{severity}'");
}

Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

// --- TẠO CONSUMER ---
var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var routingKey = ea.RoutingKey;
    Console.WriteLine($" [x] Received '{routingKey}':'{message}'");

    // XỬ LÝ XONG, GỬI TÍN HIỆU ACK (ACKNOWLEDGMENT) CHO RABBITMQ
    // Báo rằng "Tôi đã xử lý xong message này, bạn có thể xóa nó khỏi queue"
    // Nếu chương trình bị crash trước dòng này, message sẽ không bị mất mà được gửi lại cho consumer khác (hoặc chính nó khi khởi động lại).
    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
};

// --- BẮT ĐẦU LẮNG NGHE ---
// autoAck: false -> Chuyển sang chế độ xác nhận thủ công. Đây là cách làm an toàn nhất.
await channel.BasicConsumeAsync(queue: queueName,
                     autoAck: false,
                     consumer: consumer);

// Giữ cho chương trình chạy để lắng nghe
Console.ReadLine();


/*
# Di chuyển vào thư mục project Consumer
cd Consumer

# Chạy chương trình và truyền vào 2 tham số là 'warning' và 'error'
dotnet run -- warning error
*/