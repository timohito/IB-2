using IB2;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Unity;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Dependency]
        public IUnityContainer Container { get; set; }

        private string Password { get; set; }
        private EncryptLogic _cryptoLogic;
        public MainWindow(EncryptLogic cryptoLogic)
        {
            InitializeComponent();
            _cryptoLogic = cryptoLogic;
        }
        private void buttonEncode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxPassword.Text))
            {
                MessageBox.Show("Заполните пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxInputFile.Text))
            {
                MessageBox.Show("Выберете входной файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxOutputFile.Text))
            {
                MessageBox.Show("Выберете выходной файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                _cryptoLogic.Encode(TextBoxPassword.Text, TextBoxInputFile.Text, TextBoxOutputFile.Text);
                MessageBox.Show("Закодировано", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void buttonDecode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxPassword.Text))
            {
                MessageBox.Show("Заполните пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxInputFile.Text))
            {
                MessageBox.Show("Выберете входной файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(TextBoxOutputFile.Text))
            {
                MessageBox.Show("Выберете выходной файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                _cryptoLogic.Decode(TextBoxPassword.Text, TextBoxInputFile.Text, TextBoxOutputFile.Text);
                MessageBox.Show("Раскодировано", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemRef_Click(object sender, RoutedEventArgs e)
        {
            var window = Container.Resolve<RefWindow>();
            window.ShowDialog();
        }

        private void buttonSelectFilePassword_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt|*.txt";

            if ((bool)dialog.ShowDialog())
            {
                try
                {
                    TextBoxPassword.Text = _cryptoLogic.readFile(dialog.FileName);
                    MessageBox.Show("Пароль выбран из файла", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void buttonSelectInputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt|*.txt";

            if ((bool)dialog.ShowDialog())
            {
                try
                {
                    TextBoxInputFile.Text = dialog.FileName;
                    MessageBox.Show("Выбран входной файл", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void buttonSelectOutputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt|*.txt";

            if ((bool)dialog.ShowDialog())
            {
                try
                {
                    TextBoxOutputFile.Text = dialog.FileName;
                    MessageBox.Show("Выбран выходной файл", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CheckBoxPasswordOnFile_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
