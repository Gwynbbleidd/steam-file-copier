# Steam File Copier

Простая утилита для копирования `.lua` и `.manifest` файлов в папки Steam.

## 📋 Назначение

Автоматически сортирует и копирует:
- `.lua` файлы → `C:\Program Files (x86)\Steam\config\stplug-in`
- `.manifest` файлы → `C:\Program Files (x86)\Steam\depotcache`

Полезно для различных "кряков" и модификаций Steam-клиента.

## 🚀 Использование

1. Скачайте **SteamFileCopier.exe** из раздела [Releases](https://github.com/ТВОЙ_ЛОГИН/steam-file-copier/releases)
2. Запустите двойным кликом
3. Укажите исходную папку с файлами
4. Нажмите **«Начать копирование»**

> **Важно:** Для копирования в `Program Files` может потребоваться запуск от имени администратора.

## 🔧 Возможности

- Выбор исходной папки и папок назначения
- Пропуск дубликатов (не перезаписывает существующие файлы)
- Рекурсивный поиск файлов во всех подпапках
- Лог-бокс с отображением процесса
- Прогресс-бар

## 🛠 Сборка из исходников

Требуется: .NET Framework 4.x (установлен в Windows по умолчанию)

```cmd
csc.exe /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:release\SteamFileCopier.exe src\SteamFileCopier.cs
```

Или откройте `src\SteamFileCopier.cs` в Visual Studio / SharpDevelop и соберите проект.

## 📄 Лицензия

MIT License — можно использовать, изменять и распространять свободно.
