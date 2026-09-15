#!/usr/bin/env bash
set -euo pipefail

: "${JIRA_BASE_URL:?Debes definir JIRA_BASE_URL, p.ej. https://tu-org.atlassian.net}"
: "${JIRA_EMAIL:?Debes definir JIRA_EMAIL}"
: "${JIRA_API_TOKEN:?Debes definir JIRA_API_TOKEN}"

BASE_URL="${JIRA_BASE_URL%/}"

HTTP_CODE=$(curl -sS -u "$JIRA_EMAIL:$JIRA_API_TOKEN" \
  -H "Accept: application/json" \
  -o /tmp/jira-myself.json \
  -w "%{http_code}" \
  "$BASE_URL/rest/api/3/myself")

if [[ "$HTTP_CODE" != "200" ]]; then
  echo "❌ No se pudo conectar a JIRA. HTTP $HTTP_CODE"
  echo "Respuesta guardada en /tmp/jira-myself.json"
  exit 1
fi

DISPLAY_NAME=$(node -e 'const fs=require("fs");const d=JSON.parse(fs.readFileSync("/tmp/jira-myself.json","utf8"));console.log(d.displayName||"(sin displayName)")')
ACCOUNT_ID=$(node -e 'const fs=require("fs");const d=JSON.parse(fs.readFileSync("/tmp/jira-myself.json","utf8"));console.log(d.accountId||"(sin accountId)")')

echo "✅ Conexión exitosa a JIRA"
echo "Usuario: $DISPLAY_NAME"
echo "Account ID: $ACCOUNT_ID"
