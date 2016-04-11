// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: CalendarDatesBindingHelper.cs
// Version: 20160411

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace AutoCheckIn.Binding
{
    public static class CalendarDatesBindingHelper
    {
        public static readonly DependencyProperty BindedDatesProperty = DependencyProperty.RegisterAttached(
            "BindedDates", typeof (DatesCollection), typeof (CalendarDatesBindingHelper),
            new PropertyMetadata(default(DatesCollection), OnBindedDatesChanged));

        public static readonly DependencyProperty BindContextProperty = DependencyProperty.RegisterAttached(
            "BindContext", typeof (CalendarDatesBindContext), typeof (CalendarDatesBindingHelper),
            new PropertyMetadata(default(CalendarDatesBindContext)));

        public static void SetBindContext(DependencyObject element, CalendarDatesBindContext value)
        {
            element.SetValue(BindContextProperty, value);
        }

        public static CalendarDatesBindContext GetBindContext(DependencyObject element)
        {
            return (CalendarDatesBindContext) element.GetValue(BindContextProperty);
        }

        private static void OnBindedDatesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var calendar = obj as Calendar;
            var context = GetBindContext(calendar);

            if (context == null)
            {
                SetBindContext(calendar, context = new CalendarDatesBindContext(calendar, e.NewValue as DatesCollection));
            }
            else
            {
                context.Collection = e.NewValue as DatesCollection;
            }
        }

        public static void SetBindedDates(DependencyObject element, DatesCollection value)
        {
            element.SetValue(BindedDatesProperty, value);
        }

        public static DatesCollection GetBindedDates(DependencyObject element)
        {
            return (DatesCollection) element.GetValue(BindedDatesProperty);
        }
    }

    public class CalendarDatesBindContext
    {
        private DatesCollection _collection;

        public CalendarDatesBindContext(Calendar calendar, DatesCollection collection)
        {
            Calendar = calendar;
            Collection = collection;
            Sync();
        }

        public Calendar Calendar { get; private set; }

        public DatesCollection Collection
        {
            get { return _collection; }
            set
            {
                if (_collection != null)
                {
                    _collection.CollectionChanged -= CollectionOnCollectionChanged;
                }

                if (value != null)
                {
                    value.CollectionChanged += CollectionOnCollectionChanged;
                }
                _collection = value;
                Sync();
            }
        }

        private void Sync()
        {
            Calendar.SelectedDates.Clear();

            if (Collection != null)
            {
                foreach (DateTime dateTime in Collection)
                {
                    Calendar.SelectedDates.Add(dateTime);
                }
            }
        }

        private void CollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (Calendar != null)
                    {
                        foreach (DateTime item in e.NewItems)
                        {
                            Calendar.SelectedDates.Add(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (Calendar != null)
                    {
                        foreach (DateTime item in e.OldItems)
                        {
                            Calendar.SelectedDates.Remove(item);
                        }
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported collection action: {e.Action}.");
            }
        }
    }
}