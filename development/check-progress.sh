#!/bin/bash
# Veritheia Progress Checker
# Run this script to quickly see implementation status

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

# Check phase status
echo "Phase Status:"
grep -E "^### Phase [0-9]+:|^\*\*Status\*\*:" development/PROGRESS.md | paste - - | while IFS=$'\t' read -r phase status; do
    phase_name=$(echo "$phase" | sed 's/### //')
    status_value=$(echo "$status" | sed 's/\*\*Status\*\*: //')
    printf "%-40s %s\n" "$phase_name" "$status_value"
done
echo ""

# Check for TODO comments
echo "Outstanding TODOs in Code:"
grep -r "TODO" --include="*.cs" . 2>/dev/null | grep -v "/obj/" | grep -v "/bin/" | head -10
if [ $(grep -r "TODO" --include="*.cs" . 2>/dev/null | grep -v "/obj/" | grep -v "/bin/" | wc -l) -gt 10 ]; then
    echo "... and $(( $(grep -r "TODO" --include="*.cs" . 2>/dev/null | grep -v "/obj/" | grep -v "/bin/" | wc -l) - 10 )) more TODOs"
fi
echo ""

# Check test status
echo "Test Status:"
if command -v dotnet &> /dev/null; then
    echo "Running tests..."
    dotnet test --no-build --nologo -v q 2>/dev/null || echo "No tests found or build required"
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
    psql -U postgres -h localhost -c "SELECT version();" 2>/dev/null || echo "PostgreSQL not accessible"
else
    echo "psql not found"
fi
echo ""

# Recent progress notes
echo "Recent Progress Notes:"
grep -A 3 "^- Note:" development/PROGRESS.md | tail -15
echo ""

echo "=== End of Progress Check ==="