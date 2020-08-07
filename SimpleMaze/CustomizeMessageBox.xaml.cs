using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleMaze {
    /// <summary>
    /// Interaction logic for ModalMessageBox.xaml
    /// </summary>
    public partial class CustomizeMessageBox : Window, INotifyPropertyChanged {
        public string MbTitle { get; set; }
        public string MbMessage { get; set; }

        public MessageBoxResult Result { get; set; }

        public event RoutedEventHandler OnActionButtonClick;

        private CustomizeMessageBox() {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Result = MessageBoxResult.OK;
            OnActionButtonClick?.Invoke(sender, e);
            this.Close();
        }

        public static MessageBoxResult Show(Window owner, string message, string title) {
            var msgbox = new CustomizeMessageBox();
            if (owner.ActualHeight < msgbox.Height) {
                throw new Exception("The height of owner window is less than ModalMessageBox.");
            }
            if (owner.Content == null) {
                throw new Exception("The main container of this windows is null.");
            }

            if (owner.Content is Panel panel) {
                panel.Children.Add(new Rectangle() { Width = panel.ActualWidth, Height = panel.ActualHeight, Fill = new SolidColorBrush(Color.FromArgb(120, 120, 120, 120)) });
            } else {
                throw new Exception("The main container of this windows is not inherit from panel.");
            }

            msgbox.Width = owner.Width - 20;
            msgbox.Left = owner.Left + (owner.Width / 2) - (msgbox.Width / 2);
            msgbox.Top = owner.Top + (owner.Height / 2) - (msgbox.Height / 2);
            msgbox.MbMessage = message;
            msgbox.MbTitle = title;
            msgbox.OnActionButtonClick += new RoutedEventHandler((obj, args) => {
                panel.Children.Remove(panel.Children[panel.Children.Count - 1]);
            });
            msgbox.Owner = owner;
            msgbox.ShowDialog();
            return msgbox.Result;
        }
    }
}
