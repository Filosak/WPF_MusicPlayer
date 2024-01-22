using Music_player_working.Model;
using Music_player_working.ViewModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Music_Player_working.UserControls
{
    /// <summary>
    /// Interaction logic for AllSongs.xaml
    /// </summary>
    public partial class AllSongs : UserControl
    {
        public AllSongs()
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

        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isOver)
            {
                return;
            }

            if (sender is FrameworkElement element)
            {
                MainViewModel a = (MainViewModel)DataContext;
                a.PlaySongCommand.Execute(element.DataContext);
            }

        }

        private bool isOver = false;
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            isOver = true;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            isOver = false;
        }
    }
}
