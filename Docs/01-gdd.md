# MangaLegends — Game Design Document

> Statut : **brouillon v0.1** (2026-11-24), rédigé à partir des deux PDF de conception.
> Légende : ✅ décidé (vient des PDF ou validé) · 🟡 proposition à valider · ❓ question ouverte.

## 1. Vision

| | |
|---|---|
| Genre | MOBA 5v5 en vue isométrique, temps réel, multijoueur en ligne ✅ |
| Moteur | Unity 3D, C# ✅ |
| Pitch | Les héros de mangas et d'animés s'affrontent dans une arène à trois voies. Chaque champion traduit en gameplay ce qui rend son personnage iconique (formes, invocations, transformations). ✅ |
| Piliers | 1) Des kits **fidèles à l'esprit du personnage** plutôt que génériques. 2) Des mécaniques de **forme / invocation / marque** riches (stand, pantins, marques). 3) Une lecture claire du combat malgré la complexité des kits. 🟡 |
| Plateforme | PC (Windows) en premier 🟡 |
| Public | **Entre amis uniquement**, pas de diffusion publique ✅ |

### 1.1 Positionnement et originalité

Le jeu reprend les **conventions du genre MOBA** (trois voies, jungle, tours, sbires, objets, quatre compétences + un passif). Son contenu (champions, monstres, lieux) vient des mangas, animés et autres œuvres choisis par le propriétaire (§1.2). On ne recopie pas les éléments propres à League of Legends (noms de ses champions, objets, monstres, carte, assets, textes) : les notes de design décrivent la mécanique plutôt que « comme tel champion ».

### 1.2 Licences : jeu de fans privé ✅

MangaLegends est un **jeu de fans privé**, joué uniquement entre amis et **non diffusé**. Les personnages, noms et références de mangas, d'animés et d'autres œuvres (champions, monstres de jungle, lieux) sont utilisés **tels quels, volontairement**. Le propriétaire du projet connaît les risques de licence, et c'est précisément pour ça que le jeu reste privé.
→ **Ne pas proposer de renommer ou de remplacer** ces noms. Ils restent des données (pas en dur dans le code), uniquement pour la propreté de l'architecture.

## 2. Boucle de jeu

1. **Sélection** : chaque joueur choisit un champion (pas de doublon par équipe 🟡) et deux sorts d'invocateur ✅ (§2.1).
2. **Phase de voie** : farm des sbires et monstres → or et expérience → objets et niveaux.
3. **Escarmouches et objectifs** : monstres épiques, tours.
4. **Fin** : détruire la structure principale (le « Cœur » 🟡, nom à définir) de la base adverse.

Durée cible d'une partie : 25–35 min 🟡.

### 2.1 Sorts d'invocateur ✅

Chaque joueur en choisit deux (touches D et F), dans la liste suivante, comme le veut le genre. Chaque sort est un `SummonerSpellDefinition` qui pointe vers une compétence (`AbilityDefinition`) : nom affiché, recharge, portée et valeurs sont des données.

| Sort | Effet | Construction en données |
|---|---|---|
| Flash (`summoner.blink`) | courte téléportation vers le point visé | `DashEffect` en `Blink` |
| Téléportation (`summoner.teleport`) | après une canalisation, téléporte vers une unité alliée | canalisation + `TeleportEffect` (destinations autorisées en données) |
| Smite (`summoner.smite`) | dégâts aux monstres de la jungle : 600 / 1000 / 1400 | ciblage d'unité neutre + `DamageEffect` (Brut 🟡), 3 rangs |
| Heal (`summoner.heal`) | soigne l'allié ciblé, ou à défaut l'allié le plus bas en PV dans la portée | `HealEffect` + repli `LowestHealthAllyInRange` |
| Shield (`summoner.barrier`) | bouclier sur soi | `ShieldEffect` |
| Fatigue (`summoner.exhaust`) | affaiblit un ennemi | `ApplyStatusEffect` |
| Ghost (`summoner.ghost`) | vitesse de déplacement accrue | `ApplyStatusEffect` (modificateur `MoveSpeed`) |

❓ À définir : ce qui fait passer Smite de 600 à 1000 puis 1400 (niveau du champion ? nombre de monstres tués ?), type de dégâts de Smite (brut proposé), effet exact de Fatigue (ralentissement, réduction de dégâts ?), destinations de la Téléportation (tours, sbires, balises ?), le Heal soigne-t-il aussi le lanceur, recharges et valeurs de chaque sort.

## 3. Contrôles 🟡

| Action | Touche (AZERTY) |
|---|---|
| Déplacement / attaque | Clic droit |
| Compétences | A, Z, E, R ✅ (notation des PDF) |
| Sorts d'invocateur | D, F |
| Objets actifs | 1–6 |
| Rappel à la base | B |
| Caméra verrouillée / libre | Espace / Y |

Les touches sont **remappables** (Input System d'Unity, actions nommées, pas de touches en dur).

## 4. Champions

Fiches détaillées : [02-champions.md](02-champions.md).

### 4.1 Structure d'un kit ✅
- **P** passif, **A / Z / E** compétences de base, **R** ultime.
- Chaque compétence a : coût, temps de recharge (entre crochets dans les PDF), portée, ciblage, effets.
- Les valeurs notées `?` dans les PDF sont **à équilibrer** : elles deviennent des champs de données, jamais des constantes dans le code.

### 4.2 Rôles ✅ / 🟡
Cinq rôles, un par position ✅ : **Top**, **Jungle**, **Mid**, **Bot** et **Support**. Le champion qui joue Bot est appelé l'**ADC** (dégâts physiques à distance) ; « ADC » désigne le type de champion, le rôle s'appelle Bot.

### 4.3 Mécaniques transverses issues des kits
Ces mécaniques apparaissent dans plusieurs kits : elles doivent être des **systèmes génériques** réutilisables, pas du code par champion.

| Mécanique | Champions concernés | Système |
|---|---|---|
| Ralentissement, étourdissement, immobilisation (root), silence, projection (bump), provocation, attraction | presque tous | `StatusEffect` |
| Boucliers et absorptions | Ainz, Zoro, Gakidô (Pain) | `Shield` |
| Marques / cumuls sur une cible | Gon (Pierre/Papier/Ciseaux), Kirito, Sasuke (électrisation), Tanjiro | `Mark` / `Stack` |
| Charges de compétence | Ainz (essences), Giorno (fleurs) | `AbilityCharges` |
| Formes / transformations qui changent le kit | Pain (6 pantins), Rimuru (monstre absorbé), Jotaro et Giorno (stand), Sung Jin-Woo (Igris) | `AbilitySet` interchangeable |
| Invocations contrôlables | Sung Jin-Woo (Igris) uniquement ✅ | `Summon` |
| Invocations purement visuelles | Naruto (clones), Pain (chien) ✅ : ce sont des animations au service d'une attaque de zone ou en ligne, pas des unités | VFX de la compétence (`Projectile` / `Area`) |
| Bonus de terrain | Naruto (zone spéciale), Killua (jungle/rivière), Giorno (buissons), fruits du démon (perte de PV dans l'eau) | `TerrainZone` + modificateurs |
| Liens entre champions | Gon ↔ Killua (rejoindre le rappel), Mark Evans (allié choisi en début de partie) | `ChampionSynergy` |
| Effets globaux | Jotaro (arrêt du temps), Shanks (vision totale) | `GlobalEffect` (à traiter avec soin côté réseau) |
| Dégâts bruts (ignorent les résistances) | Rimuru, Kaido, Killua | type de dégâts `True` |

### 4.4 Types de dégâts ✅
Physique (AD), Magique (AP), Brut ✅. Il n'y a pas d'autre type : les « dégâts du chaos » d'Ainz dans la clean doc sont des dégâts magiques.

## 5. Statistiques 🟡

PV, Mana (ou ressource alternative, voir ci-dessous), AD, AP, Armure, Résistance magique, Vitesse d'attaque, Vitesse de déplacement, Portée, Régénération PV/Mana, Réduction de recharge, Pénétrations, Vol de vie, Chances de critique.
**Ressource** ✅ : tous les champions utilisent du **mana**, sauf mention contraire dans leur fiche ; un champion qui a une autre ressource la fait fonctionner à sa façon (`ResourceKind.Custom` + comportement propre au champion), ou n'en a aucune (`ResourceKind.None`).
Calcul standard : `valeurFinale = (base + bonusPlats) × (1 + somme des bonus %)`. Réduction de dégâts par résistance : `dégâts × 100 / (100 + résistance)` 🟡.

## 6. Carte 🟡

- 3 voies, jungle entre les voies, une **rivière** au centre.
- **Points d'eau** : les champions porteurs d'un fruit du démon (Luffy, Kaido, Law) perdent des PV en les traversant ✅. La rivière et les points d'eau sont donc des zones de terrain explicites.
- **Buissons** qui cachent les unités (Giorno y régénère) ✅.
- **Vallée de la Fin** : zone de la carte où Naruto se déplace plus vite ✅ (emplacement sur la carte ❓).
- Brouillard de guerre, vision par unités et balises.
- Murs destructibles temporairement (Jotaro + stand) ✅ : la navigation doit gérer des obstacles dynamiques.

### 6.1 Structures de chaque équipe ✅

Disposition classique du genre, par équipe :

| Structure | Nombre | Emplacement |
|---|---|---|
| Tour | 3 par voie (9 en tout) | le long de la voie, de l'extérieur vers la base |
| Inhibiteur | 1 par voie (3 en tout) | à l'entrée de la base, au bout de chaque voie |
| Tour du Cœur | 2 | devant le Cœur |
| Cœur | 1 | au centre de la base ; sa destruction termine la partie |

Les noms « Inhibiteur » et « Cœur » sont provisoires 🟡. Dans les données, ce sont des `UnitDefinition` de type `Tower` (tours) et `Structure` (inhibiteur, Cœur).

Règles ✅ :
- **Ordre de destruction** : sur une voie, la tour 1 doit tomber avant qu'on puisse attaquer la tour 2, puis la tour 3, puis l'inhibiteur. Les 2 tours du Cœur deviennent attaquables quand un inhibiteur est tombé, et le Cœur quand les 2 tours du Cœur sont tombées 🟡 (déduit de « comme le genre », à confirmer). Tant que la structure précédente tient, la suivante ne prend pas de dégâts.
- **Inhibiteur détruit** : les vagues de cette voie remplacent le sbire de siège par un **méga sbire**. L'inhibiteur réapparaît après un délai, et les vagues redeviennent normales.
- **Récompenses** : chaque structure détruite donne de l'or.
- **Début de partie** : les tours sont plus résistantes pendant une durée donnée.

Valeurs en données ✅ : or de chaque structure (`UnitDefinition.GoldBounty`), durée et bonus de résistance des tours en début de partie, délai de réapparition de l'inhibiteur, sbire de siège et méga sbire (`GameRulesConfig`, section *Structures* et *Minion waves*). ❓ Les valeurs elles-mêmes restent à équilibrer, ainsi que le partage de l'or (destructeur seul ou toute l'équipe ?).

## 7. Unités non-joueurs 🟡

| Unité | Rôle |
|---|---|
| Sbires | Vagues régulières sur chaque voie (mêlée, distance, siège) ; méga sbire à la place du siège quand l'inhibiteur de la voie est détruit ✅ |
| Tours | 3 par voie, plus 2 devant le Cœur ✅ (§6.1) |
| Inhibiteurs | 1 par voie, à l'entrée de la base ✅ (§6.1) |
| Cœur | Structure principale de chaque base ✅ (§6.1) |
| Monstres de jungle | Normaux, gros, **épiques** ✅ (Rimuru gagne 3/10/20 AP selon la taille). Liste au §7.1. |

### 7.1 Monstres de jungle ✅

Chaque monstre a une compétence que Rimuru peut absorber avec son R (voir [02-champions.md](02-champions.md), Rimuru).

| Monstre | Rôle dans la jungle | Taille (gain d'AP de Rimuru) | Compétence absorbée par Rimuru |
|---|---|---|---|
| Sulfura | Dragon élémentaire de **feu** | épique (20) | grand souffle de feu, dégâts magiques + brûlure 1.5 s |
| Artikodin | Dragon élémentaire de **glace** | épique (20) | souffle de glace, dégâts magiques + immobilisation 1.75 s |
| Électhor | Dragon élémentaire **électrique** | épique (20) | souffle de foudre, dégâts magiques + étourdissement 1 s |
| Lugia | Dragon élémentaire **des profondeurs** | épique (20) | très puissant souffle, dégâts magiques + projection en l'air 1 s |
| Shenron | **Dragon ancestral** | épique (20) | +30 % à toutes les stats d'un allié pendant 2.5 s |
| Ogrest | Monstre épique de **début de partie** (côté opposé aux dragons) | épique (20) | saut, projection des champions à l'atterrissage 1 s, faibles dégâts |
| Zodd | Monstre épique **majeur de fin de partie** | épique (20) | saut, projection des champions à l'atterrissage 1 s, faibles dégâts (même compétence qu'Ogrest, voulu, peut évoluer) |
| Totoro | Camp de **buff rouge** | gros (10) | soigne les alliés de 30 % des PV manquants (40 % sous 40 % de PV) |
| Roi des profondeurs | Camp de **buff bleu** | gros (10) | multitude de loupes d'eau autour de Rimuru, gros dégâts en 1.5 s |
| Korogu | Petit camp en **groupe** (oiseaux) | normal (3) | invisibilité 5 s, rompue par une compétence |
| Hawk | Petit camp **solitaire** (amphibien) | normal (3) | −20 % de résistances sur un ennemi |
| Pompoko | Petit camp en **meute** (loups) | normal (3) | immunité aux contrôles pendant 3 s |
| Père de Rebecca | Petit camp de **deux colosses** | normal (3) | ralenti, +20 % vitesse d'attaque aux alliés pendant 3 s |

❓ À définir : récompenses de chaque monstre (buffs donnés, bonus d'équipe des dragons, effet du dragon ancestral, du monstre de début et de fin de partie), temps de réapparition, nombre de camps et positions sur la carte. La taille des petits camps et des buffs est déduite de leur rôle : à confirmer.

## 8. Économie et progression 🟡

- Or : sbires, monstres, éliminations, tours, revenu passif.
- Niveaux 1 à 18 ; points de compétence ; ultime aux niveaux 6/11/16.
- Objets : composants → objets complets, 6 emplacements. Détail dans [03-objets.md](03-objets.md).

## 9. Multijoueur

- 5v5 en ligne entre amis : un joueur héberge, les autres rejoignent par code de salon ✅/🟡.
- **Serveur autoritaire** pour tous les calculs et validations, **client prédictif** pour garder la sensation temps réel (animations et « faux » dégâts immédiats, corrigés par le serveur) ✅. Voir [04-architecture.md §3](04-architecture.md#3-vue-densemble).
- Tout ce qui sert à l'équilibrage est configurable en données ✅.
- Mode entraînement solo contre bots ❓.
- Matchmaking, comptes, progression hors partie : ❓ hors périmètre du premier jalon.

## 10. Questions ouvertes

1. ~~Sorts d'invocateur ?~~ Réglé (§2.1) ; restent les détails de Smite, Fatigue, Téléportation et Heal.
2. ~~Ressource alternative ?~~ Réglé : mana par défaut, autre ressource seulement si la fiche du champion le précise (§5).
3. ~~Type de dégât « chaos » ?~~ Réglé : dégâts magiques.
4. Pain : « (mana/sec) » sur Shuradô veut-il dire coût continu en mana ?
5. Arrêt du temps (Jotaro) et vision totale (Shanks) : comment rester jouables et justes en réseau ?
6. Bojji « ne prend aucun dégât tant qu'un allié est en vie » : risque d'équilibrage majeur.
7. Structures (§6.1) : valeurs à équilibrer (or, bonus de début de partie, réapparition de l'inhibiteur), partage de l'or, condition exacte pour attaquer les tours du Cœur.
8. Premier jalon jouable : quels champions ? Proposition : Luffy, Zoro, Naruto, Ainz, Kirito (kits complets, mécaniques variées sans les cas les plus complexes).
