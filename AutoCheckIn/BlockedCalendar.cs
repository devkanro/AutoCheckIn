// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: BlockedCalendar.cs
// Version: 20160411

using System.Windows.Controls;
using System.Windows.Input;

namespace AutoCheckIn
{
    public class BlockedCalendar : Calendar
    {
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (DisplayMode == CalendarMode.Month)
            {
                if (e.GetPosition(this).Y > 128)
                {
                    e.Handled = true;
                }
            }
            base.OnPreviewMouseDown(e);
        }
    }
}