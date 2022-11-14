#!/bin/bash
if [[ -z $1 ]]; then
    printf "Provide rid\n"
    exit
fi

rm -r bin
rm -r obj

dotnet publish \
-r $1 \
-c release \
-o ./publish-output \
-p:Version=$(cat ./VERSION/VERSION)

mv ./publish-output/CLI ./publish-output/amcli
