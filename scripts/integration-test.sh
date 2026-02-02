#!/usr/bin/env bash
set -euo pipefail

ENV_NAME="${1:-dev}"
JWT_TOKEN="${2:-}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

API_BASE_URL="${API_BASE_URL:-}"
if [ -z "${API_BASE_URL}" ]; then
  case "${ENV_NAME}" in
    dev) API_BASE_URL="http://localhost:3000" ;;
    *) API_BASE_URL="http://localhost:3000" ;;
  esac
fi

log() {
  echo "[integration-test] $*"
}

require_cmd() {
  local cmd="$1"
  if ! command -v "$cmd" >/dev/null 2>&1; then
    echo "Missing required command: $cmd" >&2
    return 1
  fi
}

require_cmd curl

if [ -z "${JWT_TOKEN}" ]; then
  echo "JWT token is required as the second argument." >&2
  echo "Usage: ./scripts/integration-test.sh dev \$JWT_TOKEN" >&2
  exit 1
fi

EMAIL="test+$(date +%s)@cambrian.local"
AUTH_HEADER=("-H" "Authorization: Bearer ${JWT_TOKEN}")
EMAIL_HEADER=("-H" "x-email: ${EMAIL}")
JSON_HEADER=("-H" "Content-Type: application/json")

log "Environment: ${ENV_NAME}"
log "Base URL: ${API_BASE_URL}"

log "Health check..."
curl -fsS "${API_BASE_URL}/auth/health" >/dev/null

log "Registering user..."
REGISTER_PAYLOAD=$(cat <<EOF
{"email":"${EMAIL}","password":"Password!234","plan":"Creator"}
EOF
)

curl -fsS -X POST "${API_BASE_URL}/auth/register" \
  "${JSON_HEADER[@]}" \
  -d "${REGISTER_PAYLOAD}" >/dev/null

log "Fetching account..."
curl -fsS "${API_BASE_URL}/data/account" "${AUTH_HEADER[@]}" "${EMAIL_HEADER[@]}" >/dev/null

log "Fetching catalog..."
curl -fsS "${API_BASE_URL}/catalog" "${AUTH_HEADER[@]}" "${EMAIL_HEADER[@]}" >/dev/null

log "Fetching purchase library..."
curl -fsS "${API_BASE_URL}/purchase/library" "${AUTH_HEADER[@]}" "${EMAIL_HEADER[@]}" >/dev/null

log "Starting stream..."
STREAM_START_PAYLOAD=$(cat <<EOF
{"title":"Integration Stream"}
EOF
)

curl -fsS -X POST "${API_BASE_URL}/stream/start" \
  "${JSON_HEADER[@]}" \
  "${AUTH_HEADER[@]}" \
  "${EMAIL_HEADER[@]}" \
  -d "${STREAM_START_PAYLOAD}" >/dev/null

log "Writing play event..."
PLAY_EVENT_PAYLOAD=$(cat <<EOF
{"trackId":"integration-track-1","title":"Integration Track","artist":"Integration Artist","durationSeconds":42,"source":"integration"}
EOF
)

curl -fsS -X POST "${API_BASE_URL}/play/events" \
  "${JSON_HEADER[@]}" \
  "${AUTH_HEADER[@]}" \
  "${EMAIL_HEADER[@]}" \
  -d "${PLAY_EVENT_PAYLOAD}" >/dev/null

log "Fetching AI trending..."
curl -fsS "${API_BASE_URL}/ai/trending" "${AUTH_HEADER[@]}" "${EMAIL_HEADER[@]}" >/dev/null

log "Integration tests complete."