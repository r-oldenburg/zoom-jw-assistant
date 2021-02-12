// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ZoomJWAssistant.Core;
using NHotkey;
using NHotkey.Wpf;
using System.Windows.Data;
using ZoomJWAssistant.Models;

namespace ZoomJWAssistant
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public ZoomService ZoomService { get; } = ZoomService.Instance;

        private string attendeeSearchText = "";

        public string AttendeeSearchText
        {
            get => this.attendeeSearchText;
            set {
                this.Set(ref this.attendeeSearchText, value);
                CollectionViewSource.GetDefaultView(ZoomService.Attendees)?.Refresh();
            }
        }

        public string Version { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public MainWindowViewModel()
        {
            this.CultureInfos = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).OrderBy(c => c.DisplayName).ToList();

            CollectionViewSource.GetDefaultView(ZoomService.Attendees).Filter = new Predicate<object>(FilterAttendees);
        }

        private bool FilterAttendees(object obj)
        {
            return string.IsNullOrWhiteSpace(attendeeSearchText) || obj is MeetingAttendee && (obj as MeetingAttendee).CurrentTechnicalName.ToLower().Contains(attendeeSearchText.ToLower());
        }

        public void Dispose()
        {
            HotkeyManager.Current.Remove("demo");
        }

        public string Title { get; set; }

        public int SelectedIndex { get; set; }

        public List<CultureInfo> CultureInfos { get; set; }

        private CultureInfo currentCulture = CultureInfo.CurrentCulture;

        public CultureInfo CurrentCulture
        {
            get => this.currentCulture;
            set => this.Set(ref this.currentCulture, value);
        }

        private double? numericUpDownValue = null;

        public double? NumericUpDownValue
        {
            get => this.numericUpDownValue;
            set => this.Set(ref this.numericUpDownValue, value);
        }

        public ICommand CloseCmd { get; }

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(HotKey) && this.HotKey != null && this.HotKey.Key == Key.D && this.HotKey.ModifierKeys == ModifierKeys.Shift)
                {
                    return "SHIFT-D is not allowed";
                }

                return null;
            }
        }

        public ICommand ShowCustomDialogCommand { get; }

        private HotKey _hotKey = new HotKey(Key.Home, ModifierKeys.Control | ModifierKeys.Shift);

        public HotKey HotKey
        {
            get => this._hotKey;
            set
            {
                if (this.Set(ref this._hotKey, value))
                {
                    if (value != null && value.Key != Key.None)
                    {
                        HotkeyManager.Current.AddOrReplace("demo", value.Key, value.ModifierKeys, async (sender, e) => await this.OnHotKey(sender, e));
                    }
                    else
                    {
                        HotkeyManager.Current.Remove("demo");
                    }
                }
            }
        }

        private async Task OnHotKey(object sender, HotkeyEventArgs e)
        {
            await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(
                "Hotkey pressed",
                "You pressed the hotkey '" + this.HotKey + "' registered with the name '" + e.Name + "'");
        }
    }
}