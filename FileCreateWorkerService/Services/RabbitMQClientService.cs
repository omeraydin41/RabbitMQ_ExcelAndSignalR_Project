using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCreateWorkerService.Services
{
    public class RabbitMQClientService : IDisposable //yenısı
    {
        // RabbitMQ bağlantı ayarlarını ve bağlantı üretimini yapan factory
        private readonly ConnectionFactory _connectionFactory;

        // Uygulama loglarını yazmak için logger
        private readonly ILogger<RabbitMQClientService> _logger;

        // RabbitMQ server ile kurulan fiziksel bağlantı (TCP connection)
        private IConnection? _connection;

        // RabbitMQ üzerinde mesaj gönderip almak için kullanılan kanal
        private IChannel? _channel;



        // Mesajların yönlendirileceği exchange adı
        //public static string ExchangeName = "ExcelDirectExchange";



        // Exchange üzerinden kuyruğa yönlendirme yapılırken kullanılacak routing key
        public static string RoutingExcel = "excel-route-file";

        // Mesajların düşeceği kuyruk adı
        public static string QueueName = "queue-excel-image";

        // DI container tarafından ConnectionFactory ve Logger enjekte edilir
        public RabbitMQClientService(
            ConnectionFactory connectionFactory,
            ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory; // RabbitMQ bağlantı üreticisi atanır
            _logger = logger;                       // Logger atanır
        }

        // RabbitMQ bağlantısı ve kanalını oluşturan async metot
        public async Task<IChannel> ConnectAsync()
        {
            // Eğer daha önce oluşturulmuş ve açık bir kanal varsa tekrar oluşturma
            if (_channel is { IsOpen: true })
                return _channel;

            // Eğer connection daha önce oluşturulmadıysa RabbitMQ server'a bağlan
            _connection ??= await _connectionFactory.CreateConnectionAsync();

            // Açık connection üzerinden yeni bir kanal oluştur
            _channel = await _connection.CreateChannelAsync();



            //BU 3 U GEREKSIZ ZATEN VARLAR 

            //// Direct tipinde bir exchange oluştur (varsa tekrar oluşturmaz)
            //await _channel.ExchangeDeclareAsync(
            //    exchange: ExchangeName,             // Exchange adı
            //    type: ExchangeType.Direct,          // Direct exchange (routing key birebir eşleşir)
            //    durable: true,                      // RabbitMQ restart olsa bile silinmez
            //    autoDelete: false);                 // Kullanıcılar kopunca silinmez

            //// Mesajların tutulacağı kuyruğu oluştur
            //await _channel.QueueDeclareAsync(
            //    queue: QueueName,                   // Queue adı
            //    durable: true,                      // Kalıcı kuyruk
            //    exclusive: false,                   // Başka consumer'lar da bağlanabilir
            //    autoDelete: false);                 // Consumer kopunca silinmez

            //// Kuyruğu exchange'e routing key ile bağla
            //await _channel.QueueBindAsync(
            //    queue: QueueName,                   // Bağlanacak kuyruk
            //    exchange: ExchangeName,             // Bağlanacağı exchange
            //    routingKey: RoutingExcel);      // Mesajların yönleneceği anahtar






            // Bağlantının başarılı olduğunu logla
            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu.");

            // Oluşturulan ve ayarlanan kanalı geri döndür
            return _channel;
        }

        // Uygulama kapanırken veya servis dispose edilirken çalışır
        public void Dispose()
        {
            // Kanal açıksa kapat
            _channel?.CloseAsync();
            _channel?.Dispose();

            // Connection açıksa kapat
            _connection?.CloseAsync();
            _connection?.Dispose();

            // Bağlantının kapatıldığını logla
            _logger.LogInformation("RabbitMQ bağlantısı kapatıldı.");
        }
    }
}
