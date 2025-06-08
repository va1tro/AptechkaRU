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
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();
        private int userId;

        public CartPage()
        {
            InitializeComponent();
            userId = AppConnect.CurrentUser.user_id;
            LoadCart();
        }

        private void LoadCart()
        {
            var cartItems = db.Cart
                .Where(c => c.user_id == userId)
                .ToList();

            // Установка заглушек для изображений
            //foreach (var item in cartItems)
            //{
            //    if (item.Medicines.image_url == null || item.Medicines.image_url.Length == 0)
            //    {
            //        item.Medicines.CurrentPhoto = new BitmapImage(new Uri("/Images/picture.jpg", UriKind.Relative));
            //    }
            //    else
            //    {
            //        using (var ms = new System.IO.MemoryStream(item.Medicines.image_url))
            //        {
            //            BitmapImage image = new BitmapImage();
            //            image.BeginInit();
            //            image.CacheOption = BitmapCacheOption.OnLoad;
            //            image.StreamSource = ms;
            //            image.EndInit();
            //            item.Medicines.CurrentPhoto = image;
            //        }
            //    }
            //}

            CartList.ItemsSource = cartItems;
            UpdateTotal(cartItems);
        }

        private void UpdateTotal(List<Cart> cartItems)
        {
            decimal total = cartItems.Sum(i => i.Medicines.price * i.quantity);
            TotalText.Text = $"Итого: {total:C}";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int cartId))
            {
                var toRemove = db.Cart.FirstOrDefault(c => c.cart_id == cartId);
                if (toRemove != null)
                {
                    db.Cart.Remove(toRemove);
                    db.SaveChanges();
                    LoadCart();
                }
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            var cartItems = db.Cart.Where(c => c.user_id == userId).ToList();
            if (!cartItems.Any())
            {
                MessageBox.Show("Корзина пуста!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var total = cartItems.Sum(i => i.Medicines.price * i.quantity);

            // Здесь можно выбрать аптеку — пока берём первую
            int pharmacyId = db.Pharmacies.FirstOrDefault()?.pharmacy_id ?? 1;

            var newPurchase = new PurchaseHistory
            {
                user_id = userId,
                pharmacy_id = pharmacyId,
                purchase_date = DateTime.Now,
                total_amount = total
            };

            db.PurchaseHistory.Add(newPurchase);
            db.SaveChanges(); // Сначала сохраняем, чтобы получить purchase_id

            foreach (var item in cartItems)
            {
                db.PurchasedMedicines.Add(new PurchasedMedicines
                {
                    purchase_id = newPurchase.purchase_id,
                    medicine_id = item.medicine_id,
                    quantity = item.quantity,
                    price_per_unit = item.Medicines.price
                });
            }

            db.Cart.RemoveRange(cartItems);
            db.SaveChanges();

            MessageBox.Show("Заказ успешно оформлен!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCart();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

