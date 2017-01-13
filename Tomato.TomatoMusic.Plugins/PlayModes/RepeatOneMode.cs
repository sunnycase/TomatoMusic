﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Plugins.PlayModes
{
    class RepeatOneMode : IPlayModeProvider
    {
        public string DisplayName => "单曲模式";

        private static readonly Guid _id = new Guid("8513750B-6423-4B68-8DA1-98538EFE2493");
        public Guid Id => _id;

        public Symbol Icon => Symbol.RepeatOne;

        private MediaPlaybackList _playbackList;

        public void Attach(MediaPlaybackList playbackList)
        {
            Detach();
            SetShuffledItems(playbackList, playbackList.CurrentItem);
            playbackList.AutoRepeatEnabled = true;
            playbackList.ShuffleEnabled = true;
        }

        public void Detach()
        {
            if (_playbackList != null)
            {
                _playbackList.CurrentItemChanged -= playbackList_CurrentItemChanged;
                _playbackList = null;
            }
        }

        private void playbackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            SetShuffledItems(sender, args.NewItem);
        }

        private static void SetShuffledItems(MediaPlaybackList playbackList, MediaPlaybackItem newItem = null)
        {
            if (playbackList.Items.Any())
                playbackList.SetShuffledItems(Enumerable.Repeat(newItem ?? playbackList.Items.First(), playbackList.Items.Count));
        }
    }
}
