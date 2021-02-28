// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    public class AttendeeToImageConverter : BaseToCachedImageConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as MeetingAttendee == null) return null;

            var name = GetImageNameFromAttendee((MeetingAttendee)value);
            return getCachedImageFromName(name);
        }

        private string GetImageNameFromAttendee(MeetingAttendee attendee)
        {
            if (attendee.IsPhone)
            {
                return "call";
            }

            if (attendee.IsHost)
            {
                return "star";
            }

            return "desktop";
        }
    }
}