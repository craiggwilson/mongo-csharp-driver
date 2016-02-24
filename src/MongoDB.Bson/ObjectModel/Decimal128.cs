using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Shared;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a Decimal128 value.
    /// </summary>
    [Serializable]
    public struct Decimal128 : IConvertible, IComparable<Decimal128>, IEquatable<Decimal128>
    {
        private const short __maxSignificandDigits = 34;
        private const short __exponentMax = 6111;
        private const short __exponentMin = -6176;
        private const short __exponentBias = 6176;

        /// <summary>
        /// Represents negative infinity.
        /// </summary>
        public static Decimal128 NegativeInfinity =>
            new Decimal128(new uint[] { 0xF8000000, 0, 0, 0 });

        /// <summary>
        /// Represents positive infinity.
        /// </summary>
        public static Decimal128 PositiveInfinity =>
            new Decimal128(new uint[] { 0x78000000, 0, 0, 0 });

        /// <summary>
        /// Represents a value that is not a number.
        /// </summary>
        public static Decimal128 QNaN =>
            new Decimal128(new uint[] { 0x7C000000, 0, 0, 0 });

        /// <summary>
        /// Represents a value that is not a number and raises errors when used in calculations.
        /// </summary>
        public static Decimal128 SNaN =>
            new Decimal128(new uint[] { 0x7E000000, 0, 0, 0 });

        private readonly byte _flags;
        private readonly short _exponent;

        // below only represent the significand
        private readonly uint _high;
        private readonly uint _highMid;
        private readonly uint _lowMid;
        private readonly uint _low;

        private Decimal128(byte flags, uint[] bits, short exponent)
        {
            _flags = flags;
            _high = bits[0];
            _highMid = bits[1];
            _lowMid = bits[2];
            _low = bits[3];
            _exponent = exponent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(decimal value)
        {
            var bits = decimal.GetBits(value);

            _flags = 0;
            if ((bits[3] & 0x80000000) != 0)
            {
                _flags = 0x80;
            }
            _exponent = (short)((bits[3] >> 16) & 0x7F);
            _exponent *= -1;

            _high = 0;
            _highMid = unchecked((uint)bits[2]);
            _lowMid = unchecked((uint)bits[1]);
            _low = unchecked((uint)bits[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public Decimal128(sbyte value)
        {
            _flags = 0;
            int v = value;
            if (value < 0)
            {
                // sign bit
                _flags = 0x80;
                v = -v;
            }

            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = 0;
            _low = (uint)v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(byte value)
        {
            _flags = 0;
            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = 0;
            _low = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(short value)
        {
            _flags = 0;
            int v = value;
            if (value < 0)
            {
                // sign bit
                _flags = 0x80;
                v = -v;
            }

            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = 0;
            _low = (uint)v;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public Decimal128(ushort value)
        {
            _flags = 0;
            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = 0;
            _low = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(int value)
        {
            _flags = 0;
            if (value < 0)
            {
                // sign bit
                _flags = 0x80;
                value = -value;
            }

            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = 0;
            _low = (uint)value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public Decimal128(uint value)
        {
            _flags = 0;
            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = 0;
            _low = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(long value)
        {
            _flags = 0;
            if (value < 0)
            {
                // sign bit
                _flags = 0x80;
                value = -value;
            }

            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = (uint)(value >> 32);
            _low = (uint)value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public Decimal128(ulong value)
        {
            _flags = 0;
            _exponent = 0;
            _high = 0;
            _highMid = 0;
            _lowMid = (uint)(value >> 32);
            _low = (uint)value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="bits">The binary representation in the form of four 32-bit unsigned integers.</param>
        [CLSCompliant(false)]
        public Decimal128(uint[] bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException("parts");
            }
            if (bits.Length != 4)
            {
                throw new ArgumentException("Must be of length 4.", "parts");
            }

            _flags = (byte)(bits[0] >> 24);

            // combination will be the low 5 bits
            var combination = (_flags >> 2) & 0x1F;
            uint biasedExponent;
            uint significandMsb;
            // 2 high combination bits are set
            if ((combination >> 3) == 0x3)
            {
                biasedExponent = (bits[0] >> 15) & 0x3FFF;
                significandMsb = 0x8 + ((bits[0] >> 14) & 0x1);
            }
            else
            {
                biasedExponent = (bits[0] >> 17) & 0x3FFF;
                significandMsb = (bits[0] >> 14) & 0x7;
            }

            _exponent = (short)(biasedExponent - __exponentBias);

            _high = (bits[0] & 0x3FFF) + ((significandMsb & 0xF) << 14);
            _highMid = bits[1];
            _lowMid = bits[2];
            _low = bits[3];
        }

        /// <inheritdoc />
        public int CompareTo(Decimal128 other)
        {
            if (Equals(other))
            {
                return 0;
            }

            // TODO: obviously, this needs to get fixed
            return -1;
        }

        /// <inheritdoc />
        public bool Equals(Decimal128 other)
        {
            // TODO: problem is that this representation
            // doesn't normalize... Hence 120 * 10^3 and 12.0 * 10 ^ 4
            // aren't stored the same way. We need to normalize both
            // prior to an equality comparison.
            // For now, we'll simply compare the bits...
            var bits = GetBits(this);
            var otherBits = GetBits(other);
            return bits[0] == otherBits[0] &&
                bits[1] == otherBits[1] &&
                bits[2] == otherBits[2] &&
                bits[3] == otherBits[3];
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Decimal128)
            {
                return Equals((Decimal128)obj);
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _flags.GetHashCode();
            hash = 37 * hash + _exponent.GetHashCode();
            hash = 37 * hash + _high.GetHashCode();
            hash = 37 * hash + _highMid.GetHashCode();
            hash = 37 * hash + _lowMid.GetHashCode();
            hash = 37 * hash + _low.GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(NumberFormatInfo.CurrentInfo);
        }

        /// <inheritdoc />
        public string ToString(IFormatProvider provider)
        {
            return ToString(NumberFormatInfo.GetInstance(provider));
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException("Invalid cast from Decima128 to Char.");
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            if (IsNegative(this))
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.");
            }

            throw new NotImplementedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("Invalid cast from Decima128 to DateTime.");
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        private string ToString(NumberFormatInfo formatInfo)
        {
            var result = new StringBuilder();

            // high bit is 1
            bool isNegative = (_flags & 0x80) != 0;

            // combination will be the low 5 bits
            var combination = (_flags >> 2) & 0x1F;

            // 2 high combination bits are set
            if ((combination >> 3) == 0x3)
            {
                if (combination == 0x1E)
                {
                    if (isNegative)
                    {
                        // TODO: should we use formatInfo.NegativeInfinitySymbol?
                        result.Append("-Infinity");
                    }
                    else
                    {
                        // TODO: should we use formatInfo.PositiveInfinitySymbol?
                        result.Append("Infinity");
                    }
                    return result.ToString();
                }
                else if (combination == 0x1F)
                {
                    result.Append("NaN");
                    // TODO: should put an S in front when SNaN...
                    return result.ToString();
                }
            }

            if (isNegative && formatInfo.NumberNegativePattern <= 2)
            {
                if (formatInfo.NumberNegativePattern == 0)
                {
                    result.Append('(');
                }
                else
                {
                    result.Append(formatInfo.NegativeSign);
                    if (formatInfo.NumberNegativePattern == 2)
                    {
                        result.Append(' ');
                    }
                }
            }

            var significand = new byte[36];
            bool isZero = false;
            var high = _high;
            var highMid = _highMid;
            var lowMid = _lowMid;
            var low = _low;
            if (high == 0 && highMid == 0 && lowMid == 0 && low == 0)
            {
                isZero = true;
            }
            else
            {
                for (int k = 3; k >= 0; k--)
                {
                    uint remainder;
                    DivideByOneBillion(ref high, ref highMid, ref lowMid, ref low, out remainder);

                    if (remainder != 0)
                    {
                        for (int j = 8; j >= 0; j--)
                        {
                            significand[k * 9 + j] = (byte)(remainder % 10);
                            remainder /= 10;
                        }
                    }
                }
            }

            int significandDigits = 0;
            int significandRead = 0;
            if (isZero)
            {
                significandDigits = 1;
                significandRead = 0;
            }
            else
            {
                significandDigits = 36;
                while (significand[significandRead] == 0)
                {
                    significandDigits--;
                    significandRead++;
                }
            }

            var scientificExponent = significandDigits - 1 + _exponent;

            if (scientificExponent >= 12
                || scientificExponent <= -4
                || _exponent > 0
                || (isZero && scientificExponent != 0))
            {
                result.Append(significand[significandRead++]);
                significandDigits--;

                if (significandDigits != 0)
                {
                    result.Append(formatInfo.NumberDecimalSeparator);
                }

                for (int i = 0; i < significandDigits; i++)
                {
                    result.Append(significand[significandRead++]);
                }

                result.Append('E');
                if (scientificExponent > 0)
                {
                    result.Append(formatInfo.PositiveSign);
                }
                result.Append(scientificExponent.ToString(formatInfo));
            }
            else
            {
                if (_exponent >= 0)
                {
                    for (int i = 0; i < significandDigits; i++)
                    {
                        result.Append(significand[significandRead++]);
                    }
                }
                else
                {
                    int radixPosition = significandDigits + _exponent;

                    if (radixPosition > 0) // non-zero digits before radix
                    {
                        for (int i = 0; i < radixPosition; i++)
                        {
                            result.Append(significand[significandRead++]);
                        }
                    }
                    else
                    {
                        result.Append('0');
                    }

                    result.Append(formatInfo.NumberDecimalSeparator);

                    while (radixPosition++ < 0)
                    {
                        result.Append('0');
                    }

                    for (int i = 0; i < significandDigits - (int)Math.Max(radixPosition - 1, 0); i++)
                    {
                        result.Append(significand[significandRead++]);
                    }
                }
            }

            if (isNegative && formatInfo.NumberNegativePattern == 0)
            {
                result.Append(')');
            }
            else if (isNegative && formatInfo.NumberNegativePattern >= 3)
            {
                if (formatInfo.NumberNegativePattern == 4)
                {
                    result.Append(' ');
                }
                result.Append(formatInfo.NegativeSign);
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts the value of the specified number to its equivalent binary representation.
        /// </summary>
        /// <returns>A 32-bit unsigned integer array with four elements that contain the binary representation of <paramref name="d"/>.</returns>
        [CLSCompliant(false)]
        public static uint[] GetBits(Decimal128 d)
        {
            var parts = new uint[4];
            var biasedExponent = (uint)(d._exponent + __exponentBias);

            parts[3] = d._low;
            parts[2] = d._lowMid;
            parts[1] = d._highMid;

            if (((d._high >> 17) & 0x1) == 0x1)
            {
                parts[0] |= 0x3 << 29;
                parts[0] |= (biasedExponent & 0x3FFF) << 15;
                parts[0] |= d._high & 0x7FFF;
            }
            else
            {
                parts[0] |= (biasedExponent & 0x3FFF) << 17;
                parts[0] |= d._high & 0x1FFFFFFF;
            }

            if ((d._flags & 0x80) == 0x80)
            {
                parts[0] |= 0x80000000;
            }

            return parts;
        }

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to negative or positive infinity.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> evaluates to negative or positive infinity; otherwise, false.</returns>
        public static bool IsInfinity(Decimal128 d) => IsPositiveInfinity(d) || IsNegativeInfinity(d);

        /// <summary>
        /// Returns a value indicating whether the specified number is negative.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is negative; otherwise, false.</returns>
        public static bool IsNegative(Decimal128 d) => (d._flags & 0x80) != 0;

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to negative infinity.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> evaluates to negative infinity; otherwise, false.</returns>
        public static bool IsNegativeInfinity(Decimal128 d) => (d._flags & 0xF8) == 0xF8;

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to positive infinity.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> evaluates to positive infinity; otherwise, false.</returns>
        public static bool IsPositiveInfinity(Decimal128 d) => (d._flags & 0x78) == 0x78 && (d._flags & 0x80) == 0 && (d._flags & 0x7C) != 0x7C;

        /// <summary>
        /// Returns a value indicating whether the specified number is not a number.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is not a number; otherwise, false.</returns>
        public static bool IsNaN(Decimal128 d) => IsQNaN(d) || IsSNaN(d);

        /// <summary>
        /// Returns a value indicating whether the specified number is a quiet not a number.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is a quiet not a number; otherwise, false.</returns>
        public static bool IsQNaN(Decimal128 d) => (d._flags & 0x7C) == 0x7C && (d._flags & 0x7E) != 0x7E;

        /// <summary>
        /// Returns a value indicating whether the specified number is a signaled not a number.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is a signaled not a number; otherwise, false.</returns>
        public static bool IsSNaN(Decimal128 d) => (d._flags & 0x7E) == 0x7E;

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <returns>
        /// The equivalent to the number contained in <paramref name="s" />.
        /// </returns>
        public static Decimal128 Parse(string s)
        {
            return Parse(s, NumberStyles.Float, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its <see cref="Decimal128" /> equivalent.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
        /// <returns>
        /// The <see cref="Decimal128" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" />.
        /// </returns>
        public static Decimal128 Parse(string s, NumberStyles style)
        {
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent using the specified culture-specific format information.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific parsing information about <paramref name="s" />.</param>
        /// <returns>
        /// The <see cref="Decimal128" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="provider" />.
        /// </returns>
        public static Decimal128 Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Float, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent using the specified style and culture-specific format.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> object that supplies culture-specific information about the format of <paramref name="s" />.</param>
        /// <returns>
        /// The <see cref="Decimal128" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" /> and <paramref name="provider" />.
        /// </returns>
        public static Decimal128 Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            Decimal128 result;
            if (!TryParse(s, style, provider, out result))
            {
                throw new ArgumentException($"{s} is not a valid Decimal128.", "s");
            }

            return result;
        }

        /// <summary>
        /// Rounds a <see cref="Decimal128" /> value to a specified number of decimal places.
        /// </summary>
        /// <param name="d">A decimal number to round.</param>
        /// <param name="decimals">A value from 0 to 34 that specifies the number of decimal places to round to.</param>
        /// <returns>
        /// The decimal number equivalent to <paramref name="d" /> rounded to <paramref name="decimals" /> number of decimal places.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static decimal Round(Decimal128 d, int decimals)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="Decimal128" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or is zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not a number in a valid format, or represents a number less than the min value or greater than the max value. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if <paramref name="s" /> was converted successfully; otherwise, false.
        /// </returns>
        public static bool TryParse(string s, out Decimal128 result)
        {
            return TryParse(s, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
        /// <param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />. </param>
        /// <param name="result">When this method returns, contains the <see cref="Decimal128" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or is zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not a number in a valid format, or represents a number less than the min value or greater than the max value. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if <paramref name="s" /> was converted successfully; otherwise, false.
        /// </returns>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Decimal128 result)
        {
            return new Decimal128Parser(s, style, NumberFormatInfo.GetInstance(provider)).TryParse(out result);
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent <see cref="decimal"/>.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A <see cref="decimal"/> equivalent to <paramref name="d" />.</returns>
        public static decimal ToDecimal(Decimal128 d)
        {
            if (d._high == 0 && d._exponent >= -128 && d._exponent <= 127)
            {
                if (d._flags == 0x80 || d._flags == 0)
                {
                    var scale = (byte)-d._exponent;
                    bool isNegative = d._flags == 0x80; // only the negative bit is set
                    return new decimal((int)d._low, (int)d._lowMid, (int)d._highMid, isNegative, scale);
                }
            }

            throw new OverflowException("Value was either too large or too small for a Decimal.");
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 8-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 8-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        public static byte ToByte(Decimal128 d)
        {
            uint num;
            try
            {
                num = ToUInt32(d);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("Value was either too large or too small for a Byte.", ex);
            }

            if (num < byte.MinValue || num > byte.MaxValue)
            {
                throw new OverflowException("Value was either too large or too small for a Byte.");
            }

            return (byte)num;
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 16-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 16-bit signed integer equivalent to <paramref name="d" />.</returns>
        public static short ToInt16(Decimal128 d)
        {
            int num;
            try
            {
                num = ToInt32(d);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("Value was either too large or too small for a Int16.", ex);
            }

            if (num < short.MinValue || num > short.MaxValue)
            {
                throw new OverflowException("Value was either too large or too small for a Int16.");
            }

            return (short)num;
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 32-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 32-bit signed integer equivalent to <paramref name="d" />.</returns>
        public static int ToInt32(Decimal128 d)
        {
            if (d._high == 0 && d._highMid == 0 && d._lowMid == 0)
            {
                int num = (int)d._low;
                if (d._flags == 0x80) // only the negative bit is set...
                {
                    num = -num;
                    if (num <= 0)
                    {
                        return num;
                    }
                }
                else if (d._flags == 0)
                {
                    if (num >= 0)
                    {
                        return num;
                    }
                }
            }

            throw new OverflowException("Value was either too large or too small for a Int32.");
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 64-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 64-bit signed integer equivalent to <paramref name="d" />.</returns>
        public static long ToInt64(Decimal128 d)
        {
            if (d._high == 0 && d._highMid == 0)
            {
                long num = ((long)d._lowMid << 32) | d._low;
                if (d._flags == 0x80) // only the negative bit is set...
                {
                    num = -num;
                    if (num <= 0L)
                    {
                        return num;
                    }
                }
                else if (d._flags == 0)
                {
                    if (num >= 0L)
                    {
                        return num;
                    }
                }
            }

            throw new OverflowException("Value was either too large or too small for a Int64.");
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 8-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 8-bit signed integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static sbyte ToSByte(Decimal128 d)
        {
            int num;
            try
            {
                num = ToInt32(d);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.", ex);
            }

            if (num < sbyte.MinValue || num > sbyte.MaxValue)
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.");
            }

            return (sbyte)num;
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 16-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 16-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static ushort ToUInt16(Decimal128 d)
        {
            uint num;
            try
            {
                num = ToUInt32(d);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.", ex);
            }

            if (num < 0u || num > ushort.MaxValue)
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.");
            }

            return (ushort)num;
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 32-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 32-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static uint ToUInt32(Decimal128 d)
        {
            if (d._high == 0 && d._highMid == 0 && d._lowMid == 0)
            {
                uint num = d._low;
                if (d._flags == 0 || num == 0)
                {
                    return num;
                }
            }

            throw new OverflowException("Value was either too large or too small for a UInt32.");
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 64-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 64-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static ulong ToUInt64(Decimal128 d)
        {
            if (d._high == 0 && d._highMid == 0)
            {
                ulong num = ((ulong)d._lowMid << 32) | d._low;
                if (d._flags == 0 || num == 0)
                {
                    return num;
                }
            }

            throw new OverflowException("Value was either too large or too small for a UInt64.");
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Decimal128 lhs, Decimal128 rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Decimal128 lhs, Decimal128 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Byte"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(byte value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.SByte"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(sbyte value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int16"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(short value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt16"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(ushort value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(int value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt32"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(uint value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int64"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(long value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(ulong value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Decimal"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(decimal value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Byte"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator byte(Decimal128 value)
        {
            return ToByte(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.SByte"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator sbyte(Decimal128 value)
        {
            return ToSByte(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator short(Decimal128 value)
        {
            return ToInt16(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.UInt16"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator ushort(Decimal128 value)
        {
            return ToUInt16(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int(Decimal128 value)
        {
            return ToInt32(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.UInt32"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator uint(Decimal128 value)
        {
            return ToUInt32(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator long(Decimal128 value)
        {
            return ToInt64(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator ulong(Decimal128 value)
        {
            return ToUInt64(value);
        }
        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Decimal"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator decimal(Decimal128 value)
        {
            return ToDecimal(value);
        }

        private static void DivideByOneBillion(ref uint high, ref uint highMid, ref uint lowMid, ref uint low, out uint remainder)
        {
            const uint divisor = 1000 * 1000 * 1000;

            ulong tempRemainder = 0;

            if (high == 0 && highMid == 0 && lowMid == 0 && low == 0)
            {
                remainder = 0;
                return;
            }

            tempRemainder <<= 32;
            tempRemainder += high;
            high = (uint)(tempRemainder / divisor);
            tempRemainder %= divisor;

            tempRemainder <<= 32;
            tempRemainder += highMid;
            highMid = (uint)(tempRemainder / divisor);
            tempRemainder %= divisor;

            tempRemainder <<= 32;
            tempRemainder += lowMid;
            lowMid = (uint)(tempRemainder / divisor);
            tempRemainder %= divisor;

            tempRemainder <<= 32;
            tempRemainder += low;
            low = (uint)(tempRemainder / divisor);
            tempRemainder %= divisor;

            remainder = (uint)tempRemainder;
        }

        private class Decimal128Parser
        {
            private readonly string _s;
            private readonly NumberStyles _style;
            private readonly NumberFormatInfo _formatInfo;

            public Decimal128Parser(string s, NumberStyles style, NumberFormatInfo formatInfo)
            {
                if (s == null)
                {
                    throw new ArgumentNullException("s");
                }
                if ((style & ~(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowHexSpecifier)) != NumberStyles.None)
                {
                    throw new ArgumentException("An undefined NumberStyles value is being used.", "style");
                }
                if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
                {
                    throw new ArgumentException("The number style AllowHexSpecifier is not supported on floating point data types.", "style");
                }
                if (formatInfo == null)
                {
                    throw new ArgumentNullException("formatInfo");
                }

                _s = s;
                _style = style;
                _formatInfo = formatInfo;
            }

            [Flags]
            private enum ParseState
            {
                None = 0,
                Sign = 1,
                Parenthesis = 2,
                Digits = 4,
                SeenNonZero = 8,
                Decimal = 16,
                Infinity = 32,
                NaN = 64
            }

            public bool TryParse(out Decimal128 result)
            {
                result = default(Decimal128);
                bool negative = false;
                bool exponentNegative = false;
                var digits = new List<byte>();
                var scale = 0;
                ParseState state = ParseState.None;
                int i = 0;
                int i2;

                // read leading whitespace, specials, and sign
                while (i < _s.Length)
                {
                    var allowLeadingSign = (_style & NumberStyles.AllowLeadingSign) != 0 && (state & ParseState.Sign) == 0;
                    var allowSpecial = (state & ParseState.Infinity) == 0 && (state & ParseState.NaN) == 0;
                    // Read whitespace
                    if (!char.IsWhiteSpace(_s, i) ||
                        (_style & NumberStyles.AllowLeadingWhite) == 0 ||
                        (((state & ParseState.Sign) != 0 || _formatInfo.NumberNegativePattern != 2) &&
                            (state & ParseState.Infinity) == 0 && (state & ParseState.NaN) == 0))
                    {
                        if (state == 0 && (i2 = MatchChars(_s, i, _formatInfo.NegativeInfinitySymbol)) != i)
                        {
                            state |= ParseState.Infinity | ParseState.Sign;
                            negative = true;
                            i = i2 - 1;
                        }
                        else if (state == 0 && (i2 = MatchChars(_s, i, _formatInfo.PositiveInfinitySymbol)) != i)
                        {
                            state |= ParseState.Infinity | ParseState.Sign;
                            i = i2 - 1;
                        }
                        else if (allowSpecial && (
                            (i2 = MatchChars(_s, i, "Infinity")) != i ||
                            (i2 = MatchChars(_s, i, "Inf")) != i))
                        {
                            state |= ParseState.Infinity;
                            i = i2 - 1;
                        }
                        else if (allowSpecial && (
                            (i2 = MatchChars(_s, i, _formatInfo.NaNSymbol)) != i ||
                            (i2 = MatchChars(_s, i, "NaN")) != i))
                        {
                            state |= ParseState.NaN;
                            i = i2 - 1;
                        }
                        else if (allowLeadingSign && ((i2 = MatchChars(_s, i, _formatInfo.PositiveSign)) != i))
                        {
                            state |= ParseState.Sign;
                            i = i2 - 1;
                        }
                        else if (allowLeadingSign && ((i2 = MatchChars(_s, i, _formatInfo.NegativeSign)) != i))
                        {
                            state |= ParseState.Sign;
                            negative = true;
                            i = i2 - 1;
                        }
                        else if (_s[i] == '(' && (_style & NumberStyles.AllowParentheses) != 0 && (state & ParseState.Sign) == 0)
                        {
                            state |= ParseState.Parenthesis | ParseState.Sign;
                        }
                        else
                        {
                            break;
                        }
                    }
                    i++;
                }

                // read digits, decimal point, and scale
                while (i < _s.Length)
                {
                    if (char.IsDigit(_s, i))
                    {
                        state |= ParseState.Digits;

                        if (_s[i] != '0' || (state & ParseState.SeenNonZero) != 0)
                        {
                            if (_s[i] == '0' && digits.Count >= __maxSignificandDigits)
                            {
                                if ((state & ParseState.Decimal) == 0)
                                {
                                    scale++;
                                }
                                else
                                {
                                    // too many digits... we cannot represent this number...
                                    return false;
                                }
                            }
                            else
                            {
                                if (digits.Count >= __maxSignificandDigits)
                                {
                                    // too many digits... we cannot represent this number...
                                    return false;
                                }
                                digits.Add((byte)(_s[i] - '0'));
                                state |= ParseState.SeenNonZero;
                            }
                        }

                        if ((state & ParseState.Decimal) != 0)
                        {
                            scale--;
                        }
                    }
                    else if ((_style & NumberStyles.AllowDecimalPoint) != 0 && (state & ParseState.Decimal) == 0 && (i2 = MatchChars(_s, i, _formatInfo.NumberDecimalSeparator)) != i)
                    {
                        i = i2 - 1;
                        state |= ParseState.Decimal;
                    }
                    else
                    {
                        if ((_style & NumberStyles.AllowThousands) == 0 || (state & ParseState.Decimal) != 0 && (state & ParseState.Digits) == 0 || (i2 = MatchChars(_s, i, _formatInfo.NumberGroupSeparator)) == i)
                        {
                            break;
                        }

                        i = i2 - 1;
                    }
                    i++;
                }

                // parse exponent and scale
                if ((state & ParseState.Digits) != 0 && i < _s.Length)
                {
                    if (_s[i] == 'E' && (_style & NumberStyles.AllowExponent) != 0)
                    {
                        i++;

                        if ((i2 = MatchChars(_s, i, _formatInfo.PositiveSign)) != i)
                        {
                            i = i2;
                        }
                        else if ((i2 = MatchChars(_s, i, _formatInfo.NegativeSign)) != i)
                        {
                            exponentNegative = true;
                            i = i2;
                        }
                        if (i < _s.Length && char.IsDigit(_s, i))
                        {
                            int tempScale = 0;
                            do
                            {
                                tempScale = tempScale * 10 + (_s[i] - '0');
                                i++;
                            }
                            while (i < _s.Length && char.IsDigit(_s, i));

                            if (exponentNegative)
                            {
                                tempScale = -tempScale;
                            }
                            if (tempScale <= scale && (scale - tempScale) > (1 << 14))
                            {
                                scale = __exponentMin;
                            }
                            else
                            {
                                scale += tempScale;
                            }
                        }
                    }
                }

                // Parse trailing whitespace, sign, etc...
                while (i < _s.Length)
                {
                    if (!char.IsWhiteSpace(_s, i) || (_style & NumberStyles.AllowTrailingWhite) == 0)
                    {
                        var allowTrailingSign = (_style & NumberStyles.AllowTrailingSign) != 0 && (state & ParseState.Sign) == 0;
                        if (allowTrailingSign && (i2 = MatchChars(_s, i, _formatInfo.PositiveSign)) != i)
                        {
                            state |= ParseState.Sign;
                            i = i2 - 1;
                        }
                        else if (allowTrailingSign && (i2 = MatchChars(_s, i, _formatInfo.NegativeSign)) != i)
                        {
                            state |= ParseState.Sign;
                            negative = true;
                            i = i2 - 1;
                        }
                        else if (_s[i] == ')' && (state & ParseState.Parenthesis) != 0)
                        {
                            state ^= ParseState.Parenthesis;
                        }
                        else
                        {
                            break;
                        }
                    }

                    i++;
                }

                if (i != _s.Length || (state & ParseState.Parenthesis) != 0)
                {
                    return false;
                }

                if ((state & ParseState.Infinity) != 0)
                {
                    result = negative ? NegativeInfinity : PositiveInfinity;
                    return true;
                }
                if ((state & ParseState.NaN) != 0)
                {
                    result = QNaN;
                    return true;
                }

                ulong sigHigh = 0;
                ulong sigLow = 0;
                if (digits.Count != 0 && digits.Count < 17)
                {
                    var digitIndex = 0;
                    sigLow = digits[digitIndex++];
                    for (; digitIndex < digits.Count; digitIndex++)
                    {
                        sigLow *= 10;
                        sigLow += digits[digitIndex];
                    }
                }
                else if (digits.Count != 0)
                {
                    var digitIndex = 0;
                    sigHigh = digits[digitIndex++];
                    for (; digitIndex < digits.Count - 17; digitIndex++)
                    {
                        sigHigh *= 10;
                        sigHigh += digits[digitIndex];
                    }

                    sigLow = digits[digitIndex++];
                    for (; digitIndex < digits.Count; digitIndex++)
                    {
                        sigLow *= 10;
                        sigLow += digits[digitIndex];
                    }
                }

                ulong newSigHigh;
                ulong newSigLow;
                Multiply(sigHigh, 100000000000000000ul, out newSigHigh, out newSigLow);

                newSigLow += sigLow;

                if (newSigLow < sigLow)
                {
                    newSigHigh++;
                }

                uint[] bits = new uint[4];
                bits[0] = (uint)(newSigHigh >> 32);
                bits[1] = (uint)newSigHigh;
                bits[2] = (uint)(newSigLow >> 32);
                bits[3] = (uint)newSigLow;

                short exponent = (short)scale;
                byte flags = 0;
                if (negative)
                {
                    flags |= 0x80;
                }

                result = new Decimal128(flags, bits, exponent);
                return true;
            }

            private static int MatchChars(string s, int i, string match)
            {
                int sIndex = i;
                int matchIndex = 0;
                while (matchIndex < match.Length)
                {
                    if (s[sIndex] != match[matchIndex] && (match[matchIndex] != '\u00a0' || s[sIndex] != ' '))
                    {
                        return i;
                    }

                    sIndex++;
                    matchIndex++;
                }

                return sIndex;
            }

            private static void Multiply(ulong left, ulong right, out ulong high, out ulong low)
            {
                if (left == 0 && right == 0)
                {
                    high = 0;
                    low = 0;
                    return;
                }


                ulong leftHigh = left >> 32;
                ulong leftLow = (uint)left;
                ulong rightHigh = right >> 32;
                ulong rightLow = (uint)right;

                ulong productHigh = leftHigh * rightHigh;
                ulong productHighMid = leftHigh * rightLow;
                ulong productLowMid = leftLow * rightHigh;
                ulong productLow = leftLow * rightLow;

                productHigh += productHighMid >> 32;
                productHighMid = (uint)productHighMid + productLowMid + (productLow >> 32);

                productHigh += productHighMid >> 32;
                productLow = (productHighMid << 32) + (uint)productLow;

                high = productHigh;
                low = productLow;
            }
        }
    }


}

