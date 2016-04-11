// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: DatesCollection.cs
// Version: 20160411

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoCheckIn
{
    [Serializable]
    public class DatesCollection : ObservableCollection<DateTime>
    {
        public new void Add(DateTime date)
        {
            date = date.Date;

            if (!Contains(date))
            {
                base.Add(date);
            }
        }

        public new void Remove(DateTime date)
        {
            date = date.Date;

            base.Remove(date);
        }
    }

    [Serializable]
    public class UserSignLog : Dictionary<int, DatesCollection>
    {
    }
}