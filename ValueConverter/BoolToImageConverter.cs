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

namespace ZoomJWAssistant.ValueConverter
{
    public class BoolToImageConverter : BaseToCachedImageConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = ((string)parameter);

            var boolValue = (bool)value;
            if (boolValue == true)
            {
                return getCachedImageFromName(name);
            }
            return null;
        }
    }
}