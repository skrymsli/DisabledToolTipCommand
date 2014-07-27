using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Implbits
{
    public interface IXCommand : ICommand
    {
        string Reason { get; }
    }

    public class XCommand : IXCommand, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected ICommand WrappedCommand { get; set; }
        private readonly Func<object, string> _reasonCallback;

        public XCommand(ICommand toWrap, Func<object, string> reasonCallback)
        {
            WrappedCommand = toWrap;
            _reasonCallback = reasonCallback;
            WrappedCommand.CanExecuteChanged += WrappedCommand_CanExecuteChanged;
        }

        void WrappedCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged(sender, e);
        }

        public XCommand(ICommand toWrap, string reason)
        {
            WrappedCommand = toWrap;
            _reasonCallback = x=>reason;
            WrappedCommand.CanExecuteChanged +=WrappedCommand_CanExecuteChanged;
        }

        private string _reason;
        public string Reason
        {
            get { return _reason; }
            protected set
            {
                if (_reason == value) return;
                _reason = value;
                FirePropertyChanged();
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public bool CanExecute(object parameter)
        {
            var canExecute = WrappedCommand.CanExecute(parameter);
            Reason = canExecute ? null : _reasonCallback(parameter);
            return canExecute;
        }

        public void Execute(object parameter)
        {
            WrappedCommand.Execute(parameter);
        }
        
        void FirePropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }


        
    }

    public static class CommandExtensions
    {
        public static IXCommand WithDisabledTooltip(this ICommand command, Func<object,string> reasonCallback)
        {
            return new XCommand(command, reasonCallback);
        }

        public static IXCommand WithDisabledTooltip(this ICommand command, string reason)
        {
            return new XCommand(command, reason);
        }
    }

}
