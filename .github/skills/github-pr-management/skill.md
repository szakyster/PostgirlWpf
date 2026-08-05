# GitHub PR Management

## Cél
Ez a skill a GitHub pull requestek kezeléséhez ad strukturált eljárást és döntési keretet.
Segít PR létrehozásban, review-folyamatban, merge-stratégia kiválasztásában és branch protectionnel kapcsolatos döntésekben.

## Felhasználási terület
- új pull request létrehozása
- meglévő pull request értelmezése vagy karbantartása
- review előkészítése
- merge döntés
- branch protection szabályok figyelembevétele
- GitHub-alapú együttműködési gyakorlatok

## Bemenet
- repository-kontextus
- branch neve vagy forrás és cél branch
- változások rövid leírása
- opcionálisan: issue, ticket, release vagy hotfix kontextus
- opcionálisan: CI állapot, review státusz, merge korlátozások

## Kimenet
- javasolt PR-lépések
- rövid PR-leírás vagy review-fókusz
- merge-stratégia javaslat
- figyelmeztetések és kockázatok

## Lépések
1. Azonosítsd a PR célját.
   - feature, fix, refactor, hotfix vagy release jellegű-e
2. Ellenőrizd a branch viszonyokat.
   - megfelelő forrás- és célbranch van-e kiválasztva
   - szükséges-e előbb sync vagy rebase
3. Foglald össze a változásokat.
   - mi változott
   - miért változott
   - mely területeket érinti
4. Készíts PR-leírást.
   - rövid összefoglaló
   - főbb módosítások
   - tesztelési információk
   - review-fókuszpontok
5. Értékeld a review-állapotot.
   - van-e hiányzó reviewer
   - van-e nyitott megjegyzés
   - átment-e a CI
6. Javasolj merge-stratégiát.
   - `merge commit`, ha a history megőrzése fontos
   - `squash`, ha tisztább main history kell
   - `rebase and merge`, ha lineáris history kívánatos és a csapat ezt használja
7. Jelezd a kockázatokat.
   - nagy diff
   - vegyes célú változtatások
   - hiányzó tesztelés
   - védett branch vagy policy akadály

## Döntési szabályok
- Alapértelmezetten a legolvashatóbb és legkisebb kockázatú PR-folyamatot javasold.
- Ha a változás túl nagy, javasolj bontást kisebb PR-ekre.
- Ha a branch protection ezt megköveteli, ne javasolj megkerülő megoldást.
- Ha a history átírása vagy force push érintett, ezt külön jelezd.
- Review alatt álló branch esetén konzervatívabb javaslatot adj.

## Jó gyakorlatok
- a PR legyen egyértelmű célú
- a cím legyen rövid és informatív
- a leírás tartalmazza a miértet, ne csak a mit
- a reviewerek számára emeld ki a kritikus részeket
- merge előtt ellenőrizd a CI-t és a nyitott megjegyzéseket
- lehetőleg ne keverd az unrelated változtatásokat ugyanabba a PR-be