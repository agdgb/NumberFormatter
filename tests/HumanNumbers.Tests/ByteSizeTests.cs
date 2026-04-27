using System.Globalization;
using HumanNumbers.Bytes;
using Xunit;

namespace HumanNumbers.Tests
{
    public class ByteSizeTests
    {
        [Theory]
        [InlineData(1024, "1.00 KiB")]
        [InlineData(1048576, "1.00 MiB")]
        [InlineData(1073741824, "1.00 GiB")]
        public void ToHumanBytes_By_Default_Uses_Binary_Prefixes(long bytes, string expected)
        {
            var result = bytes.ToHumanBytes(culture: CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1000, "1.00 KB")]
        [InlineData(1000000, "1.00 MB")]
        public void ToHumanBytes_Can_Explicitly_Use_Decimal_Prefixes(long bytes, string expected)
        {
            var result = bytes.ToHumanBytes(useBinaryPrefixes: false, culture: CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToHumanBytes_Handles_Negative_Values()
        {
            var result = (-1024L).ToHumanBytes(culture: CultureInfo.InvariantCulture);
            Assert.Equal("-1.00 KiB", result);
        }

        [Fact]
        public void ToHumanBytes_Handles_Zero()
        {
            var result = 0L.ToHumanBytes();
            Assert.Equal("0 B", result);
        }

        [Fact]
        public void ToHumanBytes_Handles_Rounding_Promotion()
        {
            // 1023.999 bytes rounds to 1024.00 which should promote to 1.00 KiB
            var result = 1023.999.ToHumanBytes(2, true, CultureInfo.InvariantCulture);
            Assert.Equal("1.00 KiB", result);
        }
    }
}
