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
    /// Логика взаимодействия для UserMedicineKitsPage.xaml
    /// </summary>
    public partial class UserMedicineKitsPage : Page
    {
        private Users currentUser;
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();

        public UserMedicineKitsPage(Users user)
        {
            InitializeComponent();
            currentUser = user;
            LoadKits();
        }

        private void LoadKits()
        {
            KitsListBox.ItemsSource = db.UserMedicineKits
                .Where(k => k.user_id == currentUser.user_id)
                .ToList();
        }

        private void KitsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (KitsListBox.SelectedItem is UserMedicineKits selectedKit)
            {
                MedicinesListView.ItemsSource = db.KitMedicines
                    .Where(km => km.kit_id == selectedKit.kit_id)
                    .ToList();
            }
            else
            {
                MedicinesListView.ItemsSource = null;
            }
        }

        private void AddKit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog();
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                var newKit = new UserMedicineKits
                {
                    name = dialog.Answer,
                    user_id = AppConnect.CurrentUser.user_id, // замените на вашу текущую переменную
                    created_at = DateTime.Now
                };

                db.UserMedicineKits.Add(newKit);
                db.SaveChanges();
                MessageBox.Show("Аптечка добавлена!", "Успешно");
            }
        }

        private void AddMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (KitsListBox.SelectedItem is UserMedicineKits selectedKit)
            {
                NavigationService.Navigate(new AddKitMedicinePage(selectedKit.kit_id));
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

