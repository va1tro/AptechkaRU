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
    /// Логика взаимодействия для AddKitMedicinePage.xaml
    /// </summary>
    public partial class AddKitMedicinePage : Page
    {
        private int kitId;
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();

        public AddKitMedicinePage(int selectedKitId)
        {
            InitializeComponent();
            kitId = selectedKitId;

            MedicineComboBox.ItemsSource = db.Medicines.ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MedicineComboBox.SelectedItem is Medicines selectedMedicine &&
                int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                var newEntry = new KitMedicines
                {
                    kit_id = kitId,
                    medicine_id = selectedMedicine.medicine_id,
                    quantity = quantity,
                    expiry_date = ExpiryDatePicker.SelectedDate,
                    notes = NotesTextBox.Text
                };

                db.KitMedicines.Add(newEntry);
                db.SaveChanges();
                NavigationService.Navigate(new UserMedicineKitsPage(AppConnect.CurrentUser));

                MessageBox.Show("Лекарство добавлено в аптечку!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Проверьте введённые данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
