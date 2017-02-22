using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kley.Base.Infrastructure
{
	public class RelayCommand : ICommand
	{
		private Func<object, bool> canExecute;
		private Action<object> execute;
		private bool canExecuteState;

		#region ICommand Members

		public virtual bool CanExecute(object parameter)
		{
			if (canExecute != null)
				SetCanExecute(canExecute(parameter));
			else
				SetCanExecute(true);
			return canExecuteState;
		}

		private void SetCanExecute(bool newValue)
		{
			if (canExecuteState != newValue)
			{
				canExecuteState = newValue;
				if (CanExecuteChanged != null)
					CanExecuteChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			if (execute != null)
				execute(parameter);
		}

		#endregion

		public RelayCommand(Func<object, bool> canExecute, Action<object> execute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public void InformCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}
	}
}
