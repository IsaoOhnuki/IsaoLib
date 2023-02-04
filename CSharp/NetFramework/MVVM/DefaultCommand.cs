using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MVVM
{
    public class DefaultCommand : ICommand
    {
        private Func<object, bool> IsCanExecute { get; set; }
        private Action<object> OnExecuted { get; set; }

        public DefaultCommand() { }

        public DefaultCommand(Action<object> onExecuted, Func<object, bool> isCanExecute = null)
        {
            OnExecuted = onExecuted;
            IsCanExecute = isCanExecute ?? new Func<object, bool>(v => true);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return IsCanExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            OnExecuted.Invoke(parameter);
        }

        public void Update()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
