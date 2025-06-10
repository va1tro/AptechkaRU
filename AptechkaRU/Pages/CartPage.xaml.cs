using AptechkaRU.AppData;
using iText.IO.Font.Constants;
using iText.Kernel.Font; // Добавьте эту директиву using в начале файла
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
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
using iTextCell = iText.Layout.Element.Cell;
using iTextImage = iText.Layout.Element.Image;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextTable = iText.Layout.Element.Table;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;

namespace AptechkaRU.Pages
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
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
            GenerateReceipt(newPurchase, cartItems);

            MessageBox.Show("Заказ успешно оформлен!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCart();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        //private void GenerateReceipt(PurchaseHistory purchase, List<Cart> cartItems)
        //{
        //    try
        //    {
        //        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        //        {
        //            Filter = "PDF files (*.pdf)|*.pdf",
        //            FileName = $"Чек_{purchase.purchase_id}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
        //        };

        //        if (saveFileDialog.ShowDialog() == true)
        //        {
        //            using (var writer = new PdfWriter(saveFileDialog.FileName))
        //            {
        //                using (var pdf = new PdfDocument(writer))
        //                {
        //                    var document = new Document(pdf);

        //                    // Заголовок чека
        //                    document.Add(new iTextParagraph(new Text("Аптечка.РУ")
        //                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
        //                        .SetFontSize(20))
        //                        .SetTextAlignment(iTextTextAlignment.CENTER));

        //                    document.Add(new iTextParagraph("Кассовый чек")
        //                        .SetTextAlignment(iTextTextAlignment.CENTER)
        //                        .SetFontSize(16));

        //                    document.Add(new iTextParagraph($"Номер заказа: {purchase.purchase_id}")
        //                        .SetTextAlignment(iTextTextAlignment.LEFT)
        //                        .SetFontSize(12));

        //                    document.Add(new iTextParagraph($"Дата: {purchase.purchase_date:dd.MM.yyyy HH:mm:ss}")
        //                        .SetTextAlignment(iTextTextAlignment.LEFT)
        //                        .SetFontSize(12));

        //                    // Информация о покупателе
        //                    var user = db.Users.FirstOrDefault(u => u.user_id == purchase.user_id);
        //                    if (user != null)
        //                    {
        //                        document.Add(new iTextParagraph($"Покупатель: {user.first_name} {user.last_name}")
        //                            .SetTextAlignment(iTextTextAlignment.LEFT)
        //                            .SetFontSize(12));
        //                    }

        //                    // Таблица с товарами
        //                    var productsTable = new iTextTable(4).UseAllAvailableWidth(); // Изменили имя переменной

        //                    // Заголовки таблицы
        //                    productsTable.AddHeaderCell(new iTextCell().Add(new iTextParagraph("Наименование")));
        //                    productsTable.AddHeaderCell(new iTextCell().Add(new iTextParagraph("Цена")));
        //                    productsTable.AddHeaderCell(new iTextCell().Add(new iTextParagraph("Кол-во")));
        //                    productsTable.AddHeaderCell(new iTextCell().Add(new iTextParagraph("Сумма")));

        //                    // Строки с товарами
        //                    foreach (var item in cartItems)
        //                    {
        //                        productsTable.AddCell(new iTextCell().Add(new iTextParagraph(item.Medicines.name)));
        //                        productsTable.AddCell(new iTextCell().Add(new iTextParagraph(item.Medicines.price.ToString("C"))));
        //                        productsTable.AddCell(new iTextCell().Add(new iTextParagraph(item.quantity.ToString())));
        //                        productsTable.AddCell(new iTextCell().Add(new iTextParagraph((item.Medicines.price * item.quantity).ToString("C"))));
        //                    }

        //                    document.Add(productsTable);

        //                    // Итоговая сумма
        //                    document.Add(new iTextParagraph(new Text($"Итого: {purchase.total_amount:C}")
        //                        .SetTextAlignment(iTextTextAlignment.RIGHT)
        //                        .SetFontSize(14)
        //                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))));

        //                    // Благодарность
        //                    document.Add(new iTextParagraph(new Text("Спасибо за покупку!")
        //                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
        //                        .SetFontSize(12))
        //                        .SetTextAlignment(iTextTextAlignment.CENTER));

        //                    document.Close();
        //                }
        //            }

        //            MessageBox.Show($"Чек успешно сохранен: {saveFileDialog.FileName}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при создании чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        private void GenerateReceipt(PurchaseHistory purchase, List<Cart> cartItems)
        {
            // Инициализация шрифтов один раз
            PdfFont normalFont = null;
            PdfFont boldFont = null;
            PdfFont italicFont = null;

            try
            {
                // Инициализация шрифтов
                normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Чек_{purchase.purchase_id}_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                    OverwritePrompt = true
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Проверка доступности файла
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        File.Delete(saveFileDialog.FileName);
                    }

                    using (var writer = new PdfWriter(saveFileDialog.FileName, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0)))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf);

                            try
                            {
                                // Заголовок
                                document.Add(new iTextParagraph()
                                    .Add(new Text("Аптечка.РУ\n")
                                        .SetFont(boldFont)
                                        .SetFontSize(20))
                                    .SetTextAlignment(iTextTextAlignment.CENTER));

                                // Основной контент
                                document.Add(new iTextParagraph()
                                    .Add(new Text("Кассовый чек\n")
                                        .SetFont(boldFont)
                                        .SetFontSize(16))
                                    .SetTextAlignment(iTextTextAlignment.CENTER));

                                // Добавьте остальные элементы аналогичным образом

                                // Таблица товаров
                                var table = new iTextTable(UnitValue.CreatePercentArray(4)).UseAllAvailableWidth();

                                // Заголовки таблицы
                                table.AddHeaderCell(new Cell().Add(new iTextParagraph("Наименование").SetFont(boldFont)));
                                table.AddHeaderCell(new Cell().Add(new iTextParagraph("Цена").SetFont(boldFont)));
                                table.AddHeaderCell(new Cell().Add(new iTextParagraph("Кол-во").SetFont(boldFont)));
                                table.AddHeaderCell(new Cell().Add(new iTextParagraph("Сумма").SetFont(boldFont)));

                                // Строки таблицы
                                foreach (var item in cartItems)
                                {
                                    table.AddCell(new Cell().Add(new iTextParagraph(item.Medicines.name).SetFont(normalFont)));
                                    table.AddCell(new Cell().Add(new iTextParagraph(item.Medicines.price.ToString("C")).SetFont(normalFont)));
                                    table.AddCell(new Cell().Add(new iTextParagraph(item.quantity.ToString()).SetFont(normalFont)));
                                    table.AddCell(new Cell().Add(new iTextParagraph((item.Medicines.price * item.quantity).ToString("C")).SetFont(normalFont)));
                                }

                                document.Add(table);

                                // Итог
                                document.Add(new iTextParagraph()
                                    .Add(new Text($"\nИтого: {purchase.total_amount:C}")
                                        .SetFont(boldFont)
                                        .SetFontSize(14))
                                    .SetTextAlignment(iTextTextAlignment.RIGHT));

                                // Закрытие документа
                                document.Close();

                                MessageBox.Show("Чек успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                document?.Close();
                                throw new Exception("Ошибка при создании содержимого PDF: " + ex.Message);
                            }
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Ошибка доступа к файлу: {ioEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


