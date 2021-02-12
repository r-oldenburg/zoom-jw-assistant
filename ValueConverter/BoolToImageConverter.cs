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

namespace ZoomJWAssistant.ValueConverter
{
    public class BoolToImageConverter : IValueConverter
    {
        public char Separator { get; set; } = ';';

        private static Dictionary<string, DrawingImage> _cache = new Dictionary<string, DrawingImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = ((string)parameter);

            var boolValue = (bool)value;
            if (boolValue == true)
            {
                if (_cache.ContainsKey(name))
                {
                    return _cache[name];
                }
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(string.Format("Resources/symbols/{0}.svg", name), UriKind.Relative));
                if (sri != null)
                {
                    using (Stream s = sri.Stream)
                    {
                        return _cache[name] = SvgReader.Load(sri.Stream);
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}