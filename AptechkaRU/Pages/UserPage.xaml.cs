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
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();
        private List<Medicines> allMedicines;

        public UserPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadMedicines();
            Loaded += UserPage_Loaded;
        }

        private void UserPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
            LoadMedicines();
        }
        private void LoadFilters()
        {
            ComboFilter.Items.Add("Все категории");
            foreach (var cat in db.MedicineCategories.ToList())
                ComboFilter.Items.Add(cat.name);
            ComboFilter.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
        }

        private void LoadMedicines()
        {
            allMedicines = db.Medicines.ToList();
            ApplySearchFilterSort();
        }

        private void ApplySearchFilterSort()
        {
            if (allMedicines == null)
            {
                listMedicines.ItemsSource = new List<Medicines>();
                tbCounter.Text = "Найдено: 0";
                return;
            }

            IEnumerable<Medicines> filtered = allMedicines;

            // Поиск
            string search = TextSearch?.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(m => m.name != null && m.name.ToLower().Contains(search));

            // Фильтрация по категории
            if (ComboFilter?.SelectedIndex > 0)
            {
                string selectedCategory = ComboFilter.SelectedItem.ToString();
                filtered = filtered.Where(m => m.MedicineCategories != null && m.MedicineCategories.name == selectedCategory);
            }

            // Сортировка
            string sortOption = (ComboSort?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            switch (sortOption)
            {
                case "По алфавиту":
                    filtered = filtered.OrderBy(m => m.name);
                    break;
                case "По цене (по возрастанию)":
                    filtered = filtered.OrderBy(m => m.price);
                    break;
                case "По цене (по убыванию)":
                    filtered = filtered.OrderByDescending(m => m.price);
                    break;
            }

            var filteredList = filtered.ToList(); // Теперь точно не null
            listMedicines.ItemsSource = filteredList;
            tbCounter.Text = $"Найдено: {filteredList.Count}";
        }


        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilterSort();
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySearchFilterSort();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySearchFilterSort();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Medicines med)
            {
                var userId = AppConnect.CurrentUser.user_id; // предполагается, что пользователь уже авторизован
                var existing = db.Cart.FirstOrDefault(c => c.user_id == userId && c.medicine_id == med.medicine_id);
                if (existing == null)
                {
                    db.Cart.Add(new Cart
                    {
                        user_id = userId,
                        medicine_id = med.medicine_id,
                        quantity = 1
                    });
                }
                else
                {
                    existing.quantity++;
                }
                db.SaveChanges();
                MessageBox.Show("Товар добавлен в корзину!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Medicines med)
            {
                var userId = AppConnect.CurrentUser.user_id;
                if (!db.Favorites.Any(f => f.user_id == userId && f.medicine_id == med.medicine_id))
                {
                    db.Favorites.Add(new Favorites
                    {
                        user_id = userId,
                        medicine_id = med.medicine_id
                    });
                    db.SaveChanges();
                    MessageBox.Show("Добавлено в избранное", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Уже в избранном", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void listMedicines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listMedicines.SelectedItem is Medicines med)
            {
                NavigationService.Navigate(new MedicineDetailsPage(med));
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage());
        }

        private void Favorites_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FavoritesPage());
        }

        private void MyOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MyOrdersPage());
        }

        private void UserInfo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserInfoPage(AppConnect.CurrentUser));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.CurrentUser = null;
            NavigationService.Navigate(new LoginPage());
        }
        
    }
}
