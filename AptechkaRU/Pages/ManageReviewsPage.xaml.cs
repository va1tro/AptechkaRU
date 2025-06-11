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
using System.Data.Entity;

namespace AptechkaRU.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManageReviewsPage.xaml
    /// </summary>
    public partial class ManageReviewsPage : Page
    {
        private List<MedicineReviews> _allReviews;

        public ManageReviewsPage()
        {
            InitializeComponent();
            LoadReviews();
            cbFilter.SelectionChanged += Filter_SelectionChanged;
        }

        private void LoadReviews()
        {
            using (var context = new AptechkaRUEntities1())
            {
                _allReviews = context.MedicineReviews
                    .Include(r => r.Users)
                    .Include(r => r.Medicines)
                    .OrderByDescending(r => r.review_date)
                    .ToList();

                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            var filteredReviews = _allReviews.AsQueryable();

            switch (cbFilter.SelectedIndex)
            {
                case 1: // На модерации
                    filteredReviews = filteredReviews.Where(r => !r.is_approved);
                    break;
                case 2: // Одобренные
                    filteredReviews = filteredReviews.Where(r => r.is_approved);
                    break;
                case 3: // Отклоненные
                    filteredReviews = filteredReviews.Where(r => r.is_approved == false &&
                                                               !string.IsNullOrEmpty(r.comment));
                    break;
            }

            dgReviews.ItemsSource = filteredReviews.ToList();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (dgReviews.SelectedItem is MedicineReviews selectedReview)
            {
                using (var context = new AptechkaRUEntities1())
                {
                    var review = context.MedicineReviews.Find(selectedReview.review_id);
                    if (review != null)
                    {
                        review.is_approved = true;
                        context.SaveChanges();
                        LoadReviews();
                        MessageBox.Show("Отзыв одобрен и опубликован");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите отзыв для одобрения");
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (dgReviews.SelectedItem is MedicineReviews selectedReview)
            {
                using (var context = new AptechkaRUEntities1())
                {
                    var review = context.MedicineReviews.Find(selectedReview.review_id);
                    if (review != null)
                    {
                        context.MedicineReviews.Remove(review);
                        context.SaveChanges();
                        LoadReviews();
                        MessageBox.Show("Отзыв отклонен и удален");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите отзыв для отклонения");
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
