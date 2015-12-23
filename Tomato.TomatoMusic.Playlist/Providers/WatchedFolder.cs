using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Core;
using Windows.Storage;
using Windows.Storage.Search;

namespace Tomato.TomatoMusic.Playlist.Providers
{
    class WatchedFolder
    {
        private readonly StorageFolder _folder;
        private readonly StorageFileQueryResult _fileQueryResult;
        private readonly WatchedFolderDispatcher _dispatcher;

        public StorageFolder Folder => _folder;

        public WatchedFolder(WatchedFolderDispatcher dispatcher, StorageFolder folder)
        {
            _dispatcher = dispatcher;
            _folder = folder;
            _fileQueryResult = _folder.CreateFileQueryWithOptions(_queryOptions);
            _fileQueryResult.ContentsChanged += _fileQueryResult_ContentsChanged;
        }

        public void Refresh()
        {
            _dispatcher.RequestFileUpdate(this);
        }

        private void _fileQueryResult_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            Refresh();
        }

        public Task<IReadOnlyList<StorageFile>> GetFilesAsync()
        {
            return _fileQueryResult.GetFilesAsync().AsTask();
        }

        private static readonly QueryOptions _queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, AudioConstants.SupportedFileExtensions)
        {
            IndexerOption = IndexerOption.UseIndexerWhenAvailable,
            FolderDepth = FolderDepth.Deep
        };
    }
}
