---
mode: ask
description: Git hibák és szokatlan repository-állapotok gyors, szakmai elemzése.
---

# Git hibaelemzés

## Kontextus
Ez a prompt git hibák, sikertelen műveletek és szokatlan repository-állapotok gyors elemzésére szolgál.
Kifejezetten olyan helyzetekre készült, mint merge conflict, rebase probléma, detached HEAD, non-fast-forward push vagy stash körüli bizonytalanság.

## Feladat
Elemezd a felhasználó által megadott git hibát, állapotot vagy parancskimenetet.

Elvárások:
- azonosítsd a probléma valószínű okát
- röviden írd le, mi történt
- adj konkrét, végrehajtható következő lépéseket
- ha van kockázatos művelet, külön jelezd
- ha több megoldás van, a legkisebb kockázatúval kezdj
- ne magyarázd túl az alapokat

## Bemenet
- hibaüzenet vagy parancskimenet
- jelenlegi cél
- opcionálisan: aktuális branch, remote állapot, előzmény

## Kimenet
A válasz ideális szerkezete:
1. probléma rövid azonosítása
2. ok
3. javasolt lépések
4. kockázat vagy megjegyzés, ha szükséges