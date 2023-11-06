using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty
{
    public class Playlist
    {
        public byte[] Songs;
        /// <summary>
        /// Is the index of which song is playing in SONGS aka 0-Songs.Length
        /// </summary>
        public int CurrentSongIndex = 0;

        public Song CurrentSong { get { return Audio.songs[Songs[CurrentSongIndex]]; } }

        public Playlist(byte[] songs)
        {
            Songs = songs;
        }
    }

    public static class Audio
    {
        public const byte SOUND_UICLICK = 0;
        public const byte SOUND_UIERROR = 1;
        public const byte SOUND_AMMOPURCHASE = 2;
        public const byte SOUND_FIRE = 3;
        public const byte SOUND_EQUIP = 4;
        public const byte SOUND_RELOAD = 5;
        public const byte SOUND_FOOTSTEP = 6;
        public const byte SOUND_MINEHARD = 7;
        public const byte SOUND_MINESOFT = 8;
        public const byte SOUND_PLACEHARD = 9;
        public const byte SOUND_PLACESOFT = 10;
        public const byte SOUND_ZOMBIE = 11;
        public const byte SOUND_GRENADE = 12;
        public const byte SOUND_SMOKEGRENADE = 13;
        public const byte SOUND_SWORD = 14;

        public const byte SONG_STORMFRONT = 0;
        public const byte SONG_RAW = 1;
        public const byte SONG_PULSEROCK = 2;
        public static Dictionary<byte, SoundEffect> sounds;
        public static Dictionary<byte, Song> songs;

        public static float MusicVolume = 1;
        public static float SoundVolume = 1;
        private static AudioListener audioListener;
        private static Playlist CurrentPlaylist;

        static Audio()
        {
            sounds = new Dictionary<byte, SoundEffect>();
            audioListener = new AudioListener();
            songs = new Dictionary<byte, Song>();
            MediaPlayer.MediaStateChanged += new EventHandler<EventArgs>(MediaPlayer_MediaStateChanged);
        }

        public static void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.GameHasControl && CurrentPlaylist != null)
            {
                if (MediaPlayer.State == MediaState.Paused || MediaPlayer.State == MediaState.Stopped )
                {
                    CurrentPlaylist.CurrentSongIndex++;
                    if (CurrentPlaylist.CurrentSongIndex >= CurrentPlaylist.Songs.Length)
                        CurrentPlaylist.CurrentSongIndex = 0;
                    MediaPlayer.Play(CurrentPlaylist.CurrentSong);
                }
            }
        }

        /// <summary>
        /// Stops playing music
        /// </summary>
        /// <param name="list"></param>
        public static void SetPlaylist(Playlist list)
        {
            CurrentPlaylist = list;
            Stop();
        }

        public static void PlaySound(byte sound)
        {
            sounds[sound].Play(.9f, 0, 0);
        }

        public static void PlaySound(byte sound, float volume)
        {
            if (volume > 1)
                volume = 1;
            else if (volume < 0)
                volume = 0;

            if (volume * SoundVolume * .9f > .01f)
                sounds[sound].Play(volume * .9f, 0, 0);
        }

        public static void SetMusicVolume(float volume)
        {
            if (volume <= 0f)
                volume = 0;
            else if (volume > 1)
                volume = 1;
            MusicVolume = volume;
            if (MediaPlayer.GameHasControl)
            {
                MediaPlayer.Volume = volume;
                if (MusicVolume + fadeAmount >= 1)
                    MediaPlayer.Volume = 1;
                else if (MusicVolume + fadeAmount <= 0)
                    MediaPlayer.Volume = 0;
                else
                    MediaPlayer.Volume = MusicVolume + fadeAmount;
            }
        }

        public static void SetSoundVolume(float volume)
        {
            if (volume <= 0f)
                volume = 0;
            else if (volume > 1)
                volume = 1;
            SoundVolume = volume;
            SoundEffect.MasterVolume = volume;
        }

        public static void PlaySong()
        {
            CurrentPlaylist.CurrentSongIndex = 0;
            if (MediaPlayer.GameHasControl)
            {
                MediaPlayer.Play(CurrentPlaylist.CurrentSong);
                MediaPlayer.IsRepeating = false;
            }
        }


        /// <summary>
        /// Plays all the songs in the playlist randomly
        /// </summary>
        public static void PlayAllRandom()
        {
            Dictionary<int, byte> vals = new Dictionary<int, byte>();
            List<int> keys = new List<int>();
            Random ran = new Random();
            for (int i = 0; i < CurrentPlaylist.Songs.Length; i++)
            {
                keys.Add(ran.Next());
                vals.Add(keys[i], CurrentPlaylist.Songs[i]);
            }
            List<int> closed = new List<int>();

            while (keys.Count > 0)
            {
                int indexHigh = 0;
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i] > keys[indexHigh])
                    {
                        indexHigh = i;
                    }
                }
                closed.Add(keys[indexHigh]);
                keys.RemoveAt(indexHigh);
            }

            byte[] songsToPlay = new byte[CurrentPlaylist.Songs.Length];
            for (int i = 0; i < CurrentPlaylist.Songs.Length; i++)
            {
                songsToPlay[i] = vals[closed[i]];
            }

            if (MediaPlayer.GameHasControl)
            {
                CurrentPlaylist.Songs = songsToPlay;
                CurrentPlaylist.CurrentSongIndex = 0;
                MediaPlayer.Play(CurrentPlaylist.CurrentSong);
                MediaPlayer.IsRepeating = false;
            }
        }

        public static void Stop()
        {
            if (MediaPlayer.GameHasControl)
            {
                MediaPlayer.Stop();
            }
        }

        public static void ResetFade()
        {
            fadeAmount = 1;
            fade = Fading.None;
        }

        private const float FadeOutDuration = .5f; //half a second
        private static float fadeAmount = 1;
        public enum Fading { In, Out, None }
        private static Fading fade;
        private static void FadeOut(float amount)
        {
            fadeAmount -= amount;
            if (MediaPlayer.GameHasControl)
            {
                if (MusicVolume + fadeAmount <= .06)
                    MediaPlayer.Volume = 0;
                else
                    MediaPlayer.Volume = MusicVolume + fadeAmount;
            }

            if (fadeAmount <= -1)
            {
                fade = Fading.None;
                if (fadeChange != null)
                {
                    fadeChange.Invoke();
                }
                fadeChange = null;
            }
        }

        private static void FadeIn(float amount)
        {
            fadeAmount += amount;
            if (MediaPlayer.GameHasControl)
            {
                if (MusicVolume + fadeAmount >= 1)
                    MediaPlayer.Volume = 1;
                else
                    MediaPlayer.Volume = MusicVolume + fadeAmount;
            }

            if (fadeAmount >= 0)
            {
                fade = Fading.None;
                if (fadeChange != null)
                {
                    fadeChange.Invoke();
                }
                fadeChange = null;
            }

        }

        public static void SetFading(Fading fade)
        {
            Audio.fade = fade;
            if (fade == Fading.In)
                fadeAmount = -1f;
            else
                fadeAmount = 0;
        }

        public static void SetFading(Fading fade, FadeChange fc)
        {
            Audio.fade = fade;
            if (fade == Fading.In)
                fadeAmount = -1f;
            else
                fadeAmount = 0;
            fadeChange = fc;
        }

        public delegate void FadeChange();
        private static FadeChange fadeChange;

        public static void Update(GameTime gameTime)
        {
            switch (fade)
            {
                case Fading.In:
                    FadeIn((float)(gameTime.ElapsedGameTime.TotalSeconds * FadeOutDuration));
                    break;
                case Fading.Out:
                    FadeOut((float)(gameTime.ElapsedGameTime.TotalSeconds * FadeOutDuration));
                    break;
            }
        }
    }
}