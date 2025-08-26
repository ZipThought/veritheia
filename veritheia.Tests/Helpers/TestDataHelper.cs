using System.Reflection;
using System.IO;

namespace Veritheia.Tests.Helpers;

/// <summary>
/// Helper class to access embedded test data resources
/// </summary>
public static class TestDataHelper
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Gets CSV sample data as a string
    /// </summary>
    /// <param name="filename">The CSV filename (e.g., "ieee_sample.csv")</param>
    /// <returns>The CSV content as a string</returns>
    public static string GetCsvSample(string filename)
    {
        var resourceName = $"veritheia.Tests.TestData.Csv.{filename}";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Available resources: {string.Join(", ", Assembly.GetManifestResourceNames())}");
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Gets CSV sample data as a Stream
    /// </summary>
    /// <param name="filename">The CSV filename (e.g., "ieee_sample.csv")</param>
    /// <returns>The CSV content as a Stream</returns>
    public static Stream GetCsvSampleStream(string filename)
    {
        var resourceName = $"veritheia.Tests.TestData.Csv.{filename}";
        var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Available resources: {string.Join(", ", Assembly.GetManifestResourceNames())}");
        }
        return stream;
    }

    /// <summary>
    /// Gets research questions from a text file
    /// </summary>
    /// <param name="filename">The research questions filename (e.g., "cybersecurity_llm_rqs.txt")</param>
    /// <returns>Array of research questions</returns>
    public static string[] GetResearchQuestions(string filename)
    {
        var resourceName = $"veritheia.Tests.TestData.ResearchQuestions.{filename}";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Available resources: {string.Join(", ", Assembly.GetManifestResourceNames())}");
        }
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        return content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                     .Select(line => line.Trim())
                     .Where(line => !string.IsNullOrEmpty(line))
                     .ToArray();
    }

    /// <summary>
    /// Gets research questions as a single string (newline-separated)
    /// </summary>
    /// <param name="filename">The research questions filename (e.g., "cybersecurity_llm_rqs.txt")</param>
    /// <returns>Research questions as a single string</returns>
    public static string GetResearchQuestionsText(string filename)
    {
        var resourceName = $"veritheia.Tests.TestData.ResearchQuestions.{filename}";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Available resources: {string.Join(", ", Assembly.GetManifestResourceNames())}");
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }

    /// <summary>
    /// Lists all available embedded resource names for debugging
    /// </summary>
    /// <returns>Array of all embedded resource names</returns>
    public static string[] GetAllResourceNames()
    {
        return Assembly.GetManifestResourceNames();
    }
}
