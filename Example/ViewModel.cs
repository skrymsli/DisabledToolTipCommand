using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
        private ICommand _command;
        public ICommand ReactiveCommand
        {
            get
            {
                if (_command != null) return _command;
                var obsText = this.WhenAny(x => x.Text, x => TextIsOkay(x.GetValue()));
                var obsDisableAll = this.WhenAnyValue(x => x.DisableAll);
                var obsDisableDynamic = this.WhenAnyValue(x => x.DisableDynamic);
                var canExecute = obsText.CombineLatest(obsDisableAll, obsDisableDynamic, (x, y, z) => x && !y && !z);

                var reactiveCommand = ReactiveUI.ReactiveCommand.Create(canExecute);
                reactiveCommand.Subscribe(x => MessageBox.Show(string.Format("ReactiveCommand executed: {0}", x)));
                _command = reactiveCommand.WithDisabledTooltip(DisabledReason);
                return _command;
            }
        }
        #endregion

        #region Command: SomeCommand

        private ICommand _someCommand;
        public ICommand SomeCommand
        {
            get
            {
                if(_someCommand != null) return _someCommand;
                return (_someCommand = new SomeCommand(this).WithDisabledTooltip("Sorry, can't execute right now."));
            }
        }
        #endregion

        #region Property Text
        private string _text = default(string);
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }
        #endregion

        #region Property DisableDynamic
        private bool _disableDynamic = default(bool);
        public bool DisableDynamic
        {
            get { return _disableDynamic; }
            set { this.RaiseAndSetIfChanged(ref _disableDynamic, value); }
        }
        #endregion

        #region Property DisableAll
        private bool _disableAll = default(bool);
        public bool DisableAll
        {
            get { return _disableAll; }
            set { this.RaiseAndSetIfChanged(ref _disableAll, value); }
        }
        #endregion

        private string DisabledReason(object parameter)
        {
            if (DisableAll) return "All commands are disabled right now.";
            if (DisableDynamic) return "This is a dynamic disabled tooltip command and those are disabled right now.";
            if (!TextIsOkay(parameter as string)) return "You need to enter some text.";
            return null;
        }

        private static bool TextIsOkay(string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }
    }

    
}
