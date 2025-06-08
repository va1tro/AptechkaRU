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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбор данных
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string email = EmailTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string middleName = MiddleNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на уникальность логина
            if (AppConnect.model0db.Users.Any(u => u.login == login))
            {
                MessageBox.Show("Такой логин уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Users newUser = new Users
                {
                    login = login,
                    password = password,
                    email = email,
                    first_name = firstName,
                    middle_name = middleName,
                    last_name = lastName,
                    phone = phone,
                    role_id = 2 // 1 - Admin, 2 - User (проверь в БД)
                };

                AppConnect.model0db.Users.Add(newUser);
                AppConnect.model0db.SaveChanges();

                MessageBox.Show("Вы успешно зарегистрированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new LoginPage());
            }
            catch
            {
                MessageBox.Show("Ошибка при регистрации. Проверьте корректность данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
        private void FieldChanged(object sender, RoutedEventArgs e)
        {
            // Просто заглушка для обновления привязки
        }

    }
}

