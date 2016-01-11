using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.IO
{
    internal static class EncodingHelper
    {
        public static int GetBytes(string value, byte[] target, int targetOffset, out bool? hasNullBytes)
        {
            hasNullBytes = null;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] > 127)
                {
                    return Utf8Encodings.Strict.GetBytes(value, 0, value.Length, target, 0);
                }
                else if (value[i] == 0)
                {
                    hasNullBytes = true;
                    return -1;
                }

                target[targetOffset + i] = (byte)value[i];
            }

            hasNullBytes = false;
            return value.Length;
        }

        public static byte[] GetBytes(string value, out bool? hasNullBytes)
        {
            hasNullBytes = null;
            byte[] ascii = new byte[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] > 127)
                {
                    return Utf8Encodings.Strict.GetBytes(value);
                }
                else if (value[i] == 0)
                {
                    hasNullBytes = true;
                    return null;
                }

                ascii[i] = (byte)value[i];
            }

            hasNullBytes = false;
            return ascii;
        }
    }
}
