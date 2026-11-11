# Публикация на GitHub

Готовая шпаргалка: что нажимать и что вставлять. Ничего придумывать не нужно, тексты ниже можно копировать как есть.

## 1. Создание репозитория

Страница: github.com/new

| Поле | Значение |
|---|---|
| Owner | Dol4ywr |
| Repository name | `SteamCurtain` |
| Description | `Панель-шторка для Steam: окно плавно выезжает из правого угла экрана при наведении мыши. Windows 10/11, C# WinForms.` |
| Public / Private | Public |
| Add a README file | выключить |
| Add .gitignore | None |
| Choose a license | None |

Кнопка **Create repository**.

Почему README и лицензию выключаем: они уже лежат в этой папке и зальются файлами. Если включить,
GitHub создаст свои — получится дубль и конфликт при загрузке.

## 2. Загрузка файлов

Страница репозитория → ссылка **uploading an existing file** (или «Add file» → «Upload files»).

Перетащить из этой папки:

- `SteamCurtain.cs`
- `icon.ico`
- `build.ps1`
- `install.ps1`
- `README.md`
- `LICENSE`
- `.gitignore`

Внизу — **Commit changes**.

## 3. Topics (метки)

На главной странице репозитория справа, шестерёнка у «About»:

```
steam windows tray utility winforms csharp csahrp desktop
```

Блок «About» → Description тот же, что при создании.

## 4. Релиз

Справа **Releases** → **Create a new release**.

| Поле | Значение |
|---|---|
| Choose a tag | `v1.0.0` (Create new tag on publish) |
| Release title | `SteamCurtain 1.0.0` |
| Description | текст из `RELEASE_NOTES.md` |

Attach binaries: прикрепить `dist\SteamCurtain-1.0.0.zip`.

Кнопка **Publish release**.

## 5. Обновление версии в будущем

1. Поправить код и `const string Version` в `SteamCurtain.cs`.
2. `powershell -ExecutionPolicy Bypass -File .\build.ps1`
3. Пересобрать zip: `dist\SteamCurtain-<версия>.zip` (exe + README + LICENSE + icon.ico).
4. Обновить `RELEASE_NOTES.md`, залить новый тег `v1.0.1` и прикрепить новый zip.

## 6. Если через git, а не через сайт

```bash
cd SteamCurtain
git init
git add .
git commit -m "SteamCurtain 1.0.0"
git branch -M main
git remote add origin https://github.com/Dol4ywr/SteamCurtain.git
git push -u origin main
```

## 7. Ссылка для людей

После релиза ссылка на скачивание будет такой:

```
https://github.com/Dol4ywr/SteamCurtain/releases/latest
```
