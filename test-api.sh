#!/bin/bash

# Veritheia API Test Script
# Tests all available endpoints with curl

API_URL="http://localhost:5000/api"

echo "========================================"
echo "Veritheia API Endpoint Test"
echo "========================================"
echo ""

# Health Check
echo "1. Health Check"
echo "---------------"
curl -s "$API_URL/health" | jq '.'
echo ""

# Users
echo "2. Create User"
echo "--------------"
USER_RESPONSE=$(curl -s -X POST "$API_URL/users" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "researcher@example.com",
    "displayName": "Research User"
  }')
echo "$USER_RESPONSE" | jq '.'
USER_ID=$(echo "$USER_RESPONSE" | jq -r '.id')
echo ""

echo "3. List Users"
echo "-------------"
curl -s "$API_URL/users" | jq '.'
echo ""

# Personas
echo "4. Create Persona"
echo "-----------------"
PERSONA_RESPONSE=$(curl -s -X POST "$API_URL/personas" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Academic Researcher",
    "perspective": "Systematic literature review focused on AI in education",
    "vocabulary": ["epistemic", "sovereignty", "grounded", "contextualized"],
    "patterns": ["evidence-based", "peer-reviewed"],
    "context": "Educational technology research"
  }')
echo "$PERSONA_RESPONSE" | jq '.'
PERSONA_ID=$(echo "$PERSONA_RESPONSE" | jq -r '.id')
echo ""

echo "5. List Personas"
echo "----------------"
curl -s "$API_URL/personas" | jq '.'
echo ""

# Journeys
echo "6. Create Journey"
echo "-----------------"
JOURNEY_RESPONSE=$(curl -s -X POST "$API_URL/journeys" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"personaId\": \"$PERSONA_ID\",
    \"purpose\": \"Literature review on AI-assisted education\"
  }")
echo "$JOURNEY_RESPONSE" | jq '.'
JOURNEY_ID=$(echo "$JOURNEY_RESPONSE" | jq -r '.id')
echo ""

echo "7. Get Journey Details"
echo "----------------------"
curl -s "$API_URL/journeys/$JOURNEY_ID" | jq '.'
echo ""

echo "8. Get Active Journeys for User"
echo "--------------------------------"
curl -s "$API_URL/journeys/user/$USER_ID/active" | jq '.'
echo ""

# Documents
echo "9. List Documents"
echo "-----------------"
curl -s "$API_URL/documents" | jq '.'
echo ""

echo "10. Search Documents"
echo "--------------------"
curl -s "$API_URL/search?query=education&limit=10" | jq '.'
echo ""

# Processes
echo "11. List Available Processes"
echo "-----------------------------"
curl -s "$API_URL/processes" | jq '.'
echo ""

echo "12. Get Process Details"
echo "------------------------"
curl -s "$API_URL/processes/BasicSystematicScreeningProcess" | jq '.'
echo ""

# Journals
echo "13. Get Journey Journal Entries"
echo "--------------------------------"
curl -s "$API_URL/journals/journey/$JOURNEY_ID" | jq '.'
echo ""

# Scopes (Journey Document Projections)
echo "14. Get Journey Scopes"
echo "-----------------------"
curl -s "$API_URL/scopes/journey/$JOURNEY_ID" | jq '.'
echo ""

echo ""
echo "========================================"
echo "API Test Complete"
echo "========================================"
echo ""
echo "Summary:"
echo "- API Base URL: $API_URL"
echo "- User ID: $USER_ID"
echo "- Persona ID: $PERSONA_ID"  
echo "- Journey ID: $JOURNEY_ID"
echo ""
echo "Available endpoints tested:"
echo "  GET  /api/health"
echo "  POST /api/users"
echo "  GET  /api/users"
echo "  POST /api/personas"
echo "  GET  /api/personas"
echo "  POST /api/journeys"
echo "  GET  /api/journeys/{id}"
echo "  GET  /api/journeys/user/{userId}/active"
echo "  POST /api/journeys/{id}/resume"
echo "  PUT  /api/journeys/{id}/state"
echo "  POST /api/journeys/{id}/archive"
echo "  GET  /api/documents"
echo "  POST /api/documents (file upload)"
echo "  GET  /api/documents/{id}"
echo "  GET  /api/search"
echo "  GET  /api/processes"
echo "  GET  /api/processes/{name}"
echo "  POST /api/processes/execute"
echo "  GET  /api/journals/journey/{journeyId}"
echo "  GET  /api/scopes/journey/{journeyId}"
echo ""