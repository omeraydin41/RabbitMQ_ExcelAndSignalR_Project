using ClosedXML.Excel;                  // Excel dosyasý oluþturmak için kullanýlan kütüphane
using FileCreateWorkerService.Models;   // Projede kullanýlan model sýnýflarýný ekliyoruz
using FileCreateWorkerService.Services; // RabbitMQ gibi servis sýnýflarýna eriþmek için
using Microsoft.EntityFrameworkCore.Metadata; // Entity Framework metadata iþlemleri için
using Microsoft.Extensions.DependencyInjection; // Dependency Injection (DI) kullanabilmek için
using Microsoft.Extensions.Hosting;     // Worker Service altyapýsý için gerekli
using Microsoft.Extensions.Logging;     // Loglama iþlemleri için
using RabbitMQ.Client;                  // RabbitMQ baðlantý ve channel iþlemleri için
using RabbitMQ.Client.Events;           // RabbitMQ event (mesaj alma) iþlemleri için
using Shared;                           // Ortak (shared) mesaj modellerini kullanmak için
using System.Data;                      // DataTable ve DataSet iþlemleri için
using System.Net.Http;                  // HTTP istekleri atabilmek için
using System.Text;                      // Encoding iþlemleri için
using System.Text.Json;                 // JSON serialize / deserialize iþlemleri için

namespace FileCreateWorkerService;

public sealed class Worker : BackgroundService // Worker Service olduðu için BackgroundService’den kalýtým alýr
{
    private readonly ILogger<Worker> _logger; // Log yazmak için logger
    private readonly RabbitMQClientService _rabbitMQClientService; // RabbitMQ baðlantýsýný yöneten servis
    private readonly IServiceProvider _serviceProvider; // Scoped servisleri kullanabilmek için
    private readonly IHttpClientFactory _httpClientFactory; // HttpClient üretmek için

    private IModel? _channel; // RabbitMQ ile haberleþmek için channel nesnesi

    public Worker(
        ILogger<Worker> logger, // Logger DI ile alýnýr
        RabbitMQClientService rabbitMQClientService, // RabbitMQ servisi DI ile alýnýr
        IServiceProvider serviceProvider, // Service provider DI ile alýnýr
        IHttpClientFactory httpClientFactory) // HttpClientFactory DI ile alýnýr
    {
        _logger = logger; // Logger atanýr
        _rabbitMQClientService = rabbitMQClientService; // RabbitMQ servisi atanýr
        _serviceProvider = serviceProvider; // Service provider atanýr
        _httpClientFactory = httpClientFactory; // HttpClientFactory atanýr
    }

    protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
    {
        // Worker ayaða kalkýnca RabbitMQ baðlantýsý kurulur ve channel alýnýr
        _channel = (IModel?)await _rabbitMQClientService.ConnectAsync();

        // Aþaðýdaki kodlar RabbitMQ’dan mesaj dinlemek için yazýldý ama þu an yorum satýrýnda

        //var consumer = new AsyncEventingBasicConsumer((IChannel)_channel); // Asenkron consumer oluþturulur
        //consumer.ReceivedAsync += OnMessageReceived; // Mesaj gelince çalýþacak metot baðlanýr

        //_channel.BasicConsumeAsync(
        //   queue: RabbitMQClientService.QueueName, // Dinlenecek kuyruk adý
        //   autoAck: false,                          // Mesajý manuel onaylayacaðýz
        //   consumer: consumer);                    // Consumer baðlanýr

        return Task.CompletedTask; // Worker sürekli açýk kalýr ama burada iþ bitmiþ gibi döner
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs @event)
    {
        await Task.Delay(5000); // Mesaj geldikten sonra 5 saniye bekletiyoruz (örnek amaçlý)

        // Gelen mesajý JSON’dan CreateExcelMessage modeline çeviriyoruz
        var message = JsonSerializer.Deserialize<CreateExcelMessage>(
            Encoding.UTF8.GetString(@event.Body.ToArray()));

        if (message is null) // Eðer mesaj boþ gelirse devam etme
            return;

        using var memoryStream = new MemoryStream(); // Excel dosyasýný bellekte tutmak için
        using var workbook = new XLWorkbook();       // Yeni bir Excel dosyasý oluþturuyoruz

        var dataSet = new DataSet(); // Excel içine yazýlacak veriler için DataSet oluþturulur
        dataSet.Tables.Add(GetTable("products")); // Veritabanýndan gelen tablo eklenir

        workbook.Worksheets.Add(dataSet); // DataSet Excel sayfasýna eklenir
        workbook.SaveAs(memoryStream);    // Excel dosyasý memory stream içine kaydedilir

        using var content = new MultipartFormDataContent(); // Dosya upload için multipart content oluþturulur
        content.Add(
            new ByteArrayContent(memoryStream.ToArray()), // Excel byte dizisine çevrilir
            "file",                                       // API’de karþýlanacak alan adý
            $"{Guid.NewGuid()}.xlsx");                    // Dosyaya rastgele isim verilir

        var client = _httpClientFactory.CreateClient(); // HttpClient oluþturulur
        var baseUrl = "https://localhost:44321/api/files"; // Dosyanýn gönderileceði API adresi

        // Excel dosyasý HTTP POST ile baþka bir API’ye gönderilir
        var response = await client.PostAsync(
            $"{baseUrl}?fileId={message.FileId}",
            content);

        if (response.IsSuccessStatusCode) // Eðer dosya baþarýyla gönderildiyse
        {
            _logger.LogInformation( // Log atýlýr
                "File (Id: {FileId}) successfully created",
                message.FileId);

            //_channel?.BasicAck(@event.DeliveryTag, false); // Mesaj iþlendi diye RabbitMQ’ya ACK gönderilir
        }
    }

    private DataTable GetTable(string tableName) // Veritabanýndan tablo oluþturup geri döner
    {
        List<Product> products; // Ürün listesi tutulur

        using var scope = _serviceProvider.CreateScope(); // Scoped servisler için scope açýlýr
        var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>(); // DbContext alýnýr

        products = context.Products.ToList(); // Products tablosundaki veriler çekilir

        var table = new DataTable { TableName = tableName }; // DataTable oluþturulur

        // Excel’de gözükecek kolonlar tanýmlanýr
        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("ProductNumber", typeof(string));
        table.Columns.Add("Color", typeof(string));

        products.ForEach(p => // Her product için tabloya satýr eklenir
        {
            table.Rows.Add(
                p.ProductId,     // Ürün ID
                p.Name,          // Ürün adý
                p.ProductNumber, // Ürün numarasý
                p.Color);        // Ürün rengi
        });

        return table; // Oluþturulan tablo geri döndürülür
    }
}
