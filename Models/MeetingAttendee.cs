// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using ZOOM_SDK_DOTNET_WRAP;
using ZoomJWAssistant.Core;

namespace ZoomJWAssistant.Models
{
    public class MeetingAttendee : ViewModelBase
    {
        public static Regex NameNumberRegEx = new Regex(@"^\s*[^a-zA-Z0-9\s]*\s*(\d)\s*[^a-zA-Z0-9\s]*[\s-_/]*(.*[^0-9]+.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex NameNumberAtTheEndRegEx = new Regex(@"(.*?[^0-9]+.*?)\s*[^a-zA-Z0-9\s]*\s*(\d)\s*[^a-zA-Z0-9\s]*[\s-_/]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private uint _userId;
        private string _name;
        private string _currentTechnicalName;
        private int _numberOfPersons = 1;
        private bool _isPhone = false;
        private bool _isMuted = false;
        private bool _isWaiting = false;
        private bool _isHandRaised = false;
        private bool _isHost = false;
        private bool _isCoHost = false;

        private ObservableCollection<string> _previousNames = new ObservableCollection<string>();

        public MeetingAttendee() { }

        public MeetingAttendee(IUserInfoDotNetWrap user)
        {
            this.MergeUserData(user);
        }

        public uint UserId
        {
            get => this._userId;
            set => this.Set(ref this._userId, value);
        }

        public bool IsHost
        {
            get => this._isHost;
            set => this.Set(ref this._isHost, value);
        }

        public bool IsCoHost
        {
            get => this._isCoHost;
            set => this.Set(ref this._isCoHost, value);
        }

        public bool IsWaiting
        {
            get => this._isWaiting;
            set => this.Set(ref this._isWaiting, value);
        }

        public bool IsMuted
        {
            get => this._isMuted;
            set => this.Set(ref this._isMuted, value);
        }

        public bool IsHandRaised
        {
            get => this._isHandRaised;
            set => this.Set(ref this._isHandRaised, value);
        }

        public bool IsPhone
        {
            get => this._isPhone;
            set => this.Set(ref this._isPhone, value);
        }

        public string CurrentTechnicalName
        {
            get => this._currentTechnicalName;
            set {
                if (!EqualityComparer<string>.Default.Equals(_currentTechnicalName, value) && value != null)
                {
                    if (value != null && _currentTechnicalName != null && _currentTechnicalName.Trim().Length > 0)
                    {
                        this.PreviousNames.Insert(0, this._currentTechnicalName);
                        OnPropertyChanged(nameof(PreviousNamesJoined));
                        Console.WriteLine("Meeting-Name geändert auf '" + value + "' (initialer Name: '" + PreviousNames[PreviousNames.Count - 1] + "')");
                    }

                    this.Set(ref this._currentTechnicalName, value);
                    this.ProcessNewIncomingName(value);
                } else
                {
                    this.Set(ref this._currentTechnicalName, value);
                }
            }
        }

        public string Name
        {
            get => this._name;
            set
            {
                this.Set(ref this._name, value);
                this.GenerateNewTechnicalName();
            }
        }

        public int NumberOfPersons
        {
            get => this._numberOfPersons;
            set
            {
                this.Set(ref this._numberOfPersons, value);
                this.GenerateNewTechnicalName();
            }
        }

        public ObservableCollection<string> PreviousNames
        {
            get => this._previousNames;
            set {
                this.Set(ref this._previousNames, value);
                this.OnPropertyChanged(nameof(PreviousNamesJoined));
            }
        }

        public string PreviousNamesJoined
        {
            get => string.Join("\n", this._previousNames);
        }

        public StateEnum State
        {
            get {
                if (IsHost)
                {
                    if (IsMuted)
                    {
                        if (IsHandRaised)
                            return StateEnum.HostMutedHand;
                        else 
                            return StateEnum.HostMuted;
                    } 
                    else
                    {
                        if (IsHandRaised)
                            return StateEnum.HostLoudHand;
                        else
                            return StateEnum.HostLoud;
                    }
                } else {
                    if (IsMuted)
                    {
                        if (IsHandRaised)
                            return StateEnum.MutedHand;
                        else
                            return StateEnum.Muted;
                    }
                    else
                    {
                        if (IsHandRaised)
                            return StateEnum.LoudHand;
                        else
                            return StateEnum.Loud;
                    }
                }
            }
        }

        public void MergeUserData(IUserInfoDotNetWrap user)
        {
            this.IsHost = user.IsHost();
            this.IsPhone = user.IsPurePhoneUser();
            this.IsHandRaised = user.IsRaiseHand();
            this.IsMuted = user.IsAudioMuted();
            this.IsWaiting = user.IsInWaitingRoom();
            this.UserId = user.GetUserID();
            this.CurrentTechnicalName = user.GetUserNameW();
        }

        private void GenerateNewTechnicalName()
        {
            var newTechnicalName = Name;
            if (NumberOfPersons != 1 && !string.IsNullOrWhiteSpace(newTechnicalName))
            {
                newTechnicalName = FormatTechnicalName(Name, NumberOfPersons);
            }
            if (!EqualityComparer<string>.Default.Equals(_currentTechnicalName, newTechnicalName) && newTechnicalName != null) 
            {
                Console.Write("Analyse... ");
            }
            this.CurrentTechnicalName = newTechnicalName;
        }

        private void ProcessNewIncomingName(string newName)
        {
            _numberOfPersons = 1;
            _name = newName;

            var match = NameNumberRegEx.Match(newName);
            if (match.Success)
            {
                _numberOfPersons = int.Parse(match.Groups[1].Value);
                _name = match.Groups[2].Value;
            }
            //else
            //{
            //    match = NameNumberAtTheEndRegEx.Match(newName);
            //    if (match.Success)
            //    {
            //        _name = match.Groups[1].Value;
            //        _numberOfPersons = int.Parse(match.Groups[2].Value);
            //    }
            //}

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(NumberOfPersons));

            this.GenerateNewTechnicalName();
        }

        public static bool IsNameWithNumber(string name)
        {
            return NameNumberRegEx.IsMatch(name) || NameNumberAtTheEndRegEx.IsMatch(name);
        }

        private static string FormatTechnicalName(string name, int numberOfPersons)
        {
            var formatString = Properties.Settings.Default.NameFormat ?? "{name} - {number}";
            formatString = formatString.Replace("{name}", "{0}").Replace("{number}", "{1}");
            return string.Format(formatString, name, numberOfPersons);
        }

        public enum StateEnum
        {
            Loud            = 0,
            LoudHand        = 1,
            Muted           = 2,
            MutedHand       = 3,
            HostLoud        = 4,
            HostLoudHand    = 5,
            HostMuted       = 6,
            HostMutedHand   = 7,
            PhoneLoud       = 8,
            PhoneLoudHand   = 9,
            PhoneMuted      = 10,
            PhoneMutedHand  = 11,
        }
    }
}