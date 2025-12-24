using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class CreateExcelMessage
    {
        public string UserId { get; set; }//HANGI KULLANICI ICIN EXCEL OLUŞACAK 
        public int FileId { get; set; }  
    }
}
