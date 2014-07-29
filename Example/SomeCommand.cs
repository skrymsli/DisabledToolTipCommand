using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DisabledCommandToolTips
{

    // This is a quickie command implementation just to show that 
    // DisabledCommandTooltips work with any type of ICommand-derived
    // class.
    class SomeCommand : ICommand
    {
        private readonly ViewModel _vm;
        public SomeCommand(ViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return !_vm.DisableAll && !string.IsNullOrEmpty(parameter as string);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            MessageBox.Show(string.Format("SomeCommand executed: {0}", parameter));
        }
    }
}
