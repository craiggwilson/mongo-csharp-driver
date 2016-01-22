using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON Decimal value.
    /// </summary>
    /// <seealso cref="MongoDB.Bson.BsonValue" />
    public class BsonDecimal : BsonValue, IComparable<BsonDecimal>, IEquatable<BsonDecimal>
    {
        private readonly Decimal128 _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDecimal" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonDecimal(Decimal128 value)
        {
            _value = value;
        }

        /// <inheritdoc />
        public override BsonType BsonType => BsonType.Decimal;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public Decimal128 Value
        {
            get { return _value; }
        }

        /// <inheritdoc />
        public int CompareTo(BsonDecimal other)
        {
            if (other == null) { return 1; }
            return _value.CompareTo(other._value);
        }

        /// <inheritdoc />
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherDecimal = other as BsonDecimal;
            if (otherDecimal != null)
            {
                return CompareTo(otherDecimal);
            }
            return CompareTypeTo(other);
        }

        /// <inheritdoc />
        public bool Equals(BsonDecimal other)
        {
            if (other == null)
            {
                return false;
            }

            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonDecimal);
        }

        /// <inheritdoc />
        public override int GetHashCode() => _value.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => _value.ToString();

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Byte"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BsonDecimal(byte value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.SByte"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator BsonDecimal(sbyte value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int16"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BsonDecimal(short value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt16"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator BsonDecimal(ushort value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BsonDecimal(int value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt32"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator BsonDecimal(uint value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int64"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BsonDecimal(long value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator BsonDecimal(ulong value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Decimal"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BsonDecimal(decimal value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Decimal128"/> to <see cref="BsonDecimal"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator BsonDecimal(Decimal128 value)
        {
            return new BsonDecimal(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.Byte"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator byte(BsonDecimal value)
        {
            return (byte)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.SByte"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator sbyte(BsonDecimal value)
        {
            return (sbyte)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator short(BsonDecimal value)
        {
            return (short)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.UInt16"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator ushort(BsonDecimal value)
        {
            return (ushort)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int(BsonDecimal value)
        {
            return (int)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.UInt32"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator uint(BsonDecimal value)
        {
            return (uint)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator long(BsonDecimal value)
        {
            return (long)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator ulong(BsonDecimal value)
        {
            return (ulong)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="System.Decimal"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator decimal(BsonDecimal value)
        {
            return (decimal)value._value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="BsonDecimal"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Decimal128(BsonDecimal value)
        {
            return value._value;
        }
    }
}