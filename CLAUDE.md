# CLAUDE.md

Guidance for Claude Code and other contributors working in this repository.

## Project

**Mobile Network Info** is a .NET MAUI app (Android only) published on Google Play as `lk.stechbuzz.networktoggler`.
It shows network, device, battery and sensor information. It is free and ad-free.

The code is **source-available, not open source**: it is licensed under the PolyForm Strict License 1.0.0 with an added
permission for contributions (see [LICENSE](LICENSE)), which forbids commercial use and distribution. Never describe
the project as "open source" in the app, README or store texts; use "source available" or "source code on GitHub".

## Always update the changelog

Every new feature, improvement, bug fix or removal **must** be added to [CHANGELOG.md](CHANGELOG.md) as part of the
same change, without being asked. The file is bundled into the app and shown to users on the "What's new" page, so:

- Write for non-technical users: plain, friendly language with no class names, APIs or developer jargon.
  Describe what the user notices ("The compass pointed the wrong way"), not how it was implemented.
- Add entries to the version at the top of the file while it is still in development. Once that version has been
  released on Google Play, start a new `## Version X.Y · Month YYYY` section above it and bump both
  `ApplicationDisplayVersion` and `ApplicationVersion` (the Play versionCode, which must always increase) in
  `MobileNetworkInfo/MobileNetworkInfo.csproj`. If unsure whether the top version has shipped, ask.
- Use only these section headings, in this order: `### New`, `### Improved`, `### Fixed`, `### Removed`,
  with one `- ` bullet per change. The unit test `RepositoryChangelog_IsValid` checks the format.
- When the UI changes noticeably, refresh the screenshots in `docs/screenshots/` used by README.md.

## Build, run and test

- Requirements: .NET 10 SDK with the `maui` workload, Android SDK (API 36), JDK 21.
- Run on a device or emulator: `dotnet build MobileNetworkInfo/MobileNetworkInfo.csproj -t:Run -f net10.0-android`
- Release build (AAB for Google Play + APK): `dotnet publish MobileNetworkInfo/MobileNetworkInfo.csproj -f net10.0-android -c Release`
  Output: `MobileNetworkInfo/bin/Release/net10.0-android/publish/`
- Signing reads `MobileNetworkInfo/signing.local.props` (git-ignored; template in `signing.local.props.example`).
  Never commit keystore files or passwords.
- Unit tests: `dotnet test MobileNetworkInfo.Tests`. Builds must stay warning-free.

## Architecture

- MVVM with CommunityToolkit.Mvvm.
  - `Views/`: XAML pages deriving from `BasePage`, which forwards visibility to the view model and adds the shared toolbar.
  - `ViewModels/`: derive from `BaseViewModel`; live data uses `StartPolling`, which stops automatically when the page
    is hidden or the app goes to the background.
  - `Services/`: Android API access (network, device, battery, sensors, report, changelog).
  - `Helpers/`: pure .NET with no MAUI or Android references. These files are compiled into `MobileNetworkInfo.Tests`
    and must stay platform-independent. Put testable logic here.
  - `Controls/`: `InfoCard` (titled card of label/value rows), `CompassView`, `RingGauge`.
- Info cards: services return a `RowList` (empty values are skipped automatically); view models push it into an
  `InfoSection`, which updates rows in place.
- `minSdk` is 23: guard newer Android APIs with `OperatingSystem.IsAndroidVersionAtLeast(n)`.
- Colors are light/dark token pairs in `Resources/Styles/Colors.xaml` (`XxxLight` / `XxxDark`). Use `AppThemeBinding` in
  XAML and `ThemeColors.Get("Xxx")` in code. Everything must look right in both themes.
- Icons come from the Material Icons font; add glyphs to `Resources/Icons.cs` as `\uXXXX` escapes.

## Google Play rules to respect

- Keep targeting the API level Google Play requires (currently API 36 via `net10.0-android`).
- Do not add in-app donation, tip or external payment links (Play Payments policy). Donations are mentioned only in README.md.
- Request permissions only when a feature needs them and explain why in the UI. If data handling changes, update
  `PRIVACY_POLICY.md` (linked from the app) and remind the maintainer to update the Play Console Data safety form.
