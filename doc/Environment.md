# Environment és projektkezeklés

## Cél
A felhasználó számára lehetőséget biztosítani egymástool elkülönített munkaterületek, és munkaterületeken belüli
változóverziók kezelésére. A projektkezelés célja, hogy a felhasználó könnyen tudjon váltani a különböző projektek között, 
és minden projektnek saját lekérdezéskészlete, változókészlete és historyja legyen.

## Projektkezelés
A projektkezelés célja, hogy a felhasználó könnyen tudjon váltani a különböző projektek között. Mindig létezik egy 
alpértelmezett projekt (default project), amely a felhasználó elsődleges munkaterülete, és nem törölhető, nem átnevezhető. 
A felhasználó létrehozhat új projekteket, és a projektek közötti váltás egyszerűen elvégezhető. 
Minden projektnek saját lekérdezéskészlete, változókészlete és historyja van, így a felhasználó könnyen nyomon követheti a változásokat.
A projektek közötti váltás a felhasználói felületen keresztül történik, és a kiválasztott projekt lesz az aktív projekt, 
amelyben a felhasználó dolgozik.
A projektek, és a projekt tartalma a szokásos módon kerül elmentésre. Az eddigi projekt nélküli mentett lekérdezések, változók, 
és historyk megszűnnek.

### Process

- Első induláskor létrejön a default projekt, amely a felhasználó elsődleges munkaterülete. 
- A felhasználó létrehozhat új projektet, vagy duplikálhat egy meglévőt, aminek kötelező egyedi nevet adnia.
- A felhasználó bármikor válthat a projektek között, és az aktív projekt lesz az, amelyben a felhasználó dolgozik. 
Projektváltáskor a projekt tartalma a szokásos módon kerül elmentésre.
- Csak az aktuális projekt tartalma tárolódik a memóriában, projektváltáskor a projekt
 tartalma a mentésből kerül beolvasásra.
- A felhasználó bármikor törölhet egy projektet, kivéve a default projektet, amely nem törölhető. A törlés előtt a 
felhasználót figyelmeztetni kell, kiemelve, hogy a törlés végleges, és a projekt tartalma elveszik. A törlés után a 
felhasználó visszatér a default projektre. A törlés a mentésből is megtörténik.


## Environmentkezelés
A Environmentkezelés (továbbiakban 'Env') célja, hogy egy változó-készletet több értéket is feltudjon venni, és a 
felhasználó könnyen tudjon váltani a különböző értékek között. Minden env egy projekthez tartozik, és a 
projekten belül a felhasználó létrehozhat több env-et is. Minden projekthez tartozik egy alapértelmezett env, 
amely a felhasználó elsődleges változó-készlete, és nem törölhető, de átnevezhető, és a színe is módosítható.
Az env-ek közötti váltás a felhasználói felületen keresztül történik, és a kiválasztott environment lesz az 
aktív environment. Az egyes env-ekhez meg lehet adni színeket, amelyek a felhasználói felületen megjelennek, 
így a felhasználó könnyen tudja azonosítani az aktív environmentet.
Az env-ek, és az env tartalma a szokásos módon kerül elmentésre. Az eddigi env nélküli mentett változók
és értékek megszűnnek.

### Változók tárolási helye, értékeik
A változók nevei a projektben tárolódnak, értékei az env-ekben tárolódnak. Egy változó törlésekor az összes env-ből 
törlődik a változó hivatkozás. Egy új változó létrehozásakor az összes env-ben létrejön a változó hivatkozás, de 
értéke üres lesz. Projekt betöltésekor a konzisztenciát ellenőrizni kell, és ha egy env-ben hiányzik egy változó, 
akkor létre kell hozni a változó hivatkozást, de értéke üres lesz, illetve ha env ben nemlétező változóra történik 
hivatkozás, azt törölni kell.

### Példa

Definiált változók: `url`, `username`

| Változó    | Development env       | Production env         |
|------------|-----------------------|------------------------|
| `url`      | `http://localhost`    | `https://remotehost`   |
| `username` | `admin`               | `prod-admin`           |

Aktív environment: **Development** → a lekérdezésben `{{url}}` értéke `http://localhost` lesz.  
Átváltás **Production**-re → `{{url}}` értéke automatikusan `https://remotehost` lesz, a lekérdezés változatlan marad.

### Process

- Első induláskor létrejön a default environment, amely a felhasználó elsődleges változó-készlete. 
- A felhasználó létrehozhat új environmentet, vagy duplikálhat egy meglévőt, aminek kötelező egyedi nevet adni, és opcionálisan egyedi
színt adni.
- A felhasználó bármikor válthat az environmentek között, és az aktív environment lesz az, amelyben a felhasználó dolgozik.
- Lekérdezéskor minden változó az environmentben megadott értéket veszi fel.
- Az aktív environment információ része a projektnek, így ha egy projekt újra megnyílik, az utoljára használt environment lesz az aktív.
- Environment váltáskor nem történik mentés. Az aktuális projekt minden environmentje a memóriában van, és a felhasználó bármikor visszatérhet az előző environmenthez. 
- A mentés a projekt mentésével történik.

## Felhasználói felület
- Az aktív projekt és environment információ a felhasználói felületen felül a címsorban, vagy a menüsorban megjelenik, 
így a felhasználó mindig tudja, hogy melyik projektben és environmentben dolgozik. Ide kattintva egy legördülő menü 
jelenik meg, amelyből a felhasználó választhat a projektek és environmentek között. A kiválasztás után a váltás azonnal megtörténik, 
és a felhasználói felület frissül az új projekt és environment információval.
- Ha az env-nek van szín megadva, akkor az megjelenik az aktuális environment információ mellett, illetve az 
oldalpanelon a variables blokk mellett is (színes kör), így a felhasználó könnyen tudja azonosítani az aktív 
environmentet.
- Az új projekt, vagy új env gomb a felhasználói felületen a projektek és environmentek listája mellett jelenik meg, 
így a felhasználó könnyen tud létrehozni új projektet vagy environmentet.
- Az új projekt, vagy új env létrehozásakor a felhasználó egy modális ablakban adhatja meg a projekt/env nevét, 
és opcionálisan a színét. A névnek egyedinek kell lennie a projektek/env-ek között. A színek fixek, a felhasználó 
nem adhat meg egyedi színt. A színek a felhasználói felületen megjelennek, így a felhasználó könnyen tudja azonosítani 
az aktív environmentet.
- Menüből elérhető legyen egy "Projekt/env kezelése" menüpont, amely megnyit egy modális ablakot, ahol a felhasználó 
    láthatja az összes projektet/env-et, és ott tudja törölni, duplikálni, vagy átnevezni azokat, illetve 
    environmentnél a színt változtatni. A Változtatások azonnal érvényesülnek, és a felhasználói felület frissül az 
    új projekt/env információval. 