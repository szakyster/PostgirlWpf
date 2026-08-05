---
mode: ask
description: Pull request összefoglaló készítése commitok, diff vagy branch-változások alapján.
---

# Pull request összefoglaló

## Kontextus
Ez a prompt pull request leírás, összefoglaló és review-kontextus előállítására szolgál git commitok, diffek vagy branch-változások alapján.

## Feladat
Készíts tömör, jól használható pull request összefoglalót a megadott változások alapján.

Elvárások:
- foglald össze, mi változott
- emeld ki a célt és a hatást
- szükség esetén sorold fel a főbb technikai módosításokat
- ha releváns, írd le a kockázatokat vagy review-fókuszpontokat
- legyen alkalmas GitHub pull request leírásnak
- kerüld a túl részletes, commitonkénti narrációt, hacsak azt külön nem kérik

## Bemenet
- commit lista, diff, branch-összehasonlítás vagy változásleírás
- opcionálisan: ticket, user story, issue link
- opcionálisan: tesztelési információk

## Kimenet
A válasz ideális szerkezete:
1. rövid összefoglaló
2. főbb változások
3. hatás vagy érintett területek
4. tesztelés
5. review megjegyzések, ha szükséges