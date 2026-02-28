@echo off
chcp 65001 >nul
echo Проверка Git...
where git >nul 2>&1
if errorlevel 1 (
    echo Git не найден. Установите Git: https://git-scm.com/download/win
    echo После установки перезапустите этот файл.
    pause
    exit /b 1
)

cd /d "%~dp0"

echo Инициализация репозитория...
if not exist .git git init

echo Добавление файлов...
git add .

echo Создание коммита...
git commit -m "Добавьте файлы проекта." 2>nul
if errorlevel 1 (
    echo Нет изменений для коммита или коммит уже есть.
) else (
    echo Коммит создан.
)

echo Ветка main...
git branch -M main

echo Подключение к GitHub...
git remote remove origin 2>nul
git remote add origin https://github.com/LF-13/val12.git

echo Отправка на GitHub...
git push -u origin main

if errorlevel 1 (
    echo.
    echo Если запросили логин/пароль: используйте Personal Access Token вместо пароля.
    echo Создать: GitHub - Settings - Developer settings - Personal access tokens.
) else (
    echo.
    echo Готово. Откройте https://github.com/LF-13/val12
)

pause
