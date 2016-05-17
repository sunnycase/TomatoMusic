using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Tomato.TomatoMusic
{
    public static class DefaultPickers
    {
        private static readonly Lazy<FolderPicker> _folderPicker = new Lazy<FolderPicker>(() =>
        {
            var picker = new FolderPicker()
            {
                ViewMode = PickerViewMode.List
            };
            picker.FileTypeFilter.Add(".");
            return picker;
        });

        public static FolderPicker FolderPicker => _folderPicker.Value;
    }
}
