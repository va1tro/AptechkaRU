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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginPlaceholder.Visibility = string.IsNullOrWhiteSpace(LoginTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(LoginTextBox); // Установить фокус на логин при загрузке
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(null, null); // Вызов обработчика входа
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            var user = AppConnect.model0db.Users
                .FirstOrDefault(u => u.login == login && u.password == password);

            if (user != null)
            {
                AppConnect.CurrentUser = user;
                MessageBox.Show("Успешный вход!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (user.role_id == 1)
                    NavigationService.Navigate(new AdminPage());
                else
                    NavigationService.Navigate(new UserPage());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

    }
}
