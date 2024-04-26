#!/bin/bash
if [[ -z "$1" ]] || [[ -z "$2" ]]
then
  echo "ERROR: You must specify either a -get, major, minor or patch argument, and a project file."
  echo "Usage: $0 (-get|major|minor|patch) MyProject.csproj"
  exit 1
fi

if [[ ! -w $2 ]]
then
  echo "ERROR: The project file '$2' either does not exist, or is not writable."
  exit 2
fi

v=$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' $2)
if [ -z "$v" ]
then
  echo "ERROR: Could not find a <Version/> tag in the project file '$2'."
  echo "Please add one in between the <Project><PropertyGroup> tags and try again."
  exit 3
fi

parts=(${v//./ })
case "$1" in
  -get) echo $v; exit 0 ;;
  major) ((parts[0]++)) ;;
  minor) ((parts[1]++)) ;;
  patch) ((parts[2]++)) ;;
  *)
    echo "ERROR: Invalid SemVer position name supplied, '$1' was not understood."
    echo "Usage: $0 (-get|major|minor|patch) $2"
    exit 4
esac

nv="${parts[0]}.${parts[1]}.${parts[2]}"
$(sed -i -e "s/<Version>$v<\/Version>/<Version>$nv<\/Version>/g" $2)
echo $nv
