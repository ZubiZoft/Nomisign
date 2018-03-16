using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace CfdiService.Services
{
    class FullNameSplitterFromRFCService
    {
        public static string[] SplitName(string rfc, string fullName)
        {
            try
            {
                var r = rfc.ToUpper();
                var n = fullName.ToUpper();

                var first = r.Substring(0, 2).RemoveAccents();
                var second = r.Substring(2, 1).RemoveAccents();
                var third = r.Substring(3, 1).RemoveAccents();

                if (n.Contains(" " + first) && n.Contains(" " + second) && n.Substring(0, 1).Equals(third))
                {
                    var nameSplit = GetSplitterName(first, second, third, n);
                    return nameSplit;
                }

                if ((first.Substring(0, 1) + "H").RemoveAccents().Equals("CH"))
                {
                    first = "CH";
                    var nameSplit = GetSplitterName(first, second, third, n);
                    if (nameSplit != null)
                        return nameSplit;
                }

                first = r.Substring(0, 1).RemoveAccents();
                second = r.Substring(1, 1).RemoveAccents();
                third = r.Substring(2, 2).RemoveAccents();
                if (n.Contains(" " + first) && n.Contains(" " + second) && n.Substring(0, 2).RemoveAccents().Equals(third))
                {
                    var nameSplit = GetSplitterName(first, second, third, n);
                    return nameSplit;
                }

                first = r.Substring(0, 2).RemoveAccents();
                second = r.Substring(2, 1).RemoveAccents();
                third = r.Substring(3, 1).RemoveAccents();
                if (n.Contains(" " + first) && n.Contains(" " + second) && n.Contains(" " + third))
                {
                    var nameSplit = GetSplitterName(first, second, third, n);
                    return nameSplit;
                }

                if (third.Equals("X"))
                {
                    var nameSplit = GetSplitterName(first, second, n.Substring(0, 1), n);
                    return nameSplit;
                }
            }
            catch { }
            return null;
        }

        private static string[] GetSplitterName(string first, string second, string third, string n)
        {
            var arr = n.Split(new char[] { ' ' });
            var name = "";
            var firstLast = "";
            var secondLast = "";
            for (int j = arr.Length - 1, k = j; j >= 0; j--)
            {
                if (arr[j].RemoveAccents().StartsWith(second) && string.IsNullOrEmpty(secondLast))
                {
                    secondLast = arr.JoinEntries(j, k);
                    k = j - 1;
                }
                else if (arr[j].RemoveAccents().StartsWith(first) && !string.IsNullOrEmpty(secondLast) && string.IsNullOrEmpty(firstLast))
                {
                    firstLast = arr.JoinEntries(j, k);
                    k = j;

                    name = arr.JoinEntries(0, j - 1);
                    break;
                }
            }
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(firstLast) && !string.IsNullOrEmpty(secondLast))
            {
                return new string[] { name.CapitalizeString(), firstLast.CapitalizeString(), secondLast.CapitalizeString() };
            }
            return null;
        }
    }

    public static class StringExtension
    {
        public static string RemoveAccents(this string s)
        {
            return s.Replace("á", "a").Replace("Á", "A").Replace("é", "e").Replace("É", "E").Replace("í", "i")
                .Replace("Í", "I").Replace("ó", "o").Replace("Ó", "O").Replace("ú", "u").Replace("Ú", "U");
        }

        public static string JoinEntries(this string[] s, int from, int to)
        {
            var res = "";
            for (var i = to; i >= from; i--)
            {
                res = s[i] + " " + res;
            }
            return res.Trim();
        }

        public static string CapitalizeString(this string s)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(s.ToLower());
        }
    }
}
