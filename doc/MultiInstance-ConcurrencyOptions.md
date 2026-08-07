# Többpéldányos futtatás – konkurenciakezelési lehetőségek

## Háttér

Az alkalmazás jelenleg fájlalapú adattárolást használ. Ha a felhasználó egyszerre több példányban futtatja az alkalmazást, konkurencia-probléma léphet fel: a példányok egymás adatait felülírhatják, vagy korrupt állapotot hozhatnak létre.

Ez a dokumentum a lehetséges megoldásokat mutatja be a legegyszerűbbtől a legrobusztusabbig.

---

## Megoldási javaslatok

### 1. Lockfile – egy példány engedélyezése

Az alkalmazás induláskor létrehoz egy `postgirl.lock` fájlt. Ha egy második példány indul, és a lockfile létezik, figyelmeztető üzenetet jelenít meg, majd kilép (vagy csak figyelmeztet).

- **Előny:** nulla fejlesztési költség, egyszerű.
- **Kompromisszum:** felhasználó nem nyithat két ablakot.
- **Adatvesztés:** nem lehetséges, mert a második példány el sem indul.

---

### 2. Last-write-wins (utolsó írás nyer)

Az alkalmazás nem akadályoz semmit. Minden példány a saját memória-állapotát menti felül mentéskor. Az utolsó mentés érvényes.

- **Előny:** nulla fejlesztési költség, nem zavaró a felhasználónak.
- **Kompromisszum:** ha két példányban dolgozik, az egyik munkája csendben elvész mentéskor.
- **Adatvesztés:** lehetséges, nem látható.

---

### 3. Read-on-focus (fókuszváltáskor újraolvas)

Amikor az alkalmazás ablaka fókuszt kap, újraolvassa a fájlt. Ha a fájl változott, értesíti a felhasználót („Egy másik példány módosított valamit — betöltöd?").

- **Előny:** viszonylag egyszerű, a felhasználó dönthet.
- **Kompromisszum:** nem valós idejű, csak fókuszváltáskor frissül; ütközéskezelés nincs, csak felülírás.
- **Adatvesztés:** csökkentett kockázat, de nem nulla.

---

### 4. File-locking (fájlzár olvasás-íráshoz)

Mentéskor a fájlt exkluzív zárral nyitja az alkalmazás (`FileShare.None`). Ha másik példány épp ment, az vár vagy hibát kap. Olvasáshoz shared lock.

- **Előny:** .NET-ben natívan támogatott, nem kell külső függőség.
- **Kompromisszum:** csak az egyidejű írást védi, a közben elvégzett memóriabeli munkát nem. Lassú gépen vagy nagy fájlon pillanatnyi UI-fagyást okozhat.
- **Adatvesztés:** az egyidejű írás nem korruptálja a fájlt, de a két példány egymás változásait felülírhatja.

---

### 5. FileSystemWatcher (valós idejű figyelés)

Az alkalmazás figyeli a saját adatfájlját (`FileSystemWatcher`). Ha egy másik példány módosítja, azonnal értesítést kap, és felajánlja az újratöltést vagy az ütközés megjelenítését.

- **Előny:** valós idejű, felhasználóbarát.
- **Kompromisszum:** az ütközésfeloldás logikája összetett lehet; a watcher néha több eseményt dob egyszerre (debounce szükséges).
- **Adatvesztés:** alacsony kockázat, de manuális feloldás kell.

---

### 6. SQLite adatbázis (egyfájlos, beágyazott)

A fájlalapú JSON/XML mentést SQLite váltja fel. Az SQLite natívan kezeli a konkurens hozzáférést, WAL (Write-Ahead Logging) módban több olvasó + egy író párhuzamosan is működhet.

- **Előny:** robusztus, iparági standard, .NET-ben kiváló támogatás (`Microsoft.Data.Sqlite`); a mentési struktúra is tisztul.
- **Kompromisszum:** adatmodell-átírás szükséges; a meglévő fájlalapú mentések migrációt igényelnek.
- **Adatvesztés:** minimális (tranzakciók védik).

---

### 7. Named Mutex + verzióbélyeg (hibrid)

A fájlalapú mentés megmarad, de kiegészül egy named mutex-szel (rendszerszintű zár) és egy verziószámmal/timestamppal a fájlban. Mentés előtt az alkalmazás ellenőrzi, hogy a fájl verziója megegyezik-e azzal, amit betöltött. Ha nem, ütközést jelez.

- **Előny:** nem kell DB-re áttérni; optimistic concurrency mintát követ.
- **Kompromisszum:** több fejlesztési munka, mint a lockfile; az ütközésfeloldó UI szükséges.
- **Adatvesztés:** védett, ha az ütközéskezelés jól van implementálva.

---

## Összefoglaló táblázat

| # | Megoldás | Fejlesztési költség | Adatvesztés kockázata | Felhasználói limitáció | Robusztusság |
|---|----------|--------------------|-----------------------|------------------------|--------------|
| 1 | Lockfile (1 példány) | 🟢 Minimális | 🟢 Nincs | 🔴 Csak 1 példány | 🟡 Közepes |
| 2 | Last-write-wins | 🟢 Nulla | 🔴 Magas (néma) | 🟢 Nincs | 🔴 Alacsony |
| 3 | Read-on-focus | 🟡 Kis | 🟡 Csökkentett | 🟢 Nincs | 🟡 Közepes |
| 4 | File-locking | 🟡 Kis | 🟡 Közepes | 🟢 Nincs | 🟡 Közepes |
| 5 | FileSystemWatcher | 🟡 Közepes | 🟡 Alacsony | 🟢 Nincs | 🟡 Közepes |
| 6 | SQLite | 🔴 Nagy | 🟢 Minimális | 🟢 Nincs | 🟢 Magas |
| 7 | Named Mutex + verzióbélyeg | 🟠 Közepes-nagy | 🟢 Védett | 🟢 Nincs | 🟢 Magas |

---

## Megjegyzés

- **Gyors, pragmatikus megoldáshoz** → **#1 (lockfile)** ajánlott.
- **Hosszú távú stabilitáshoz**, különösen ha a projekt/env struktúra bevezetése amúgy is adatmodell-átírással jár → **#6 (SQLite)** megéri a befektetést.
