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
using System.Windows.Shapes;

namespace AptechkaRU.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddReviewWindow.xaml
    /// </summary>
    public partial class AddReviewWindow : Window
    {
        private int _medicineId;
        private int _userId; // получи текущего пользователя

        public AddReviewWindow(int medicineId)
        {
            InitializeComponent();
            _medicineId = medicineId;
            _userId = AppConnect.CurrentUser.user_id; // замените на вашу систему авторизации
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new AptechkaRUEntities1())
            {
                var review = new MedicineReviews
                {
                    medicine_id = _medicineId,
                    user_id = _userId,
                    rating = int.Parse(RatingTextBox.Text),
                    comment = CommentTextBox.Text,
                    review_date = DateTime.Now,
                    is_approved = false // на модерацию
                };

                context.MedicineReviews.Add(review);
                context.SaveChanges();
            }

            MessageBox.Show("Отзыв отправлен на модерацию.");
            this.Close();
        }
    }
}
