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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        List<Medicines> medicines;

        public AdminPage()
        {
            InitializeComponent();
            LoadData();
            SetupFilters();
        }

        private void LoadData()
        {
            medicines = AppConnect.model0db.Medicines.ToList();
            DisplayMedicines(medicines);
        }

        private void DisplayMedicines(List<Medicines> list)
        {
            MedicineWrapPanel.Children.Clear();

            foreach (var med in list)
            {
                var border = new Border
                {
                    Width = 200,
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                    BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10)
                };

                var stack = new StackPanel();

                // Изображение или заглушка
                var image = new Image
                {
                    Height = 100,
                    Margin = new Thickness(0, 0, 0, 10),
                    Stretch = System.Windows.Media.Stretch.UniformToFill
                };

                try
                {
                    if (!string.IsNullOrWhiteSpace(med.image_url))
                        image.Source = new BitmapImage(new Uri(med.image_url, UriKind.RelativeOrAbsolute));
                    else
                        image.Source = new BitmapImage(new Uri("/Resources/placeholder.png", UriKind.Relative)); // Заглушка
                }
                catch
                {
                    image.Source = new BitmapImage(new Uri("/Resources/placeholder.png", UriKind.Relative));
                }

                var nameText = new TextBlock
                {
                    Text = med.name,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var descText = new TextBlock
                {
                    Text = $"Цена: {med.price}₽",
                    FontSize = 12
                };

                stack.Children.Add(image);
                stack.Children.Add(nameText);
                stack.Children.Add(descText);
                border.Child = stack;

                MedicineWrapPanel.Children.Add(border);
            }
        }

        private void SetupFilters()
        {
            // Пример фильтра по категории
            var categories = AppConnect.model0db.MedicineCategories.Select(c => c.name).ToList();
            categories.Insert(0, "Все");
            FilterComboBox.ItemsSource = categories;
            FilterComboBox.SelectedIndex = 0;

            SortComboBox.ItemsSource = new List<string> { "По названию", "По цене (возр.)", "По цене (убыв.)" };
            SortComboBox.SelectedIndex = 0;

            SearchTextBox.TextChanged += (s, e) => ApplyFilters();
            FilterComboBox.SelectionChanged += (s, e) => ApplyFilters();
            SortComboBox.SelectionChanged += (s, e) => ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = medicines.AsQueryable();

            // Поиск
            string search = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(m => m.name.ToLower().Contains(search));

            // Фильтрация
            string selectedCategory = FilterComboBox.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(selectedCategory) && selectedCategory != "Все")
            {
                filtered = filtered.Where(m => m.MedicineCategories.name == selectedCategory);
            }

            // Сортировка
            string sortOption = SortComboBox.SelectedItem?.ToString();
            switch (sortOption)
            {
                case "По названию":
                    filtered = filtered.OrderBy(m => m.name);
                    break;
                case "По цене (возр.)":
                    filtered = filtered.OrderBy(m => m.price);
                    break;
                case "По цене (убыв.)":
                    filtered = filtered.OrderByDescending(m => m.price);
                    break;
            }

            DisplayMedicines(filtered.ToList());
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыть окно добавления лекарства", "Добавить", MessageBoxButton.OK, MessageBoxImage.Information);
            // NavigationService.Navigate(new AddEditMedicinePage(null)); // если будет отдельная страница
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выберите элемент для редактирования", "Редактировать", MessageBoxButton.OK, MessageBoxImage.Information);
            // Можно реализовать выбор через клик по карточке
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выберите элемент для удаления", "Удалить", MessageBoxButton.OK, MessageBoxImage.Warning);
            // Реализация удаления будет позже
        }
    }
}
