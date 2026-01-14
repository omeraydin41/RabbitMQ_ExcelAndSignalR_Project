using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ_ExcelAndSignalR_Project.Models;

namespace RabbitMQ_ExcelAndSignalR_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
        }


        //değişekn bılgılerı worker a gondermek ıcın kullanılacak
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file,int fileId)
        //gelen dosya Iform fıle uzerınden alınacaktir / userı ıd ıse dosya hangi kullanıcı için / file ıd ise hangı dosya excel oldu 
        {
            if (file is not {Length:>0}) return BadRequest("Dosya boş olamaz");

            //UserFiles context classında bulunan veritabanında arama yapmamıza izin veriri ve onun uzerınden Id ile yeni değişken fileId eşleştirdik
            var userFile =await  _context.UserFiles.FirstAsync(x => x.Id == fileId);

            var filePath =userFile.FileName + Path.GetExtension(file.FileName);

            // file path ı "wwwroot/files" klasorune kaydeder 
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            using FileStream stream = new(path, FileMode.Create);// yukardan alınana path ile yenı bir dosya oluştur
             
            //bu dosyanın içerği ne olacak 

            await file.CopyToAsync(stream);//gelen dosyayı oluşturulan dosyaya kopyala

            userFile.CreatedDate=DateTime.Now;//oluşan dosyanın oluşturulma tarihini ekledik 

            userFile.FilePath = filePath;//oluşan dosyanın dosya yolunu ekledik

            userFile.FileStatus=FileStatus.Completed;//dosya durumu tamamlandı olarak güncellendi ENUM YAPISINDAN GELEN DEĞER 

            await _context.SaveChangesAsync();//değişiklikleri veritabanına kaydet

            return Ok("Dosya yüklendi");
        }
    }
}
