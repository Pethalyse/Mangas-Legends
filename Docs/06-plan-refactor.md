# MangaLegends — Plan de refonte

> Repo analysé : `Pethalyse/Mangas-Legends`, commit `2366901` (2023-11-10), Unity 2023.1.13f1, Mirror 81.4.0.
> Analyse : 2026-11-24. **Claude Code coche les étapes et complète la section « Notes » au fur et à mesure.**

## 1. Audit du code existant

### 1.1 Inventaire

| Zone | Fichiers | Lignes | Remarque |
|---|---|---|---|
| Code de jeu (`Assets/Scripts` + `DescriptionPopup.cs`) | ~45 | ~3 550 | dont `ChampionControleur` 477, `UI_Shop` 315, `StatsManager` 252 |
| Champion implémenté | Ainz (contrôleur + 5 compétences) | ~500 | |
| Systèmes présents | stats + niveaux, objets/boutique/inventaire, sbires (spawn, IA), auto-attaque, 4 types de compétences (zone de soin, skillshot, projectile ciblé, frappe du ciel), boucliers, caméra, HUD, tableau des scores | | |
| Scènes | `PlayerName`, `Lobby`, `Loading`, `Game`, `Test` | | |
| Tiers | Mirror (+ exemples, 8 Mo), ParrelSync (**en double** : `Assets/ParrelSync-master` et `Assets/Plugins/ParrelSync`), SerializedCollections, icônes CaptainCatSparrow, TextMesh Pro | | |
| Hors code | `Run/` (124 Mo) et `Versions/` (68 Mo) : **builds commités** ; `Assets/LoL_icon.svg.png` (**asset d'un jeu existant, à supprimer**) | | |

Ce qui est récupérable : les **assets** (modèle, animations et VFX d'Ainz, icônes, sprites d'UI), les **données** des objets (valeurs, recettes), et les idées (stats avec croissance par niveau, ciblage des sbires). Le code lui-même ne sera pas migré : avec l'abandon de Mirror (ADR-0006) et le passage au modèle par ticks avec prédiction, il sert seulement de référence.

### 1.2 Problèmes bloquants (architecture)

| # | Problème | Où | Conséquence |
|---|---|---|---|
| A1 | **Logique de jeu calculée dans des `ClientRpc`** : dégâts, soins, ralentissements, stats d'objets, montée de niveau sont appliqués sur chaque client | `StatsManager.RpcTakeDamage`, `RpcTakeHeal`, `RpcSetSlow`, `RpcStatsLevelUp`, `ChampionControleur.RpcActualiserStatsSelonItems*` | En serveur dédié rien n'est appliqué ; en hôte les clients divergent ; les `SyncVar` modifiées côté client sont écrasées. Le multi ne peut pas être fiable. |
| A2 | **Détection des impacts par le client du lanceur** puis `CmdTakeDamage(requiresAuthority = false)` | `Skillshot`, `Projectile`, `FromSky`, `HealCircle` | N'importe quel client peut infliger n'importe quels dégâts à n'importe qui (triche triviale). |
| A3 | **Le client envoie le prix** d'achat | `CmdAddItem(key, prix)` | Objets gratuits. |
| A4 | **Commandes envoyées à chaque frame** pour la régénération | `ChampionControleur.Update → CmdRegenerationVie/Mana` | Saturation réseau, régénération dépendante des FPS. |
| A5 | **Classe dieu** et héritage profond `ChampionControleur : StatsManager : TeamManager : NetworkBehaviour` | `ChampionControleur` (input, UI, or, objets, KDA, régénération, AA, triche) | Impossible à tester, chaque champion hérite de tout. |
| A6 | Compétences couplées à l'UI : `GameObject.Find("Ability 1 Icon")`, `Canvas`, lecture d'`Input` dans chaque compétence | `Ability` et sous-classes | Une compétence ne peut pas exister sans HUD ; duplication par compétence. |
| A7 | Registre global par **nom de GameObject** (`"Player" + netId`) dans des dictionnaires statiques | `GameManager` | Fragile, jamais vidé entre parties. |
| A8 | Singletons statiques (`instance`) partout | `GameManager`, `ItemsManager`, `GUIControleur`, `Shop`, `ScoreboardControleur`, `UI_Shop.self` | Couplage caché, ordre d'initialisation fragile. |
| A9 | Masquage du cycle de vie Unity par `new void Awake/Start/Update` | `ChampionControleur`, compétences d'Ainz, `MinionAIAttack` | Méthodes parentes appelées ou non selon le hasard du code. |
| A10 | Ancien `Input Manager` avec touches en dur (`KeyCode.A`, `KeyCode.S`, axes `Ab1`, `QClick`) | partout | Pas de remappage, input dispersé. |
| A11 | Valeurs de gameplay dans les composants/prefabs (portée 6.7f, vitesse 10f…) | compétences | Équilibrage impossible sans ouvrir les prefabs. |

### 1.3 Bugs relevés

| # | Bug | Où |
|---|---|---|
| B1 | Bouclier : `setShield(0)` puis `dmg -= getShield()` soustrait 0 → un bouclier qui casse n'absorbe **rien** du coup qui le casse | `StatsManager.RpcTakeDamage` (×3) |
| B2 | Recette : boucle interne teste `items.Contains(item)` au lieu de `item2` | `ChampionControleur.CmdAddItem` |
| B3 | Vitesses inversées entre sbire normal et sbire canon | `MinionSpawner.spawnRegularMinion/spawnMinionCanon` |
| B4 | Sbire n'attaque que si `transform.position == stoppingPosition` (égalité exacte de flottants) | `MinionAIAttack.TryAttack` |
| B5 | Sbire : `CmdTakeDamage` appelé depuis le serveur ; recharge d'attaque = `attackSpeed` utilisé comme secondes | `MinionAIAttack.Attack` |
| B6 | La mort ne fait rien (`Debug.Log("mort")`), pas d'or ni d'XP | `StatsManager.Death` |
| B7 | `StatCharacter.RemoveAlteration` retire par **valeur** : deux sources de même valeur se confondent | `StatCharacter` |
| B8 | Triche en jeu : clic molette = +niveau, −100 PV, +3 100 or, +1 KDA, présent dans tous les builds | `ChampionControleur.Inputs` |
| B9 | `MantleOfChaos.launchAbility` appelle `setValue` deux fois et tire le critique côté client | Ainz |
| B10 | Barres de vie / mana des autres joueurs incorrectes (bug connu du README) | `ChampionControleur.updateGUI` |
| B11 | Plusieurs fichiers en Windows-1252 (accents cassés) | `StatsManager.cs`, compétences d'Ainz… |

### 1.4 Hygiène du dépôt

- Unity **2023.1** (tech stream, plus supporté) → passer sur **Unity 6 LTS** ([ADR-0005](adr/0005-unity-6-lts.md)).
- `.gitignore` incomplet (builds commités) ; pas de Git LFS pour les modèles/textures.
- Paquets inutiles : `visualscripting`, `collab-proxy`, `ide.vscode` (obsolète).

## 2. Stratégie

Décision du 2026-11-24 : **abandon de Mirror**, serveur autoritaire avec prédiction client, tout en données, code en anglais, Unity 6 LTS.
Conséquence : on ne refactore pas l'ancien code fichier par fichier, on **reconstruit** sur une base saine en récupérant assets et données.

- Le nouveau projet repart dans le même repo, sur une branche de refonte, avec l'ancien code déplacé hors de `Assets/` (dossier `Legacy/`, non compilé, consultable) puis supprimé une fois la refonte jouable.
- Ordre : fondations → cœur testable → prototype réseau risqué → systèmes → contenu.
- Chaque étape = une branche et une PR validée par l'humain, projet qui compile, docs à jour.

## 3. Étapes

### Étape 0 — Hygiène du dépôt
- [x] Branche `rewrite/step-0-hygiene`, PR vers `main` (l'ancienne version reste dans l'historique, commit `2366901`).
- [x] Retirer `Run/`, `Versions/` du suivi git ; `.gitignore` complété (`/Run/`, `/Versions/`, IDE, OS).
- [x] Supprimer `Assets/LoL_icon.svg.png`, Mirror (et exemples), ParrelSync (×2), métadonnées orphelines.
- [x] Déplacer `Assets/Scripts`, `Assets/DescriptionPopup.cs` et les scripts d'objets de `Assets/Resources/Items` vers `Legacy/Scripts` (hors `Assets/`, sans `.meta`, convertis en UTF-8).
- [x] Nettoyer les symboles de compilation hérités (PUN, Mirror, Fusion) dans `ProjectSettings`.
- [x] `.editorconfig` (conventions de nommage), `.gitattributes` (LF, fusion YAML Unity, binaires).
- [ ] Git LFS : reporté. Activer LFS sur des binaires déjà commités sans LFS demande de réécrire l'historique ou de tout re-téléverser ; à décider avant d'importer de gros assets.
- [x] Copier `Docs/` et `CLAUDE.md` dans le repo.
- **Fini quand** : le projet s'ouvre sans erreur de compilation (scènes cassées acceptées).

### Étape 1 — Unity 6 LTS et SDK réseau
Guide pas à pas pour l'humain : [guides/etape-1-unity6-fusion.md](guides/etape-1-unity6-fusion.md).
- [x] Retirer `visualscripting`, `collab-proxy`, `ide.vscode` de `Packages/manifest.json`.
- [x] Migrer vers la dernière Unity 6 LTS (6000.3.26f1).
- [x] Installer Input System 1.20, AI Navigation 2.0, Multiplayer Play Mode 2.0, Test Framework 1.6.
- [x] Créer l'app Photon Fusion 2, importer le SDK (2.1.4), renseigner l'App ID (`Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`).
- [x] Inventorier les objets (`Resources/Items`) → [03-objets.md §4](03-objets.md).
- [ ] Inventorier prefabs et animations récupérables (à faire une fois le projet ouvert en Unity 6).

### Étape 2 — Squelette d'architecture
- [x] `Assets/_Project/`, assembly `MangaLegends.Core` (`noEngineReferences`) ; les autres assemblies seront créées avec leur premier script (Data à l'étape 4, Simulation/Networking à l'étape 5 après import de Fusion).
- [x] Assembly `MangaLegends.Tests.EditMode`, tests verts.
- [x] Tests du Core lançables hors Unity : `dotnet test Tools/CoreTests/Tests` (documenté dans `CLAUDE.md`).
- [ ] `GameLog`, composition root, scène `Boot` → déplacés à l'étape 5 (dépendent de UnityEngine et de Fusion).

### Étape 3 — Core (C# pur, testé)
- [x] `StatType`, `StatModifier`, `StatBlock` (retrait par source, cache par stat).
- [x] `DamageRequest`, `DamageCalculator` (résistances, pénétrations, résistance négative), `CombatRules` (formules paramétrables), `ShieldPool` (boucliers typés puis généraux).
- [x] `StatusEffectSet` (durée en ticks, Refresh / AddStack / Independent / KeepExisting, immunité aux contrôles, restrictions par drapeaux), `ScalingValue`.
- [x] `TickTime`, `CooldownCalculator` (hâte), `DeterministicRandom` (tirages rejouables).
- [x] 35 tests EditMode, verts hors Unity (.NET 8, C# 9).

### Étape 4 — Données
- [x] Core : `ContentId`, `ContentIndex<T>` (index réseau trié par id, identique sur tous les pairs), `ValidationReport`, `StatBaselineTable`, `AbilitySlot`, `RankValues`, `ItemPricing` (coût total, devis d'achat avec composants possédés, revente → corrige A3/B2 côté règles).
- [x] Assembly `MangaLegends.Data` : `UnitDefinition`, `ChampionDefinition` (+ `AbilitySet` alternatifs), `AbilityDefinition`, effets (`Damage`, `Heal`, `Shield`, `ApplyStatus`, `Dash`, `Displace`, `SwapPositions`, `LaunchProjectile`, `CreateArea`, `Summon`, `SwapAbilitySet`), `ProjectileDefinition`, `AreaDefinition`, `StatusEffectDefinition`, `ItemDefinition` (+ passifs, actif), `GameRulesConfig`, `ContentRegistry`.
- [x] Assembly `MangaLegends.Editor` : `TypeSelectorDrawer` (choix du type d'effet dans l'inspecteur), menus *Validate* et *Rebuild Registry From Project*, validation en batchmode.
- [x] Compilation Data + Editor vérifiée hors Unity contre des bouchons (`Tools/CoreTests/DataCheck`) ; 52 tests Core verts.
- [x] Créer les assets dans Unity (`Assets/_Project/Content/`) : règles, registre, 15 objets, 11 statuts, 10 unités (sbires, tours, inhibiteur, Cœur), Ainz et ses 5 compétences, 10 sorts d'invocateur, rappel. Validation : 0 erreur.

### Étape 5 — Prototype réseau (risques d'abord)
- [x] Salon Host / Join par code (`GameLauncher`), mode solo, spawn d'un champion par joueur (`MatchController`, `PlayerSlot`). Hôte + client à tester par l'humain.
- [x] `PlayerInput` + `PlayerInputCollector` (Input System, `MangaControls.inputactions`).
- [ ] `UnitMotor` : clic-pour-bouger prédit sur NavMesh **écrit et testé en solo** ; test avec latence simulée (100–150 ms) à faire.
- [ ] Projectile prédit et faux dégâts : faits (tampon réseau, dégâts flottants) ; compensation de latence reportée (ADR-0007).
- **Fini quand** : deux instances (Multiplayer Play Mode) se tirent dessus avec 150 ms de latence simulée et la sensation reste immédiate. Résultats notés ici.

### Étape 6 — Unités
- [x] `Unit`, `UnitStats`, `Health`, `ResourcePool`, `StatusEffects`, `AutoAttack`.
- [x] Mort, réapparition, or et XP, régénération par tick.
- [x] HUD (PV, mana, recharges, rangs, or, niveau, inventaire, chronomètre), dégâts flottants, barres de vie.

### Étape 7 — Compétences
- [x] `AbilityCaster`, `AbilitySet`, ciblages, effets génériques, tampon de projectiles, zones, canalisation, charges, sorts d'invocateur, rappel.
- [x] Indicateurs de visée (`AimIndicator`) et modes de lancement LoL (normal, rapide avec indicateur, rapide), lancer sur soi (Alt), touches réassignables (page Options).
- [x] **Ainz** complet, uniquement en données (passif par `BehaviourId`).

### Étape 8 — Sbires, tours, jungle
- [x] `MinionBrain`, vagues pilotées par `WaveDirector`, tours (`TowerBrain`, aggro sur harcèlement, dégâts croissants), structures protégées, inhibiteurs qui réapparaissent, méga sbires, monstres (`JungleDirector`, `MonsterBrain`, 9 monstres dont 2 épiques). Valeurs en données.

### Étape 9 — Économie et objets
- [x] Or (`ChampionProgress`), `Inventory`, achat et vente via l'input validés par l'autorité, recettes, UI boutique. Actifs et passifs des objets (touches 1, 2, 3, 5, 6, 7 ; déclencheurs `CombatTriggers`).

### Étape 10 — Flux de partie
- [x] `MainMenu → Match (salon, partie, fin)`, `MatchController`, victoire, horloge (ADR-0007 : salon et sélection dans la scène de match).
- [ ] Suppression de `Legacy/`.
- **Fini quand** : une partie complète 2v2 entre amis, en ligne, se joue du salon jusqu'à la victoire.

### Après la refonte
Champions du premier jalon (GDD §10) : Luffy, Zoro, Naruto, Kirito ; brouillard de guerre ; carte définitive.

## 4. Notes d'avancement

_Claude Code ajoute ici, par étape terminée : date, PR, ce qui a changé, ce qui reste._
- 2026-11-25 (étapes 5 à 10, branche `feat/step-5-simulation`) : couche Simulation complète, entrées, lancement réseau, présentation, UI, monstres, prefabs, scènes `MainMenu` et `Match` (carte de test symétrique, 30 structures, 16 camps, NavMesh). **Testé en solo dans l'éditeur** : vagues, combat sbires/tours, sort d'Ainz, XP, or, mort et réapparition, achat et montée de rang par l'input réseau. Pas encore testé : hôte + client en ligne, latence simulée, clavier réel (les touches simulées par la CLI n'atteignent pas l'Input System hors focus). Écarts : ADR-0007.

- 2026-11-25 (étape 1) : Unity 6000.3.26f1, Input System, AI Navigation, Multiplayer Play Mode, Fusion 2.1.4 et App ID. Paquet `com.unity.pipeline` ajouté pour piloter l'éditeur depuis la CLI `unity`. Exemples de l'ancien TextMesh Pro supprimés (ne compilaient plus avec le TMP intégré à Unity UI 2.0). Console sans erreur, 53 tests EditMode verts dans Unity. Corrigé : `ItemPricing.SellValue` donnait 804 au lieu de 805 sous Mono (arrondi float), calcul passé en `decimal`. Les projets `Tools/CoreTests/Tests` et `DataCheck` n'avaient jamais été commités (règle `*.csproj` du `.gitignore`) : recréés et suivis. Reste : inventaire des prefabs et animations.

- 2026-11-24 (PR #4) : complément de l'étape 4. `GameRulesConfig` gagne les règles de structures (bonus des tours en début de partie, réapparition de l'inhibiteur) et le remplacement siège → méga sbire, avec une validation. Valeurs à 0 en attendant l'équilibrage.
- 2026-11-24 (PR #4) : toutes les valeurs chiffrées des compétences, projectiles, zones et effets passent en `RankedFloat` / `RankedInt` ; validation des rangs des projectiles et zones depuis la compétence (corrige une fausse erreur sur les dégâts par rang d'un projectile).
- 2026-11-24 (PR #4) : ratios par rang (`ScalingRatio.RatioAt`, `ScalingRatioData.RatioByRank`), 53 tests verts.
- 2026-11-24 (PR #4) : règles du genre (LoL 2026) dans `07-regles-de-jeu.md` ; `GameRulesConfig` : rythme des vagues (`TimedValue`), réapparition de l'inhibiteur à 300 s par défaut.
- 2026-11-24 (PR #4) : sorts d'invocateur en données (`SummonerSpellDefinition`, `TeleportEffect`, repli de ciblage, `SummonerSpellSlots`).

### Étape 0 — 2026-11-24
- Dépôt nettoyé (builds, Mirror, ParrelSync, icône LoL), ancien code isolé dans `Legacy/`.
- **Conséquences attendues à l'ouverture dans Unity** : les scènes, prefabs et assets d'objets/stats référencent des scripts déplacés → « Missing Script ». Normal : le contenu sera recréé aux étapes 4 à 7. Les valeurs des objets restent lisibles dans les `.asset` (YAML) pour la migration.
- Non vérifié dans l'éditeur (pas d'Unity dans l'environnement de Claude) : à confirmer par l'humain à l'ouverture du projet.
- Reste : Git LFS (voir ci-dessus).

### Étape 4 (code) — 2026-11-24
- Modèle de données complet et outils d'éditeur, sans assets : ceux-ci se créent dans l'éditeur (guide de reprise §2).
- Non vérifié dans Unity : compilation réelle, rendu de l'inspecteur, sérialisation des listes `[SerializeReference]`.

### Étapes 1 à 3 — 2026-11-24
- Étape 1 : manifest nettoyé, inventaire des objets, guide pour l'humain. **Reste côté humain** : Unity 6 LTS, paquets, Fusion 2 (voir le guide).
- Étapes 2–3 : Core en C# pur et ses tests (35 verts avec `dotnet test`). Les `.meta` ont été générés à la main : Unity les gardera tels quels.
- Non vérifié dans Unity : compilation des asmdef et Test Runner à confirmer à l'ouverture.
- Corrige les bugs B1 (bouclier qui casse n'absorbait rien) et B7 (modificateurs retirés par valeur) au niveau des règles.
