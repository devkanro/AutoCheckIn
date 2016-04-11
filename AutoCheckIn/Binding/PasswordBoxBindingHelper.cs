// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: PasswordBoxBindingHelper.cs
// Version: 20160411

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace AutoCheckIn.Binding
{
    public static class PasswordBoxBindingHelper
    {
        public static readonly DependencyProperty IsPasswordBindingEnabledProperty =
            DependencyProperty.RegisterAttached("IsPasswordBindingEnabled", typeof (bool),
                typeof (PasswordBoxBindingHelper),
                new UIPropertyMetadata(false, OnIsPasswordBindingEnabledChanged));

        private static bool _inputFlag;

        public static readonly DependencyProperty BindedPasswordProperty =
            DependencyProperty.RegisterAttached("BindedPassword", typeof (string),
                typeof (PasswordBoxBindingHelper),
                new UIPropertyMetadata(string.Empty, OnBindedPasswordChanged));

        public static bool GetIsPasswordBindingEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsPasswordBindingEnabledProperty);
        }

        public static void SetIsPasswordBindingEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPasswordBindingEnabledProperty, value);
        }

        private static void OnIsPasswordBindingEnabledChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = obj as PasswordBox;

            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= PasswordBoxPasswordChanged;

                if ((bool) e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBoxPasswordChanged;
                }
            }
        }

        //when the passwordBox's password changed, update the buffer
        private static void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox) sender;

            if (!String.Equals(GetBindedPassword(passwordBox), passwordBox.Password))
            {
                _inputFlag = true;
                SetBindedPassword(passwordBox, passwordBox.Password);
            }
        }

        public static string GetBindedPassword(DependencyObject obj)
        {
            return (string) obj.GetValue(BindedPasswordProperty);
        }

        public static void SetBindedPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BindedPasswordProperty, value);
        }

        //when the buffer changed, upate the passwordBox's password
        private static void OnBindedPasswordChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = obj as PasswordBox;
            if (passwordBox != null)
            {
                if (_inputFlag)
                {
                    _inputFlag = false;
                    return;
                }
                passwordBox.Password = e.NewValue == null ? string.Empty : e.NewValue.ToString();
                var select = passwordBox.GetType().GetMethod("Select",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                select.Invoke(passwordBox, new object[] {passwordBox.Password.Length, 0});
            }
        }
    }
}