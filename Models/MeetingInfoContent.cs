﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows.Input;
using ZoomJWAssistant.Core;

namespace ZoomJWAssistant.Models
{
    public class MeetingInfoContent : ViewModelBase
    {
        private string _meetingId;
        private string _userName;
        private bool _remember;

        public MeetingInfoContent(Action<MeetingInfoContent> closeHandler)
        {
            this.CloseCommand = new SimpleCommand(o => true, o => closeHandler(this));
        }

        public string MeetingId
        {
            get => this._meetingId;
            set
            {
                this._meetingId = value;
                this.OnPropertyChanged();
            }
        }

        public string UserName
        {
            get => this._userName;
            set
            {
                this._userName = value;
                this.OnPropertyChanged();
            }
        }

        public bool Remember
        {
            get => this._remember;
            set
            {
                this._remember = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }
    }
}