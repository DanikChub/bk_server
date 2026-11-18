#!/usr/bin/env bash
set -Eeuo pipefail
umask 077
revision=${1:?Verified revision required}
source_dir=${2:?Source checkout required}
[[ $revision =~ ^[0-9a-f]{40}$ ]] || exit 64
[[ $(realpath "$source_dir") == /opt/consulting-lk-deploy/source ]] || exit 64
cd /opt/consulting-lk
dc() { docker compose -f compose.yaml -f deploy/compose.showcase.yaml --project-directory . "$@"; }
backup="backups/$(date -u +%Y%m%dT%H%M%S)-${revision:0:12}"
install -m 700 -d "$backup"
dc exec -T postgres pg_dump -U consulting -d consulting_lk > "$backup/database.sql"
cp compose.yaml "$backup/compose.yaml"
cp deploy/compose.showcase.yaml "$backup/compose.showcase.yaml"
previous=$(cat backend-revision 2>/dev/null || true)
printf '%s\n' "$previous" > "$backup/previous-revision"
docker tag consulting-lk-app:latest consulting-lk-app:rollback
docker tag consulting-lk-worker:latest consulting-lk-worker:rollback
switched=false
rollback() {
  status=$?
  trap - ERR
  set +e
  echo "Deployment failed; restoring previous images. Backup: $backup" >&2
  docker tag consulting-lk-app:rollback consulting-lk-app:latest
  docker tag consulting-lk-worker:rollback consulting-lk-worker:latest
  cp "$backup/compose.yaml" compose.yaml
  cp "$backup/compose.showcase.yaml" deploy/compose.showcase.yaml
  if [[ $switched == true ]]; then dc up -d --no-build app worker; fi
  exit "$status"
}
trap rollback ERR
for directory in frontend database/migrations deploy; do
  rsync -a --delete --exclude='.env*' --exclude=node_modules --exclude=.next --exclude=.local "$source_dir/$directory/" "/opt/consulting-lk/$directory/"
done
cp "$source_dir/database/init.sql" database/init.sql
cp "$source_dir/compose.yaml" compose.yaml
cp "$source_dir/.dockerignore" .dockerignore
dc build --build-arg "APP_REVISION=$revision" app worker
dc stop worker
switched=true
dc run --rm --no-deps -T worker npm run db:migrate
dc up -d --no-build app worker
ready=false
for attempt in $(seq 1 30); do
  if curl --fail --silent http://127.0.0.1:3000/api/ready >/dev/null; then ready=true; break; fi
  sleep 2
done
[[ $ready == true ]]
curl --fail --silent --show-error --retry 5 --retry-delay 3 --retry-all-errors https://consulting-lk.103-74-92-127.sslip.io/api/ready
printf '%s\n' "$revision" > backend-revision
trap - ERR
echo "Deployed $revision"
