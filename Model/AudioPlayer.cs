using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music_player_working.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Music_Player_working.Model
{
    public partial class AudioPlayer : ObservableObject
    {
        private readonly MediaPlayer _player = new();
        private readonly DispatcherTimer timer = new();
        private readonly Random rand = new Random();
        public SongModel? currentSong = null;
        public PlaylistModel? currPlaylist = null;
        public int currentpossition = 0;
        public bool playlistIsSelected = false;
        public bool isPlaying = false;

        private double tepmCurrentProgress = 0;
        private readonly string outputDirectoryThumbnails;

        [ObservableProperty] private ObservableCollection<SongModel> songs = new();
        [ObservableProperty] private ObservableCollection<PlaylistModel> playlists = new();
        [ObservableProperty] private ObservableCollection<SongModel> playlistSongs = new();
        [ObservableProperty] private ObservableCollection<SongModel> lastPlayedSongs = new();
        [ObservableProperty] private string currentThumbnailSource = string.Empty;
        [ObservableProperty] private string currentSongName = string.Empty;
        [ObservableProperty] private string currentSongAuthor = string.Empty;
        [ObservableProperty] private string formatedCurrentProgress = string.Empty;
        [ObservableProperty] private double currentProgress = 0;
        [ObservableProperty] private int minimumProgress = 0;
        [ObservableProperty] private int maximumProgress = 100;
        [ObservableProperty] private double volume = 50;
        [ObservableProperty] private bool isPaused = false;
        [ObservableProperty] private bool isLooping = false;
        [ObservableProperty] private bool isShufleing = false;

        public AudioPlayer(string outThumbnails)
        {
            outputDirectoryThumbnails = outThumbnails;

            _player.MediaOpened += MediaPlayer_MediaOpened;
            _player.MediaEnded += MediaPlayer_MediaEnded;

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += TimerTick;
            timer.Start();
        }

        public void Play(SongModel song)
        {
            if (playlistIsSelected)
            {
                currentpossition = PlaylistSongs.IndexOf(song);
            }
            else
            {
                currentpossition = song.Possition;
            }

            SelectSong(currentpossition);
            song.IsSelected = true;
        }

        public void Next()
        {
            if (IsShufleing)
            {
                RandomSong();
                return;
            }

            currentpossition++;
            if (!SelectSong(currentpossition))
            {
                currentpossition--;
            }
        }

        public void Previous()
        {
            currentpossition--;
            if (!SelectSong(currentpossition))
            {
                currentpossition++;
            }
        }

        public void RandomSong()
        {
            if (playlistIsSelected)
            {
                currentpossition = rand.Next(PlaylistSongs.Count);
            }
            else
            {
                currentpossition = rand.Next(Songs.Count);
            }

            SelectSong(currentpossition);
        }
        private void SameSong()
        {
            SelectSong(currentpossition);
        }

        private bool SelectSong(int index)
        {
            if (currentSong != null)
            {
                currentSong.IsSelected = false;
            }

            if (playlistIsSelected)
            {
                if (index >= PlaylistSongs.Count || index < 0)
                {
                    _player.Stop();
                    return false;
                }
                currentSong = PlaylistSongs[index];
            }
            else
            {
                if (index >= Songs.Count || index < 0)
                {
                    _player.Stop();
                    isPlaying = false;
                    return false;
                }
                currentSong = Songs[index];
            }

            _player.Open(currentSong.SongPath);
            _player.Play();
            CurrentThumbnailSource = currentSong.ThumbnailFullPath;
            CurrentSongName = currentSong.Name;
            CurrentSongAuthor = currentSong.AuthorName;
            currentSong.IsSelected = true;
            isPlaying = true;
            CurrentProgress = 0;

            if (!currentSong.isInHistory)
            {
                if (LastPlayedSongs.Count > 10)
                {
                    LastPlayedSongs[9].RemoveFromHistory();
                }

                if (playlistIsSelected)
                {
                    currentSong.AddToHistory(LastPlayedSongs.Count, currPlaylist);
                    LastPlayedSongs.Add(currentSong);
                } else
                {
                    currentSong.AddToHistory(LastPlayedSongs.Count);
                    LastPlayedSongs.Add(currentSong);
                }
            }

            return true;
        }

        public void PauseOrResume()
        {
            if (!IsPaused)
            {
                _player.Pause();
                IsPaused = true;
            }
            else
            {
                _player.Play();
                IsPaused = false;
            }
        }

        public void SwapSongs(SwapObject songsToSwap, string ActiveView)
        {
            if (ActiveView == "PlaylistSongs")
            {
                currPlaylist?.SwapSongsInPlaylist(songsToSwap.SongA, songsToSwap.SongB);

                int songAIndex = PlaylistSongs.IndexOf(songsToSwap.SongA);
                int songBIndex = PlaylistSongs.IndexOf(songsToSwap.SongB);
                PlaylistSongs[songAIndex] = songsToSwap.SongB;
                PlaylistSongs[songBIndex] = songsToSwap.SongA;

                if (currentSong == songsToSwap.SongA)
                {
                    currentpossition = songBIndex;
                }
                else if (currentSong == songsToSwap.SongB)
                {
                    currentpossition = songAIndex;
                }

            }
            else
            {
                int temp = songsToSwap.SongA.Possition;
                songsToSwap.SongA.Possition = songsToSwap.SongB.Possition;
                songsToSwap.SongB.Possition = temp;

                Songs[songsToSwap.SongA.Possition] = songsToSwap.SongA;
                Songs[songsToSwap.SongB.Possition] = songsToSwap.SongB;

                if (songsToSwap.SongA == currentSong)
                {
                    currentpossition = songsToSwap.SongA.Possition;
                }
                else if (songsToSwap.SongB == currentSong)
                {
                    currentpossition = songsToSwap.SongB.Possition;
                }
            }
        }

        public double GetProgress()
        {
            return _player.Position.TotalSeconds;
        }

        public long GetTicks()
        {
            return _player.Position.Ticks;
        }

        public void ChangeVolume(double volumeValue)
        {
            _player.Volume = volumeValue / 100;
        }

        public void ChangePlayerPossition(double goToTime)
        {
            _player.Position = TimeSpan.FromSeconds(goToTime);
        }


        private void MediaPlayer_MediaOpened(object? sender, EventArgs e)
        {
            MaximumProgress = (int)_player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            if (IsLooping)
            {
                SameSong();
                return;
            }
            else if (IsShufleing)
            {
                RandomSong();
                return;
            }
            Next();
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            if (!isPlaying)
            {
                return;
            }

            if (CurrentProgress > GetProgress() || CurrentProgress < tepmCurrentProgress)
            {
                ChangePlayerPossition(CurrentProgress);
            }
            else
            {
                CurrentProgress = GetProgress();
            }

            tepmCurrentProgress = CurrentProgress;
            FormatedCurrentProgress = $"{new DateTime(GetTicks()).ToString("HH:mm:ss")} / {currentSong.Lenght}";
            ChangeVolume(Volume);
        }

        public void SaveToJSON()
        {
            if (currentSong != null)
            {
                currentSong.IsSelected = false;
            }

            foreach (var item in Playlists)
            {
                item.SetSongPositions();
            }

            File.WriteAllText(@"SavedSongs.json", JsonConvert.SerializeObject(Songs, Formatting.Indented));
            File.WriteAllText(@"SavedPlaylists.json", JsonConvert.SerializeObject(Playlists, Formatting.Indented));
        }

        public void LoadFromJSON()
        {
            if (!File.Exists(@"SavedSongs.json"))
            {
                return;
            }

            foreach (var item in JsonConvert.DeserializeObject<List<SongModel>>(File.ReadAllText(@"SavedSongs.json")))
            {
                Songs.Add(item);

                if (item.isInHistory)
                {
                    LastPlayedSongs.Add(item);
                }
            }

            if (!File.Exists(@"SavedPlaylists.json"))
            {
                return;
            }

            foreach (var item in JsonConvert.DeserializeObject<List<PlaylistModel>>(File.ReadAllText(@"SavedPlaylists.json")))
            {
                foreach (var i in item.SongsPositions)
                {
                    item.AddSong(Songs[i]);
                }

                Playlists.Add(item);
            }
        }
    }
}
