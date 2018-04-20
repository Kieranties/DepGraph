using DepGraph.Serialization;
using System;
using Xunit;
using FluentAssertions;
using System.IO;
using NuGet.Common;
using NSubstitute;

namespace DepGraph.Tests.Serialization
{
    public class DefaultLockFileReaderFixture
    {
        private readonly DefaultLockFileReader _sut;

        public DefaultLockFileReaderFixture()
        {
            _sut = new DefaultLockFileReader();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("  \t")]
        public void Read_InvalidPath_Throws(string path)
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            Action action = () => _sut.Read(path, logger);

            // Act / Assert
            action.Should()
                .Throw<FileNotFoundException>()
                .And.Message.Should().Be($"Could not find file '{path}'");
        }

        [Fact]
        public void Read_NullLogger_Throws()
        {
            // Arrange
            Action action = () => _sut.Read("./sample.assets.json", null);

            // Act / Assert
            action.Should()
                .Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");
        }

        [Fact]
        public void Read_ValidValues_Returns()
        {
            // Arrange
            var logger = NullLogger.Instance;
            var path = "./sample.assets.json";

            // Act
            var result = _sut.Read(path, logger);

            // Assert
            result.Should().NotBeNull();
            result.Libraries.Count.Should().Be(16);
        }
    }
}
