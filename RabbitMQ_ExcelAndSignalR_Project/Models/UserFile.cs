using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace RabbitMQ_ExcelAndSignalR_Project.Models
{
    public enum FileStatus //enum : sabit ve anlamlı durumlar kümesi
    {
        Creating, //Dosya oluşturuluyor
        Completed //Dosya oluşturuldu 
    }
    //EF Core enum’u int olarak saklar: Creating 0, Completed 1

    public class UserFile
    {//veri tabanında tablo tutmak için kullanılır .dosya hakındakı bılgılerı tutar .

        public int Id { get; set; }
        public string UserId { get; set; }//ef corde ıd strıng olarak tutulur 
        public string FileName { get; set; }//excel dosyasının ısmı 
        public string FilePath { get; set; }//dosyaya tıklandığı zaman nerden ındırecek 
        public DateTime? CreatedDate { get; set; }//ne zaman oluşturuldu dosya : ? başta null olsun demek 

        public FileStatus FileStatus { get; set; }
        //fıle status ilk başta creatıngte tutulur çunku tuşa basıldığında hemen oluşmaz
        //ama worker servis işi yaptıktan sonra file status  completeye alınır 
        


        [NotMapped]//bu property(özellik) veri tabanına maplanmasın(yapıda bulunmayacak)
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() : "-";
        //eğer CreatedDate değeri yoksa geriye duz bır strıng donsun 
        //=> Tek satırlık property tanımı yapmayı sağlar. { get { } } yerine kullanılır. set yok 
        //has value Değer var mı, yok mu? diye kontrol eder.bizim değişken olan CreatedDate.HasValue değer varmı 
        // ? : Koşul doğruysa soldaki, yanlışsa sağdaki değeri döndürür.
        //ToShortDateString() Method : DateTime değerini kısa tarih formatına çevirir
        //  :	   Ternary Else    Koşul false ise döndürülecek değeri ayırır.
        //"-"	string literal  Tarih yoksa UI’da boş kalmaması için gösterilecek placeholder değer.
    }
}
