#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ZAP_TARGET="${ZAP_TARGET:-}"

cd "$ROOT_DIR"

dotnet test src/ELearning.sln --no-restore -m:1 /nr:false
dotnet list src/ELearning.sln package --vulnerable --include-transitive

pushd frontend/web >/dev/null
npm audit --omit=dev --audit-level=high
if [[ "${INCLUDE_DEV_AUDIT:-0}" == "1" ]]; then
  npm audit --audit-level=high
fi
npm run build
popd >/dev/null

if [[ -n "$ZAP_TARGET" ]]; then
  docker run --rm \
    -t ghcr.io/zaproxy/zaproxy:stable \
    zap-baseline.py \
    -t "$ZAP_TARGET" \
    -I
else
  echo "Skipping OWASP ZAP baseline. Set ZAP_TARGET=https://your-host to enable it."
fi
