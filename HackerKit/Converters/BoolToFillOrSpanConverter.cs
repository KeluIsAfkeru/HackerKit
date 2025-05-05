using System.Globalization;

namespace HackerKit.Converters
{
	public class BoolToFillOrSpanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (value is bool b && b) ? LayoutOptions.Fill : LayoutOptions.FillAndExpand;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
