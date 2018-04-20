using DepGraph.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Nuget = NuGet.Common;

namespace DepGraph.Tests.Logging
{
    public class NugetLoggerWrapperFixture
    {
        private readonly ILogger _logger;
        private readonly NugetLoggerWrapper _sut;

        public static IEnumerable<object[]> LogLevelMap()
        {
            yield return new object[] { Nuget.LogLevel.Debug, LogLevel.Debug };
            yield return new object[] { Nuget.LogLevel.Error, LogLevel.Error };
            yield return new object[] { Nuget.LogLevel.Information, LogLevel.Information };
            yield return new object[] { Nuget.LogLevel.Minimal, LogLevel.Information };
            yield return new object[] { Nuget.LogLevel.Verbose, LogLevel.Trace };
            yield return new object[] { Nuget.LogLevel.Debug, LogLevel.Debug };
        }

        public static IEnumerable<object[]> Messages()
        {
            yield return new object[] { string.Empty };
            yield return new object[] { "   \t\t" };
            yield return new object[] { "example" };
            yield return new object[] { new Random().Next().ToString() };
        }

        public NugetLoggerWrapperFixture()
        {
            _logger = Substitute.For<ILogger>();
            _sut = new NugetLoggerWrapper(_logger);
        }

        [Fact]
        public void Ctor_NullLogger_Throws()
        {
            // Arrange
            Action action = () => new NugetLoggerWrapper(null);

            // Act / Assert
            action.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");
        }

        [Theory]
        [MemberData(nameof(LogLevelMap))]
        public void Log_IsHandled(Nuget.LogLevel nugetLevel, LogLevel logLevel)
        {
            // Arrange
            var message = "example";

            // Act
            _sut.LogAsync(nugetLevel, message);

            // Assert
            AssertLogMessage(message, logLevel);
        }

        [Theory]
        [MemberData(nameof(LogLevelMap))]
        public void Log_ByMessage_IsHandled(Nuget.LogLevel nugetLevel, LogLevel logLevel)
        {
            // Arrange
            var message = Substitute.For<Nuget.ILogMessage>();
            message.Level.Returns(nugetLevel);

            // Act
            _sut.LogAsync(message);

            // Assert
            AssertLogMessage(message.Message, logLevel);
        }

        [Theory]
        [MemberData(nameof(LogLevelMap))]
        public async Task LogAsync_IsHandled(Nuget.LogLevel nugetLevel, LogLevel logLevel)
        {
            // Arrange
            var message = "example";

            // Act
            await _sut.LogAsync(nugetLevel, message);

            // Assert
            AssertLogMessage(message, logLevel);
        }

        [Theory]
        [MemberData(nameof(LogLevelMap))]
        public async Task LogAsync_ByMessage_IsHandled(Nuget.LogLevel nugetLevel, LogLevel logLevel)
        {
            // Arrange
            var message = Substitute.For<Nuget.ILogMessage>();
            message.Level.Returns(nugetLevel);

            // Act
            await _sut.LogAsync(message);

            // Assert
            AssertLogMessage(message.Message, logLevel);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogDebug_IsHandled(string message)
        {
            // Act
            _sut.LogDebug(message);

            // Assert
            AssertLogMessage(message, LogLevel.Debug);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogError_IsHandled(string message)
        {
            // Act
            _sut.LogError(message);

            // Assert
            AssertLogMessage(message, LogLevel.Error);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogInformation_IsHandled(string message)
        {
            // Act
            _sut.LogInformation(message);

            // Assert
            AssertLogMessage(message, LogLevel.Information);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogInformationSummary_IsHandled(string message)
        {
            // Act
            _sut.LogInformationSummary(message);

            // Assert
            AssertLogMessage(message, LogLevel.Information);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogMinimal_IsHandled(string message)
        {
            // Act
            _sut.LogMinimal(message);

            // Assert
            AssertLogMessage(message, LogLevel.Information);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogVerbose_IsHandled(string message)
        {
            // Act
            _sut.LogVerbose(message);

            // Assert
            AssertLogMessage(message, LogLevel.Trace);
        }

        [Theory]
        [MemberData(nameof(Messages))]
        public void LogWarning_IsHandled(string message)
        {
            // Act
            _sut.LogWarning(message);

            // Assert
            AssertLogMessage(message, LogLevel.Warning);
        }

        private void AssertLogMessage(string message, LogLevel level)
        {
            _logger.Received(1)
                .Log(
                    level,
                    Arg.Any<EventId>(),
                    Arg.Is<FormattedLogValues>(x => x.ToString() == message),
                    null,
                    Arg.Any<Func<FormattedLogValues, Exception, string>>()
                );
        }
    }
}
