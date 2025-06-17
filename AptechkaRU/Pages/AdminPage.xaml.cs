using AptechkaRU.AppData;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private AptechkaRUEntities1 context = new AptechkaRUEntities1();
        private List<Medicines> allMedicines;

        public AdminPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            allMedicines = context.Medicines.Include("Countries").ToList();
            LoadFilters();
            LoadMedicines();
        }
        private void LoadFilters()
        {
            ComboFilter.Items.Add("Все категории");
            foreach (var cat in context.MedicineCategories.ToList())
                ComboFilter.Items.Add(cat.name);
            ComboFilter.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
        }

        private void LoadMedicines()
        {
            allMedicines = context.Medicines.ToList();
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
        
        //private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        //private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        //private void TextSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();

        private void AddMedicine_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditMedicinePage(null));
        }

        private void EditMedicine_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)(sender as Button)?.Tag;
            var item = context.Medicines.Find(id);
            NavigationService?.Navigate(new AddEditMedicinePage(item));
        }

        private void DeleteMedicine_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)(sender as Button)?.Tag;
            var item = context.Medicines.Find(id);

            if (MessageBox.Show($"Удалить {item.name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                context.Medicines.Remove(item);
                context.SaveChanges();
                LoadData();
            }
        }

        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            int medicineId = (int)(sender as Button)?.Tag;
            NavigationService?.Navigate(new ReviewsPage(medicineId));
        }

        private void ManageReviews_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ManageReviewsPage()); // создадим позже
        }
        private void listMedicines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedMedicine = listMedicines.SelectedItem as Medicines;
            if (selectedMedicine != null)
            {
                NavigationService?.Navigate(new MedicineDetailsPage(selectedMedicine)); // создадим эту страницу
            }
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                FileName = "export_medicines.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Название;Описание;Цена;Страна");

                foreach (var m in listMedicines.Items.Cast<Medicines>())
                {
                    string name = m.name?.Replace(";", ",") ?? "";
                    string desc = m.description?.Replace(";", ",") ?? "";
                    string country = m.Countries?.country_name ?? "";

                    sb.AppendLine($"{name};{desc};{m.price};{country}");
                }

                File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Файл успешно сохранён:\n" + saveFileDialog.FileName, "Экспорт CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminProfilePage(AppConnect.CurrentUser));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
        private void AboutPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AboutPage());
        }
    }
}
