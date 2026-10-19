#!/bin/zsh
set -euo pipefail
repo_dir="${0:A:h:h}"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_dir/Builds"
"$unity_editor" -batchmode -nographics -quit -projectPath "$repo_dir/unity/CabinGame" \
  -executeMethod StyleStudyBuilder.BuildMac -logFile "$repo_dir/Builds/build-style.log"
print "Built: $repo_dir/Builds/CabinHome-Style009.app"
