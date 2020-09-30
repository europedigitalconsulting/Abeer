using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Abeer.Client
{
    public static class StringCutterExtension
    {
        public static string Short(this string content, int maxLength)
        {
            if (content.Length > maxLength)
                return content.Substring(0, content.Length);

            return content;
        }

        public static string Finished(this string content, int maxLength, string finisedSymbol)
        {
            if (content.Length <= maxLength)
                return ".";
            else
                return finisedSymbol;
        }

        public static string FormatCurrency(this decimal value, CultureInfo cultureInfo)
        {
            return string.Format(cultureInfo, "{0:N2}", value);
        }

        public static string FormatCurrency(this decimal value)
        {
            return FormatCurrency(value, CultureInfo.CurrentCulture);
        }

        public static string FormatPercent(this decimal value, CultureInfo cultureInfo)
        {
            return string.Format(cultureInfo, "{0:P2}", value);
        }

        public static string FormatPercent(this decimal value)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:P2}", value);
        }
    }
}
