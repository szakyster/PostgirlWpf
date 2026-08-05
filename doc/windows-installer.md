# Windows installer

## Cél
Ez a projekt egy nagyon alap Windows telepítőt kapott, amely egyetlen `.exe` fájlból telepíti az alkalmazást.

## Megoldás
- `dotnet publish` készíti el a `win-x64` publish outputot
- az `installer/Postgirl.iss` Inno Setup script ebből készít telepítőt
- a buildet az `installer/build-installer.ps1` script fogja össze
- minden telepítő build kap egy folyamatosan növekvő revíziószámot
- a kész setup fájlnév tartalmazza a teljes verziót

## Előfeltétel
Telepítve kell lennie az `Inno Setup 6` vagy `Inno Setup 7` eszköznek.

## Használat
Repository gyökérből:

```powershell
.\installer\build-installer.ps1
```

## Verziózás
- a projekt alapverziója a `Postgirl.csproj` `Version` mezőjéből jön
- minden installer build növeli az `Artifacts\Installer\installer-revision.txt` számlálót
- a teljes installer verzió formátuma: `<alapverzió>-revNNNN`

Példa:

```text
0.1.0-alpha-rev0001
```

## Eredmény
A kész installer ide kerül:

```text
Artifacts\Installer\Postgirl-Setup-0.1.0-alpha-rev0001.exe
```

A pontos fájlnév a növekvő revíziótól függ.

## Megjegyzés
- alapértelmezett célplatform: `win-x64`
- per-user telepítést használ
- Start Menu shortcut készül
- opcionális Desktop shortcut kérhető a telepítőben
