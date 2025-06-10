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
            UpdateList();
        }

        private void UpdateList()
        {
            var filtered = allMedicines.Where(m => string.IsNullOrWhiteSpace(TextSearch.Text) ||
                                                   m.name.ToLower().Contains(TextSearch.Text.ToLower())).ToList();

            switch ((ComboSort.SelectedItem as ComboBoxItem)?.Content.ToString())
            {
                case "По алфавиту":
                    filtered = filtered.OrderBy(m => m.name).ToList(); break;
                case "По цене (по возрастанию)":
                    filtered = filtered.OrderBy(m => m.price).ToList(); break;
                case "По цене (по убыванию)":
                    filtered = filtered.OrderByDescending(m => m.price).ToList(); break;
            }

            listMedicines.ItemsSource = filtered;
            tbCounter.Text = $"Найдено: {filtered.Count} из {allMedicines.Count}";
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();

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

    }
}
