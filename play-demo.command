#!/bin/zsh
set -euo pipefail
repo_dir="${0:A:h}"
demo_app="$repo_dir/Builds/CabinHome.app"
if [[ ! -d "$demo_app" ]]; then
  print 'Demo not built yet. Run: zsh development/build-mac-demo.sh'
  read '?Press Enter to close.'
  exit 1
fi
open "$demo_app"
