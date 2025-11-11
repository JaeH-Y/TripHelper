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

namespace Trip.Views
{
    /// <summary>
    /// FavoriteDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FavoriteDialog : Window
    {
        public FavoriteDialogResult Result { get; private set; } = FavoriteDialogResult.None;
        public List<string> InputTexts { get; private set; } = new List<string>();

        public FavoriteDialog(string titleT, string headt, string t1, string t2, string t3, string okText, string cancelText)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Title = titleT;
            HeadText.Text = headt;
            Text1.Text = t1;
            Text2.Text = t2;
            Text3.Text = t3;
            TextBox3.Text = "Default";
            OkBtnText.Text = okText;
            CancelBtnText.Text = cancelText;

            IsTextUseCheck();
        }

        private void IsTextUseCheck()
        {
            HeadText.Visibility = string.IsNullOrEmpty(HeadText.Text) ? Visibility.Collapsed : Visibility.Visible;

            // Text1~3
            Text1.Visibility = string.IsNullOrEmpty(Text1.Text) ? Visibility.Collapsed : Visibility.Visible;
            Text2.Visibility = string.IsNullOrEmpty(Text2.Text) ? Visibility.Collapsed : Visibility.Visible;
            Text3.Visibility = string.IsNullOrEmpty(Text3.Text) ? Visibility.Collapsed : Visibility.Visible;

            // TextBox1~3
            TextBox1.Visibility = Text1.Visibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
            TextBox2.Visibility = Text2.Visibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
            TextBox3.Visibility = Text3.Visibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;

            // OK/Cancel 버튼 텍스트가 없으면 버튼 자체를 감추기
            OkBtn.Visibility = string.IsNullOrEmpty(OkBtnText.Text) ? Visibility.Collapsed : Visibility.Visible;
            CancelBtn.Visibility = string.IsNullOrEmpty(CancelBtnText.Text) ? Visibility.Collapsed : Visibility.Visible;
        }
        private void ResultBtn_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn)
            {
                switch (btn.Name)
                {
                    case "OkBtn":
                        InputTexts = new List<string>
                        {
                            TextBox1.Text, TextBox2.Text, TextBox3.Text,
                        };
                        Result = FavoriteDialogResult.Ok;
                        this.DialogResult = true;
                        break;
                    case "CancelBtn":
                        Result= FavoriteDialogResult.Cancel;
                        this.DialogResult = true;
                        break;
                }
            }
        }
        public enum FavoriteDialogResult
        {
            None,
            Ok,
            Cancel
        }
    }
}
