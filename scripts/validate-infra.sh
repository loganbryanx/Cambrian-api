#!/usr/bin/env bash
set -euo pipefail

ENV_NAME="${1:-dev}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

log() {
  echo "[validate-infra] $*"
}

require_cmd() {
  local cmd="$1"
  if ! command -v "$cmd" >/dev/null 2>&1; then
    echo "Missing required command: $cmd" >&2
    return 1
  fi
}

log "Environment: ${ENV_NAME}"
log "Root: ${ROOT_DIR}"

require_cmd dotnet

if [ -f "${ROOT_DIR}/docker/docker-compose.yml" ]; then
  require_cmd docker
  log "Validating docker compose config..."
  docker compose -f "${ROOT_DIR}/docker/docker-compose.yml" config >/dev/null
  log "Docker compose config OK."
else
  log "No docker-compose.yml found; skipping docker validation."
fi

log "Validating .NET solution build..."
dotnet build "${ROOT_DIR}/Cambrian.sln" -c Debug

log "Infrastructure validation complete."