#!/bin/zsh
# Read-only check. No installation or changes to repositories.
set -u
export PATH="$HOME/.local/bin:$HOME/.dotnet:$PATH"
print 'Architecture and macOS:'
uname -m
sw_vers -productVersion
for app in 'Visual Studio Code' 'Unity Hub' 'Blender' 'Xcode'; do
  plist="/Applications/${app}.app/Contents/Info.plist"
  if [[ -f "$plist" ]]; then
    print -n "${app}: "
    /usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$plist"
  else
    print "${app}: NOT INSTALLED"
  fi
done
print '\nCommand line tools:'
for tool in git git-lfs dotnet code blender; do
  if command -v "$tool" >/dev/null 2>&1; then
    command -v "$tool"
  else
    print "${tool}: NOT ON PATH"
  fi
done
git --version
if command -v git-lfs >/dev/null 2>&1; then git-lfs version; fi
if command -v dotnet >/dev/null 2>&1; then dotnet --list-sdks; fi
print '\nXcode:'
xcode-select -p
xcodebuild -version
print '\nUnity installations:'
for editor in /Applications/Unity/Hub/Editor/*/Unity.app/Contents/Info.plist(N); do
  print "$editor"
  /usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$editor"
done
print '\nThis check does not verify Unity licensing, project compilation or device performance.'
