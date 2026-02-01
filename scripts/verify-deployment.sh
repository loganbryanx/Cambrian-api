#!/bin/bash

# Cambrian API Deployment Verification Script
# This script verifies that your Render API deployment is working correctly
# and that CORS is configured for your Vercel frontend.

# CONFIGURATION - Update these with your actual URLs
RENDER_API_URL="https://YOUR-RENDER-API.onrender.com"
VERCEL_URL="https://your-app.vercel.app"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "Cambrian API Deployment Verification"
echo "=========================================="
echo ""

# Check if URLs are configured
if [[ $RENDER_API_URL == *"YOUR-RENDER-API"* ]]; then
  echo -e "${RED}❌ Error: Please update RENDER_API_URL in this script${NC}"
  echo "   Edit this file and replace YOUR-RENDER-API with your actual Render service name"
  exit 1
fi

if [[ $VERCEL_URL == *"your-app"* ]]; then
  echo -e "${YELLOW}⚠️  Warning: VERCEL_URL appears to be a placeholder${NC}"
  echo "   Make sure to update it with your actual Vercel URL if you have CORS issues"
  echo ""
fi

echo "Configuration:"
echo "  Render API: $RENDER_API_URL"
echo "  Vercel URL: $VERCEL_URL"
echo ""

# Test 1: Health Check
echo "----------------------------------------"
echo "Test 1: Health Check"
echo "----------------------------------------"
echo "Testing: $RENDER_API_URL/auth/health"
echo ""

HEALTH_RESPONSE=$(curl -s -w "\n%{http_code}" "$RENDER_API_URL/auth/health" 2>&1)
HTTP_CODE=$(echo "$HEALTH_RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$HEALTH_RESPONSE" | head -n-1)

if [[ $HTTP_CODE == "200" ]] && [[ $RESPONSE_BODY == *"ok"* ]]; then
  echo -e "${GREEN}✅ Health check passed${NC}"
  echo "   Response: $RESPONSE_BODY"
  echo "   Status: $HTTP_CODE"
  HEALTH_PASS=1
else
  echo -e "${RED}❌ Health check failed${NC}"
  echo "   Response: $RESPONSE_BODY"
  echo "   Status: $HTTP_CODE"
  echo ""
  echo "   Troubleshooting:"
  echo "   - Verify service is running in Render Dashboard"
  echo "   - Check Render logs for errors"
  echo "   - Ensure URL is correct (check Render Dashboard)"
  echo "   - If free tier, first request may take 30-60 seconds"
  HEALTH_PASS=0
fi
echo ""

# Test 2: CORS Preflight
echo "----------------------------------------"
echo "Test 2: CORS Configuration"
echo "----------------------------------------"
echo "Testing CORS for origin: $VERCEL_URL"
echo ""

CORS_HEADERS=$(curl -s -I \
  -H "Origin: $VERCEL_URL" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type" \
  -X OPTIONS "$RENDER_API_URL/auth/login" 2>&1)

CORS_ALLOW_ORIGIN=$(echo "$CORS_HEADERS" | grep -i "access-control-allow-origin" | tr -d '\r')

if [[ $CORS_ALLOW_ORIGIN == *"$VERCEL_URL"* ]]; then
  echo -e "${GREEN}✅ CORS configured correctly${NC}"
  echo "   $CORS_ALLOW_ORIGIN"
  CORS_PASS=1
else
  echo -e "${RED}❌ CORS not configured for $VERCEL_URL${NC}"
  if [[ -z "$CORS_ALLOW_ORIGIN" ]]; then
    echo "   No Access-Control-Allow-Origin header found"
  else
    echo "   Found: $CORS_ALLOW_ORIGIN"
  fi
  echo ""
  echo "   Troubleshooting:"
  echo "   - Add environment variable in Render Dashboard:"
  echo "     Key: Cors__AllowedOrigins"
  echo "     Value: $VERCEL_URL"
  echo "   - Ensure service redeployed after adding variable"
  echo "   - Check that URL matches exactly (including https://)"
  CORS_PASS=0
fi
echo ""

# Test 3: Full API Request
echo "----------------------------------------"
echo "Test 3: API Request with CORS"
echo "----------------------------------------"
echo "Testing POST request to /auth/login"
echo ""

API_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$RENDER_API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -H "Origin: $VERCEL_URL" \
  -d '{"email":"test@example.com","password":"test123"}' 2>&1)

HTTP_CODE=$(echo "$API_RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$API_RESPONSE" | head -n-1)

# Any response from 200-499 is considered successful (API is responding)
if [[ $HTTP_CODE =~ ^[2-4][0-9][0-9]$ ]]; then
  echo -e "${GREEN}✅ API responding correctly${NC}"
  echo "   Status: $HTTP_CODE"
  if [[ $HTTP_CODE == "401" ]]; then
    echo "   (401 is expected - invalid test credentials)"
  fi
  echo "   Response: $RESPONSE_BODY"
  API_PASS=1
else
  echo -e "${RED}❌ API request failed${NC}"
  echo "   Status: $HTTP_CODE"
  echo "   Response: $RESPONSE_BODY"
  echo ""
  echo "   Troubleshooting:"
  echo "   - Check if API is deployed correctly"
  echo "   - Verify endpoint exists (should return 401 for invalid creds)"
  API_PASS=0
fi
echo ""

# Summary
echo "=========================================="
echo "Verification Summary"
echo "=========================================="
echo ""

PASSED=$((HEALTH_PASS + CORS_PASS + API_PASS))
FAILED=$((3 - PASSED))

echo "Tests Passed: $PASSED/3"
echo "Tests Failed: $FAILED/3"
echo ""

if [[ $FAILED == 0 ]]; then
  echo -e "${GREEN}🎉 All verification checks passed!${NC}"
  echo ""
  echo "Next Steps:"
  echo "1. Verify Vercel environment variables:"
  echo "   - Go to Vercel Dashboard → Your Project → Settings → Environment Variables"
  echo "   - Ensure API URL env var points to: $RENDER_API_URL"
  echo "   - Redeploy if you made any changes"
  echo ""
  echo "2. Test frontend integration:"
  echo "   - Open: $VERCEL_URL"
  echo "   - Open browser Developer Tools (F12)"
  echo "   - Test flow: Signup → Discover → Save → Library"
  echo "   - Verify no CORS errors in console"
  echo ""
  echo "3. If you see CORS errors in browser:"
  echo "   - Check that the Vercel URL in the browser matches: $VERCEL_URL"
  echo "   - Update Cors__AllowedOrigins if needed"
  echo ""
else
  echo -e "${RED}⚠️  Some verification checks failed${NC}"
  echo ""
  echo "Please review the errors above and:"
  echo "1. Follow the troubleshooting steps for each failed test"
  echo "2. See TROUBLESHOOTING.md for detailed help"
  echo "3. Check VERIFICATION.md for complete verification guide"
  echo ""
  echo "Common Issues:"
  echo "- Health check fails: Service not deployed or crashed"
  echo "- CORS fails: Cors__AllowedOrigins not set in Render"
  echo "- API request fails: Check Render logs for errors"
fi

echo ""
echo "For detailed help, see:"
echo "- VERIFICATION.md - Complete verification checklist"
echo "- TROUBLESHOOTING.md - Troubleshooting guide"
echo "- DEPLOYMENT.md - Deployment instructions"
echo ""
