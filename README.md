# SteamCurtain 1.0.0

Автор: **๖ۣۜMoήSter√♫**

Панель-шторка для правого края экрана: подводишь мышь к углу — окно Steam плавно выезжает
из-за края, как «Правый док». Убирается кликом мимо, клавишей Esc или при переключении окон.

## Возможности

- Выезд окна Steam из правого верхнего или нижнего угла (углы настраиваются).
- Плавный выезд и уезд, скруглённые углы окна.
- Не срабатывает, пока тянешь ползунок или держишь кнопку мыши.
- Не появляется в полноэкранных играх.
- Перетаскивание мышью с запоминанием позиции.
- Настройки в трее: размер зоны, порог скорости мыши, длительность анимации, задержка скрытия, углы.
- Автозапуск с Windows, иконка в системном трее.

## Требования

- Windows 10 или 11 (x64).
- .NET Framework 4.x — уже входит в Windows.
- Запущенный клиент Steam.

## Установка

1. Распакуйте архив в любую папку.
2. Запустите `SteamCurtain.exe`.
3. Иконка появится в трее (возле часов). Если не видно — нажмите стрелку «показать скрытые значки».

Автозапуск с Windows: правый клик по иконке в трее → «Запускать с Windows».

Либо установка скриптом (PowerShell):

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

Удаление:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1 -Uninstall
```

## Как пользоваться

| Действие | Что происходит |
|---|---|
| Мышь в правый верхний или нижний угол | Шторка выезжает |
| Клик мимо окна | Шторка уезжает |
| Esc | Шторка уезжает |
| Потянуть окно мышью | Позиция запоминается |
| Клик по иконке в трее | Меню настроек |

## Сборка из исходников

Нужен .NET Framework 4.x (компилятор `csc.exe` уже в системе).

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

Готовый файл появится в `build\SteamCurtain.exe`.

Структура проекта:

```
SteamCurtain.cs   исходный код (C#, WinForms)
icon.ico          иконка (16–256 px)
build.ps1         сборка
install.ps1       установка и удаление
build\            результат сборки
```

## Настройки

Хранятся в `%APPDATA%\SteamCurtain\settings.txt`, лог — в `%APPDATA%\SteamCurtain\log.txt`.



# SteamCurtain 1.0.0 Author: **๖ۣۜMoήSter√♫** A curtain panel for the right edge of the screen: move your mouse to the corner — the Steam window smoothly slides out from the edge, like a "Right dock." It hides when you click away, press Esc, or switch windows. ## Features - Slide-out Steam window from the top or bottom right corner (corners are configurable). - Smooth sliding in and out, rounded window corners. - Doesn’t trigger while dragging the slider or holding the mouse button. - Doesn’t appear in full-screen games. - Drag with the mouse with position memory. - Tray settings: zone size, mouse speed threshold, animation duration, hide delay, corners. - Auto-start with Windows, system tray icon. ## Requirements - Windows 10 or 11 (x64). - .NET Framework 4.x — already included in Windows. - Running Steam client. ## Installation 1. Unpack the archive to any folder. 2. Run `SteamCurtain.exe`. 3. The icon will appear in the tray (near the clock). If you don’t see it — click the “show hidden icons” arrow.
