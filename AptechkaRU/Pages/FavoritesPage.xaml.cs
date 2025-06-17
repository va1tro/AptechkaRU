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
    /// Логика взаимодействия для FavoritesPage.xaml
    /// </summary>
    public partial class FavoritesPage : Page
    {
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();

        public FavoritesPage()
        {
            InitializeComponent();
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            int userId = AppConnect.CurrentUser.user_id;

            var favorites = db.Favorites
                .Where(f => f.user_id == userId)
                .AsEnumerable()
                .Select(f => new
                {
                    f.favorite_id,
                    f.Medicines.name,
                    f.Medicines.description,
                    f.Medicines.price,
                    f.Medicines.CurrentPhoto
                })

                .ToList();

            FavoritesList.ItemsSource = favorites;
        }

        private void RemoveFromFavorites_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int favoriteId)
            {
                var favorite = db.Favorites.FirstOrDefault(f => f.favorite_id == favoriteId);
                if (favorite != null)
                {
                    db.Favorites.Remove(favorite);
                    db.SaveChanges();
                    LoadFavorites();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}