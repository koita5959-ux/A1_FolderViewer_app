@echo off
echo === DesktopKit.FolderViewer ビルド＆パブリッシュ ===
cd %~dp0FolderViewer
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ..\publish\
echo.
echo 完了しました。publish\ フォルダにexeがあります。
pause
