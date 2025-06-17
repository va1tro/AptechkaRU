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
        private AptechkaRUEntities1 context; // Уберите инициализацию здесь
        private string selectedImageFile = null;
        private string _imagePath = null;
        private string _imageFileName = null;

        public string PageTitle => currentMedicine.medicine_id == 0 ? "Добавление лекарства" : "Редактирование лекарства";

        public AddEditMedicinePage(Medicines medicine)
        {
            InitializeComponent();
            context = new AptechkaRUEntities1(); // Инициализируем контекст в конструкторе
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
                try
                {
                    string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(imagesDir))
                    {
                        Directory.CreateDirectory(imagesDir);
                    }

                    _imageFileName = System.IO.Path.GetFileName(dlg.FileName); // Используем исходное имя
                    _imagePath = System.IO.Path.Combine(imagesDir, _imageFileName);

                    // Копируем файл, если его еще нет, чтобы избежать перезаписи
                    if (!File.Exists(_imagePath))
                    {
                        File.Copy(dlg.FileName, _imagePath);
                    }

                    imgPreview.Source = new BitmapImage(new Uri(_imagePath));
                    tbImageFile.Text = _imageFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _imageFileName = null;
                    _imagePath = null;
                    tbImageFile.Text = "";
                    imgPreview.Source = null;
                }
            }
        }

        private string ValidateImagePath(string imageFileName)
        {
            string defaultImage = "picture.jpg";
            string imagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            if (string.IsNullOrWhiteSpace(imageFileName))
                return defaultImage;

            string fullPath = System.IO.Path.Combine(imagesDir, imageFileName);
            return File.Exists(fullPath) ? imageFileName : defaultImage;
        }



        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем обязательные поля
                if (string.IsNullOrWhiteSpace(tbName.Text) || cbManufacturer.SelectedValue == null ||
                    cbCategory.SelectedValue == null || string.IsNullOrWhiteSpace(tbPrice.Text))
                {
                    MessageBox.Show("Заполните обязательные поля: Название, Производитель, Категория, Цена");
                    return;
                }

                using (var context = new AptechkaRUEntities1())
                {
                    Medicines medicineToSave;
                    if (currentMedicine.medicine_id == 0)
                    {
                        medicineToSave = new Medicines(); // Новый объект для добавления
                    }
                    else
                    {
                        // Загружаем объект заново из текущего контекста
                        medicineToSave = context.Medicines.FirstOrDefault(m => m.medicine_id == currentMedicine.medicine_id);
                        if (medicineToSave == null)
                        {
                            MessageBox.Show("Лекарство не найдено");
                            return;
                        }
                    }

                    // Заполняем данные
                    medicineToSave.name = tbName.Text;
                    medicineToSave.description = tbDescription.Text;
                    medicineToSave.price = decimal.Parse(tbPrice.Text);
                    medicineToSave.stock_quantity = int.TryParse(tbStock.Text, out int stock) ? stock : 0;
                    medicineToSave.requires_prescription = cbPrescription.IsChecked ?? false;
                    medicineToSave.manufacturer_id = (int)cbManufacturer.SelectedValue;
                    medicineToSave.category_id = (int)cbCategory.SelectedValue;
                    medicineToSave.country_id = cbCountry.SelectedValue as int?;

                    // Обновляем изображение
                    medicineToSave.image_url = ValidateImagePath(_imageFileName ?? tbImageFile.Text);

                    // Добавляем или обновляем
                    if (currentMedicine.medicine_id == 0)
                    {
                        context.Medicines.Add(medicineToSave);
                    }
                    else
                    {
                        context.Entry(medicineToSave).State = System.Data.Entity.EntityState.Modified;
                    }

                    context.SaveChanges();
                    MessageBox.Show("Сохранено успешно!");
                    NavigationService.Navigate(new AdminPage());
                }
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