using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_player_working.Model;
using Music_Player_working.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Music_player_working.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly YoutubeClient youtube = new();
        private readonly Random rand = new();
        private readonly string outputDirectoryVideos = @"YouTubeVideo\";
        private readonly string outputDirectoryVideosNoDash = @"YouTubeVideo";
        private readonly string outputDirectoryThumbnails = @"YoutubeThumbnails\";
        private readonly string outputDirectoryThumbnailsNoDash = @"YoutubeThumbnails";
        private bool playlistIsSelected = false;
        private bool playlistViewIsSelected = false;
        private bool editing = false;
        private SongModel? popupSelectedSong = null;
        private List<SongModel> tempSongs = new();


        [ObservableProperty] private AudioPlayer audioPlayer;
        [ObservableProperty] private ObservableCollection<SongModel> remainingSongs = new ObservableCollection<SongModel>();
        [ObservableProperty] private ObservableCollection<VideoSearchResult> searchResults = new();
        [ObservableProperty] private string searchedPhrase;
        [ObservableProperty] private string songUlr = string.Empty;
        [ObservableProperty] private string currentlyDownloadingSong = string.Empty;
        [ObservableProperty] private string activeView = "Normal";
        [ObservableProperty] private string? playlistName = string.Empty;
        [ObservableProperty] private string? playlistDescription = string.Empty;
        [ObservableProperty] private bool isPopupOpened = false;

        public MainViewModel()
        {
            audioPlayer = new AudioPlayer(outputDirectoryThumbnails);
            LoadSongsFromJson();

            if (!Directory.Exists(outputDirectoryVideosNoDash))
            {
                Trace.WriteLine("no videos");
                Directory.CreateDirectory(outputDirectoryVideosNoDash);
            }

            if (!Directory.Exists(outputDirectoryThumbnailsNoDash))
            {
                Trace.WriteLine("no thumbnails");
                Directory.CreateDirectory(outputDirectoryThumbnailsNoDash);
            }
        }

        [RelayCommand]
        async private Task AddSong()
        {
            if (SongUlr.Contains("playlist"))
            {
                var videos = await youtube.Playlists.GetVideosAsync(SongUlr).CollectAsync(30);

                int i = 1;
                foreach (var video in videos)
                {
                    CurrentlyDownloadingSong = $"[{i}/{videos.Count}] {video.Title}";
                    await DownloadYouTubeVideo(video.Url, outputDirectoryVideos);
                    i++;
                }
            }
            else
            {
                await DownloadYouTubeVideo(SongUlr, outputDirectoryVideos);
            }
            SongUlr = string.Empty;
            CurrentlyDownloadingSong = string.Empty;
        }

        [RelayCommand]
        private async Task AddSongFromSearch(object sender)
        {
            VideoSearchResult Song = (VideoSearchResult)sender;

            SongUlr = Song.Url;
            await AddSong();
        }

        [RelayCommand]
        private void PlaySong(object sender)
        {
            AudioPlayer.playlistIsSelected = playlistViewIsSelected;
            AudioPlayer.Play((SongModel)sender);
        }

        [RelayCommand] void PlayFromHistory(object sender)
        {
            SongModel tempSongModel = (SongModel)sender;

            if (tempSongModel.IsInPlaylistHistory)
            {
                ChangeView("PlaylistSongs");
            }
            else
            {
                ChangeView("Normal");
            }

            SelectPlaylist(AudioPlayer.Playlists.First(c => c.Name == tempSongModel.playlistNameInHistory));
            PlaySong(sender);
        }

        [RelayCommand]
        private void DeleteSong(object sender)
        {
            SongModel songm = (SongModel)sender;

            if (File.Exists($"{outputDirectoryVideos}{songm.Name}.Mp4")) {
                File.Delete($"{outputDirectoryVideos}{songm.Name}.Mp4");
            }
            if (File.Exists($"{outputDirectoryThumbnails}{songm.Name}.jpg")) {
                File.Delete($"{outputDirectoryThumbnails}{songm.Name}.jpg");
            }

            if (ActiveView == "PlaylistSongs")
            {
                AudioPlayer.currPlaylist?.RemoveSong(songm);
                AudioPlayer.PlaylistSongs.Remove(songm);
            } else
            {
                AudioPlayer.Songs?.Remove(songm);
                for (int i = songm.Possition; i < AudioPlayer.Songs?.Count; i++)
                {
                    AudioPlayer.Songs[i].Possition -= 1;

                    if (AudioPlayer.Songs[i] == AudioPlayer.currentSong)
                    {
                        AudioPlayer.currentpossition -= 1;
                    }
                }
            }
        }

        [RelayCommand]
        private async Task SearchSong()
        {
            SearchResults.Clear();
            var videos = await youtube.Search.GetResultsAsync(SearchedPhrase).CollectAsync(10);

            foreach (var result in videos)
            {
                switch (result)
                {
                    case VideoSearchResult videoResult:
                        {
                            SearchResults.Add(videoResult);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        [RelayCommand]
        private void SelectPlaylist(object sender)
        {
            ChangeView("PlaylistSongs");
            AudioPlayer.currPlaylist = (PlaylistModel)sender;
            AudioPlayer.PlaylistSongs.Clear();

            foreach (var item in AudioPlayer.currPlaylist.GetSongs())
            {
                AudioPlayer.PlaylistSongs.Add(item);
            }
        }

        [RelayCommand]
        private void EditPlaylist(object sender)
        {
            ResetRemainingSongs();
            ChangeView("PlaylistEdit");

            PlaylistModel tempPlaylist = (PlaylistModel)sender;
            AudioPlayer.Playlists.Remove(tempPlaylist);

            PlaylistName = tempPlaylist.Name;
            PlaylistDescription = tempPlaylist.Description;

            foreach (var item in tempPlaylist.GetSongs())
            {
                tempSongs.Add(item);
                RemainingSongs.Remove(item);
            }
        }

        [RelayCommand]
        private void StopOrContinueMusic()
        {
            AudioPlayer.PauseOrResume();
        }

        [RelayCommand]
        private void PreviousSongBtn() // if shufleing sotre the previous
        {
            AudioPlayer.Previous();
        }

        [RelayCommand]
        private void NextSongBtn()
        {
            AudioPlayer.Next();
        }

        [RelayCommand]
        private void LoopSongBtn()
        {
            AudioPlayer.IsLooping = !AudioPlayer.IsLooping;
            AudioPlayer.IsShufleing = false;
        }

        [RelayCommand]
        private void ShufleSongsBtn()
        {
            AudioPlayer.IsShufleing = !AudioPlayer.IsShufleing;
            AudioPlayer.IsLooping = false;
        }

        async private Task DownloadStream(string Url, string title)
        {
            var video = await youtube.Videos.Streams.GetManifestAsync(Url);
            var desired_video = video.GetAudioOnlyStreams().Where(x => x.Container == YoutubeExplode.Videos.Streams.Container.Mp4).GetWithHighestBitrate();
            var stream = await youtube.Videos.Streams.GetAsync(desired_video);

            await youtube.Videos.Streams.DownloadAsync(desired_video, $"{outputDirectoryVideos}{title}.Mp4");
        }

        async private Task DownloadThumbnail(Uri Url, string title)
        {
            using (WebClient client = new())
            {
                client.DownloadFile(Url, $"{outputDirectoryThumbnails}{title}.jpg");
            }
        }

        async private Task DownloadYouTubeVideo(string videoUrl, string outputDirectory)
        {
            var videoInfo = await youtube.Videos.GetAsync(videoUrl);

            if (!File.Exists($"{outputDirectoryVideos}{videoInfo.Title}.Mp4"))
            {
                await DownloadStream(videoUrl, videoInfo.Title);
            }

            if (!File.Exists($"{outputDirectoryThumbnails}{videoInfo.Title}.jpg"))
            {
                await DownloadThumbnail(new Uri(videoInfo.Thumbnails.GetWithHighestResolution().Url), videoInfo.Title);
            }

            SongModel currVideoModel = new SongModel(videoInfo.Title,
                                                     videoInfo.Duration.Value,
                                                     videoInfo.Author.ChannelTitle,
                                                     videoUrl,
                                                     videoInfo.Thumbnails.GetWithHighestResolution().Url,
                                                     $"{outputDirectory}{videoInfo.Title}.Mp4",
                                                     $"{outputDirectoryThumbnails}{videoInfo.Title}.jpg",
                                                     AudioPlayer.Songs.Count);
            AudioPlayer.Songs.Add(currVideoModel);
        }

        [RelayCommand]
        private void SwapSongs(SwapObject songsToSwap)
        {
            AudioPlayer.SwapSongs(songsToSwap, ActiveView);
        }

        [RelayCommand(CanExecute = nameof(CanAddSong))]
        private void AddSongToPlaylist(object sender)
        {
            SongModel currSong = (SongModel)sender;
            tempSongs.Add(currSong);
            RemainingSongs.Remove(currSong);
        }

        private bool CanAddSong(object sender)
        {
            SongModel currSong = (SongModel)sender;
            if (tempSongs.Contains(currSong)) 
            {
                return false;
            }
            return true;
        }

        private void ResetRemainingSongs()
        {
            RemainingSongs.Clear();

            foreach (var item in AudioPlayer.Songs)
            {
                RemainingSongs.Add(item);
            }
        }

        [RelayCommand]
        private void AddPlaylist()
        {
            PlaylistModel playlistToAdd = new PlaylistModel(PlaylistName, PlaylistDescription);
            PlaylistName = string.Empty; 
            PlaylistDescription = string.Empty;

            foreach (var item in tempSongs)
            {
                playlistToAdd.AddSong(item);
            }

            AudioPlayer.Playlists.Add(playlistToAdd);
            tempSongs.Clear();
            ResetRemainingSongs();
            ActiveView = "Playlists";
            editing = false;
        }

        [RelayCommand]
        private void CancelPlaylist()
        {
            PlaylistName = string.Empty;
            PlaylistDescription = string.Empty;
            tempSongs.Clear();
            ResetRemainingSongs();
            editing = false;
            ChangeView("Normal");
        }

        [RelayCommand]
        private void ChangeStateOfPopup(object sender)
        {
            IsPopupOpened = !IsPopupOpened;

            if (sender == null)
            {
                return;
            }

            popupSelectedSong = (SongModel)sender;

            foreach (var item in AudioPlayer.Playlists)
            {
                if (item.Contains(popupSelectedSong))
                {
                    item.CheckedInPopup = true;
                }
                else
                {
                    item.CheckedInPopup = false;
                }
            }
        }

        [RelayCommand]
        private void PopupSave()
        {
            foreach (var item in AudioPlayer.Playlists)
            {
                if (item.CheckedInPopup)
                {
                    item.AddSong(popupSelectedSong);

                    if (item.Equals(AudioPlayer.currPlaylist))
                    {
                        AudioPlayer.PlaylistSongs.Add(popupSelectedSong);
                    }
                } 
                else
                {
                    item.RemoveSong(popupSelectedSong);

                    if (item.Equals(AudioPlayer.currPlaylist))
                    {
                        AudioPlayer.PlaylistSongs.Remove(popupSelectedSong);
                    }
                }
            }

            IsPopupOpened = false;
        }

        [RelayCommand]
        private void ChangeView(string mode)
        {
            if (editing == true)
            {
                AddPlaylist();
            }

            ActiveView = mode;

            if (ActiveView == "PlaylistAdd")
            {
                ResetRemainingSongs();
            }

            if (ActiveView == "PlaylistEdit")
            {
                editing = true;
            }

            if (ActiveView != "PlaylistSongs")
            {
                playlistViewIsSelected = false;
            } else
            {
                playlistViewIsSelected = true;
            }
        }

        async private Task LoadSongsFromJson()
        {
            AudioPlayer.LoadFromJSON();

            foreach (var item in AudioPlayer.Songs)
            {
                if (!File.Exists($"{outputDirectoryVideos}{item.Name}.Mp4"))
                {
                    await DownloadStream(item.VideoUrl, item.Name);
                }

                if (!File.Exists($"{outputDirectoryThumbnails}{item.Name}.jpg"))
                {
                    await DownloadThumbnail(new Uri(item.ThumbnailUrl), item.Name);
                }
            }

            ResetRemainingSongs();
        }
        
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            AudioPlayer.SaveToJSON();
        }
    }
}