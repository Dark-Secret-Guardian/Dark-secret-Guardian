@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo   复制酒馆背景图到项目 Resources 目录
echo ========================================
echo.

set "SRC=C:\Users\zsy13\Documents\xwechat_files\wxid_veef7t0qtjja22_3c4f\temp\RWTemp\2026-06\9e20f478899dc29eb19741386f9343c8\a37c49fcf50a875fcf306f6a5272467d.jpg"
set "DEST_DIR=%~dp0Assets\Resources\Backgrounds"
set "DEST=%DEST_DIR%\tavern.jpg"

if not exist "%DEST_DIR%" (
    mkdir "%DEST_DIR%"
    echo [创建目录] %DEST_DIR%
)

if exist "%SRC%" (
    copy /Y "%SRC%" "%DEST%" >nul
    echo [成功] 背景图已复制到: %DEST%
) else (
    echo [错误] 找不到源文件: %SRC%
    echo.
    echo 请手动将酒馆图片复制到: %DEST%
    echo 并确保文件名为 tavern.jpg
)

echo.
echo 完成！请在 Unity 中打开项目，BackgroundManager 将自动加载并显示背景。
pause
