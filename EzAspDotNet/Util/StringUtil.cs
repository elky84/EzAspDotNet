﻿using System;
using System.Text.RegularExpressions;

namespace WebShared.Util
{
    public static class StringUtil
    {

        public static int ToInt(this string str)
        {
            return int.Parse(str.OnlyDigit());
        }

        public static int ToInt(this double value)
        {
            return Convert.ToInt32(value);
        }

        public static double ToDouble(this string str)
        {
            return double.Parse(Regex.Match(str, @"[0-9\-.]+").Value);
        }

        public static float ToFloat(this string str)
        {
            return float.Parse(Regex.Match(str, @"[0-9\-.]+").Value);
        }

        public static string OnlyDigit(this string str)
        {
            return Regex.Match(str, @"[0-9\-]+").Value;
        }

        public static string ExtractKorean(this string str)
        {
            return Regex.Replace(str, "[^가-힣]", "");
        }

    }
}
