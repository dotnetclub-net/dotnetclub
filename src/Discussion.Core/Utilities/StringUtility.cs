using System;
using System.Text;

namespace Discussion.Core.Utilities
{
    public static class StringUtility
    {
        public static string Random(int length = 8)
        {
            var sb = new StringBuilder();
            var random = new Random(Guid.NewGuid().GetHashCode());
            
            do
            {
                var rand = random.Next(87, 122);
                if (rand < 97)
                {
                    rand -= 39;
                }

                sb.Append((char) rand);
            } while (sb.Length < length);

            return sb.ToString();
        }
        
        public static string RandomNumbers(int length = 6)
        {
            var sb = new StringBuilder();
            var random = new Random();
            
            do
            {
                var rand = random.Next(0, 9);
                sb.Append(rand);
            } while (sb.Length < length);

            return sb.ToString();
        }

        public static string MaskPhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null || phoneNumber.Length < 7)
            {
                return phoneNumber;
            }

            return string.Concat(phoneNumber.Substring(0, 3), "****", phoneNumber.Substring(7));
        }
        
        
        public static bool IgnoreCaseEqual(this string one, string theOther)
        {
            if (one == null && theOther == null)
            {
                return true;
            }
            
            if (one == null || theOther == null)
            {
                return false;
            }
            
            return one.Equals(theOther, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            if (str == null)
            {
                return null;
            }

            startIndex = Math.Max(startIndex, 0);
            startIndex = Math.Min(startIndex, str.Length - 1);
            
            length = Math.Max(length, 0);
            length = Math.Min(str.Length - startIndex, length);

            return str.Substring(startIndex, length);
        }
        
        public static string SafeSubstring(this string str, int startIndex)
        {
            return str?.SafeSubstring(startIndex, str.Length);
        }
    }
}