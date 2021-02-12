// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Svg2Xaml;
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
    public class AttendeeToImageConverter : IValueConverter
    {
        public char Separator { get; set; } = ';';

        private static Dictionary<string, DrawingImage> _cache = new Dictionary<string, DrawingImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as MeetingAttendee == null) return null;

            var name = GetImageNameFromAttendee((MeetingAttendee)value);
            if (_cache.ContainsKey(name))
            {
                return _cache[name];
            }
            try
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(string.Format("Resources/symbols/{0}.svg", name), UriKind.Relative));
                if (sri != null)
                {
                    using (Stream s = sri.Stream)
                    {
                        return _cache[name] = SvgReader.Load(sri.Stream);
                    }
                }
            } catch (Exception) { }
            return null;
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}