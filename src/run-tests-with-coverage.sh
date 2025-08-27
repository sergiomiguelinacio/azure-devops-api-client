#!/bin/bash

# Azure DevOps API - Test Coverage Script
# This script runs unit tests with code coverage and generates reports

set -e

echo "🧪 Azure DevOps API - Running Tests with Coverage"
echo "=================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_PROJECT="$PROJECT_DIR/AzureDevopsApi.UnitTests/AzureDevopsApi.UnitTests.csproj"
COVERAGE_DIR="$PROJECT_DIR/TestResults/Coverage"
REPORTS_DIR="$PROJECT_DIR/TestResults/Reports"

echo -e "${BLUE}📁 Project Directory: $PROJECT_DIR${NC}"
echo -e "${BLUE}🧪 Test Project: $TEST_PROJECT${NC}"
echo -e "${BLUE}📊 Coverage Directory: $COVERAGE_DIR${NC}"
echo -e "${BLUE}📋 Reports Directory: $REPORTS_DIR${NC}"
echo ""

# Clean previous results
echo -e "${YELLOW}🧹 Cleaning previous test results...${NC}"
rm -rf "$PROJECT_DIR/TestResults"
mkdir -p "$COVERAGE_DIR"
mkdir -p "$REPORTS_DIR"

# Restore packages
echo -e "${BLUE}📦 Restoring NuGet packages...${NC}"
dotnet restore "$TEST_PROJECT"

# Build the solution
echo -e "${BLUE}🔨 Building solution...${NC}"
dotnet build "$TEST_PROJECT" --configuration Release --no-restore

# Run tests with coverage
echo -e "${GREEN}🧪 Running unit tests with coverage...${NC}"
dotnet test "$TEST_PROJECT" \
    --configuration Release \
    --no-build \
    --verbosity normal \
    --collect:"XPlat Code Coverage" \
    --results-directory "$COVERAGE_DIR" \
    --settings "$PROJECT_DIR/AzureDevopsApi.UnitTests/coverlet.runsettings" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Find the coverage file
COVERAGE_FILE=$(find "$COVERAGE_DIR" -name "coverage.opencover.xml" | head -1)

if [ -z "$COVERAGE_FILE" ]; then
    echo -e "${RED}❌ Coverage file not found!${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Coverage file found: $COVERAGE_FILE${NC}"

# Generate HTML report
echo -e "${BLUE}📊 Generating HTML coverage report...${NC}"
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.3.10 || true

reportgenerator \
    -reports:"$COVERAGE_FILE" \
    -targetdir:"$REPORTS_DIR/html" \
    -reporttypes:"Html;HtmlSummary;Badges;TextSummary" \
    -verbosity:Info \
    -title:"Azure DevOps API - Code Coverage Report" \
    -tag:"$(date +%Y%m%d_%H%M%S)"

# Generate Cucumber-style report
echo -e "${BLUE}🥒 Generating Cucumber-style test report...${NC}"
cat > "$REPORTS_DIR/cucumber-style-report.md" << EOF
# 🥒 Azure DevOps API - Test Execution Report

**Generated on:** $(date '+%Y-%m-%d %H:%M:%S')
**Environment:** $(uname -s) $(uname -r)
**Test Framework:** xUnit.net
**Coverage Tool:** Coverlet

## 📊 Test Summary

EOF

# Extract test results and add to report
echo "### ✅ Test Results" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "" >> "$REPORTS_DIR/cucumber-style-report.md"

# Generate coverage summary
echo -e "${BLUE}📈 Generating coverage summary...${NC}"
COVERAGE_SUMMARY=$(reportgenerator -reports:"$COVERAGE_FILE" -targetdir:"$REPORTS_DIR/temp" -reporttypes:"TextSummary" -verbosity:Error)

# Extract coverage percentage
COVERAGE_PERCENT=$(grep -o "Line coverage: [0-9.]*%" "$REPORTS_DIR/temp/Summary.txt" | grep -o "[0-9.]*" || echo "0")

echo "### 📊 Code Coverage" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "- **Line Coverage:** ${COVERAGE_PERCENT}%" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "- **Target Coverage:** 80%" >> "$REPORTS_DIR/cucumber-style-report.md"

if (( $(echo "$COVERAGE_PERCENT >= 80" | bc -l) )); then
    echo "- **Status:** ✅ **PASSED** - Coverage target met!" >> "$REPORTS_DIR/cucumber-style-report.md"
    COVERAGE_STATUS="PASSED"
else
    echo "- **Status:** ❌ **FAILED** - Coverage below target!" >> "$REPORTS_DIR/cucumber-style-report.md"
    COVERAGE_STATUS="FAILED"
fi

echo "" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "### 📁 Report Files" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "- [📊 HTML Coverage Report]($REPORTS_DIR/html/index.html)" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "- [📋 Coverage XML]($COVERAGE_FILE)" >> "$REPORTS_DIR/cucumber-style-report.md"
echo "- [🥒 This Report]($REPORTS_DIR/cucumber-style-report.md)" >> "$REPORTS_DIR/cucumber-style-report.md"

# Clean temp directory
rm -rf "$REPORTS_DIR/temp"

# Final summary
echo ""
echo "=================================================="
echo -e "${GREEN}🎉 Test execution completed!${NC}"
echo ""
echo -e "${BLUE}📊 Coverage: ${COVERAGE_PERCENT}%${NC}"
echo -e "${BLUE}🎯 Target: 80%${NC}"

if [ "$COVERAGE_STATUS" = "PASSED" ]; then
    echo -e "${GREEN}✅ Coverage target MET!${NC}"
    EXIT_CODE=0
else
    echo -e "${RED}❌ Coverage target NOT met!${NC}"
    EXIT_CODE=1
fi

echo ""
echo -e "${BLUE}📁 Reports available at:${NC}"
echo -e "   📊 HTML: $REPORTS_DIR/html/index.html"
echo -e "   🥒 Summary: $REPORTS_DIR/cucumber-style-report.md"
echo ""

exit $EXIT_CODE
