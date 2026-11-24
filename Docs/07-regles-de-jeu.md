# MangaLegends — Règles de jeu (référence du genre)

> Statut : **v0.1** (2026-11-24).
> **Décision du propriétaire ✅** : les règles et mécaniques suivent celles de **League of Legends, version 2026** (saison 2026, patchs 26.x). Ce qui change, c'est le contenu : champions, objets, monstres et lieux viennent des mangas (GDD §1.2). Ce document traduit ces règles pour MangaLegends. Les assets, textes et objets de LoL ne sont pas recopiés.
>
> Légende des sources :
> - ✅ **vérifié** dans une source 2026 (notes du patch 26.1, aperçu de la saison 2026, wiki ; liste en fin de document) ;
> - 🔁 **référence connue** du genre, non revérifiée pour 2026 ;
> - ❓ question propre à MangaLegends.
>
> Toutes les valeurs sont des **données** (`GameRulesConfig`, `UnitDefinition`, `AbilityDefinition`…). Les chiffres ci-dessous sont les valeurs par défaut à saisir, pas des constantes du code.

## 1. Correspondance des éléments

| Élément du genre | MangaLegends |
|---|---|
| Nexus | Cœur |
| Inhibiteur | Inhibiteur |
| Drakes élémentaires | Sulfura (feu), Artikodin (glace), Électhor (électrique), Lugia (profondeurs) |
| Dragon ancestral (Elder) | Shenron |
| Héraut (Herald) | Ogrest |
| Baron | Zodd |
| Larves du Néant (Voidgrubs) | ❓ aucun équivalent nommé |
| Buff rouge / bleu | Totoro / Roi des profondeurs |
| Raptors, Gromp, Loups, Krugs | Korogu, Hawk, Pompoko, Père de Rebecca |
| Carapateur (Scuttle) | ❓ aucun équivalent nommé |
| Objets | objets de mangas ([03-objets.md](03-objets.md)) |

## 2. Partie et carte

- 5 contre 5, 3 voies, jungle, rivière ✅.
- Victoire : destruction du Cœur adverse ✅.
- Retour à la base : canalisation de 8 s 🔁. Vitesse de retour (Homeguard) : +80 % avant 14:00, +150 % après, s'arrête en combat ou en entrant dans la jungle ✅.
- Fontaine : soin rapide, achat d'objets uniquement à la base 🔁.
- Recréer une partie (remake) entre 0:55 et 2:25 ; abandon possible dès 2:55 ✅. ❓ Utile entre amis ?

## 3. Progression

- Niveau maximum : **18** ✅. Le Top peut monter à **20** grâce à sa quête (§9) ✅.
- Expérience jusqu'au niveau suivant : 280, puis +100 par niveau (380, 480…) 🔁 (déjà dans `RULES_Default`).
- Points de compétence : un par niveau ; compétences de base au rang 5 maximum, ultime aux niveaux 6, 11 et 16 🔁.
- Expérience des sbires : mêlée 62, distance 31, siège et méga 75 ✅. Elle est partagée entre les champions ennemis proches dans un rayon de 1 500 unités. Chacun reçoit 100 / 65 / 43,3 / 32,5 / 26 / 21,7 % selon le nombre de champions présents ✅.
- Réapparition après la mort : durée qui dépend du niveau et qui augmente en fin de partie 🔁. Tableau à saisir dans `RespawnSecondsByLevel`.

## 4. Or et primes

- Or de départ : 500 🔁. Revenu passif à partir de 1:05 ✅ ; montant par seconde 🔁 (≈ 2 par seconde).
- Premier sang : +100 or ✅.
- Élimination d'un champion : 300 or de base 🔁, avec des primes. Une série d'éliminations augmente la prime, et une série de morts la diminue. Dévaluation de 0,4 par or donné à la mort, revalorisation de 0,2 par or gagné ✅.
- Assistances : une part de l'or de l'élimination, répartie entre les assistants 🔁.
- Sbires (or) : mêlée 20, distance 14, siège de 50 à 69 selon le temps (50 à 0:30, 59 à 14:00, 66 à 25:00, 69 à 30:00) ✅.
- Pénalité hors voie : −25 % d'or et d'expérience des sbires pour un joueur hors de sa voie, jusqu'au niveau 3 ✅.
- Coup critique : 200 % des dégâts par défaut ✅ (stat `CritDamage` de base à 2).

## 5. Sbires

- Première vague à **0:30** ✅. Ensuite une vague toutes les 30 s, toutes les 25 s à partir de 14:00, toutes les 20 s à partir de 30:00 ✅.
- Composition : 3 mêlée et 3 distance ✅.
  - Siège : à partir de la vague 3 (1:30), une vague sur 3 jusqu'à 14:00, puis une sur 2, puis chaque vague après 25:00 ✅.
  - Quand un siège apparaît après 14:00, la vague a un mêlée de moins ✅.
  - Après 30:00, chaque vague a un sbire à distance de moins ✅.
- **Méga sbire** : quand un inhibiteur adverse est détruit, un méga sbire par vague sur cette voie, à la place du siège ✅. Si tous les inhibiteurs sont détruits, 2 méga sbires par vague sur chaque voie ✅.
- Les sbires infligent 60 % de leurs dégâts aux champions et aux structures ✅. Ils se renforcent toutes les 90 s ✅.
- Vitesse de déplacement : de 350 à 450 au fil de la partie ✅.
- Acquisition de cible à 500 unités, avec priorités et appels à l'aide 🔁 (le sbire cible celui qui attaque un champion allié).

## 6. Structures

| Structure | PV | Résistances | Particularités |
|---|---|---|---|
| Tour extérieure (T1) | 9 000 ✅ | 60 ✅ | plaques ; −15 armure et RM par minute dès la 11e min (−60 max) ✅ |
| Tour intérieure (T2) | 5 000 ✅ | 60 ✅ | plaques |
| Tour d'inhibiteur (T3) | 4 750 ✅ | 60 ✅ | plaques |
| Tour du Cœur (×2) | 3 500 ✅ | 60 ✅ | pas de plaques ; réapparaît avec 40 % de PV ✅ |
| Inhibiteur | 🔁 | — | réapparaît **5 min** après sa destruction 🔁 |
| Cœur | 🔁 | — | fin de partie |

- **Ordre** : sur une voie, T1, puis T2, puis T3, puis l'inhibiteur. Les tours du Cœur deviennent attaquables dès qu'**un** inhibiteur est détruit, et le Cœur quand les deux tours du Cœur sont détruites 🔁.
- **Plaques** : toutes les tours sauf celles du Cœur. Elles tombent à 10 / 25 / 45 / 70 / 100 % de PV manquants ✅.
  - Tour extérieure : 120 or au total, puis −10 par minute dès la 11e min (−40 max) ✅.
  - Tours intérieure et d'inhibiteur : 600 or répartis sur 5 plaques ✅.
- **Première tour** : +300 or aux joueurs proches ✅.
- **Or** : partagé entre les champions proches (or local) ; certaines structures donnent aussi de l'or à toute l'équipe 🔁. Valeurs dans `UnitDefinition.GoldBounty`, répartition à coder en local par défaut.
- Les tours subissent 20 % de dégâts en plus des champions au corps à corps ✅. La 3e attaque d'un champion sur une tour inflige un bonus (« Démolition », recharge 30 s) ✅.
- **Début de partie** : les tours sont plus résistantes (fortification) 🔁. Elles réduisent les dégâts subis pendant les premières minutes et perdent ensuite leurs résistances (voir T1). Durée et bonus : `EarlyTowerDefenseSeconds` et `EarlyTowerModifiers`.
- **Excroissance cristalline** : après 90 s sans attaque, la prochaine attaque d'un champion sur la tour inflige des dégâts bruts en % des PV max de la tour, qui augmentent avec le niveau moyen de l'équipe ✅.
- Tir de tour : priorité aux sbires, bascule sur un champion qui attaque un champion allié à portée, dégâts croissants sur la même cible 🔁.

## 7. Jungle

### 7.1 Camps

| Camp | Apparition | Réapparition |
|---|---|---|
| Korogu, Pompoko, Totoro, Roi des profondeurs | 0:55 ✅ | 🔁 2:15 pour les petits camps, 5:00 pour les buffs |
| Hawk, Père de Rebecca | 1:07 ✅ | 🔁 2:15 |
| (équivalent du Scuttle) ❓ | 2:55 ✅ | 🔁 |

- Le jungler subit 50 % de dégâts des monstres non épiques ✅.
- Tuer un monstre soigne le jungler : 90 + 20 par niveau (niveaux 1 à 9), 250 au maximum ✅.
- **Buff rouge (Totoro)** : régénération de 0,5 / 1 / 1,5 / 3 % aux niveaux 1 / 4 / 6 / 11, ralentissement des cibles (mêlée 10 / 15 / 20 %, distance 5 / 7,5 / 10 %) et brûlure de 15 à 54 dégâts ✅. Durée 🔁 2 min.
- **Buff bleu (Roi des profondeurs)** : régénération de mana et réduction de recharge 🔁. Durée 🔁 2 min.
- Les buffs ne sont plus renforcés après 25 min ✅.

### 7.2 Compagnon et Smite

- Le jungler prend Smite et un **compagnon** qui grandit avec les monstres tués ✅.
  - Première évolution à 15 monstres ; quête terminée à 35 ✅.
  - Le compagnon inflige de 20 à 150 dégâts par seconde et soigne de 6 à 36 PV par seconde ✅.
- **Smite** : **600 / 1 000 / 1 400** dégâts aux monstres ✅. Le palier monte avec les évolutions du compagnon 🔁. Les dégâts ignorent les résistances des monstres ✅, donc type `True`. 2 charges 🔁, recharge 90 s ✅. Une fois amélioré, Smite peut viser un champion ennemi pour des dégâts bruts et un ralentissement 🔁.
- Quête du jungler terminée : +4 % de vitesse dans la jungle et la rivière, +8 % hors combat, +10 or et +10 XP par gros monstre ✅.

### 7.3 Objectifs épiques

| Monstre (équivalent) | Apparition | Réapparition | Récompense |
|---|---|---|---|
| ❓ (Larves du Néant) | 8:00, 3 larves, une seule fois ✅ ; disparaissent à 14:45 | — | 30 or chacune au tueur, 65 XP ✅ ; bonus de dégâts aux structures 🔁 |
| Ogrest (Héraut) | 15:00 ✅ ; disparaît à 19:45 | — | 100 or au tueur ✅ ; il peut être invoqué et charge une tour (3 000 dégâts) ✅ |
| Drakes (Sulfura, Artikodin, Électhor, Lugia) | 5:00 ✅ | 5:00 ✅ | 75 or et 160 à 400 XP ✅ ; buff d'équipe permanent selon l'élément 🔁 |
| Âme du dragon | au 4e drake d'une équipe ✅ | — | bonus permanent selon l'élément de l'âme 🔁 |
| Shenron (Ancestral) | après une âme ✅ | 6:00 ✅ | 650 XP globale, 100 or + 150 or à l'équipe ✅ ; buff de 150 s qui exécute les ennemis sous 20 % de PV ✅ |
| Zodd (Baron) | **20:00** ✅ | 6:00 ✅ | 650 XP globale, 100 or + 150 or à l'équipe ✅ ; buff qui renforce les sbires proches et le retour à la base 🔁 |

- Les objectifs épiques gagnent des PV et des résistances avec le niveau moyen des champions, pas avec le temps ✅.
- **Buffs d'équipe** ✅ (décision du propriétaire) :
  - **Sulfura** : buff du drake de feu du genre (bonus de dégâts d'attaque et de puissance, âme de feu) 🔁 pour les valeurs.
  - **Shenron** : buff du dragon ancestral, exécution des ennemis sous 20 % de PV pendant 150 s ✅.
  - **Zodd** : buff du Baron, sbires proches renforcés et retour à la base accéléré 🔁 pour les valeurs.
  - **Artikodin, Électhor, Lugia** : buffs **adaptés** à leur élément (glace, électrique, profondeurs), à inventer ensemble ❓.
- Indépendamment, les PDF donnent à chaque monstre une compétence absorbée par Rimuru (GDD §7.1).

## 8. Sorts d'invocateur

Chaque joueur en prend deux (D, F) ✅. Recharges 2026 ✅ :

| Sort | Recharge | Effet de référence 🔁 |
|---|---|---|
| Flash | 300 s | téléportation instantanée d'environ 400 unités vers le point visé |
| Téléportation | 300 s | canalisation de 4 s, puis téléportation sur une tour alliée ; améliorée en milieu de partie (aussi vers les sbires et balises alliés, recharge réduite) |
| Smite | 90 s | voir §7.2 |
| Heal | 240 s | soigne le lanceur et l'allié ciblé (ou à défaut l'allié le plus blessé à portée), plus un bonus de vitesse ; effet réduit sur une cible déjà soignée récemment |
| Shield | 180 s | bouclier sur soi pendant 2,5 s, qui augmente avec le niveau |
| Fatigue | 240 s | sur un ennemi pendant 3 s : ralentissement et −40 % de dégâts infligés |
| Ghost | 240 s | +vitesse de déplacement pendant 10 s, traverse les unités |
| Ignite ✅ (existe dans le genre) | 180 s | dégâts bruts sur la durée et réduction des soins de la cible |
| Cleanse ✅ (existe dans le genre) | 240 s | retire les contrôles et réduit les suivants |
| Clarity ✅ (existe dans le genre) | 240 s | rend du mana au lanceur et aux alliés proches |

- La plupart des sorts restent utilisables sous un contrôle qui empêche de lancer. La stase et la suppression bloquent tout. L'immobilisation bloque Flash et Téléportation ✅.
- Ignite, Cleanse et Clarity font partie de la liste ✅ (10 sorts en tout).

## 9. Quêtes de rôle (saison 2026)

Chaque rôle a une quête qui se remplit en jouant sur sa voie ✅ :

| Rôle | Objectif | Récompense |
|---|---|---|
| Top | 1 200 points | niveau max 20, +600 XP, +12,5 % d'XP ensuite ; Téléportation gratuite (7 min de recharge) si le joueur n'a pas pris Téléportation, sinon un bouclier à l'arrivée ✅ |
| Jungle | 35 monstres | voir §7.2 ✅ |
| Mid | 1 350 points | bottes améliorées d'un cran, retour à la base renforcé toutes les 5 min (4 s au lieu de 8), recharge réduite d'1 min par élimination ✅ |
| Bot | 1 350 points | +300 or, puis +2 or par sbire et +50 or par élimination ; les bottes passent dans un emplacement dédié ✅ |
| Support | — | balises de contrôle moins chères (40 or) et 2 stockées dans l'emplacement de quête ; revenu de l'objet de support ✅ |

❓ Les récompenses touchent des objets (bottes, objet de support, balises). Il faut les équivalents manga dans [03-objets.md](03-objets.md) avant de coder les quêtes.

## 10. Vision

- Brouillard de guerre ; chaque unité, tour et balise révèle autour d'elle 🔁.
- Balises : balise furtive (bijou jaune, recharge de 170 à 90 s ✅), balise de contrôle (achetable, révèle et désactive les balises 🔁), objectif rouge (Oracle, 8 s ✅).
- Cercles de champignons (Faelights) : une balise posée dessus voit 25 % plus loin et révèle une zone pendant 45 s ✅. ❓ Équivalent visuel manga ?
- Buissons : cachent les unités qui s'y trouvent ✅ (GDD §6).

## 11. Combat

- Dégâts réduits par les résistances : `dégâts × 100 / (100 + résistance)` 🔁 (déjà dans `CombatRules`).
- Critique : 200 % ✅.
- Compétences à coût continu : le coût est prélevé chaque seconde. La compétence s'arrête quand la ressource ne suffit plus 🔁 (comportement standard des compétences activables).
- Hors combat après 5 s sans dégâts subis ni infligés 🔁 (`OutOfCombatSeconds`).

## 12. Ce qui reste à demander

Seules les questions propres à MangaLegends restent ouvertes :
1. Équivalents manga des Larves du Néant et du Carapateur : il en faut ✅, propositions en cours (voir ci-dessous).
2. Buffs adaptés d'Artikodin, Électhor et Lugia.
3. Objets des quêtes de rôle et de la vision (bottes, objet de support, balises) et cercles de champignons : à choisir parmi des propositions (ci-dessous).

Propositions faites au propriétaire (2026-11-24), en attente de son choix :

| Rôle | Idées |
|---|---|
| Larves du Néant (3 petits monstres, bonus contre les structures) | Fourmis-Chimères (Hunter x Hunter), larves d'Ohmu (Nausicaä), Kikaichu (Naruto) |
| Carapateur (monstre de rivière, vision et vitesse) | Magicarpe (Pokémon), Den Den Mushi (One Piece), Kappa (folklore, Natsume) |
| Bottes | Équipement tridimensionnel (L'Attaque des Titans), Nuage Kinto (Dragon Ball), skate de Conan (Détective Conan) |
| Objet de support (revenu d'or) | Carte au trésor de Nami (One Piece), licence de Hunter (Hunter x Hunter), bourse de Zeni (Dragon Ball) |
| Balise furtive | Den Den Mushi de surveillance (One Piece), insectes de Shino (Naruto) |
| Balise de contrôle | Byakugan (Naruto), mantra de l'observation (One Piece) |
| Cercles de champignons | arbres de Totoro (Mon voisin Totoro), Sakura sacrés (Naruto) |
5. Les valeurs 🔁 sont à revérifier dans le jeu actuel avant l'équilibrage final.

## Sources (consultées le 2026-11-24)

- Notes du patch 26.1 : https://leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/
- Aperçu de la saison 2026 : https://leagueoflegends.com/en-us/news/dev/dev-2026-season-one-gameplay-preview
- Sbires : https://wiki.leagueoflegends.com/en-us/Minion_(League_of_Legends)
- Sorts d'invocateur : https://wiki.leagueoflegends.com/en-us/Summoner_spell
- Objectifs : https://www.nerfplz.com/2026/08/every-summoners-rift-objective.html
- Résumés : https://senseos.gg/guides/league-of-legends/coming-back-to-league-in-2026/ , https://patched.gg/games/league-of-legends/patch-261-notes
