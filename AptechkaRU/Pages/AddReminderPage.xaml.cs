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
    /// Логика взаимодействия для AddReminderPage.xaml
    /// </summary>
    public partial class AddReminderPage : Page
    {
        private Users currentUser;
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();

        public AddReminderPage(Users user)
        {
            InitializeComponent();
            currentUser = user;
            LoadMedicines();
            DpStartDate.SelectedDate = DateTime.Today;
        }

        private void LoadMedicines()
        {
            try
            {
                var medicines = db.Medicines.OrderBy(m => m.name).ToList();
                ComboMedicines.ItemsSource = medicines;
                ComboMedicines.DisplayMemberPath = "name";
                ComboMedicines.SelectedValuePath = "medicine_id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки лекарств: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveReminder_Click(object sender, RoutedEventArgs e)
        {
            if (ComboMedicines.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите лекарство.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TbDosage.Text))
            {
                MessageBox.Show("Введите дозировку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TbFrequency.Text))
            {
                MessageBox.Show("Введите частоту приема.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату начала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? endDate = DpEndDate.SelectedDate;
            if (endDate != null && endDate < DpStartDate.SelectedDate)
            {
                MessageBox.Show("Дата окончания не может быть меньше даты начала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newReminder = new Reminders
                {
                    user_id = currentUser.user_id,
                    medicine_id = (int)ComboMedicines.SelectedValue,
                    dosage = TbDosage.Text.Trim(),
                    frequency = TbFrequency.Text.Trim(),
                    start_date = DpStartDate.SelectedDate.Value,
                    end_date = endDate,
                    is_active = true
                };

                db.Reminders.Add(newReminder);
                db.SaveChanges();

                MessageBox.Show("Напоминание успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Вернуться на страницу Напоминаний
                NavigationService.Navigate(new RemindersPage(AppConnect.CurrentUser));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
