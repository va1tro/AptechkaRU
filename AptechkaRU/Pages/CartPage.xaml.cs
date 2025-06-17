using AptechkaRU.AppData;
using iText.IO.Font.Constants;
using iText.Kernel.Font; // Добавьте эту директиву using в начале файла
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace AptechkaRU.Pages
{
    public partial class CartPage : Page
    {
        private AptechkaRUEntities1 db = new AptechkaRUEntities1();
        private int userId;

        public CartPage()
        {
            InitializeComponent();
            userId = AppConnect.CurrentUser.user_id;
            LoadCart();
        }

        private void LoadCart()
        {
            var cartItems = db.Cart
                .Where(c => c.user_id == userId)
                .ToList();

            CartList.ItemsSource = cartItems;
            UpdateTotal(cartItems);
        }

        private void UpdateTotal(List<Cart> cartItems)
        {
            decimal total = cartItems.Sum(i => i.Medicines.price * i.quantity);
            TotalText.Text = $"Итого: {total:C}";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int cartId))
            {
                var toRemove = db.Cart.FirstOrDefault(c => c.cart_id == cartId);
                if (toRemove != null)
                {
                    db.Cart.Remove(toRemove);
                    db.SaveChanges();
                    LoadCart();
                }
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            var cartItems = db.Cart.Where(c => c.user_id == userId).ToList();
            if (!cartItems.Any())
            {
                MessageBox.Show("Корзина пуста!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var total = cartItems.Sum(i => i.Medicines.price * i.quantity);

            // Здесь можно выбрать аптеку — пока берём первую
            int pharmacyId = db.Pharmacies.FirstOrDefault()?.pharmacy_id ?? 1;

            var newPurchase = new PurchaseHistory
            {
                user_id = userId,
                pharmacy_id = pharmacyId,
                purchase_date = DateTime.Now,
                total_amount = total
            };

            db.PurchaseHistory.Add(newPurchase);
            db.SaveChanges(); // Сначала сохраняем, чтобы получить purchase_id

            foreach (var item in cartItems)
            {
                db.PurchasedMedicines.Add(new PurchasedMedicines
                {
                    purchase_id = newPurchase.purchase_id,
                    medicine_id = item.medicine_id,
                    quantity = item.quantity,
                    price_per_unit = item.Medicines.price
                });
            }

            db.Cart.RemoveRange(cartItems);
            db.SaveChanges();

            // Генерация чека
            GenerateReceiptWithQuestPDF(newPurchase, cartItems);

            MessageBox.Show("Заказ успешно оформлен!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCart();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        // Установите пакет: Install-Package QuestPDF

        private void GenerateReceiptWithQuestPDF(PurchaseHistory purchase, List<Cart> cartItems)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            if (purchase == null || cartItems == null || !cartItems.Any())
            {
                MessageBox.Show("Ошибка: Данные для чека отсутствуют.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Чек_{purchase.purchase_id}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var document = QuestPDF.Fluent.Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A5); // Установим размер страницы для читаемости
                            page.Margin(1, Unit.Centimetre); // Добавим отступы
                            page.Content().Column(column =>
                            {
                                column.Item().Text("Аптечка.РУ").Bold().FontSize(20);
                                column.Item().Text($"Кассовый чек #{purchase.purchase_id}").Bold().FontSize(16);
                                column.Item().Text($"Дата: {purchase.purchase_date:dd.MM.yyyy HH:mm}").FontSize(12);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Наименование").Bold();
                                        header.Cell().Text("Цена").Bold();
                                        header.Cell().Text("Кол-во").Bold();
                                        header.Cell().Text("Сумма").Bold();
                                    });

                                    foreach (var item in cartItems)
                                    {
                                        if (item.Medicines == null)
                                        {
                                            continue; // Пропускаем, если данные отсутствуют
                                        }
                                        table.Cell().Text(item.Medicines.name ?? "Не указано");
                                        table.Cell().Text(item.Medicines.price.ToString("C"));
                                        table.Cell().Text(item.quantity.ToString());
                                        table.Cell().Text((item.Medicines.price * item.quantity).ToString("C"));
                                    }
                                });

                                column.Item().AlignRight().Text($"Итого: {purchase.total_amount:C}").Bold().FontSize(14);
                                column.Item().Text("Спасибо за покупку!").FontSize(12);
                            });
                        });
                    });

                    document.GeneratePdf(saveFileDialog.FileName);
                    MessageBox.Show("Чек создан успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании чека: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}