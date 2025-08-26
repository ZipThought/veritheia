#!/bin/bash

# Build Configuration Validation Script
# Ensures that production projects never include test-only compilation symbols
# This script must be run as part of the CI/CD pipeline before production builds

set -e

echo "=== Veritheia Build Configuration Validation ==="

# Check for TEST_BUILD in production projects
PRODUCTION_PROJECTS=(
    "veritheia.ApiService"
    "veritheia.Web"
    "veritheia.Data"
    "veritheia.Core"
    "veritheia.Common"
    "veritheia.ServiceDefaults"
    "veritheia.AppHost"
)

# Validate each production project
for project in "${PRODUCTION_PROJECTS[@]}"; do
    if [ -d "$project" ]; then
        echo "Validating $project..."
        
        # Check .csproj file for TEST_BUILD
        if grep -q "TEST_BUILD" "$project"/*.csproj 2>/dev/null; then
            echo "❌ CRITICAL BUILD ERROR: TEST_BUILD found in production project $project"
            echo "   This would enable test doubles in production, violating data integrity requirements."
            exit 1
        fi
        
        # Check source files for TEST_BUILD conditional compilation directives
        FOUND_FILES=$(find "$project" -name "*.cs" -exec grep -l "#if TEST_BUILD\|#ifdef TEST_BUILD\|#ifndef TEST_BUILD" {} \; 2>/dev/null | grep -v ".Designer.cs" || true)
        if [ ! -z "$FOUND_FILES" ]; then
            echo "❌ CRITICAL BUILD ERROR: TEST_BUILD conditional compilation found in production project $project"
            echo "   Files containing TEST_BUILD directives:"
            echo "$FOUND_FILES" | sed 's/^/     /'
            echo "   Test-only code paths must never be accessible in production builds."
            exit 1
        fi
        
        echo "✅ $project: Build configuration valid"
    else
        echo "⚠️  Warning: Project directory $project not found"
    fi
done

# Validate that test project correctly defines TEST_BUILD
if [ -d "veritheia.Tests" ]; then
    if ! grep -q "TEST_BUILD" veritheia.Tests/veritheia.Tests.csproj; then
        echo "❌ BUILD ERROR: TEST_BUILD not defined in test project"
        echo "   Test project must define TEST_BUILD to enable test doubles."
        exit 1
    fi
    echo "✅ veritheia.Tests: Test compilation symbol correctly defined"
else
    echo "⚠️  Warning: Test project not found"
fi

# Validate configuration files don't have test settings in production
echo "Validating configuration files..."

# Check for Testing section in production appsettings
for config in veritheia.Web/appsettings.json veritheia.ApiService/appsettings.json; do
    if [ -f "$config" ]; then
        if grep -q '"Testing"' "$config"; then
            echo "❌ CRITICAL CONFIG ERROR: Testing section found in $config"
            echo "   Production configuration files must not contain Testing sections."
            exit 1
        fi
        echo "✅ $config: No test configuration found"
    fi
done

echo "=== Build Configuration Validation Complete ==="
echo "✅ All production projects are configured correctly"
echo "✅ No test doubles accessible in production builds"
echo "✅ Data integrity requirements maintained"