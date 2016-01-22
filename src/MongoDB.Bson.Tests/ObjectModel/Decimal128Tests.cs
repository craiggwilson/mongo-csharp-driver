using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using FluentAssertions;
using System.Globalization;

namespace MongoDB.Bson.Tests
{
    public class Decimal128Tests
    {
        [Test]
        public void Default_value()
        {
            var subject = default(Decimal128);

            subject.ToString().Should().Be("0");
            AssertSpecialProperties(subject);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public void GetBits(TestCase tc)
        {
            var subject = new Decimal128(tc.Bits);

            var bits = Decimal128.GetBits(subject);

            bits.Should().BeEquivalentTo(tc.Bits);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public void ToString(TestCase tc)
        {
            var subject = new Decimal128(tc.Bits);

            subject.ToString().Should().Be(tc.String);
        }

        [Test]
        [TestCase("-1.01", "-1.01")]
        [TestCase("-1", "-1")]
        [TestCase("0", "0")]
        [TestCase("1", "1")]
        [TestCase("1.01", "1.01")]
        [TestCase("79228162514264337593543950335", "7.9228162514264337593543950335E28")]
        [TestCase("-79228162514264337593543950335", "-7.9228162514264337593543950335E28")]
        public void Decimal(string valueString, string s)
        {
            var value = decimal.Parse(valueString);
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToDecimal(subject);
            result.Should().Be(value);

            result = (decimal)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase((byte)0, "0")]
        [TestCase((byte)1, "1")]
        [TestCase(byte.MaxValue, "255")]
        public void Byte(byte value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToByte(subject);
            result.Should().Be(value);

            result = (byte)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        [TestCase(byte.MaxValue + 1)]
        [TestCase(byte.MinValue - 1)]
        public void Byte_overflow(int value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToByte(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase((short)-1, "-1")]
        [TestCase((short)0, "0")]
        [TestCase((short)1, "1")]
        [TestCase(short.MaxValue, "32767")]
        [TestCase(short.MinValue, "-32768")]
        public void Int16(short value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToInt16(subject);
            result.Should().Be(value);

            result = (short)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        [TestCase(short.MaxValue + 1)]
        [TestCase(short.MinValue - 1)]
        public void Int16_overflow(int value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToInt16(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase(-1, "-1")]
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(int.MaxValue, "2147483647")]
        [TestCase(int.MinValue, "-2147483648")]
        public void Int32(int value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToInt32(subject);
            result.Should().Be(value);

            result = (int)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(long.MaxValue)]
        [TestCase(long.MinValue)]
        [TestCase((long)int.MaxValue + 1)]
        [TestCase((long)int.MinValue - 1)]
        public void Int32_overflow(long value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToInt32(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase(-1, "-1")]
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(long.MaxValue, "9.223372036854775807E18")]
        [TestCase(long.MinValue, "-9.223372036854775808E18")]
        public void Int64(long value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToInt64(subject);
            result.Should().Be(value);

            result = (long)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(long.MaxValue + 1ul)]
        [TestCase(ulong.MaxValue)]
        public void Int64_overflow(ulong value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToInt64(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase((sbyte)0, "0")]
        [TestCase((sbyte)1, "1")]
        [TestCase(sbyte.MaxValue, "127")]
        public void SByte(sbyte value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToSByte(subject);
            result.Should().Be(value);

            result = (sbyte)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        [TestCase(sbyte.MaxValue + 1)]
        [TestCase(sbyte.MinValue - 1)]
        public void SByte_overflow(int value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToSByte(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase((ushort)0, "0")]
        [TestCase((ushort)1, "1")]
        [TestCase(ushort.MaxValue, "65535")]
        public void UInt16(ushort value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToUInt16(subject);
            result.Should().Be(value);

            result = (ushort)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(ushort.MaxValue + 1L)]
        [TestCase(-1L)]
        public void UInt16_overflow(long value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToUInt16(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase(0u, "0")]
        [TestCase(1u, "1")]
        [TestCase(uint.MaxValue, "4294967295")]
        public void UInt32(uint value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToUInt32(subject);
            result.Should().Be(value);

            result = (uint)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(-1L)]
        public void UInt32_overflow(long value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToUInt32(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        [TestCase(0ul, "0")]
        [TestCase(1ul, "1")]
        [TestCase(ulong.MaxValue, "1.8446744073709551615E19")]
        public void UInt64(ulong value, string s)
        {
            var subject = new Decimal128(value);

            subject.ToString().Should().Be(s);
            AssertSpecialProperties(subject);

            var result = Decimal128.ToUInt64(subject);
            result.Should().Be(value);

            result = (ulong)subject;
            result.Should().Be(value);
        }

        [Test]
        [TestCase(-1L)]
        public void UInt64_overflow(long value)
        {
            var subject = new Decimal128(value);

            Action act = () => Decimal128.ToUInt64(subject);
            act.ShouldThrow<OverflowException>();
        }

        [Test]
        public void NegativeInfinity()
        {
            var subject = Decimal128.NegativeInfinity;

            subject.ToString().Should().Be("-Infinity");
            AssertSpecialProperties(subject, negInfinity: true);
        }

        [Test]
        public void PositiveInfinity()
        {
            var subject = Decimal128.PositiveInfinity;

            subject.ToString().Should().Be("Infinity");
            AssertSpecialProperties(subject, posInfinity: true);
        }

        [Test]
        public void QNaN()
        {
            var subject = Decimal128.QNaN;

            subject.ToString().Should().Be("NaN");
            AssertSpecialProperties(subject, qNaN: true);
        }

        [Test]
        public void SNaN()
        {
            var subject = Decimal128.SNaN;

            subject.ToString().Should().Be("NaN");
            AssertSpecialProperties(subject, sNaN: true);
        }

        [Test]
        [TestCaseSource("ParsingTestCases")]
        public void TryParse(TestCase tc)
        {
            var subject = Decimal128.Parse(tc.String);

            var bits = Decimal128.GetBits(subject);

            bits.Should().BeEquivalentTo(tc.Bits);
        }

        private void AssertSpecialProperties(Decimal128 subject, bool qNaN = false, bool sNaN = false, bool posInfinity = false, bool negInfinity = false)
        {
            Decimal128.IsNaN(subject).Should().Be(qNaN || sNaN);
            Decimal128.IsQNaN(subject).Should().Be(qNaN);
            Decimal128.IsSNaN(subject).Should().Be(sNaN);
            Decimal128.IsInfinity(subject).Should().Be(posInfinity || negInfinity);
            Decimal128.IsNegativeInfinity(subject).Should().Be(negInfinity);
            Decimal128.IsPositiveInfinity(subject).Should().Be(posInfinity);
        }

        public class TestCase
        {
            public uint[] Bits;
            public string String;
            public bool CanRoundTripString;

            public TestCase(uint high, uint highMid, uint lowMid, uint low, string s, bool canRoundTripString = true)
            {
                Bits = new uint[] { high, highMid, lowMid, low };
                String = s;
                CanRoundTripString = canRoundTripString;
            }
        }

        private IEnumerable<TestCase> ParsingTestCases
        {
            get { return TestCases.Where(x => x.CanRoundTripString); }
        }

        private IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase(0x78000000, 0, 0, 0, "Infinity");
                yield return new TestCase(0xF8000000, 0, 0, 0, "-Infinity");
                yield return new TestCase(0x7C000000, 0, 0, 0, "NaN");
                yield return new TestCase(0xFC000000, 0, 0, 0, "NaN", canRoundTripString: false);
                yield return new TestCase(0x7E000000, 0, 0, 0, "NaN", canRoundTripString: false);
                yield return new TestCase(0xFE000000, 0, 0, 0, "NaN", canRoundTripString: false);
                yield return new TestCase(0x7C000000, 0, 0, 0, "NaN", canRoundTripString: false);
                yield return new TestCase(0x7C000000, 0, 0, 12, "NaN", canRoundTripString: false);
                yield return new TestCase(0x30400000, 0, 0, 1, "1");
                yield return new TestCase(0x30400000, 0, 0, 0, "0");
                yield return new TestCase(0x30400000, 0, 0, 2, "2");
                yield return new TestCase(0xB0400000, 0, 0, 1, "-1");
                yield return new TestCase(0xB0400000, 0, 0, 0, "-0");
                yield return new TestCase(0x303e0000, 0, 0, 1, "0.1");
                yield return new TestCase(0xB03e0000, 0, 0, 1, "-0.1");
                yield return new TestCase(0x30340000, 0, 0, 0x4D2, "0.001234");
                yield return new TestCase(0x30400000, 0, 0x1C, 0xBE991A14, "123456789012");
                yield return new TestCase(0x302A0000, 0, 0, 0x75AEF40, "0.00123400000");
                yield return new TestCase(0x2FFC3CDE, 0x6FFF9732, 0xDE825CD0, 0x7E96AFF2, "0.1234567890123456789012345678901234");
            }
        }
    }
}