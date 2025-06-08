using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace AptechkaRU.AppData
{
    internal class AppConnect
    {
        public static AptechkaRUEntities1 model0db; // Замените YourDbContext на ваш класс контекста
        public static Users CurrentUser { get; set; }
        static AppConnect()
        {
            model0db = new AptechkaRUEntities1(); // Инициализация при первом обращении
        }
    }
}
    
