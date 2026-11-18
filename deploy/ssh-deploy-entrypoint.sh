#!/usr/bin/env bash
# Installed root-owned at /usr/local/sbin/consulting-lk-deploy.
# The dedicated Actions key is restricted to this command in authorized_keys.
set -euo pipefail
umask 077
if [[ ! ${SSH_ORIGINAL_COMMAND:-} =~ ^deploy\ ([0-9a-f]{40})$ ]]; then
  echo 'Only deploy <commit SHA> is allowed.' >&2
  exit 64
fi
revision=${BASH_REMATCH[1]}
exec 9>/var/lock/consulting-lk-deploy.lock
flock -w 1800 9
source_dir=/opt/consulting-lk-deploy/source
if [[ ! -d "$source_dir/.git" ]]; then
  install -m 700 -d /opt/consulting-lk-deploy
  git clone --filter=blob:none --no-checkout https://github.com/DanikChub/bk_server.git "$source_dir"
fi
git -C "$source_dir" fetch --no-tags origin main
git -C "$source_dir" cat-file -e "$revision^{commit}"
git -C "$source_dir" merge-base --is-ancestor "$revision" origin/main
git -C "$source_dir" checkout --detach --force "$revision"
exec bash "$source_dir/deploy/deploy-server.sh" "$revision" "$source_dir"
