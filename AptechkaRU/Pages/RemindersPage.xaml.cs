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
    /// Логика взаимодействия для RemindersPage.xaml
    /// </summary>
    public partial class RemindersPage : Page
    {
        private Users currentUser;
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();

        public RemindersPage(Users user)
        {
            InitializeComponent();
            currentUser = user;
            LoadReminders();
        }

        private void LoadReminders()
        {
            try
            {
                var reminders = db.Reminders
                    .Where(r => r.user_id == currentUser.user_id)
                    .ToList();

                RemindersListView.ItemsSource = reminders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки напоминаний: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddReminder_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddReminderPage(AppConnect.CurrentUser));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
             NavigationService.GoBack();
        }
    }
}