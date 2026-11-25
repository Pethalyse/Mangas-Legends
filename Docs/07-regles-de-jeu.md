# MangaLegends — Règles de jeu (référence du genre)

> Statut : **v0.1** (2026-11-24).
> **Décision du propriétaire ✅** : les règles et mécaniques suivent celles de **League of Legends, version 2026** (saison 2026, patchs 26.x). Ce qui change, c'est le contenu : champions, objets, monstres et lieux viennent des mangas (GDD §1.2). Ce document traduit ces règles pour MangaLegends. Les assets, textes et objets de LoL ne sont pas recopiés.
>
> Légende des sources :
> - ✅ **vérifié** dans une source 2026 (notes du patch 26.1, aperçu de la saison 2026, wiki ; liste en fin de document) ;
> - 🔁 **référence connue** du genre, non revérifiée pour 2026 ;
> - ❓ question propre à MangaLegends.
>
> Les valeurs manquantes sont **choisies par Claude** sans confirmation (CLAUDE.md, « Équilibrage »), en partant de ce document.
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
| Larves du Néant (Voidgrubs) | Pitou, Pufu et Youpi, les gardes royaux Fourmis-Chimères (Hunter x Hunter) ✅ |
| Buff rouge / bleu | Totoro / Roi des profondeurs |
| Raptors, Gromp, Loups, Krugs | Korogu, Hawk, Pompoko, Père de Rebecca |
| Carapateur (Scuttle) | Magicarpe (Pokémon) ✅ |
| Objets | objets de mangas ([03-objets.md](03-objets.md)) ; bottes, objet de support et balises au §5 de ce fichier-là ✅ |

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
| Magicarpe (rivière, un de chaque côté) | 2:55 ✅ | 🔁 2:30 |

- Le jungler subit 50 % de dégâts des monstres non épiques ✅.
- Tuer un monstre soigne le jungler : 90 + 20 par niveau (niveaux 1 à 9), 250 au maximum ✅.
- **Buff rouge (Totoro)** : régénération de 0,5 / 1 / 1,5 / 3 % aux niveaux 1 / 4 / 6 / 11, ralentissement des cibles (mêlée 10 / 15 / 20 %, distance 5 / 7,5 / 10 %) et brûlure de 15 à 54 dégâts ✅. Durée 🔁 2 min.
- **Buff bleu (Roi des profondeurs)** : régénération de mana et réduction de recharge 🔁. Durée 🔁 2 min.
- Les buffs ne sont plus renforcés après 25 min ✅.

### 7.2 Compagnon et Smite

- Le jungler prend Smite et un **compagnon**, une **fée de Zelda** (Navi, fée de la Grande Fée ou Taya, [03-objets.md §5.4](03-objets.md)), qui grandit avec les monstres tués ✅.
  - Première évolution à 15 monstres ; quête terminée à 35 ✅.
  - Le compagnon inflige de 20 à 150 dégâts par seconde et soigne de 6 à 36 PV par seconde ✅.
- **Smite** : **600 / 1 000 / 1 400** dégâts aux monstres ✅. Le palier monte avec les évolutions du compagnon 🔁. Les dégâts ignorent les résistances des monstres ✅, donc type `True`. 2 charges 🔁, recharge 90 s ✅. Une fois amélioré, Smite peut viser un champion ennemi pour des dégâts bruts et un ralentissement 🔁.
- Quête du jungler terminée : +4 % de vitesse dans la jungle et la rivière, +8 % hors combat, +10 or et +10 XP par gros monstre ✅.

### 7.3 Objectifs épiques

| Monstre (équivalent) | Apparition | Réapparition | Récompense |
|---|---|---|---|
| Pitou, Pufu, Youpi (Larves du Néant) | 8:00, les 3 ensemble, une seule fois ✅ ; disparaissent à 14:45 | — | 30 or chacune au tueur, 65 XP ✅ ; bonus de dégâts aux structures 🔁 |
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
  - **Artikodin, Électhor, Lugia** : buffs **adaptés** à leur élément ✅ (validés le 2026-11-24). Chaque âme a une recharge ✅. Valeurs choisies par Claude (à ajuster à l'équilibrage) :
    - **Artikodin (glace)**, esprit du drake de montagne : +6 % d'armure et de résistance magique par drake. **Âme** : après 4 s sans dégâts subis, bouclier de glace de 180 + 15 % des PV bonus ; l'ennemi qui le brise est ralenti de 30 % pendant 1,5 s. Recharge 8 s.
    - **Électhor (électrique)**, esprit des drakes de vent et hextech : +7 % de vitesse d'attaque et +6 d'accélération de compétence par drake. **Âme** : une **attaque de base** ✅ déclenche un éclair (40 + 10 % AD bonus + 15 % AP, dégâts magiques) qui rebondit sur 3 ennemis proches et les ralentit de 25 % pendant 1 s. Recharge 4 s.
    - **Lugia (profondeurs)**, esprit du drake océan : régénère 2,5 % des PV manquants toutes les 5 s par drake. **Âme** : infliger des dégâts à un champion rend 160 + 25 % AD bonus + 15 % AP + 9 % des PV bonus en PV et 80 + 2,5 % du mana max sur 3 s. Recharge 9 s.
- Indépendamment, les PDF donnent à chaque monstre une compétence absorbée par Rimuru (GDD §7.1).

### 7.4 Valeurs choisies pour la carte de test (2026-11-25)

Données dans `Assets/_Project/Content/Monsters/` (distances en mètres, 1 m = 100 unités). PV / AD / or / XP : Korogu 1 200 / 20 / 85 / 95 (camp de 3) ; Hawk 2 050 / 70 / 105 / 120 (à distance) ; Pompoko 1 600 / 35 / 95 / 105 (camp de 2) ; Père de Rebecca 1 900 / 70 / 110 / 130 ; Totoro et Roi des profondeurs 2 300 / 80 / 100 / 180 ; Magicarpe 1 100 / 0 / 55 / 115 ; Sulfura 4 500 / 100 / 75 / 300 ; Zodd 9 000 / 150 / 300 / 1 000 (+150 or à l'équipe). Bonus : Totoro +3 PV/s et +10 % AD pendant 120 s, et chaque attaque de base ralentit de 10 % pendant 1 s et brûle (10 dégâts bruts par seconde pendant 3 s) ; simplifié : pas d'écart mêlée / distance ni de progression par niveau ; Roi des profondeurs +100 % de régén. de mana et +10 d'accélération pendant 120 s ; Sulfura +3 % AD et AP permanents par drake ; Zodd +40 AD et AP pendant 180 s (voir ci-dessous).

**Objectifs épiques (2026-11-25)** : la fosse du bas fait tourner les drakes dans l'ordre Sulfura → Artikodin → Électhor → Lugia (simplifié : pas de tirage ni d'élément de la faille) toutes les 5 min ; chaque drake donne à toute l'équipe un cumul permanent (gardé à la mort, 4 au plus par élément) : Sulfura +3 % AD et AP, Artikodin +6 % armure et RM, Électhor +7 % VA et +6 accélération, Lugia 2,5 % des PV manquants toutes les 5 s (simplifié : sans multiplication par le nombre de cumuls). Au 4e drake d'une équipe, **âme** de l'élément de ce drake (permanente) : Sulfura = explosion de 3 m sur le champion touché, 80 (+22,5 % AD bonus, +13,75 % AP) magiques, 4 s de recharge ; Artikodin = quand un champion vous touche, bouclier de 180 (+15 % PV bonus) pendant 4 s et ralentissement de 30 % pendant 1,5 s de l'agresseur, 8 s (simplifié : réagit aux dégâts au lieu d'attendre 4 s sans dégâts) ; Électhor = éclair de 4 m autour du champion touché par une attaque, 40 (+10 % AD bonus, +15 % AP) et ralentissement de 25 % pendant 1 s, 4 s ; Lugia = toucher un champion rend 160 (+25 % AD bonus, +15 % AP, +9 % PV bonus) PV et 80 mana, d'un coup au lieu de 3 s, 9 s. Une fois une âme prise, la fosse fait apparaître **Shenron** (6 000 PV, 100 or au tueur, 150 or à l'équipe, 650 XP) : bénédiction de 150 s qui exécute les champions sous 20 % de PV touchés par une attaque ou une compétence (perdue à la mort). **Zodd** : +40 AD et AP, rappel 2 fois plus court, et toutes les secondes les sbires alliés à 9 m reçoivent +50 % AD, +30 armure et RM, +10 % vitesse (2 s), pendant 180 s, perdu à la mort. **Pitou, Pufu, Youpi** (fosse du haut, 8:00, partent à 14:45) : 1 500 PV, 30 or et 65 XP chacun ; chacun donne à l'équipe un cumul permanent de « Faim des Chimères » : +15 dégâts bruts par cumul sur les attaques contre tourelles et structures. **Ogrest** (fosse du haut, 15:00, part à 19:45) : 8 000 PV, 100 or ; le tueur reçoit l'Œil d'Ogrest (240 s, perdu à la mort) et sa prochaine attaque de base sur une tourelle inflige 3 000 dégâts bruts (simplifié : pas encore d'Ogrest invoqué qui charge). Les objectifs épiques apparaissent au niveau moyen des champions (PV par niveau : drakes +150, Shenron +200, Zodd +250, Ogrest +200, larves +60). Tours : fortification +40 armure et RM pendant 300 s, dégâts +40 % par tir consécutif sur un champion (max +120 %).

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
- **Insectes de Shino** (balise furtive, bijou gratuit) : recharge de 170 à 90 s ✅. Codé (03-objets §5, touche 4).
- **Den Den Mushi de surveillance** (balise de contrôle, achetable) : révèle et désactive les balises ennemies proches 🔁.
- **Haki de l'observation** (bijou de détection) : révèle les balises et unités cachées autour du champion pendant 8 s ✅.
- Pas de cercles de champignons dans MangaLegends ✅ (choix du propriétaire).
- Buissons : cachent les unités qui s'y trouvent ✅ (GDD §6). Comme dans LoL, une unité dans un buisson n'est vue que par un ennemi qui se tient dans le même buisson (codé le 2026-11-25, côté client, ADR-0007 §9).

## 11. Combat

- Compétences : base par rang + ratios de stats (eux aussi par rang) ✅ (GDD §4.1).
- Dégâts réduits par les résistances : `dégâts × 100 / (100 + résistance)` 🔁 (déjà dans `CombatRules`).
- Critique : 200 % ✅.
- Compétences à coût continu : le coût est prélevé chaque seconde. La compétence s'arrête quand la ressource ne suffit plus 🔁 (comportement standard des compétences activables).
- Hors combat après 5 s sans dégâts subis ni infligés 🔁 (`OutOfCombatSeconds`).

## 12. Ce qui reste à demander

Seules les questions propres à MangaLegends restent ouvertes :
1. Les valeurs 🔁 sont à revérifier dans le jeu actuel ; Claude les ajuste lui-même à l'équilibrage (CLAUDE.md).

## Sources (consultées le 2026-11-24)

- Notes du patch 26.1 : https://leagueoflegends.com/en-us/news/game-updates/patch-26-1-notes/
- Aperçu de la saison 2026 : https://leagueoflegends.com/en-us/news/dev/dev-2026-season-one-gameplay-preview
- Sbires : https://wiki.leagueoflegends.com/en-us/Minion_(League_of_Legends)
- Sorts d'invocateur : https://wiki.leagueoflegends.com/en-us/Summoner_spell
- Objectifs : https://www.nerfplz.com/2026/08/every-summoners-rift-objective.html
- Résumés : https://senseos.gg/guides/league-of-legends/coming-back-to-league-in-2026/ , https://patched.gg/games/league-of-legends/patch-261-notes
