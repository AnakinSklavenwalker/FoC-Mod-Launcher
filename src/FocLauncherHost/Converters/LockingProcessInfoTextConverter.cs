using System;
using System.Globalization;
using System.Windows.Data;

namespace FocLauncherHost.Converters
{
    internal class LockingProcessInfoTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // TODO:
            //if (!(value is ILockingProcessInfo lockingProcessInfo))
            //    throw new NotSupportedException();

            //return $"{lockingProcessInfo.Description}.exe [{lockingProcessInfo.Id}]";
            return new();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
