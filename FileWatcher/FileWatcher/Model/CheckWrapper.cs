using Kley.Base.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Model
{
    public class CheckWrapper<T> : BaseNotifyPropertyChanged
    {
        #region Constructor

        private readonly CheckableObservableCollection<T> _parent;

        public CheckWrapper(CheckableObservableCollection<T> parent)
        {
            _parent = parent;
        }

        #endregion

        #region Properties

        private T _value;

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyPropertyChanged("Value");
            }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                CheckChanged();
                NotifyPropertyChanged("IsChecked");
            }
        }

        #endregion

        #region Handler

        private void CheckChanged()
        {
            _parent.Refresh();
        }

        #endregion

    }
}
