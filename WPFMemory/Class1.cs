using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMemory
{
    public class ImagesListViewItem : System.ComponentModel.INotifyPropertyChanged
    {
        public string Source { get; set; }
        public string FileName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Header { get; set; }
        public string Path { get; set; }

        bool _amIExpanded = false;
        public bool AmIExpanded {
            get { return _amIExpanded; }
            set { _amIExpanded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AmIExpanded))); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
