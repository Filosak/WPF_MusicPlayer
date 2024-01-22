using Music_player_working.ViewModel;
using System.Windows;

namespace Music_Player_working
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel viewModel = new MainViewModel();
            DataContext = viewModel;
            Closing += viewModel.OnWindowClosing;
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_Maximize(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            } else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Button_Click_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
