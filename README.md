# val12

Приложение доставки замороженных продуктов (WinForms, C#, SQL Server).

- Авторизация и регистрация с капчей
- Магазин товаров для пользователей
- Админ-панель: управление пользователями (блокировка), добавление и удаление товаров

## Требования

- .NET Framework 4.7.2
- SQL Server (строка подключения в `App.config`)

## Как открыть репозиторий с файлами на GitHub

Репозиторий должен показывать папки и файлы проекта (как на первом скриншоте). Если у вас пустая страница «Quick setup»:

### Вариант 1: Через Git в командной строке

1. Установи [Git для Windows](https://git-scm.com/download/win), если ещё не установлен.
2. Открой **PowerShell** или **cmd** и выполни в папке проекта:

```bash
cd "c:\Users\lotor\source\repos\val12"
git init
git add .
git commit -m "Добавьте файлы проекта."
git branch -M main
git remote add origin https://github.com/LF-13/val12.git
git push -u origin main
```

Если репозиторий уже был добавлен и просто не запушен:

```bash
cd "c:\Users\lotor\source\repos\val12"
git init
git add .
git commit -m "Добавьте файлы проекта."
git branch -M main
git remote add origin https://github.com/LF-13/val12.git
git push -u origin main
```

### Вариант 2: Через GitHub Desktop

1. Скачай [GitHub Desktop](https://desktop.github.com/).
2. **File → Add Local Repository** → укажи папку `c:\Users\lotor\source\repos\val12`.
3. Если папка не репозиторий, программа предложит **Create a repository** — создай репозиторий здесь.
4. Напиши коммит (например: «Добавьте файлы проекта.») и нажми **Commit to main**.
5. **Repository → Push origin** (или **Publish repository**, если репо ещё не связано с GitHub).
6. Если репозиторий на GitHub уже создан (val12), при связке укажи его: **Repository → Repository settings → Primary remote repository** → `https://github.com/LF-13/val12.git`.

### Вариант 3: Загрузка через сайт GitHub

1. Открой https://github.com/LF-13/val12.
2. Нажми **Add file → Upload files**.
3. Перетащи в окно содержимое папки `val12` (все файлы и папки: val12.sln, val12\, .gitignore и т.д.) или выбери их.
4. Внизу напиши коммит и нажми **Commit changes**.

После успешной загрузки на GitHub появятся папки и файлы, как на первом скриншоте.
