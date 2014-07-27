using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Implbits;
using ReactiveUI;

namespace DisabledCommandToolTips
{
    public class ViewModel : ReactiveObject
    {
        #region Command: ReactiveCommand
        private IXCommand _command;
        public IXCommand ReactiveCommand
        {
            get
            {
                if (_command != null) return _command;
                var canExecute = this.WhenAny(x => x.Text, x => TextIsOkay(x.GetValue()));
                ReactiveCommand<object> reactiveCommand = ReactiveUI.ReactiveCommand.Create(canExecute);
                reactiveCommand.Subscribe(x => MessageBox.Show(string.Format("ReactiveCommand executed: {0}", x)));
                _command = reactiveCommand.WithDisabledTooltip(DisabledReason);
                return _command;
            }
        }
        #endregion

        #region Command: MyCommand
        private readonly IXCommand _myCommand = new MyCommand().WithDisabledTooltip("The text is not valid.");
        public IXCommand MyCommand
        {
            get { return _myCommand; }
        }
        #endregion

        string DisabledReason(object parameter)
        {
            var text = parameter as string;
            if (string.IsNullOrWhiteSpace(text)) return "No value specified.";
            if (text == "blah") return "Text is too boring";
            if (text == "hi") return "Text is too friendly";
            return null;
        }

        bool TextIsOkay(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text != "blah" && text != "hi";
        }

        #region Property Text
        private string _text = default(string);
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }
        #endregion

    }

    class MyCommand : ICommand
    {

        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(parameter as string);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            MessageBox.Show(string.Format("MyCommand executed: {0}", parameter));
        }
    }
}
