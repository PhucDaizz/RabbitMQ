// Producer/Program.cs

using RabbitMQ.Client;
using System.Text;

// --- KẾT NỐI TỚI RABBITMQ ---
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// --- KHAI BÁO EXCHANGE ---
// Đảm bảo exchange 'direct_logs' tồn tại, loại 'direct'
// Exchange 'direct' sẽ đẩy message tới queue có routing key khớp chính xác.
await channel.ExchangeDeclareAsync(exchange: "direct_logs", type: ExchangeType.Direct);

// --- CHUẨN BỊ MESSAGE ---
// Lấy severity từ tham số dòng lệnh, mặc định là "info"
var severity = (args.Length > 0) ? args[0] : "info";

// Lấy message từ các tham số còn lại, mặc định là "Hello World!"
var message = (args.Length > 1)
            ? string.Join(" ", args.Skip(1))
            : "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

// --- GỬI MESSAGE ---
// routingKey: Đóng vai trò như "địa chỉ" của message.
// Exchange 'direct_logs' sẽ tìm queue nào có binding key khớp với 'severity' này để gửi tới.
await channel.BasicPublishAsync(exchange: "direct_logs",
                     routingKey: severity,
                     body: body);

Console.WriteLine($" [x] Sent '{severity}':'{message}'");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

/*
# Di chuyển vào thư mục project Producer
cd Producer

# Chạy và truyền vào 'error' và nội dung tin nhắn
dotnet run -- error "A critical error has occurred!" 


dotnet run -- info "User has logged in successfully."    lần này không chạy đc vì không có consumer nào lắng nghe severity 'info'

dotnet run -- warning "Disk space is running low."  oke
*/