using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ_ExcelAndSignalR_Project.Models;
using RabbitMQ_ExcelAndSignalR_Project.Services;

namespace RabbitMQ_ExcelAndSignalR_Project.Controllers
{
    [Authorize]// Authentication (kimlik doğrulama) yapıldıktan SONRA çalışır.
               // Kullanıcının giriş yapıp yapmadığını ve bu Controller’a erişim yetkisi olup olmadığını kontrol eder.
               // Yetkisi yoksa → Login sayfasına yönlendirir (veya 401/403 döner).
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;//AppDbContext classından aldık nesneyı vt yonetimi için 
        private readonly UserManager<IdentityUser> _userManager;// Microsoft.AspNetCore.Identity : private değişken başına _ konulur kültür olarak 

        private readonly RabbitMQPublisher _rabbitMQPublisher; //publisher classından nesne 

        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager,RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        //task turunde çünkü kullanıcıyı bulma işlemi veritabanına gider ve bu işlem zaman alabilir..NET bu yüzden bu işlemi asenkron(async) yapar.
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new()//user alanlarının olduğu classımızdan nesne aldık bu 
            {//UserFile classından gelene alanlara yukrda oluşan aradığımız ıd değeri ve random oluşan file adını verdık ve statusu oluşturuluyor 
                UserId = user.Id,
                FilePath = string.Empty,
                FileName = fileName,
                FileStatus=FileStatus.Creating//enum yapısında olan seçeneklerden bırı
            };
            await _context.UserFiles.AddAsync(userFile);  //user files tablosuna bu değerler eklendi
                                                          // await çınku işlem yapılırken beklemesin işleme devam etsin


            await _context.SaveChangesAsync();



           await  _rabbitMQPublisher.PublishAsync(new Shared.CreateExcelMessage() {FileId=userFile.Id });



            //: view back uzerınden taşınamaz aynı requst uzerınde data taşır  
            //bir requstten başkasına data taşımanın yolu TempData dır taşıma yontemi ıse HTTP durumsuz bır protoklur .DATA COOKİDE TUTULUR 
            TempData["StartCreatingExcel"] = true;
            //rabbit mq ya bu satırda mesaj gonderilecek 




            return RedirectToAction(nameof(Files));//Kayıt işlemi bittikten sonra kullanıcıyı Files sayfasına yönlendirmek
                                                   //RedirectToAction yeni bir HTTP isteği başlatır, return View() aynı isteği kullanır.
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);//ıdentıtyden user managerın bılgılerı alındı 


            return View(await _context.UserFiles.Where(x=>x.UserId==user.Id).ToListAsync());
            //UserFilesdb deki tablomuzdan user ıd leri eşit olanları lıstele dedik 
        }

    }
}
