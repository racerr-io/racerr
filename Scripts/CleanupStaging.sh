#!/bin/bash

AUTH_HEADER=""
if [[ ! -z "$GITHUB_TOKEN" ]]; then
  echo Authenticating with GitHub authorisation token
  AUTH_HEADER="Authorization: token $GITHUB_TOKEN"
fi

API_URL="https://api.github.com/repos/racerr-io/racerr"
RELEASE_ID=$(curl -H "$AUTH_HEADER" $API_URL/releases | jq -r "map(select(.prerelease == true))[0].id")
if [[ -z "$RELEASE_ID" ]]; then
  echo "Failed to retrieve Racerr prerelease."
  exit 1
fi

curl \
  -X DELETE \
  -H "$AUTH_HEADER" \
  "$API_URL/releases/$RELEASE_ID"