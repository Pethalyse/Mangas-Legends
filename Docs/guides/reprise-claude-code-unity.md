# Reprise par Claude Code connecté à Unity

> Pour l'instance de Claude Code qui travaille avec l'éditeur Unity ouvert. Le travail fait jusqu'ici l'a été **sans Unity** : le C# pur est compilé et testé avec .NET, le reste est seulement compilé contre des bouchons d'API. Ce guide liste ce qui reste à vérifier et à créer dans l'éditeur, dans l'ordre.

## 0. Contexte à lire

`CLAUDE.md`, puis `Docs/06-plan-refactor.md` (« Notes d'avancement »), `Docs/04-architecture.md`, `Docs/05-conventions-code.md`, et les dernières entrées de `Docs/journal.md`.

## 1. Vérifications à l'ouverture (avant tout nouveau code)

| # | Vérification | Attendu |
|---|---|---|
| 1 | Étape 1 faite par l'humain ? (Unity 6 LTS, Input System, AI Navigation, Multiplayer Play Mode, Fusion 2) | sinon, la guider avec `Docs/guides/etape-1-unity6-fusion.md` |
| 2 | Console après import | **0 erreur de compilation** ; des « Missing Script » dans les anciennes scènes et prefabs sont normaux |
| 3 | Assemblies `MangaLegends.Core`, `.Data`, `.Editor`, `.Tests.EditMode` | visibles, sans avertissement d'asmdef |
| 4 | *Test Runner → EditMode → Run All* | tous verts (52 au 2026-11-24) |
| 5 | Les `.meta` sous `Assets/_Project` | Unity ne les a pas régénérés (GUID inchangés dans `git status`) |
| 6 | Menu *MangaLegends → Content → Validate* | s'exécute (« No ContentRegistry asset found » tant que l'étape 2 n'est pas faite) |
| 7 | Inspecteur d'un `AbilityDefinition` | la liste *On Cast* affiche un menu déroulant pour choisir le type d'effet (`TypeSelectorDrawer`) |

Si un point échoue, le corriger avant d'aller plus loin et le noter dans le journal.

## 2. Créer les assets de données (fin de l'étape 4)

Tout dans `Assets/_Project/Content/` (dossiers `Rules/`, `Items/`, `Status/`, `Units/`, `Champions/Ainz/`). Les ids suivent `domaine.nom` en snake_case (`ContentId`).

1. **Règles** : `Create → MangaLegends → Game Rules` → `RULES_Default` (valeurs par défaut du script).
2. **Registre** : `Create → MangaLegends → Content Registry` → `CONTENT_Registry`, y glisser `RULES_Default`.
3. **Objets** : recréer les 14 objets de `Docs/03-objets.md` §4 en `ItemDefinition`. Réutiliser les icônes des anciens assets (`Assets/Resources/Items/**`). Ids proposés :

| Ancien nom | Id | Tier |
|---|---|---|
| Bouclier en cuir | `item.leather_shield` | Basic |
| Bracelet de précision | `item.precision_bracelet` | Basic |
| Circuit de mana | `item.mana_circuit` | Basic |
| Dague | `item.dagger` | Basic |
| Grimoire des arcanes | `item.arcane_grimoire` | Basic |
| Lame | `item.blade` | Basic |
| Minerai étrange | `item.strange_ore` | Basic |
| Orbe de résistance | `item.resistance_orb` | Basic |
| Pierre de régénération | `item.regeneration_stone` | Basic |
| Veine de vie | `item.life_vein` | Basic |
| Lame d'acier de qualité supérieure | `item.superior_steel_blade` | Component |
| Pierre magique de renforcement | `item.reinforcement_stone` | Component |
| Wado Ichimonji | `item.wado_ichimonji` | Complete |
| Sceptre de la lune | `item.moon_scepter` | Complete |

   Unités des stats : pourcentages en fraction (`+10 %` vitesse d'attaque → `PercentAdd 0.1` sur `AttackSpeed`, ou valeur à trancher avec l'humain ; `+15 % critique` → `Flat 0.15` sur `CritChance`).
4. **Statuts de base** (`StatusEffectDefinition`) : `status.stun`, `status.root`, `status.silence`, `status.slow` (modificateur `MoveSpeed PercentMult` négatif), `status.knockup`. Un seul asset par statut : la durée propre à chaque compétence se règle dans `ApplyStatusEffect` (*Duration Override Seconds*).
5. **Unités** : `UNIT_Minion_Melee`, `UNIT_Minion_Caster`, `UNIT_Minion_Siege` (stats à reprendre des anciens prefabs de sbires dans `Assets/Prefabs/Minions`, à lire dans l'inspecteur), `UNIT_Minion_Mega` (stats à demander à l'humain), puis les structures `UNIT_Tower_Outer`, `UNIT_Tower_Inner`, `UNIT_Tower_Base`, `UNIT_Tower_Core`, `UNIT_Inhibitor`, `UNIT_Core` (GDD §6.1, stats et or à demander). Dans `RULES_Default`, renseigner *Siege Minion* et *Mega Minion*.
6. **Ainz** : `CHAMP_Ainz` (stats depuis `Assets/Resources/Champion/AinzOoalGown`), puis ses 5 compétences d'après `Docs/02-champions.md` :
   - A : `LaunchProjectileEffect` → projectile avec `DamageEffect` (Magical) + `ApplyStatusEffect` (slow) ;
   - Z : `ShieldEffect` (filtre Magical, `BlocksCrowdControl`) sur un allié ;
   - E : `CreateAreaEffect` → zone avec `HealEffect` par intervalle ;
   - R : projectile longue portée → zone d'impact avec dégâts + réduction de résistances (statut avec modificateurs) ;
   - P : comportement spécial (`BehaviourId`) : régénération de mana hors combat, à coder à l'étape 7.
   Les valeurs `?` restent à demander à l'humain : ne pas les inventer, mettre 0 et l'ajouter aux questions ouvertes du GDD.
6 bis. **Sorts d'invocateur** : les 10 sorts de `Docs/01-gdd.md` §2.1, chacun un `ABIL_Summoner_*` (rang 1, Smite en 3 rangs 600/1000/1400) et un `SPELL_*` qui le référence. Valeurs inconnues à 0.
7. *MangaLegends → Content → Rebuild Registry From Project*, puis *Validate* : 0 erreur.

## 3. Suite du plan

Étape 5 (prototype réseau) : c'est le point le plus risqué. Suivre `Docs/06-plan-refactor.md` et `Docs/04-architecture.md` §3, §5, §8 et §14. Créer les assemblies `MangaLegends.Simulation`, `.Networking`, `.Input`, `.Presentation`, `.UI` avec leur premier script et les ajouter aux *Assemblies to Weave* de Fusion.

## 4. Outils disponibles sans l'éditeur

- `dotnet test Tools/CoreTests/Tests` : tests du Core.
- `dotnet build Tools/CoreTests/DataCheck` : compile Data et Editor contre des bouchons d'API Unity (`Tools/CoreTests/UnityStubs`). À compléter si une nouvelle API Unity est utilisée dans ces assemblies. Ça ne remplace pas la compilation dans Unity.
- Validation en ligne de commande : `Unity -batchmode -quit -projectPath . -executeMethod MangaLegends.Editor.ContentTools.ValidateBatch` (code de sortie 1 si erreur).
