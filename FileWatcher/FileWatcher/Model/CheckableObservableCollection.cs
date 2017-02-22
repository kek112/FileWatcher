using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FileWatcher.Model
{
    public class CheckableObservableCollection<T> : ObservableCollection<CheckWrapper<T>>
    {
        #region Fields
        private ListCollectionView _selected;
        #endregion

        #region Constructor
        public CheckableObservableCollection()
        {
            _selected = new ListCollectionView(this);
            _selected.Filter = delegate(object checkObject)
            {
                return ((CheckWrapper<T>)checkObject).IsChecked;
            };
        }
        #endregion

        #region Handler 
        public void Add(T item)
        {
            this.Add(new CheckWrapper<T>(this) { Value = item });
        }

        public ICollectionView SelectedItems
        {
            get { return _selected; }
        }

        public void Refresh()
        {
            _selected.Refresh();
        }
        #endregion

    }
}
