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
    /// Логика взаимодействия для AdminProfilePage.xaml
    /// </summary>
    public partial class AdminProfilePage : Page
    {
        private Users currentUser;

        public AdminProfilePage(Users user)
        {
            InitializeComponent();

            currentUser = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            TbLastName.Text = currentUser.last_name;
            TbFirstName.Text = currentUser.first_name;
            TbMiddleName.Text = currentUser.middle_name;
            TbPhone.Text = currentUser.phone;
            TbEmail.Text = currentUser.email;
            PbNewPassword.Password = string.Empty; // пустой пароль при загрузке
        }

        private void SaveUserInfo_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new AptechkaRUEntities1())
            {
                var userToUpdate = db.Users.FirstOrDefault(u => u.user_id == currentUser.user_id);
                if (userToUpdate != null)
                {
                    userToUpdate.last_name = TbLastName.Text.Trim();
                    userToUpdate.first_name = TbFirstName.Text.Trim();
                    userToUpdate.middle_name = TbMiddleName.Text.Trim();
                    userToUpdate.phone = TbPhone.Text.Trim();
                    userToUpdate.email = TbEmail.Text.Trim();

                    // Изменяем пароль, если введено новое значение
                    if (!string.IsNullOrWhiteSpace(PbNewPassword.Password))
                    {
                        // Здесь можно добавить хэширование пароля, если нужно
                        userToUpdate.password = PbNewPassword.Password;
                    }

                    db.SaveChanges();

                    // Обновляем текущего пользователя, чтобы изменения сохранились в сессии
                    currentUser = userToUpdate;

                    MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
