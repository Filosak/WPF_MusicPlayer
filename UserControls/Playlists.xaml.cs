using Music_player_working.Model;
using Music_player_working.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Music_Player_working.UserControls
{
    /// <summary>
    /// Interaction logic for Playlists.xaml
    /// </summary>
    public partial class Playlists : UserControl
    {
        public Playlists()
        {
            InitializeComponent();
        }

        public void DragOverEvent(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                SongModel SendingSong = (SongModel)e.Data.GetData(DataFormats.Serializable);
                SongModel TargetSong = (SongModel)element.DataContext;

                if (TargetSong == SendingSong)
                {
                    return;
                }

                MainViewModel a = (MainViewModel)DataContext;
                a.SwapSongsCommand.Execute(new SwapObject(SendingSong, TargetSong));
            }
        }

        public void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source != null && sender is FrameworkElement element)
                {
                    object item = element.DataContext;


                    DragDrop.DoDragDrop(element, new DataObject(DataFormats.Serializable, item), DragDropEffects.Move);
                }
            }
        }
    }
}
