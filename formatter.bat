@echo off
setlocal enabledelayedexpansion

:: Check if gdformat is installed
where gdformat >nul 2>nul
if %errorlevel% neq 0 (
    echo gdformat not found. Please ensure it is installed and available in your PATH.
    exit /b 1
)

:: Ensure the src directory exists
if not exist "src\" (
    echo No "src" directory found. Ensure you are in the correct project folder.
    exit /b 1
)

:: Loop through all .gd files inside "src", excluding "addons"
for /r "src" %%F in (*.gd) do (
    echo %%F | findstr /i /c:"\addons\" >nul
    if errorlevel 1 (
        echo Formatting: %%F
        gdformat "%%F" --line-length 200
    )
)

echo Formatting complete!
exit /b 0
