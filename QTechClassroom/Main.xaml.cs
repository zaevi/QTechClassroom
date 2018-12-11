using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace QTechClassroom
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            searchGrid.Visibility = (sender as ToggleButton).IsChecked.GetValueOrDefault() ?
                Visibility.Visible : Visibility.Collapsed;
        }

        bool CheckText(string text)
        {
            var nums = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (nums.All(t=>int.TryParse(t, out _)))
                return true;
            return false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (CheckText(textbox.Text))
                textbox.Foreground = Brushes.Black;
            else
                textbox.Foreground = Brushes.Red;
        }

        private async void RefreshCaptcha()
        {
            var captcha = await URP.GetCaptcha();
            imageCaptcha.Source = captcha;
            captcha.DownloadCompleted += (s,e)=> txtCaptcha.Text = Captcha.Read(captcha);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTemp();
            URP.Build();
            TestLogin();
        }

        private void imageCaptcha_Click(object sender, RoutedEventArgs e)
            => RefreshCaptcha();

        private async void TestLogin()
        {
            if(await URP.Test())
            {
                loginMsg.Content = "登录状态: 已登录";
            }
            else
            {
                loginGrid.Visibility = Visibility.Visible;
                RefreshCaptcha();
            }
        }

        private async void TryLogin()
        {
            loginMsg.Content = "尝试登录...";
            var success = await URP.Login(txtUser.Text, txtPass.Password, txtCaptcha.Text);
            if(success)
            {
                loginMsg.Content = "登录状态: 已登录";
                loginGrid.Visibility = Visibility.Collapsed;
                SaveTemp();
            }
            else
            {
                loginMsg.Content = "登录状态: 登录失败";
                RefreshCaptcha();
                txtCaptcha.Clear();
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
            => TryLogin();

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var list = await URP.GetSpareClassroom(txtJC.Text, "5001", "05", "2018-2019-1-1", txtXQ.Text, txtZC.Text);
            msgBox.Text = string.Join(" ", list);
        }

        public void SearchHelper(string jc)
        {
            var week = (DateTime.Now - new DateTime(2018, 7, 27)).Days / 7 + 1;
            var day = (int)DateTime.Now.DayOfWeek;
            txtZC.Text = week.ToString();
            txtXQ.Text = day.ToString();
            txtJC.Text = jc;
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
            => SearchHelper((sender as FrameworkElement).Tag.ToString());
    }
}
