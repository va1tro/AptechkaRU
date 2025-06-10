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
    /// Логика взаимодействия для AddEditMedicinePage.xaml
    /// </summary>
    public partial class AddEditMedicinePage : Page
    {
        private Medicines currentMedicine;
        private AptechkaRUEntities1 context = new AptechkaRUEntities1();
        private string selectedImageFile = null;
        private string _imagePath = null; // Полный путь
        private string _imageFileName = null; // Только имя файла

        public string PageTitle => currentMedicine.medicine_id == 0 ? "Добавление лекарства" : "Редактирование лекарства";

        public AddEditMedicinePage(Medicines medicine)
        {
            InitializeComponent();

            currentMedicine = medicine ?? new Medicines();

            DataContext = this;
            LoadComboBoxes();
            FillFields();

            // 🟢 Отображаем фото при редактировании
            if (currentMedicine.medicine_id != 0 && !string.IsNullOrWhiteSpace(currentMedicine.image_url))
            {
                _imageFileName = currentMedicine.image_url;
                _imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _imageFileName);

                if (File.Exists(_imagePath))
                {
                    try
                    {
                        imgPreview.Source = new BitmapImage(new Uri(_imagePath));
                        tbImageFile.Text = _imageFileName;
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка загрузки изображения.");
                    }
                }
            }
        }

        private void LoadComboBoxes()
        {
            cbManufacturer.ItemsSource = context.Manufacturers.ToList();
            cbCategory.ItemsSource = context.MedicineCategories.ToList();
            cbCountry.ItemsSource = context.Countries.ToList();
        }

        private void FillFields()
        {
            if (currentMedicine.medicine_id == 0)
                return;

            tbName.Text = currentMedicine.name;
            tbDescription.Text = currentMedicine.description;
            tbPrice.Text = currentMedicine.price.ToString();
            tbStock.Text = currentMedicine.stock_quantity?.ToString() ?? "";
            cbPrescription.IsChecked = currentMedicine.requires_prescription ?? false;
            cbManufacturer.SelectedValue = currentMedicine.manufacturer_id;
            cbCategory.SelectedValue = currentMedicine.category_id;
            cbCountry.SelectedValue = currentMedicine.country_id;
            tbImageFile.Text = currentMedicine.image_url;
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (dlg.ShowDialog() == true)
            {
                _imagePath = dlg.FileName;
                _imageFileName = System.IO.Path.GetFileName(_imagePath);

                string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string destPath = System.IO.Path.Combine(imagesDir, _imageFileName);

                try
                {
                    // Копируем всегда, перезаписывая файл
                    File.Copy(_imagePath, destPath, overwrite: true);

                    // Отображаем изображение, используя корректный Uri
                    imgPreview.Source = new BitmapImage(new Uri(destPath, UriKind.Absolute));
                    tbImageFile.Text = _imageFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private string ValidateImagePath(string imageFileName)
        {
            string defaultImage = "picture.jpg";
            string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            // если путь пуст — возвращаем заглушку
            if (string.IsNullOrWhiteSpace(imageFileName))
                return defaultImage;

            string fullPath = System.IO.Path.Combine(imagesDir, imageFileName);

            // если файл не существует — возвращаем заглушку
            return File.Exists(fullPath) ? imageFileName : defaultImage;
        }



        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentMedicine.name = tbName.Text;
                currentMedicine.description = tbDescription.Text;
                currentMedicine.price = decimal.Parse(tbPrice.Text);
                currentMedicine.stock_quantity = int.TryParse(tbStock.Text, out int stock) ? stock : 0;
                currentMedicine.requires_prescription = cbPrescription.IsChecked;
                currentMedicine.manufacturer_id = (int)cbManufacturer.SelectedValue;
                currentMedicine.category_id = (int)cbCategory.SelectedValue;
                currentMedicine.country_id = (int?)cbCountry.SelectedValue;
                currentMedicine.image_url = ValidateImagePath(_imageFileName);

                if (currentMedicine.medicine_id == 0)
                    context.Medicines.Add(currentMedicine);

                context.SaveChanges();
                MessageBox.Show("Сохранено успешно!");
                NavigationService.Navigate(new AdminPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}