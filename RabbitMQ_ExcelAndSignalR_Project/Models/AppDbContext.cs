using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RabbitMQ_ExcelAndSignalR_Project.Models
{
    public class AppDbContext : IdentityDbContext //Microsoft.AspNetCore.Identity.EntityFrameworkCore
    {
        //IdentityDbContext, ASP.NET Core Identity kullandığında kullanıcı,
        //rol ve yetkilendirme altyapısını bizim adımıza hazır getiren  DbContext sınıfıdır.


        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) 
        {
            //DbContextOptions = DbContext’in yapılandırma çantasıdır İçinde:
            //Veritabanı türü Connection string EF Core ayarları Provider bilgisi
            //optıonsu ust sınıfa yollar 
        }
        public DbSet<UserFile> UserFiles { get; set; }//user  file tablosu nasıl verı tabanına yansısın 
    }
}
