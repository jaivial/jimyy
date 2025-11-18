using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WorkflowAutomation.Engine.Expressions
{
    /// <summary>
    /// Built-in helper functions for expression evaluation
    /// Provides utility functions for string, number, date, and JSON operations
    /// </summary>
    public static class BuiltInFunctions
    {
        // ==================== Type Conversion Functions ====================

        /// <summary>
        /// Convert value to number
        /// </summary>
        public static double ToNumber(object value)
        {
            if (value == null) return 0;
            if (value is double d) return d;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is float f) return f;
            if (value is decimal dec) return (double)dec;

            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Convert value to string
        /// </summary>
        public static string ToString(object value)
        {
            if (value == null) return string.Empty;
            if (value is string s) return s;
            if (value is JToken jtoken) return jtoken.ToString();
            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Convert value to integer
        /// </summary>
        public static int ToInt(object value)
        {
            return (int)Math.Round(ToNumber(value));
        }

        /// <summary>
        /// Convert value to boolean
        /// </summary>
        public static bool ToBoolean(object value)
        {
            if (value == null) return false;
            if (value is bool b) return b;

            var str = value.ToString()?.ToLower();
            return str == "true" || str == "1" || str == "yes";
        }

        /// <summary>
        /// Convert value to Date
        /// </summary>
        public static DateTime ToDate(object value)
        {
            if (value == null) return DateTime.MinValue;
            if (value is DateTime dt) return dt;

            if (DateTime.TryParse(value.ToString(), out var result))
            {
                return result;
            }

            return DateTime.MinValue;
        }

        // ==================== String Functions ====================

        /// <summary>
        /// Convert string to uppercase
        /// </summary>
        public static string ToUpper(string value)
        {
            return value?.ToUpper() ?? string.Empty;
        }

        /// <summary>
        /// Convert string to lowercase
        /// </summary>
        public static string ToLower(string value)
        {
            return value?.ToLower() ?? string.Empty;
        }

        /// <summary>
        /// Trim whitespace from string
        /// </summary>
        public static string Trim(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Get substring
        /// </summary>
        public static string Substring(string value, int start, int? length = null)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (start >= value.Length) return string.Empty;
            if (start < 0) start = 0;

            if (length.HasValue)
            {
                var len = Math.Min(length.Value, value.Length - start);
                return value.Substring(start, len);
            }

            return value.Substring(start);
        }

        /// <summary>
        /// Replace text in string
        /// </summary>
        public static string Replace(string value, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace(oldValue ?? string.Empty, newValue ?? string.Empty);
        }

        /// <summary>
        /// Split string into array
        /// </summary>
        public static string[] Split(string value, string separator)
        {
            if (string.IsNullOrEmpty(value)) return Array.Empty<string>();
            return value.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Check if string contains substring
        /// </summary>
        public static bool Contains(string value, string substring)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.Contains(substring ?? string.Empty);
        }

        /// <summary>
        /// Check if string starts with prefix
        /// </summary>
        public static bool StartsWith(string value, string prefix)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.StartsWith(prefix ?? string.Empty);
        }

        /// <summary>
        /// Check if string ends with suffix
        /// </summary>
        public static bool EndsWith(string value, string suffix)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.EndsWith(suffix ?? string.Empty);
        }

        /// <summary>
        /// Get string length
        /// </summary>
        public static int Length(string value)
        {
            return value?.Length ?? 0;
        }

        /// <summary>
        /// Match regex pattern
        /// </summary>
        public static bool RegexMatch(string value, string pattern)
        {
            if (string.IsNullOrEmpty(value)) return false;
            try
            {
                return Regex.IsMatch(value, pattern);
            }
            catch
            {
                return false;
            }
        }

        // ==================== Math Functions ====================

        /// <summary>
        /// Round number to specified decimals
        /// </summary>
        public static double Round(double value, int decimals = 0)
        {
            return Math.Round(value, decimals);
        }

        /// <summary>
        /// Round down (floor)
        /// </summary>
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        /// <summary>
        /// Round up (ceiling)
        /// </summary>
        public static double Ceil(double value)
        {
            return Math.Ceiling(value);
        }

        /// <summary>
        /// Absolute value
        /// </summary>
        public static double Abs(double value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        /// Minimum of two values
        /// </summary>
        public static double Min(double a, double b)
        {
            return Math.Min(a, b);
        }

        /// <summary>
        /// Maximum of two values
        /// </summary>
        public static double Max(double a, double b)
        {
            return Math.Max(a, b);
        }

        /// <summary>
        /// Random number between 0 and 1
        /// </summary>
        public static double Random()
        {
            return new Random().NextDouble();
        }

        // ==================== Date/Time Functions ====================

        /// <summary>
        /// Get current date and time
        /// </summary>
        public static DateTime Now()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Get current UTC date and time
        /// </summary>
        public static DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Get today's date (no time)
        /// </summary>
        public static DateTime Today()
        {
            return DateTime.Today;
        }

        /// <summary>
        /// Format date to string
        /// </summary>
        public static string FormatDate(DateTime date, string format)
        {
            return date.ToString(format);
        }

        /// <summary>
        /// Add days to date
        /// </summary>
        public static DateTime AddDays(DateTime date, int days)
        {
            return date.AddDays(days);
        }

        /// <summary>
        /// Add hours to date
        /// </summary>
        public static DateTime AddHours(DateTime date, int hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// Add minutes to date
        /// </summary>
        public static DateTime AddMinutes(DateTime date, int minutes)
        {
            return date.AddMinutes(minutes);
        }

        /// <summary>
        /// Get year from date
        /// </summary>
        public static int Year(DateTime date)
        {
            return date.Year;
        }

        /// <summary>
        /// Get month from date
        /// </summary>
        public static int Month(DateTime date)
        {
            return date.Month;
        }

        /// <summary>
        /// Get day from date
        /// </summary>
        public static int Day(DateTime date)
        {
            return date.Day;
        }

        // ==================== JSON Functions ====================

        /// <summary>
        /// Parse JSON string to object
        /// </summary>
        public static object ParseJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) return null;

            try
            {
                return JsonConvert.DeserializeObject(jsonString);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert object to JSON string
        /// </summary>
        public static string ToJson(object value)
        {
            if (value == null) return "null";

            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch
            {
                return "null";
            }
        }

        /// <summary>
        /// Get JSON property value
        /// </summary>
        public static object GetJsonProperty(object obj, string path)
        {
            if (obj == null) return null;

            try
            {
                var jobj = obj is JObject jo ? jo : JObject.FromObject(obj);
                var token = jobj.SelectToken(path);
                return token?.ToObject<object>();
            }
            catch
            {
                return null;
            }
        }

        // ==================== Array/Collection Functions ====================

        /// <summary>
        /// Check if array is empty
        /// </summary>
        public static bool IsEmpty(object value)
        {
            if (value == null) return true;
            if (value is string s) return string.IsNullOrEmpty(s);
            if (value is System.Collections.IEnumerable enumerable)
            {
                return !enumerable.Cast<object>().Any();
            }
            return false;
        }

        /// <summary>
        /// Get array length
        /// </summary>
        public static int ArrayLength(object array)
        {
            if (array == null) return 0;
            if (array is System.Collections.ICollection collection) return collection.Count;
            if (array is System.Collections.IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Count();
            }
            return 0;
        }

        // ==================== Utility Functions ====================

        /// <summary>
        /// Check if value is null or undefined
        /// </summary>
        public static bool IsNull(object value)
        {
            return value == null;
        }

        /// <summary>
        /// Get default value if null
        /// </summary>
        public static object Default(object value, object defaultValue)
        {
            return value ?? defaultValue;
        }

        /// <summary>
        /// Generate UUID
        /// </summary>
        public static string Uuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Base64 encode
        /// </summary>
        public static string Base64Encode(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64 decode
        /// </summary>
        public static string Base64Decode(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            try
            {
                var bytes = Convert.FromBase64String(value);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
