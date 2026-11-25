# Journal de développement

> Une entrée par session de travail, la plus récente en haut. Format :
> `## AAAA-MM-JJ — <titre>` puis **Fait**, **Décisions**, **Docs mis à jour**, **Prochaine étape**, **Bloquants**.

## 2026-11-25 — Lag corrigé, Summoner's Rift, sbires sur leur voie, retour à la base, barres de vie

**Fait** (retours du propriétaire) :
- **Lag** : le jeu tournait à ~0,5 image/s (spirale de rattrapage : chaque tick coûtait ~30 ms, Fusion en enchaînait 60 par image). Causes mesurées au profileur : chemins NavMesh recalculés à chaque tick par les sbires, échantillonnage NavMesh à chaque pas, séparation en O(n²), filtres coûteux avant la distance dans `Closest`. Maintenant ~11 ms par image avec les vagues et la jungle.
- **Carte** reconstruite sur les positions de Summoner's Rift (coordonnées LoL converties), symétrie centrale : bases carrées avec trois sorties et deux entrées latérales, fontaine qui tue les ennemis, tourelles T1/T2 décalées sur le côté, bords collés aux voies, jungle avec alcôves, fosses aux places de LoL.
- **Sbires** : ne quittent plus leur voie (zone NavMesh `Lane`, coût 4× ailleurs pour eux) ; les vagues se rencontrent au milieu (testé : centre de la carte au mid, coin de la voie du haut).
- **Retour à la base** (Homeguard) : +80 % (+150 % après 14:00) après réapparition ou rappel, perdu en combat ou hors des voies.
- **Barres de vie** plus grandes à la LoL (niveau, nom, mana, bouclier), cachées pour les structures encore protégées ; touches des objets affichées 1 2 3 5 6 7 en AZERTY.

**Testé** : 0 sbire hors voie sur 70, rencontres au milieu, Homeguard 3,30 → 5,94 m/s puis retiré en jungle, laser de fontaine, barres ; 0 erreur.

**Collision** (question du propriétaire) : LoL a une collision entre unités (wiki : unit collision) ; ici, collision douce côté hôte pour sbires et monstres, sans détour : avec le coût des voies, un sbire ne contourne plus par la jungle.

**À trancher** : fichiers TextMesh Pro modifiés par Unity (réimport automatique), non commités.

**Prochaine étape** : test en ligne, modèles des autres champions, zone de Naruto.

## 2026-11-25 — Legacy supprimé, tirage des drakes, Ogrest invoqué, Haki de l'observation

**Fait** :
- `Legacy/` supprimé (accord du propriétaire ; reste dans l'historique git). Étape 10 cochée.
- **Drakes** (règle précisée par le propriétaire) : 1er tiré au hasard, 2e parmi les autres, puis toujours le même élément à partir du 3e (élément de la carte), donc l'âme est de cet élément. Règle pure `ElementRotation` dans Core, 43 cas de test.
- **Ogrest** : l'Œil remplace le bijou du tueur pendant 240 s ; il invoque un Ogrest allié qui fonce sur la tourelle ennemie la plus proche (3 000 dégâts bruts).
- **Haki de l'observation** : second bijou, à choisir à la boutique depuis la fontaine ; révèle et désactive les balises à 6 m pendant 8 s.

**Testé dans l'éditeur** : tirage Électhor → Lugia → Artikodin ×2 → âme d'Artikodin → Shenron ; Ogrest invoqué : tourelle centrale 9 000 → 6 000 PV, bijou rétabli ; Haki : balise ennemie invisible puis révélée. 96 tests Core verts, 0 erreur en console, contenu valide.

**Réponse au propriétaire** : chaque drake a son bonus cumulable **et** l'âme existe (4e drake), avec un effet propre à chaque élément.

**Reste** : modèles 3D, zone de Naruto (à décider), test en ligne à deux.

**Bloquants** : aucun.

## 2026-11-25 — Objectifs épiques, balises, ligne de vue, Demon slash, décor

**Fait** (même branche) :
- **Objectifs épiques** : drakes en rotation (Sulfura, Artikodin, Électhor, Lugia) avec cumuls d'équipe gardés à la mort ; **âme** au 4e drake ; puis **Shenron** (exécution sous 20 % pendant 150 s) ; **Zodd** renforce les sbires proches et divise le rappel par 2 ; **Pitou, Pufu, Youpi** (8:00 → 14:45, dégâts bonus aux structures) ; **Ogrest** (15:00 → 19:45, l'œil inflige 3 000 à une tourelle) ; apparition au niveau moyen des champions.
- **Balises** : bijou Insectes de Shino sur la touche 4 (3 max, 90 s, recharge 170 → 90 s), Den Den Mushi de surveillance (75 or, révèle et désactive les balises ennemies) ; 1 PV par attaque de base ; ignorées par l'IA.
- **Les murs bloquent la vue** (calque `Terrain`).
- **Zoro** : Demon slash (3e Nitoryu, 150 %).
- **Décor** : arbres sur les murs et autour de la carte, rochers, fontaines.

**Testé dans l'éditeur** : 4 drakes → âme de Lugia → Shenron ; Zodd (rappel −50 %) ; larves (×3) ; Ogrest (œil) ; à la mort, cumuls de drake, âme et larves gardés, Shenron/Zodd/œil perdus ; balise ennemie invisible puis révélée par le Den Den Mushi, 1 PV par coup, compétences sans effet ; point derrière un mur caché ; cycle Nitoryu → Demon slash. 0 erreur, 53 tests verts, contenu valide.

**Décisions** (valeurs choisies par Claude, 07 §7.4 et 03 §5) : rotation fixe des drakes, âmes simplifiées (Artikodin réagit aux dégâts, Lugia soigne d'un coup), Ogrest simplifié (pas encore d'Ogrest invoqué qui charge), bijou = compétence du lanceur dont le rang suit le niveau.

**Docs mis à jour** : règles §7.3–7.4 et §10, objets §5, champions (Zoro), architecture, plan, ADR-0007.

**Reste** : Ogrest invoqué, Haki de l'observation (changement de bijou), modèles 3D, zone de Naruto (à décider), suppression de `Legacy/` (sur accord), test en ligne.

**Bloquants** : aucun.

## 2026-11-25 — Carte et HUD à la LoL, rivière et fruits du démon

**Fait** (même branche `feat/aim-indicators-controls`) :
- **Carte au tracé de LoL** (primitives) : bases dans les coins avec murs et trois sorties, rivière en diagonale jusqu'aux voies du haut et du bas, fosses de Zodd et du drake, quatre quadrants de jungle symétriques, 20 buissons, bords de carte. Korogu déplacé en miroir de Pompoko. NavMesh refait ; tous les camps, structures, points de voie et fontaines sont atteignables (vérifié par script).
- **Buissons** : une unité dans un buisson n'est vue que depuis le même buisson.
- **Rivière et fruits du démon** (décision du propriétaire : la rivière est le seul point d'eau pour l'instant) : trait `DevilFruitUser` (Luffy), −1,5 % des PV max par seconde dans la rivière, jamais mortel.
- **HUD à la LoL** : stats, portrait, niveau et XP, passif et 4 compétences avec rangs, sorts d'invocateur, PV et mana avec régénération, buffs, inventaire avec touches et rappel, or ; score en haut à droite ; cadres des alliés à gauche ; **minicarte** (clic gauche = caméra, clic droit = déplacement) ; annonces (premier sang, multi-frags, tourelles, objectifs épiques) et fil des frags.

**Testé dans l'éditeur** : rendu de la carte vue de dessus, HUD et minicarte, Luffy dans la rivière (640 → 598 PV en ~4 s), annonce de tourelle détruite. 0 erreur en console.

**Décisions** : valeurs de la rivière (1,5 %/s, non mortel), ADR-0007 §7 et §9 mis à jour. Zone de Naruto (Vallée de la Fin) : reportée par le propriétaire.

**Docs mis à jour** : architecture, GDD §6, règles §10, champions (Luffy), plan, ADR-0007.

**À vérifier par l'humain** : clics sur la minicarte, lisibilité du HUD en 1080p, déplacements dans la nouvelle jungle.

**Prochaine étape** : balises (bijou « Insectes de Shino » sur la touche 4, Den Den Mushi), murs qui bloquent la vue, Zodd (renfort des sbires et du rappel), âmes de drake et Shenron, Ogrest et larves, *Demon slash*, décor de la carte.

**Bloquants** : aucun.

## 2026-11-25 — Visée LoL, options, passifs, Luffy, Kirito, brouillard de guerre

**Fait** (branche `feat/aim-indicators-controls`, empilée sur `feat/step-5-simulation`) :
- **Visée comme dans LoL** : modes *normal* (touche = indicateur, clic gauche lance, clic droit ou Échap annule), *rapide avec indicateur* (maintenir, relâcher) et *rapide*, réglables touche par touche ; Alt + touche lance sur soi ; l'attaque-déplacement suit le même réglage. Indicateurs au sol : cercle de portée, ligne ou éventail des projectiles, cône, cercle au curseur borné à la portée, rouge si le sort n'est pas prêt.
- **Page Options** (menu principal, ou Échap en partie) : réassignation de toutes les touches (une touche déjà prise passe à l'action libérée, comme LoL), mode de lancement par touche, boutons « tout en … », réinitialisation. Sauvegarde locale. Le HUD affiche les touches réelles selon la disposition du clavier (AZERTY : A Z E R, objets sur & é " ( - è).
- **Actifs d'objets** sur 1, 2, 3, 5, 6, 7 ; **passifs d'objets** et déclencheurs de combat (`CombatTriggers`) ; actif du Sceptre de la lune relié.
- **Zoro** : passif Haki de l'armement. **Totoro** : ralentissement et brûlure sur attaque. **Luffy** (Jungle) et **Kirito** (Top) jouables en données.
- **Sbires et monstres** qui s'écartent les uns des autres (côté hôte).
- **Brouillard de guerre** côté client (rayons de vision LoL arrondis ; Wado Ichimonji +2 m).

**Testé dans l'éditeur (solo, piloté par la CLI)** : indicateurs (cercle de portée, marqueur de Flash, ligne de l'ultime d'Ainz), page Options, touches AZERTY du HUD ; Haki de Zoro (+1 AD par coup) ; brûlure de Totoro sur un sbire ; Kirito (marques, 3e coup, Starburst Stream à 5 marques = 413 dégâts, marques consommées) ; Luffy (Gatling : 8 coups en 2 s, Luffy immobile ; Gear) ; sbires sans empilement ; brouillard (6 ennemis visibles sur 36, camps masqués). 0 erreur en console, 53 tests verts, validation du contenu OK.

**Décisions** : défaut des modes = normal partout (comme le client LoL) ; Haki de Zoro plafonné à 30 cumuls et perdu à la mort ; rôle Top pour Kirito ; ADR-0007 §8 (réglages locaux, forme des indicateurs déduite des effets) et §9 (brouillard côté client). Valeurs de Luffy, Kirito, Totoro, Sceptre : 02, 03, 07.

**Corrigé** : le point 7 de l'ADR-0007 avait été effacé par une substitution ratée, rétabli. `DataCheck` recompile (bouchons `Mathf`, `TooltipAttribute`).

**Docs mis à jour** : architecture (§12, §15), plan (étapes 7 et 9, après la refonte), champions, objets, règles §7.4, ADR-0007.

**À vérifier par l'humain** : au vrai clavier et à la souris, les trois modes de lancement, la réassignation d'une touche (cliquer, appuyer, vérifier le HUD), Alt + touche ; partie en ligne à deux.

**Prochaine étape** : Zodd (renfort des sbires proches et rappel accéléré), âmes de drake et Shenron, Ogrest et larves, montée en puissance des objectifs épiques ; *Demon slash* de Zoro ; zones de terrain (eau de Luffy, zone de Naruto : à définir avec le propriétaire) ; buissons ; carte définitive ; suppression de `Legacy/` (sur accord).

**Bloquants** : aucun.

## 2026-11-25 — Étapes 5 à 10 : le jeu tourne en solo

**Fait** : assemblies `Simulation`, `Input`, `Networking`, `Presentation`, `UI` (ajoutées au weaving Fusion). Simulation complète (unités, stats, PV et boucliers, statuts, NavMesh prédit, attaques, compétences, projectiles, zones, canalisation, sorts d'invocateur, rappel, XP, or, inventaire, primes, vagues, tours, structures protégées, inhibiteurs, jungle, victoire). Entrées (`PlayerInputCollector`), lancement réseau (`GameLauncher` : héberger, rejoindre par code, solo). Présentation (caméra, vues d'unités, projectiles, zones, attaques). UI en code : menu, salon, HUD, boutique, tableau des scores, barres de vie, dégâts flottants, fin de partie. Contenu : 9 monstres de jungle et 4 bonus, prefabs (unités, runner, slot joueur), scènes `MainMenu` et `Match` (carte de test symétrique de 160 m, 3 voies, rivière, murs, 30 structures, 16 camps, NavMesh).

**Testé dans l'éditeur (mode solo, piloté par la CLI)** : le salon se crée, la partie démarre, 31 unités ; vagues toutes les 30 s et combats sbires/tours ; jungle à 0:55 ; Ainz se déplace, lance Mantle of Chaos (recharge et mana affichées), gagne XP et or, meurt sous les sbires puis réapparaît à la fontaine ; montée de rang et achat passent bien par l'input réseau. 0 erreur en console, 53 tests verts, validation du contenu sans problème.

**Décisions** : ADR-0007 (impacts sans compensation de latence, salon dans la scène de match, services par runner, UI en code). Valeurs des monstres et des tours : 07 §7.4.

**Docs mis à jour** : plan (étapes 5 à 10), architecture (§15), champions (Ainz jouable), règles (07 §7.4), ADR-0007.

**À vérifier par l'humain** :
- clavier et souris réels (les touches simulées par la CLI n'atteignent pas l'Input System quand la fenêtre Game n'a pas le focus) ;
- partie hôte + client (deux PC, ou Multiplayer Play Mode) et ressenti avec latence ;
- caméra et lisibilité.

**Fait aussi** : Naruto et Zoro jouables en données (testé : Rasengan avec approche automatique, shurikens), rôles Mid et Top choisis.

**Prochaine étape** : test en ligne à deux, indicateurs de visée, passifs et actifs d'objets (passifs de Zoro, Totoro), sbires qui s'écartent les uns des autres, Luffy et Kirito, brouillard de guerre, suppression de `Legacy/`.

**Bloquants** : aucun.

## 2026-11-25 — Étape 1 : Unity 6 LTS et Fusion 2

**Fait** : projet migré en Unity 6000.3.26f1 ; Input System 1.20, AI Navigation 2.0, Multiplayer Play Mode 2.0, Test Framework 1.6 ; Photon Fusion 2.1.4 importé, App ID renseigné. Paquet `com.unity.pipeline` ajouté : Claude Code pilote désormais l'éditeur ouvert avec la CLI `unity` (console, compilation, tests, menus, assets). Vérifications du guide de reprise §1 faites : 0 erreur ni avertissement en console, 53 tests EditMode verts dans Unity, aucun `.meta` de `Assets/_Project` régénéré, *Validate* s'exécute (« No ContentRegistry asset found », attendu).

**Corrigé** :
- `Assets/AssetsStore/TextMesh Pro/Examples & Extras` supprimé : 21 erreurs de compilation avec le TextMesh Pro intégré à Unity UI 2.0, aucune référence dans le projet. Polices et ressources de l'ancien TMP conservées.
- `ItemPricing.SellValue` : `1150 × 0.7f` donnait 804 sous Mono et 805 sous .NET ; calcul en `decimal`, identique partout.
- `Tools/CoreTests/Tests` et `DataCheck` n'existaient que sur la machine d'origine (le `.gitignore` excluait `*.csproj`) : projets recréés, exception `!/Tools/**/*.csproj` ajoutée. `RollForward` pour tourner avec un SDK plus récent que .NET 8.
- Éditeur bloqué sur « Opening project... » (lancé hors du Hub) : fermé et relancé depuis le Hub.

**Docs mis à jour** : plan (étape 1 cochée + note), CLAUDE.md (commandes CLI Unity, SDK .NET), guide de reprise.

**Prochaine étape** : guide de reprise §2, création des assets de l'étape 4 dans l'éditeur (règles, registre, objets, statuts, unités, Ainz, sorts d'invocateur), puis étape 5.

**Bloquants** : aucun. Reste de l'étape 1 : inventaire des prefabs et animations récupérables.

## 2026-11-24 — Correction des rôles

**Fait** : le rôle « Tireur » n'existe pas ; les rôles sont Top, Jungle, Mid, Bot, Support (le champion du Bot est l'ADC). GDD §4.2, fiche de Pain et enum `ChampionRole` (`Marksman` → `Bot`) corrigés.

**Fait (2)** : invocations précisées. Seul Sung Jin-Woo contrôle une invocation (Igris) ; les clones de Naruto (E, R) et le chien de Pain sont des animations d'attaques (dash, zone, ligne). GDD §4.3 et fiches mises à jour ; aucune donnée à changer (`SummonEffect` reste réservé aux vraies unités).

**Fait (3)** : pas de type de dégâts « chaos » : ce sont des dégâts magiques (GDD §4.4 et §10, fiche d'Ainz). Le code n'a que Physical, Magical, True.

**Fait (4)** : jeu de fans privé assumé, les noms de licences restent tels quels (GDD §1.2, plus aucune proposition de renommage) ; monstres de jungle définis (GDD §7.1) avec leur rôle et la compétence absorbée par Rimuru ; récompenses et positions en questions ouvertes. Zodd et Ogrest partagent volontairement la même compétence (peut évoluer).

**Fait (5)** : structures de la carte (GDD §6.1) : 3 tours par voie, un inhibiteur par voie à l'entrée de la base, 2 tours devant le Cœur, le Cœur. Les types `Tower` et `Structure` de `UnitKind` suffisent, aucun code à changer ; règles de destruction en questions ouvertes.

**Fait (6)** : règles des structures (GDD §6.1) : ordre de destruction, méga sbires après la chute d'un inhibiteur, or des structures, tours renforcées en début de partie. `GameRulesConfig` : sections *Structures* et *Minion waves* + `Validate` ; DataCheck compile. Valeurs à équilibrer en questions ouvertes.

**Fait (7)** : sorts d'invocateur (GDD §2.1) : `SummonerSpellDefinition`, `TeleportEffect`, `TargetingData.Fallback`, `GameRulesConfig.SummonerSpellSlots` ; ressource : mana par défaut, autre seulement si la fiche le précise (déjà `ResourceKind`). DataCheck compile.

**Fait (8)** : coût continu (Pain, Shuradô) : `AbilityDefinition.CostPerSecond` par rang. DataCheck compile.

**Fait (9)** : décision du propriétaire, les règles suivent League of Legends 2026. Recherche (notes 26.1, aperçu de saison, wiki) et nouveau `Docs/07-regles-de-jeu.md` (✅ vérifié 2026 / 🔁 référence à revérifier) ; questions de règles générales fermées dans le GDD ; CLAUDE.md mis à jour ; `GameRulesConfig` : rythme des vagues et réapparition de l'inhibiteur. Restent les questions propres au contenu manga (07 §12).

**Fait (10)** : réponses du propriétaire : Sulfura, Shenron et Zodd gardent les buffs du genre, les autres drakes seront adaptés ; Ignite, Cleanse et Clarity ajoutés (10 sorts) ; propositions de monstres et objets manga notées (07 §12) en attente de choix.

**Fait (11)** : les deux PDF de conception sont versionnés dans `Docs/sources/` (CLAUDE.md et README des docs les référencent).

**Fait (12)** : formule des sorts confirmée (base par rang + ratios de stats) ; les ratios peuvent maintenant varier par rang (Core `ScalingRatio`, Data `ScalingRatioData`), test ajouté, 53 tests verts, DataCheck compile.

**Fait (13)** : toute valeur chiffrée d'un sort peut varier par rang ou rester constante (`RankedFloat`, `RankedInt`, `RankedStatModifierData` pour la force des statuts). Bug corrigé : les effets des projectiles et zones étaient validés avec 1 rang, donc des dégâts par rang y auraient été signalés en erreur. DataCheck compile ; le renommage de propriétés (`XAt(rank)`) reste à vérifier dans Unity.

**Fait (14)** : consigne du propriétaire, Claude choisit seul les valeurs et petits détails (CLAUDE.md, guide de reprise, 07) ; arrêt du temps et vision globale restent déséquilibrés exprès (GDD §10.5).

**Fait (15)** : choix du propriétaire : Pitou, Pufu, Youpi (larves), Magicarpe (rivière), bottes = équipement tridimensionnel en 7 versions + améliorations (03 §5.1, noms et effets choisis par Claude), carte au trésor de Nami, insectes de Shino, Den Den Mushi de surveillance, haki de l'observation ; pas de cercles de champignons. Reste : compétences absorbables par Rimuru sur les 4 nouveaux monstres.

**Fait (16)** : compétences absorbées par Rimuru sur Pitou (bond), Pufu (silence court), Youpi (coup en zone) et Magicarpe (« Trempette », sans effet) validées par le propriétaire (GDD §7.1).

**Fait (17)** : buffs d'Artikodin (bouclier de glace), Électhor (éclair sur attaque de base) et Lugia (soin sur dégâts) validés ; toutes les âmes ont une recharge ; valeurs choisies par Claude (07 §7.3).

**Fait (18)** : compagnons du jungler = fées de Zelda (Navi vitesse, fée de la Grande Fée défense, Taya offensive), 03 §5.4.

## 2026-11-24 — Étape 4 : modèle de données (sans Unity)

**Fait** : identifiants et index de contenu, validation, prix des objets (Core, 52 tests verts) ; assembly Data (toutes les définitions et les effets) ; assembly Editor (sélecteur de type d'effet, validation, reconstruction du registre) ; compilation vérifiée contre des bouchons d'API Unity ; guide de reprise pour Claude Code connecté à Unity.

**Décisions** : effets en `[SerializeReference]` avec sélecteur de type ; durée de statut surchargeable par compétence ; index réseau du contenu = ordre des ids.

**Docs mis à jour** : plan, architecture §11, CLAUDE.md, README des docs, guide de reprise.

**Prochaine étape** : dans Unity, vérifications et création des assets (guide de reprise), puis étape 5.

**Bloquants** : Unity indisponible pour l'instant chez l'humain ; étape 1 en attente.

## 2026-11-24 — Étapes 1 à 3 : préparation moteur et Core

**Fait** : paquets inutiles retirés, objets de l'ancien prototype inventoriés, guide Unity 6 + Fusion 2 pour l'humain, assembly `MangaLegends.Core` (stats, dégâts, boucliers, statuts, mise à l'échelle, ticks, aléatoire déterministe) et 35 tests EditMode verts hors Unity.

**Décisions** : commits et PR au nom de Pethalyse uniquement, sans attribution d'outil.

**Docs mis à jour** : plan, architecture (§4 arborescence, §6 unités des stats, §7.1 formules), objets (§4), CLAUDE.md (commandes), guide d'étape 1.

**Prochaine étape** : l'humain fait l'étape 1 dans l'éditeur ; ensuite étape 4 (données).

**Bloquants** : compilation dans Unity non vérifiée.

## 2026-11-24 — Étape 0 : hygiène du dépôt

**Fait** : builds retirés du suivi, Mirror/ParrelSync/icône LoL supprimés, ancien code déplacé dans `Legacy/` (UTF-8), symboles de compilation nettoyés, `.editorconfig`, `.gitattributes`, `.gitignore`, docs et `CLAUDE.md` ajoutés au repo.

**Docs mis à jour** : plan (étape 0 cochée + notes), README des docs, README du repo.

**Prochaine étape** : étape 1 (Unity 6 LTS, Photon Fusion 2, Input System).

**Bloquants** : ouverture du projet dans Unity à vérifier par l'humain (scripts manquants attendus) ; décision Git LFS.

## 2026-11-24 — Décisions techniques de l'équipe

**Décisions** (MC MEZ)
- Abandon de Mirror. Service multijoueur avec calcul et validation côté serveur, client prédictif (animations et faux dégâts immédiats).
- Tout gameplay configurable en données ; code en anglais sans commentaires superflus ; Unity 6 LTS ; jeu réservé à un cercle d'amis.
- ADR 0002, 0003, 0004, 0005 acceptés ; 0001 abandonné ; 0006 (Photon Fusion 2) accepté.

**Docs mis à jour** : architecture (v0.2, modèle par ticks et prédiction), conventions (§1, §5, exemple), plan (refonte au lieu de refactor, nouvelle étape 5 de prototype réseau), GDD (§1, §9), CLAUDE.md.

**Prochaine étape** : étape 0 (hygiène du dépôt), qui demande un accès en écriture au repo.

## 2026-11-24 — Création de la documentation de conception

**Fait**
- Lecture des deux PDF de conception (kits des champions, un objet).
- Audit du repo `Pethalyse/Mangas-Legends` (commit `2366901`) : 11 problèmes d'architecture, 11 bugs, hygiène du dépôt.
- Rédaction de : GDD, champions, objets, architecture cible, conventions, plan de refactor, ADR 0001–0005, CLAUDE.md.

**Décisions** (toutes « Proposé », à valider par l'équipe)
- Garder Mirror, serveur autoritaire, compétences pilotées par les données, code en anglais, Unity 6 LTS.

**Prochaine étape** : validation des docs, puis étape 0 du refactor (hygiène du dépôt).

**Bloquants / questions** : GDD §10 (questions ouvertes) ; divergences entre les deux PDF (02-champions §2).
