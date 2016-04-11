// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: FloatingPopup.cs
// Version: 20160411

using AutoCheckIn.Hook;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace AutoCheckIn
{
    public class FloatingPopup : Popup
    {
        public static RoutedCommand CloseCommand = new RoutedCommand();
        private GobalMouseWaitHelper _mouseWait = new GobalMouseWaitHelper();
        private Object _result;

        private AutoResetEvent _wait = new AutoResetEvent(false);

        public FloatingPopup()
        {
            CommandBindings.Add(new CommandBinding(CloseCommand, ClosePopupHandler));

            AllowsTransparency = true;
            StaysOpen = true;
            PopupAnimation = PopupAnimation.Fade;
            Placement = PlacementMode.MousePoint;
        }

        public FloatingPopup(UIElement child) : this()
        {
            Child = child;
        }

        private void ClosePopupHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (executedRoutedEventArgs.Handled) return;

            Close(executedRoutedEventArgs.Parameter);

            executedRoutedEventArgs.Handled = true;
        }

        private void Close(Object result)
        {
            _result = result;
            _mouseWait.Cancel();
        }

        protected override async void OnOpened(EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
            base.OnOpened(e);
            await _mouseWait.Wait(arg => !IsMouseOver);
            IsOpen = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            _wait.Set();
            base.OnClosed(e);
        }

        public async Task<Object> Show()
        {
            IsOpen = true;

            await Task.Run(() => { _wait.WaitOne(); });

            var result = _result;
            _result = null;

            return result;
        }
    }

    public class GobalMouseWaitHelper
    {
        private Func<MouseEventArgs, bool> _filter;

        private AutoResetEvent _waitHandle;
        
        public GobalMouseWaitHelper()
        {
            _waitHandle = new AutoResetEvent(false);
        }
        
        public Task Wait(Func<MouseEventArgs, bool> filter = null)
        {
            _filter = filter;
            HookManager.MouseClick += HookManagerOnMouseClick;

            return Task.Run(() => { _waitHandle.WaitOne(); });
        }

        public void Cancel()
        {
            _waitHandle.Set();
            _waitHandle.Reset();
        }

        private void HookManagerOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_filter == null || _filter(mouseEventArgs))
            {
                HookManager.MouseClick -= HookManagerOnMouseClick;
                Cancel();
            }
        }
    }
}