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
    /// Логика взаимодействия для ReviewsPage.xaml
    /// </summary>
    public partial class ReviewsPage : Page
    {
        private int _medicineId;
        private AptechkaRUEntities1 context = new AptechkaRUEntities1();

        public ReviewsPage(int medicineId)
        {
            InitializeComponent();
            _medicineId = medicineId;
            LoadReviews();
        }

        private void LoadReviews()
        {
            var reviews = context.MedicineReviews
                .Where(r => r.medicine_id == _medicineId && r.is_approved)
                .Select(r => new {
                    User = r.Users.login, // или имя
                    r.rating,
                    r.comment,
                    r.review_date
                }).ToList();

            ReviewsList.ItemsSource = reviews;
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            AddReviewWindow addReviewWindow = new AddReviewWindow(_medicineId);
            addReviewWindow.ShowDialog();
            LoadReviews(); // Обновить отзывы после добавления
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
