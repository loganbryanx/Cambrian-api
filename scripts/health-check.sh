#!/usr/bin/env bash
set -euo pipefail

ENV_NAME="${1:-dev}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

API_BASE_URL="${API_BASE_URL:-}" 
if [ -z "${API_BASE_URL}" ]; then
  case "${ENV_NAME}" in
    dev) API_BASE_URL="http://localhost:3000" ;;
    *) API_BASE_URL="http://localhost:3000" ;;
  esac
fi

log() {
  echo "[health-check] $*"
}

require_cmd() {
  local cmd="$1"
  if ! command -v "$cmd" >/dev/null 2>&1; then
    echo "Missing required command: $cmd" >&2
    return 1
  fi
}

require_cmd curl

log "Environment: ${ENV_NAME}"
log "Base URL: ${API_BASE_URL}"

log "Checking /auth/health..."
curl -fsS "${API_BASE_URL}/auth/health" >/dev/null

log "Checking /purchase/health..."
curl -fsS "${API_BASE_URL}/purchase/health" >/dev/null

log "Service health OK."