using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using ReactiveUI;

namespace Implbits
{
    public class DisabledCommandTooltip : DependencyObject
    {
        #region Attached Property: Enable
        public static readonly DependencyProperty Enable = DependencyProperty.RegisterAttached(
            "Enable",
            typeof(Boolean),
            typeof(DisabledCommandTooltip), new PropertyMetadata(false, EnableDynamicChanged)
            );
        public static void SetEnable(DependencyObject element, Boolean value)
        {
            element.SetValue(Enable, value);            
        }
        public static Boolean GetEnable(DependencyObject element)
        {
            return (Boolean)element.GetValue(Enable);
        }

        public static void EnableDynamicChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool)
            {
                if ((bool)args.NewValue) WatchForCommandChange(obj as FrameworkElement);
            }
        }
        #endregion

        #region Private Attached Property: DefaultToolTip
        private static readonly DependencyProperty DefaultToolTipProperty = DependencyProperty.RegisterAttached(
            "DefaultToolTip",
            typeof(object),
            typeof(DisabledCommandTooltip)
            );
        private static void SetDefaultToolTip(FrameworkElement element, object value)
        {
            element.SetValue(DefaultToolTipProperty, value);
        }
        private static object GetDefaultToolTip(FrameworkElement element)
        {
            return element.GetValue(DefaultToolTipProperty);
        }
        #endregion

        static readonly Dictionary<FrameworkElement, IDisposable> WatchRegistrations = new Dictionary<FrameworkElement, IDisposable>(); 
        static void WatchForCommandChange(FrameworkElement element)
        {
            var cs = element as ICommandSource;
            if (cs == null) throw new ArgumentException("DisabledCommandTooltip set on non-command source element: " + element);
            if (cs.Command != null)
            {
                ConfigureDisabledTooltip(element);
                return;
            }
            var subscription = cs.WhenAnyValue(x => x.Command).Subscribe(x =>
            {
                if (x == null) return;
                if (WatchRegistrations.ContainsKey(element))
                {
                    var disp = WatchRegistrations[element];
                    if (disp != null)
                    {
                        disp.Dispose();
                    }
                    WatchRegistrations.Remove(element);
                }
                ConfigureDisabledTooltip(element);
            });
            WatchRegistrations.Add(element, subscription);
        }

        static void ConfigureDisabledTooltip(FrameworkElement element)
        {
            var cs = element as ICommandSource;
            if(cs == null) 
                throw new ArgumentException("DisabledCommandTooltip set on non-CommandSource element: " + element);

            var ixcommand = cs.Command as IDisabledReasonCommand;
            if (ixcommand == null)
            {
                throw new ArgumentException("DisabledCommandTooltip.EnableDynamic is true, but command is not derived from IDisabledReasonCommand.");    
            }

            ToolTipService.SetShowOnDisabled(element, true);
            
            SetDefaultToolTip(element, element.ToolTip ?? ToolTipService.GetToolTip(element));

            ixcommand.CanExecuteChanged += (s, e) => UpdateTooltip(element,ixcommand.Reason);

            element.ToolTipOpening += (s, e) =>
            {
                ixcommand.UpdateReason(cs.CommandParameter);
                UpdateTooltip(element, ixcommand.Reason);
            };
        }

        static void UpdateTooltip(FrameworkElement element, string reason)
        {
            ToolTipService.SetToolTip(element, string.IsNullOrEmpty(reason) ? GetDefaultToolTip(element) : reason);
        }
    }
}
