#!/bin/zsh
set -euo pipefail
repo_dir="${0:A:h:h}"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity}"
if [[ ! -x "$unity_editor" ]]; then
  print -u2 "Unity 6000.3.26f1 not found. Set UNITY_EDITOR to the editor executable."
  exit 1
fi
mkdir -p "$repo_dir/Builds"
"$unity_editor" -batchmode -nographics -quit -projectPath "$repo_dir/unity/CabinGame" \
  -executeMethod GrayboxBuilder.BuildMac -logFile "$repo_dir/Builds/build-mac.log"
print "Built: $repo_dir/Builds/CabinHome.app"
