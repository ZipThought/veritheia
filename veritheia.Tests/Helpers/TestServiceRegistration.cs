using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veritheia.Core.Interfaces;
using Veritheia.Data.Services;
using System;
using System.Linq;

namespace Veritheia.Tests.Helpers;

/// <summary>
/// Test-specific service registration helper
/// This is ONLY for tests and should NEVER be used in production
/// </summary>
public static class TestServiceRegistration
{
    /// <summary>
    /// Register cognitive adapter for tests based on configuration and environment
    /// </summary>
    public static void RegisterTestCognitiveAdapter(IServiceCollection services, IConfiguration configuration)
    {
        var useTestAdapter = configuration.GetValue<bool>("Testing:UseTestCognitiveAdapter", false);

        // In CI environment, always use test adapter (no real LLM available)
        if (IsRunningInCI())
        {
            useTestAdapter = true;
        }

        if (useTestAdapter)
        {
            // Use test adapter for mocked LLM responses
            services.AddSingleton<ICognitiveAdapter, TestCognitiveAdapter>();
            Console.WriteLine("TEST: Using TestCognitiveAdapter (mocked LLM)");
        }
        else
        {
            // Use real LLM adapter
            services.AddHttpClient<OpenAICognitiveAdapter>();
            services.AddScoped<ICognitiveAdapter, OpenAICognitiveAdapter>();

            var llmUrl = configuration["LLM:Url"] ?? "http://localhost:1234/v1";
            Console.WriteLine($"TEST: Using real LLM at {llmUrl}");
        }
    }

    /// <summary>
    /// Check if running in CI environment
    /// This check should ONLY exist in test code, NEVER in production
    /// </summary>
    private static bool IsRunningInCI()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITLAB_CI"));
    }
}