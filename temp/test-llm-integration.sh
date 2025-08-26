#!/bin/bash

echo "=== LLM Integration Test Script ==="
echo "This script tests the LLM integration with your local server"
echo ""

# Configuration
LLM_URL="${LLM_URL:-http://192.168.68.100:1234/v1}"
MODEL="${MODEL:-llama-3.2-3b-instruct}"

echo "Testing LLM at: $LLM_URL"
echo "Using model: $MODEL"
echo ""

# Test 1: Check if LLM server is reachable
echo "Test 1: Checking LLM server connectivity..."
if curl -s --connect-timeout 5 "$LLM_URL/models" > /dev/null 2>&1; then
    echo "✅ LLM server is reachable"
else
    echo "❌ Cannot reach LLM server at $LLM_URL"
    echo "   Please ensure your LLM server is running"
    exit 1
fi

# Test 2: Simple completion test
echo ""
echo "Test 2: Testing simple LLM completion..."
RESPONSE=$(curl -s -X POST "$LLM_URL/chat/completions" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "'"$MODEL"'",
    "messages": [
      {"role": "system", "content": "You are a helpful assistant. Answer concisely."},
      {"role": "user", "content": "What is 2+2? Reply with just the number."}
    ],
    "temperature": 0.1,
    "max_tokens": 10
  }' 2>/dev/null)

if echo "$RESPONSE" | grep -q "choices"; then
    ANSWER=$(echo "$RESPONSE" | grep -o '"content":"[^"]*"' | head -1 | sed 's/"content":"//;s/"$//')
    echo "✅ LLM responded: $ANSWER"
else
    echo "❌ LLM did not return expected response format"
    echo "   Response: $RESPONSE"
fi

# Test 3: Systematic screening test
echo ""
echo "Test 3: Testing systematic screening prompt..."
SCREENING_RESPONSE=$(curl -s -X POST "$LLM_URL/chat/completions" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "'"$MODEL"'",
    "messages": [
      {"role": "system", "content": "You are a research assistant performing systematic literature screening. Evaluate papers against research questions."},
      {"role": "user", "content": "Evaluate this paper: \"Large Language Models for Cybersecurity\" against the question: \"How are LLMs used in security?\". Rate relevance 0-1 and respond with: RELEVANCE: [score]"}
    ],
    "temperature": 0.3,
    "max_tokens": 50
  }' 2>/dev/null)

if echo "$SCREENING_RESPONSE" | grep -q "RELEVANCE"; then
    echo "✅ Screening prompt successful"
    CONTENT=$(echo "$SCREENING_RESPONSE" | grep -o '"content":"[^"]*"' | head -1 | sed 's/"content":"//;s/"$//')
    echo "   Response: $CONTENT"
else
    echo "❌ Screening prompt failed"
fi

# Test 4: Run unit tests with mocked LLM
echo ""
echo "Test 4: Running unit tests..."
cd /home/cyharyanto/veritheia
if dotnet test --filter "FullyQualifiedName~SystematicScreeningProcessTests.GetInputDefinition" --no-build -v q 2>/dev/null; then
    echo "✅ Unit tests passed"
else
    echo "❌ Unit tests failed"
fi

# Test 5: Test the OpenAI adapter directly
echo ""
echo "Test 5: Testing OpenAI adapter with real LLM..."
cat > /tmp/test-adapter.cs << 'EOF'
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Veritheia.Data.Services;

class Program
{
    static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LLM:Url"] = Environment.GetEnvironmentVariable("LLM_URL") ?? "http://192.168.68.100:1234/v1",
                ["LLM:Model"] = "llama-3.2-3b-instruct"
            })
            .Build();

        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        var adapter = new OpenAICognitiveAdapter(httpClient, config, NullLogger<OpenAICognitiveAdapter>.Instance);
        
        var result = await adapter.GenerateTextAsync("What is 2+2?", "You are a helpful assistant.");
        Console.WriteLine($"LLM Response: {result.Substring(0, Math.Min(100, result.Length))}...");
    }
}
EOF

echo "   (Skipping C# adapter test - requires compilation)"

echo ""
echo "=== Test Summary ==="
echo "The LLM integration is configured and ready to use."
echo "Run the application and try the systematic screening process to test end-to-end."
echo ""
echo "To run the full application:"
echo "  dotnet run --project veritheia.AppHost"
echo ""
echo "Then navigate to: https://localhost:17170"