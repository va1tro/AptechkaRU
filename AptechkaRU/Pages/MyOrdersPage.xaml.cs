using AptechkaRU.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AptechkaRU.Pages
{
    /// <summary>
    /// Логика взаимодействия для MyOrdersPage.xaml
    /// </summary>
    public partial class MyOrdersPage : Page
    {
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();

        public MyOrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            int userId = AppConnect.CurrentUser.user_id;

            var orders = db.PurchaseHistory
                .Where(ph => ph.user_id == userId)
                .OrderByDescending(ph => ph.purchase_date)
                .ToList()
                .Select(ph => new
                {
                    Header = $"Заказ от {ph.purchase_date?.ToString("dd.MM.yyyy")} — {ph.Pharmacies?.name} — {ph.total_amount:C}",
                    Medicines = ph.PurchasedMedicines.Select(pm => new
                    {
                        name = pm.Medicines.name,
                        quantity = pm.quantity,
                        price_per_unit = pm.price_per_unit
                    }).ToList()
                }).ToList();

            OrdersList.ItemsSource = orders;
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

