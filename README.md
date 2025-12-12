# ByeDPI Manager

Русский | [English](README.en.md)

Мини утилита для запуска ByeDPI и ProxiFyre.

![Скриншот интерфейса](screens/screen_ru.png)

## Требования

1. Windows 7+, [.NET Framework 4.7.2+](https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/thank-you/net472-offline-installer)
2. [ProxiFyre](https://github.com/wiresock/proxifyre), [Windows Packet Filter](https://github.com/wiresock/ndisapi), [Visual C++ Redist 2022](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)
3. [ByeDPI](https://github.com/hufrea/byedpi)

## Инструкции

* Комплексная инструкция от комьюнити [ByeDPI Manager Manual](https://github.com/BDManual/ByeDPIManager-Manual)

### Вариант 1: All-in-One (Рекомендуется для начинающих)
Этот вариант включает все необходимые компоненты в одном архиве.

1. **Скачивание:**
   - Перейдите на страницу релизов: https://github.com/romanvht/ByeDPIManager/releases/latest
   - Скачайте файл `All_In_One_w64.zip`

2. **Распаковка:**
   - Найдите скачанный файл на компьютере
   - Нажмите правой кнопкой мыши и выберите "Извлечь все…"
   - Укажите папку для установки (например, `C:\APPS\ByeDPIManager`)

3. **Установка зависимостей:**
   - Перейдите в папку `redist` внутри распакованного архива
   - Установите оба приложения из этой папки:
     - Windows Packet Filter (необходим для работы ProxiFyre)
     - Visual C++ Redistributable 2022

### Вариант 2: Раздельная установка (Для опытных пользователей)
Если вы предпочитаете управлять компонентами отдельно:

1. **Скачайте компоненты отдельно:**
   - [Manager](https://github.com/romanvht/ByeDPIManager/releases/latest)
   - [ByeDPI](https://github.com/hufrea/byedpi)
   - [ProxiFyre](https://github.com/wiresock/proxifyre)

2. **Установите зависимости:**
   - [Windows Packet Filter](https://github.com/wiresock/ndisapi) (Необходим для работы ProxiFyre)
   - [Visual C++ Redistributable 2022](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)

3. **Распакуйте все компоненты в удобные папки**

4. **Запустите и укажите пути**
   - Укажите правильный путь к файлу `ciadpi.exe` во вкладке ByeDPI
   - Укажите правильный путь к файлу `proxifyre.exe` во вкладке ProxiFyre

## Настройка

### Первоначальная настройка

1. **Запуск программы:**
   - Запустите файл `ByeDPI Manager.exe`
   - Нажмите кнопку "Настройки"

2. **Настройка ProxiFyre:**
   - Перейдите на вкладку "ProxiFyre"
   - Укажите приложения, для которых будет выполняться обход (например, Chrome, Firefox и т.д.)

### Настройка стратегии

#### Использование готовой стратегии
- В поле "Аргументы" на вкладке "ByeDPI" введите нужную стратегию

#### Подбор стратегии (опционально)
Если у вас нет готовой стратегии, вы можете воспользоваться подбором:

1. **Переход к тестированию:**
   - Перейдите на вкладку "Подбор стратегии (Beta)"

2. **Запуск теста:**
   - Нажмите кнопку "Старт"
   - При первом запуске появится запрос на разрешение доступа для `ciadpi.exe` к сети - нажмите "Разрешить"

3. **Выбор стратегии:**
   - После завершения теста в окне "Лог" будут показаны стратегии с успехом более 50%
   - Выделите лучшую стратегию мышкой и скопируйте её (Ctrl+C)

4. **Применение стратегии:**
   - Вернитесь на вкладку "ByeDPI"
   - В поле "Аргументы" вставьте скопированную стратегию (Ctrl+V)

5. **Настройка тестирования (опционально):**
   - Отредактируйте файлы в папке `proxytest`:
     - `sites.txt` - добавьте свои сайты для тестирования
     - `cmds.txt` - добавьте свои стратегии для проверки

### Запуск и проверка

1. **Активация:**
   - В главном окне нажмите кнопку "Подключить"
   - При первом запуске появится запрос на разрешение доступа для ProxiFyre к сети - нажмите "Разрешить"

2. **Проверка работы:**
   - Откройте браузер или приложение, которое указали в настройках
   - Проверьте доступ к ресурсам

## Решение проблем

- Если программа не запускается, убедитесь, что установлен .NET Framework 4.7.2+
- Если не работает обход блокировок, попробуйте другую стратегию
- При проблемах с подключением проверьте, что Windows Packet Filter установлен корректно
- Убедитесь, что антивирус или брандмауэр не блокирует работу программы

## Большое спасибо

- [ByeDPI](https://github.com/hufrea/byedpi)
- [ProxiFyre](https://github.com/wiresock/proxifyre)
- [Windows Packet Filter](https://github.com/wiresock/ndisapi)
- [SocksSharp](https://github.com/extremecodetv/SocksSharp)
