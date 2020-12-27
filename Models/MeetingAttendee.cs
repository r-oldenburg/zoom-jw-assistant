// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ZoomJWAssistant.Core;

namespace ZoomJWAssistant.Models
{
    public class MeetingAttendee : ViewModelBase
    {
        private int _userId;
        private string _name;
        private int _numberOfPersons = 1;
        private List<string> _previousNames;

        public int UserId
        {
            get => this._userId;
            set => this.Set(ref this._userId, value);
        }

        public string Name
        {
            get => this._name;
            set => this.Set(ref this._name, value);
        }

        public int NumberOfPersons
        {
            get => this._numberOfPersons;
            set => this.Set(ref this._numberOfPersons, value);
        }

        public List<string> PreviousNames
        {
            get => this._previousNames;
            set => this.Set(ref this._previousNames, value);
        }
    }
}