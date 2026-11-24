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

Le jeu reprend les **conventions du genre MOBA** (trois voies, jungle, tours, sbires, objets, quatre compétences + un passif). Ces conventions sont communes au genre. En revanche, **aucun contenu d'un autre jeu** (noms de champions, objets, monstres, cartes, assets, textes) ne doit être copié : la carte, les objets, les monstres et l'interface de MangaLegends sont conçus pour ce projet. Les références « comme tel champion de tel jeu » dans les notes de design sont remplacées par une description de la mécanique elle-même.

### 1.2 Propriété intellectuelle

Les champions sont des personnages existants (One Piece, Naruto, JoJo, Hunter×Hunter, Overlord, etc.). Le jeu est destiné à un **cercle d'amis** ✅ ; il ne doit pas être publié (store, page publique) sans revoir ce point.
Conséquence technique : noms, textes, icônes et modèles d'un champion sont **des données** (ScriptableObject + Addressables), jamais en dur dans le code, pour pouvoir renommer ou remplacer un personnage sans toucher au gameplay. Voir [04-architecture.md](04-architecture.md#données).

## 2. Boucle de jeu

1. **Sélection** : chaque joueur choisit un champion (pas de doublon par équipe 🟡) et deux sorts d'invocateur ❓.
2. **Phase de voie** : farm des sbires et monstres → or et expérience → objets et niveaux.
3. **Escarmouches et objectifs** : monstres épiques, tours.
4. **Fin** : détruire la structure principale (le « Cœur » 🟡, nom à définir) de la base adverse.

Durée cible d'une partie : 25–35 min 🟡.

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
| Invocations contrôlables | Sung Jin-Woo (Igris), Naruto (clones), Pain (chien) | `Summon` |
| Bonus de terrain | Naruto (zone spéciale), Killua (jungle/rivière), Giorno (buissons), fruits du démon (perte de PV dans l'eau) | `TerrainZone` + modificateurs |
| Liens entre champions | Gon ↔ Killua (rejoindre le rappel), Mark Evans (allié choisi en début de partie) | `ChampionSynergy` |
| Effets globaux | Jotaro (arrêt du temps), Shanks (vision totale) | `GlobalEffect` (à traiter avec soin côté réseau) |
| Dégâts bruts (ignorent les résistances) | Rimuru, Kaido, Killua | type de dégâts `True` |

### 4.4 Types de dégâts ✅
Physique (AD), Magique (AP), Brut. Le « dégât du chaos » d'Ainz (clean doc) ❓ : nouveau type ou dégât magique renommé ?

## 5. Statistiques 🟡

PV, Mana (ou ressource alternative), AD, AP, Armure, Résistance magique, Vitesse d'attaque, Vitesse de déplacement, Portée, Régénération PV/Mana, Réduction de recharge, Pénétrations, Vol de vie, Chances de critique.
Calcul standard : `valeurFinale = (base + bonusPlats) × (1 + somme des bonus %)`. Réduction de dégâts par résistance : `dégâts × 100 / (100 + résistance)` 🟡.

## 6. Carte 🟡

- 3 voies, jungle entre les voies, une **rivière** au centre.
- **Points d'eau** : les champions porteurs d'un fruit du démon (Luffy, Kaido, Law) perdent des PV en les traversant ✅. La rivière et les points d'eau sont donc des zones de terrain explicites.
- **Buissons** qui cachent les unités (Giorno y régénère) ✅.
- **Zone spéciale** où Naruto se déplace plus vite ✅ (nom original à trouver, ne pas réutiliser un lieu d'un manga si la carte doit rester diffusable).
- Brouillard de guerre, vision par unités et balises.
- Murs destructibles temporairement (Jotaro + stand) ✅ : la navigation doit gérer des obstacles dynamiques.

## 7. Unités non-joueurs 🟡

| Unité | Rôle |
|---|---|
| Sbires | Vagues régulières sur chaque voie (mêlée, distance, siège) |
| Tours | 2–3 par voie + base |
| Monstres de jungle | Normaux, gros, **épiques** ✅ (Rimuru gagne 3/10/20 AP selon la taille) |
| Monstres épiques | Liste de Rimuru (feu, glace, foudre, vent, dragon, etc.) ❓ : les noms des PDF viennent d'autres licences (Pokémon, Ghibli…). À remplacer par des monstres originaux, chacun avec une compétence absorbable par Rimuru. |

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

1. Sorts d'invocateur : oui ou non, lesquels ?
2. Ressource alternative au mana pour certains champions ?
3. Type de dégât « chaos » ?
4. Pain : « (mana/sec) » sur Shuradô veut-il dire coût continu en mana ?
5. Arrêt du temps (Jotaro) et vision totale (Shanks) : comment rester jouables et justes en réseau ?
6. Bojji « ne prend aucun dégât tant qu'un allié est en vie » : risque d'équilibrage majeur.
7. Premier jalon jouable : quels champions ? Proposition : Luffy, Zoro, Naruto, Ainz, Kirito (kits complets, mécaniques variées sans les cas les plus complexes).
