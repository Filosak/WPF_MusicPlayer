using Music_player_working.Model;
using Music_Player_working.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Music_Player_working.UserControls
{
    /// <summary>
    /// Interaction logic for AddToPlaylist.xaml
    /// </summary>
    public partial class AddToPlaylist : UserControl
    {
        public AddToPlaylist()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is FrameworkElement element)
                {
                    PlaylistModel playlist = (PlaylistModel)element.DataContext;
                    playlist.CheckedInPopup = !playlist.CheckedInPopup;
                }
            }
        }
    }
}
