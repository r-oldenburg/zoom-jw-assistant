// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZoomJWAssistant.Views
{
    /// <summary>
    /// Interaction logic for CustomDialogContent.xaml
    /// </summary>
    public partial class MeetingInfoDialog : UserControl
    {
        public MeetingInfoDialog()
        {
            InitializeComponent();
        }

        private void Textbox_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(((TextBox)sender));
        }
    }
}