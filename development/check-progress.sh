#!/bin/bash
# Veritheia Progress Checker
# Run this script to quickly see implementation status
# Must be run from repository root

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Get the repository root (parent of development folder)
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

# Change to repository root for all operations
cd "$REPO_ROOT"

echo "=== Veritheia Progress Check ==="
echo "Date: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# RTFM Reminder
echo "📚 REMINDER: Start with docs/README.md for documentation index"
echo ""

# Check current git branch
echo "Git Status:"
echo "- Branch: $(git branch --show-current)"
echo "- Last commit: $(git log -1 --oneline)"
echo ""

# Check level status
echo "Dependency Graph Status:"
if [ -f "development/PROGRESS.md" ]; then
    grep -E "^### ✅ Level [0-9]+:|^### 🔄 Level [0-9]+:|^### ⏳ Level [0-9]+:" development/PROGRESS.md | while read -r line; do
        level=$(echo "$line" | sed 's/### [✅🔄⏳] Level \([0-9]\+\):.*/\1/')
        status=$(echo "$line" | sed 's/### \([✅🔄⏳]\) Level [0-9]\+:.*/\1/')
        name=$(echo "$line" | sed 's/### [✅🔄⏳] Level [0-9]\+: \(.*\) - .*/\1/')
        printf "Level %-2s %-25s %s\n" "$level" "$name" "$status"
    done
else
    echo "PROGRESS.md not found - check development folder"
fi
echo ""

# Check for TODO comments
echo "Outstanding TODOs in Code:"
grep -r "TODO" --include="*.cs" . 2>/dev/null | grep -v "/obj/" | grep -v "/bin/" | head -10
if [ $(grep -r "TODO" --include="*.cs" . 2>/dev/null | grep -v "/obj/" | grep -v "/bin/" | wc -l) -gt 10 ]; then
    echo "... and $(( $(grep -r "TODO" --include="*.cs" . 2>/dev/null | grep -v "/obj/" | grep -v "/bin/" | wc -l) - 10 )) more TODOs"
fi
echo ""

# Check build status
echo "Build Status:"
if command -v dotnet &> /dev/null; then
    echo "Building main projects..."
    dotnet build --no-restore --nologo -v q 2>/dev/null && echo "✅ Main projects build successfully" || echo "❌ Build errors found"
else
    echo "dotnet CLI not found"
fi
echo ""

# Check test status
echo "Test Status:"
if command -v dotnet &> /dev/null; then
    echo "Building test project..."
    dotnet build veritheia.Tests --no-restore --nologo -v q 2>/dev/null && echo "✅ Tests build successfully" || echo "❌ Test build errors"
else
    echo "dotnet CLI not found"
fi
echo ""

# Check for uncommitted changes
echo "Uncommitted Changes:"
git status --porcelain | head -10
if [ $(git status --porcelain | wc -l) -eq 0 ]; then
    echo "Working directory clean"
fi
echo ""

# Check database status
echo "Database Status:"
if command -v psql &> /dev/null; then
    psql -U postgres -h localhost -c "SELECT version();" 2>/dev/null && echo "✅ PostgreSQL accessible" || echo "❌ PostgreSQL not accessible"
else
    echo "psql not found"
fi
echo ""

# Current level details
echo "Current Level Details:"
if [ -f "development/PROGRESS.md" ]; then
    grep -A 10 "### 🔄 Level [0-9]\+:" development/PROGRESS.md | head -15
fi
echo ""

echo "=== End of Progress Check ==="