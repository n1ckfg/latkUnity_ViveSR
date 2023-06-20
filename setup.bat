@echo off

git submodule init
git submodule update --init --recursive
git submodule sync
git submodule foreach git checkout master
git submodule foreach git reset --hard
git submodule foreach git pull origin master

set BASE_URL="https://fox-gieg.com/patches/github/n1ckfg/latkUnity_ViveSR/Assets/ViveSR"
cd %~dp0

powershell -Command "Invoke-WebRequest %BASE_URL%/Plugins.zip -OutFile Plugins.zip"
powershell Expand-Archive Plugins.zip -DestinationPath Assets/ViveSR
del Plugins.zip

@pause