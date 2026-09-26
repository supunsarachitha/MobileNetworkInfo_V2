# Privacy Policy – Mobile Network Info

_Last updated: 26 September 2026_

Mobile Network Info ("the app") is a free Android app available on Google Play
(package name `lk.stechbuzz.networktoggler`). This policy explains what information the app uses and how.

## Summary

- The app does **not** collect, store, sell or share any personal information.
- There are **no ads, no analytics, no tracking and no accounts**.
- Everything the app shows is read from your phone and stays on your phone.

## Information the app reads on your device

To show you information about your phone, the app reads details from Android, such as:

- **Device and system details**: model, Android version, processor, memory, storage, display, battery and sensors.
- **Network details**: connection type, Wi-Fi network name and signal, nearby Wi-Fi networks, mobile carrier and signal,
  cell tower identifiers and IP addresses.

This information is only displayed on screen. It is not saved, and it never leaves your device unless you choose to
share it (see [Sharing a report](#sharing-a-report)).

## Location permission

Android only lets apps read the Wi-Fi network name, scan for nearby Wi-Fi networks and read cell tower details if they
have location permission, because this information could be used to estimate where you are. The app:

- asks for location permission only when you tap to see these details, and explains why first;
- uses it only while the app is open ("while using the app"), never in the background;
- never records, stores or sends your location anywhere.

You can turn this permission off at any time in **Android Settings › Apps › Mobile Network Info › Permissions**.
The rest of the app keeps working without it.

## Internet access

The app only connects to the internet when you ask it to:

- **Connection test** briefly opens a connection to Cloudflare (1.1.1.1) and Google (8.8.8.8 and google.com) to measure
  how long they take to respond. No other data is sent.
- **Public IP check** sends a request to [api.ipify.org](https://www.ipify.org), which replies with your public IP address.
  Like any website, ipify can see the IP address the request comes from.
- **Links you tap** (Google Play, GitHub) open in the Play Store app or your browser, which have their own privacy policies.

## Sharing a report

"Share report" creates a text report containing the details shown in the app, which can include your Wi-Fi network name,
IP addresses and cell tower identifiers. The report is only sent to the app or person you pick in Android's share menu.
Please review it before posting it publicly.

## Clipboard

When you tap a value it is copied to your clipboard. The app never reads your clipboard.

## Children's privacy

The app does not collect personal information from anyone, including children.

## Data stored by the app and deletion

The app stores no personal data. It keeps a single setting on your device (the last app version whose "What's new" page
you have seen). Uninstalling the app removes it.

## Source code

The complete source code is publicly available at
[github.com/supunsarachitha/MobileNetworkInfo_V2](https://github.com/supunsarachitha/MobileNetworkInfo_V2),
so anyone can check exactly what the app does.

## Changes to this policy

If this policy changes, the new version will be published at this address with a new "Last updated" date.

## Contact

Questions or concerns? Please open an issue at
[github.com/supunsarachitha/MobileNetworkInfo_V2/issues](https://github.com/supunsarachitha/MobileNetworkInfo_V2/issues).
