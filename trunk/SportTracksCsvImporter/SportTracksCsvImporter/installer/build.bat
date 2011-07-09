SET ZIPCMD="C:\Program Files\7-Zip\7z.exe"
SET PLUGIN_FOLDER=WbSportTracksCsvImporter
SET INSTALL_PACKAGE_NAME=WbSportTracksCsvImporter.st3plugin
RMDIR /S /Q %PLUGIN_FOLDER%
MD %PLUGIN_FOLDER%
DEL %INSTALL_PACKAGE_NAME%
COPY ..\plugin.xml %PLUGIN_FOLDER%
COPY ..\obj\Release\WbSportTracksCsvImporter.dll %PLUGIN_FOLDER%
%ZIPCMD% -tzip a %INSTALL_PACKAGE_NAME% %PLUGIN_FOLDER%
