using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;
using System.Collections.ObjectModel;
using ReactiveUI;
using Windows.UI.Xaml.Data;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    class MusicsTrackViewModel : BindableBase, IComparable<MusicsTrackViewModel>
    {
        public TrackInfo Track { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public event EventHandler PlayRequested;

        public void Play()
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        public int CompareTo(MusicsTrackViewModel other)
        {
            return Track.CompareTo(other.Track);
        }
    }

    class MusicsViewModel : MusicsPresenterViewModelBase<MusicsTrackViewModel>, IMusicsPresenterViewModel
    {
        private IReadOnlyReactiveCollection<MusicsTrackViewModel> _tracks;

        public MusicsOrderRule[] OrderRules { get; } = new MusicsOrderRule[]
        {
            MusicsOrderRule.AddTime,
            MusicsOrderRule.Title,
            MusicsOrderRule.Album
        };

        private MusicsOrderRule _selectedOrderRule;
        public MusicsOrderRule SelectedOrderRule
        {
            get { return _selectedOrderRule; }
            set
            {
                if (this.SetProperty(ref _selectedOrderRule, value))
                    UpdateTracksCollection();
            }
        }

        public MusicsViewModel(PlaylistPlaceholder playlist)
            : base(playlist)
        {
            PlaySession.PropertyChanged += _playSession_PropertyChanged;
        }

        protected override void OnViewLoaded(object view)
        {
            LoadData();
        }

        private void LoadData()
        {
            UpdateTracksCollection();
        }

        private void UpdateTracksCollection()
        {
            Func<MusicsTrackViewModel, MusicsTrackViewModel, int> order = null;
            switch (SelectedOrderRule)
            {
                case MusicsOrderRule.AddTime:
                    order = (x, y) => Comparer<DateTime>.Default.Compare(x.Track.AddTime, y.Track.AddTime);
                    break;
                case MusicsOrderRule.Title:
                    order = (x, y) => Comparer<string>.Default.Compare(x.Track.Title, y.Track.Title);
                    break;
                case MusicsOrderRule.Album:
                    order = (x, y) => Comparer<string>.Default.Compare(x.Track.Album, y.Track.Album);
                    break;
            }
            _tracks = PlaylistContentProvider.Tracks.CreateDerivedCollection(WrapTrackInfo, orderer: order);

            switch (SelectedOrderRule)
            {
                case MusicsOrderRule.AddTime:
                    ItemsSource = _tracks;
                    IsItemsSourceGrouped = false;
                    break;
                case MusicsOrderRule.Title:
                    IsItemsSourceGrouped = true;
                    ItemsSource = new ObservableGroupingCollection<string, MusicsTrackViewModel>((IReadOnlyList<MusicsTrackViewModel>)_tracks, o => o.Track.Title,
                        Comparer<string>.Default, Comparer<MusicsTrackViewModel>.Default);
                    break;
                case MusicsOrderRule.Album:
                    IsItemsSourceGrouped = true;
                    ItemsSource = new ObservableGroupingCollection<string, MusicsTrackViewModel>((IReadOnlyList<MusicsTrackViewModel>)_tracks, o => o.Track.Album,
                        Comparer<string>.Default, Comparer<MusicsTrackViewModel>.Default);
                    break;
                default:
                    break;
            }

        }

        private void _playSession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaySessionService.CurrentTrack):
                    //Execute.BeginOnUIThread(OnPlaySessionCurrentTrackChanged);
                    break;
                default:
                    break;
            }
        }

        private MusicsTrackViewModel WrapTrackInfo(TrackInfo track)
        {
            var trackViewModel = new MusicsTrackViewModel
            {
                Track = track
            };
            trackViewModel.PlayRequested += TrackViewModel_PlayRequested;
            return trackViewModel;
        }

        private void TrackViewModel_PlayRequested(object sender, EventArgs e)
        {
            var trackVM = (MusicsTrackViewModel)sender;
            PlaySession.SetPlaylist(PlaylistContentProvider.Tracks.ToList(), trackVM.Track);
            PlaySession.PlayWhenOpened();
        }
    }
}
