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

namespace ZoomJWAssistant.ValueConverter
{
    public abstract class BaseToCachedImageConverter : IValueConverter
    {
        protected static Dictionary<string, DrawingImage> _cache = new Dictionary<string, DrawingImage>();
        protected static FileSvgReader converter = new FileSvgReader(new WpfDrawingSettings());

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        protected DrawingImage getCachedImageFromName(string name)
        {
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
                            return _cache[name] = new DrawingImage(converter.Read(sri.Stream));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(name + ":" + e.Message);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}