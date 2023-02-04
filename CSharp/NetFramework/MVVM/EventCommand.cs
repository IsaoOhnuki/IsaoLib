using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace MVVM
{
    public class EventCommand
    {
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.RegisterAttached(
                "Action",
                typeof(EventAction),
                typeof(EventCommand),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        if (s is FrameworkElement element)
                        {
                            if (e.OldValue is EventAction oldAction)
                            {
                                oldAction.Detache(element);
                            }
                            if (e.NewValue is EventAction newAction)
                            {
                                newAction.Attache(element);
                            }
                        }
                    }));

        public static void SetAction(DependencyObject obj, EventAction value)
            => obj.SetValue(ActionProperty, value);

        public static EventAction GetAction(DependencyObject obj)
            => (EventAction)obj.GetValue(ActionProperty);

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.RegisterAttached(
                "Actions",
                typeof(EventActionCollection),
                typeof(EventCommand),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        if (s is FrameworkElement element)
                        {
                            if (e.OldValue is EventActionCollection oldActions)
                            {
                                oldActions.Detache(element);
                            }
                            if (e.NewValue is EventActionCollection newActions)
                            {
                                newActions.Attache(element);
                            }
                        }
                    }));

        public static void SetActions(DependencyObject obj, EventActionCollection value)
            => obj.SetValue(ActionsProperty, value);

        public static EventActionCollection GetActions(DependencyObject obj)
            => (EventActionCollection)obj.GetValue(ActionsProperty);
    }

    public sealed class EventActionCollection : FreezableCollection<EventAction>
    {
        public void Attache(FrameworkElement element)
        {
            this.ToList().ForEach(x => x.Attache(element));
        }

        public void Detache(FrameworkElement element)
        {
            this.ToList().ForEach(x => x.Detache(element));
        }
    }

    public static class VisualTreeHelperWrapper
    {
        public static FrameworkElement FindChild(this FrameworkElement depObj, Type type)
        {
            if (depObj == null)
            {
                return null;
            }

            FrameworkElement result = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                result = VisualTreeHelper.GetChild(depObj, i) as FrameworkElement;
                if (result != null && result.GetType() != type)
                {
                    result = FindChild(result, type);
                }
                if (result != null)
                {
                    break;
                }
            }
            return result;
        }

        public static FrameworkElement FindChild(this FrameworkElement depObj, string name)
        {
            if (depObj == null)
            {
                return null;
            }

            FrameworkElement result = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                result = VisualTreeHelper.GetChild(depObj, i) as FrameworkElement;
                if (result != null && result.Name != name)
                {
                    result = FindChild(result, name);
                }
                if (result != null)
                {
                    break;
                }
            }
            return result;
        }
    }

    public class EventActionExtension : MarkupExtension
    {
        public string EventName { get; set; }

        public Binding Command { get; set; }

        public Binding CommandParameter { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            EventAction eventAction = new EventAction
            {
                EventName = EventName,
            };
            if (Command != null)
            {
                BindingOperations.SetBinding(eventAction, EventAction.CommandProperty, Command);
            }
            if (CommandParameter != null)
            {
                BindingOperations.SetBinding(eventAction, EventAction.CommandParameterProperty, CommandParameter);
            }
            return eventAction;
        }
    }

    public class EventAction : Freezable
    {
        public string EventName
        {
            get => (string)GetValue(EventNameProperty);
            set => SetValue(EventNameProperty, value);
        }
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register(
                nameof(EventName),
                typeof(string),
                typeof(EventAction),
                new PropertyMetadata(default));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(EventAction),
                new PropertyMetadata(default));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(EventAction),
                new PropertyMetadata(default));

        public Delegate EventHandlerDelegate { get; set; }

        public void EventHandler(object sender, EventArgs e)
        {
            object[] args = new object[] { sender, e, CommandParameter };
            if (Command?.CanExecute(args) ?? false)
            {
                Command?.Execute(args);
            }
        }

        public void Attache(FrameworkElement element)
        {
            if (!string.IsNullOrEmpty(EventName) &&
                element.GetType().GetEvent(EventName) is EventInfo eInfo)
            {
                if (EventHandlerDelegate == null)
                {
                    Delegate eventMethod = Delegate.CreateDelegate(
                        eInfo.EventHandlerType,
                        this,
                        typeof(EventAction).GetMethod(nameof(EventAction.EventHandler)));
                    EventHandlerDelegate = eventMethod;
                }
                eInfo.AddEventHandler(element, EventHandlerDelegate);
            }
        }

        public void Detache(FrameworkElement element)
        {
            if (!string.IsNullOrEmpty(EventName) &&
                element.GetType().GetEvent(EventName) is EventInfo eInfo)
            {
                if (EventHandlerDelegate != null)
                {
                    eInfo.RemoveEventHandler(element, EventHandlerDelegate);
                    EventHandlerDelegate = null;
                }
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new EventAction();
        }
    }
}