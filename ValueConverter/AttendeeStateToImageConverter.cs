// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Resources;
using ZoomJWAssistant.Models;

namespace ZoomJWAssistant.ValueConverter
{
    public class AttendeeStateToImageConverter : BaseToCachedImageConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = GetImageNameFromAttendee((MeetingAttendee.StateEnum)value);
            return getCachedImageFromName(name);
        }

        private string GetImageNameFromAttendee(MeetingAttendee.StateEnum attendeeState)
        {
            switch (attendeeState)
            {
                case MeetingAttendee.StateEnum.Loud:
                case MeetingAttendee.StateEnum.Muted:
                case MeetingAttendee.StateEnum.HostLoud:
                case MeetingAttendee.StateEnum.HostMuted:
                case MeetingAttendee.StateEnum.Regular:
                    return "desktop-sharp";
                case MeetingAttendee.StateEnum.Waiting:
                    return "hourglass-outline";
                case MeetingAttendee.StateEnum.MutedHand:
                case MeetingAttendee.StateEnum.HostLoudHand:
                case MeetingAttendee.StateEnum.HostMutedHand:
                case MeetingAttendee.StateEnum.LoudHand:
                    return "hand-left-outline";
                case MeetingAttendee.StateEnum.PhoneLoud:
                    return "call";
                case MeetingAttendee.StateEnum.PhoneLoudHand:
                    return "call-hand";
                case MeetingAttendee.StateEnum.PhoneMuted:
                    return "call-mute";
                case MeetingAttendee.StateEnum.PhoneMutedHand:
                    return "hand-left-outline";
            }

            return "desktop";
        }
    }
}