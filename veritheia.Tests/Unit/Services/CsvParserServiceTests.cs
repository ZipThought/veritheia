using Microsoft.Extensions.Logging;
using Moq;
using Veritheia.Data.Services;
using Veritheia.Tests.Helpers;

namespace Veritheia.Tests.Unit.Services;

/// <summary>
/// Unit tests for CsvParserService - these run in CI
/// </summary>
[Trait("Category", "Unit")]
public class CsvParserServiceTests
{
    private readonly Mock<ILogger<CsvParserService>> _mockLogger;
    private readonly CsvParserService _csvParserService;

    public CsvParserServiceTests()
    {
        _mockLogger = new Mock<ILogger<CsvParserService>>();
        _csvParserService = new CsvParserService(_mockLogger.Object);
    }

    [Fact]
    public void ParseCsv_WithIeeeSample_ShouldDetectIeeeFormat()
    {
        // Arrange
        using var csvStream = TestDataHelper.GetCsvSampleStream("ieee_sample.csv");

        // Act
        var result = _csvParserService.ParseCsv(csvStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(5, result.Count); // IEEE sample has 5 articles
        
        // Verify first article
        var firstArticle = result[0];
        Assert.Equal("LLM-Driven SAT Impact on Phishing Defense: A Cross-Sectional Analysis", firstArticle.Title);
        Assert.Equal("H. İŞ", firstArticle.Authors);
        Assert.Equal(2024, firstArticle.Year);
        Assert.Contains("phishing threats", firstArticle.Abstract);
    }

    [Fact]
    public void ParseCsv_WithScopusSample_ShouldDetectScopusFormat()
    {
        // Arrange
        using var csvStream = TestDataHelper.GetCsvSampleStream("scopus_sample.csv");

        // Act
        var result = _csvParserService.ParseCsv(csvStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(6, result.Count); // Scopus sample has 6 articles
        
        // Verify first article
        var firstArticle = result[0];
        Assert.Equal("Acceptability of artificial intelligence (AI)-led chatbot services in healthcare: A mixed-methods study", firstArticle.Title);
        Assert.Equal("Nadarzynski T.; Miles O.; Cowie A.; Ridge D.", firstArticle.Authors);
        Assert.Equal(2019, firstArticle.Year);
        Assert.Contains("Artificial intelligence", firstArticle.Abstract);
    }

    [Fact]
    public void ParseCsv_WithMalformedData_ShouldHandleErrors()
    {
        // Arrange
        using var csvStream = TestDataHelper.GetCsvSampleStream("malformed_sample.csv");

        // Act
        var result = _csvParserService.ParseCsv(csvStream);

        // Assert
        Assert.NotNull(result);
        // Should parse valid rows and skip malformed ones
        Assert.True(result.Count >= 2); // At least the valid rows should be parsed
        
        // Verify that logger was called for warnings/errors
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void ParseCsv_WithEmptyStream_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyContent = "";
        using var emptyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(emptyContent));

        // Act & Assert - Should handle empty stream gracefully
        var exception = Assert.Throws<CsvHelper.ReaderException>(() => _csvParserService.ParseCsv(emptyStream));
        Assert.Contains("No header record was found", exception.Message);
    }
}
