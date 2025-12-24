using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace RabbitMQ_ExcelAndSignalR_Project.Services
{
    public class RabbitMQPublisher
    {
        // RabbitMQ bağlantı ve kanal yönetimini yapan servisi tutar
        private readonly RabbitMQClientService _rabbitMQClientService;

        // Publisher sınıfı oluşturulurken DI container tarafından RabbitMQClientService enjekte edilir
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            // Enjekte edilen servis private alana atanır
            _rabbitMQClientService = rabbitMQClientService;
        }

        // RabbitMQ'ya mesaj göndermek için kullanılan async metot
        // CreateExcelMessage => Shared proje içinde bulunan, iki proje arasında ortak kullanılan DTO
        public async Task PublishAsync(CreateExcelMessage createExcelMessage) // Shared class bağımlılıklara eklenmelidir
        {
            // RabbitMQ ile bağlantı kurar
            // Eğer daha önce açık bir kanal varsa onu döner, yoksa yeni bir kanal oluşturur
            IChannel channel = await _rabbitMQClientService.ConnectAsync();

            // Gönderilecek nesneyi JSON string formatına çevirir
            string bodyString = JsonSerializer.Serialize(createExcelMessage);

            // JSON string'i byte dizisine çevirir (RabbitMQ byte[] kabul eder)
            byte[] bodyByte = Encoding.UTF8.GetBytes(bodyString);

            // Mesajın özelliklerini belirler
            // Persistent = true → RabbitMQ restart olsa bile mesaj kaybolmaz
            var properties = new BasicProperties
            {
                Persistent = true
            };

            // Mesajı RabbitMQ'ya gönderir
            await channel.BasicPublishAsync(
                exchange: RabbitMQClientService.ExchangeName, // Mesajın gönderileceği exchange adı
                routingKey: RabbitMQClientService.RoutingExcel, // Mesajın hangi kurala göre yönlendirileceği
                basicProperties: properties, // Mesajın kalıcı vb. özellikleri
                mandatory: false, // Kuyruk yoksa mesaj geri dönmez (false → drop)
                body: bodyByte // Gönderilecek asıl mesaj (byte[])
            );
        }

    }
}
