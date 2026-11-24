# MangaLegends — Champions

> Source : PDF « PROJET SECRET OMG » (liste complète, brouillon) et « clean doc » (version présentée, 9 champions).
> Les valeurs `?` sont à équilibrer : elles vivent dans les données (`AbilityDefinition`), jamais dans le code.
> `[x s]` = temps de recharge. ⚠️ Personnages sous licence : voir [01-gdd.md §1.2](01-gdd.md#12-️-risque-propriété-intellectuelle).

## 1. Tableau de suivi

État design : ⬜ vide · 🟨 partiel · ✅ kit complet. État code : — rien · 🔧 en cours · ✅ jouable · 🧪 testé.
**Claude Code met à jour la colonne « Code » à chaque champion implémenté.**

| Champion | Rôle | Design | Code | Systèmes nécessaires |
|---|---|---|---|---|
| Ainz Ooal Gown | Support | ✅ | 🔧 (ancien code, à refactorer) | Shield, Charges, Zone de soin |
| Luffy | Jungle | ✅ | — | TerrainZone (eau), AA chargée, Canalisation |
| Zoro Roronoa | ❓ | ✅ | — | Stack, Shield, Dash |
| Naruto Uzumaki | ❓ | ✅ | — | Dash, Knockback, Summon (clones) |
| Gon Freecss | ❓ | ✅ | — | Mark ×3, Synergy (Killua) |
| Rimuru | Jungle | ✅ | — | AbilitySet dynamique (monstre absorbé) |
| Killua Zoldyck | ❓ | ✅ | — | Invisibilité, Synergy (Gon) |
| Kaido | ❓ | ✅ | — | Compétence conditionnelle (R après E) |
| Shanks | ❓ | ✅ | — | GlobalEffect (vision totale) |
| Sasuke Uchiha | ❓ | ✅ | — | Mark (électrisation), Téléport |
| Guts | ❓ | ✅ | — | Bonus selon PV manquants, Reset sur kill |
| Sung Jin-Woo | ❓ | ✅ | — | Summon contrôlable, AbilitySet ×2 |
| Mark Evans | Support/Tank | ✅ | — | Synergy (allié lié), Mur |
| Kirito | ❓ | ✅ | — | Mark ×5, réduction de recharge croisée |
| Giorno Giovanna | Support | ✅ | — | Charges, Forme (stand) |
| Jotaro | ❓ | ✅ | — | Forme (stand), GlobalEffect (arrêt du temps), mur destructible |
| Law | Support | ✅ | — | Échange de position, téléport allié |
| Madara | Mage | ✅ | — | Canalisation, recast |
| Pain | Tireur | ✅ | — | 6 AbilitySets, jauge de passif |
| Itachi | ❓ | 🟨 | — | |
| Aizawa | ❓ | 🟨 | — | |
| Bojji | ❓ | 🟨 | — | |
| Saitama | ❓ | 🟨 | — | |
| Tanjiro | ❓ | 🟨 | — | Mark (saignement cumulable) |
| Yugo | ❓ | 🟨 | — | |
| Jiraya, Griffith, Isagi/Bachira/Nagi, Asta, Deku, Bakugo, Shinra, Ippo/Baki, Shin'ichi Izumi, Usopp, Goku, Vegeta, Trunks, Kira, Edward & Alphonse, Muzan, Zen'itsu, Inosuke | — | ⬜ | — | |

## 2. Divergences entre les deux PDF (à trancher)

| Champion | « PROJET SECRET » | « clean doc » |
|---|---|---|
| Ainz P | 2 % mana max/s | ?% |
| Ainz A | dégâts magiques, slow 20 % 2 s | « dégâts du chaos », valeurs ? |
| Ainz Z | durée 2.5 s, [17 s] | [15 s] |
| Ainz E | 3 charges, [12 s / charge] | zone unique, [10 s] |
| Luffy R | Gear 4 | Gear 5 |
| Naruto A | [7 s] | [8 s] |

Proposition : la « clean doc » fait foi quand elle donne une valeur, sinon le brouillon. 🟡

## 3. Kits complets

Format : **Touche — Nom** : effet. `[recharge]`

### Ainz Ooal Gown — Support
- **P — Bénédiction d'Albedo** : hors combat (pas de dégâts ni de compétence ennemie reçus récemment), régénère ?% du mana max par seconde.
- **A — Mantle of Chaos** : projectile ciblé, ? dégâts, ralentit ?% pendant ? s. `[7 s]`
- **Z — Infinity Wall** : bouclier sur un allié qui absorbe dégâts magiques **et contrôles** jusqu'à ? dégâts, expire après ? s. `[15 s]`
- **E — Life Essence** : zone de soin ? PV/s pendant ? s (variante : 3 charges posables à longue portée). `[10 s]`
- **R — Penetrate Up** : projectile très longue portée, dégâts de zone et −?% résistances pendant ? s. `[100 s]`

### Luffy — Jungle
- **P** : perd ? PV/s dans l'eau ; résistance physique accrue ; AA chargée qui ralentit un champion `[20 s]`.
- **A — Rifle** : coup à distance, ralentit. `[8 s]`
- **Z — Gatling** : canalise 2 s sur une cible (cible verrouillée), lourds dégâts ; portée qui croît avec l'AD. `[12 s]`
- **E — Rocket** : s'agrippe à la cible et fonce vers elle. `[10 s]`
- **R — Gear** : augmente les dégâts des compétences selon l'AD. `[90 s]`

### Zoro Roronoa
- **P — Haki de l'armement** : chaque AA sur un champion donne +1 AD (cumul).
- **A — Nitoryu slash** : ? AD `[1 s]`. Chaque coup qui touche charge ; au 3e, devient **Demon slash** (150 %) `[1.5 s]`.
- **Z — Parade** : canalise ≤ 1.5 s, bloque 40 % du premier coup ; si bloqué, bouclier = 40 % AD + dégâts bloqués. `[11 s]`
- **E — Calibre 36** : lame d'air courte portée, ? dégâts physiques + projection. `[8 s]`
- **R — Ashura** : grand dash, ? dégâts physiques, +10 % par ennemi touché. `[90 s]`

### Naruto Uzumaki
- **P** : +10 % vitesse dans une zone de carte dédiée.
- **A — Rasengan** : fonce sur l'ennemi ; touché : ? magiques + projection ; raté : ralenti 1 s. `[7–8 s]`
- **Z — Shurikens** : 3 shurikens en ligne fixe, ? physiques, −15 % vitesse par shuriken. `[8 s]`
- **E — Clone** : un clone le propulse dans une direction. `[15 s]`
- **R — Multi Rasen-Shuriken** : 2 clones combattent avec lui ; le Rasengan devient Rasen-Shuriken ciblé, ? magiques. `[90 s]`

### Gon Freecss
- **P — Kô** : par allié mort, +? AD/armure/RM jusqu'à sa réapparition (max 4). Peut rejoindre le rappel de Killua.
- **A — Pierre** : saut + coup de poing, ? physiques, pose la marque Pierre. `[8 s]`
- **Z — Papier** : −20 % armure 3 s, pose la marque Papier. `[12 s]`
- **E — Ciseaux** : zone puis deux lames qui se rejoignent, ? physiques + ralentissement, pose la marque Ciseaux. `[12 s]`
- **R — Déferlement de Nen** : uniquement sur une cible portant les 3 marques ; immobilise pendant l'animation, ? physiques. `[60 s]`

### Rimuru — Jungle
- **P — Raphaël** : +3/+10/+20 AP par monstre normal/gros/épique absorbé par R ; se soigne de 66 % des dégâts infligés aux monstres.
- **A — Aqualame** : lame d'eau frontale, ? magiques. `[6 s]`
- **Z — Petit slime** : petit saut directionnel. `[12 s]`
- **E — Souffle paralysant** : ? magiques + étourdissement 1 s. `[10 s]`
- **R — Belzébuth** `[60–90 s selon le monstre]` : estomac vide → ?–? dégâts bruts à un monstre ; s'il meurt, il est stocké. Estomac plein → se transforme et lance la compétence du monstre (souffle de feu/brûlure, souffle de glace/immobilisation 1.75 s, foudre/étourdissement 1 s, souffle aérien/projection 1 s, buff +30 % stats d'un allié 2.5 s, saut + projection, soin de groupe 30–40 % PV manquants, rayon concentré, invisibilité 5 s, buff +20 % vitesse d'attaque d'équipe, −20 % résistances, immunité aux contrôles 3 s).
  → **Les monstres des PDF portent des noms d'autres licences : à remplacer par les monstres originaux de la carte (GDD §7).**

### Killua Zoldyck
- **P — Divine Speed** : +10 % vitesse en jungle et rivière. Peut rejoindre le rappel de Gon.
- **A — Lightning Palm** : se rue sur la cible, ? magiques + saignement ? bruts sur 1.5 s. `[8 s]`
- **Z — The Lightning** : éclair en zone circulaire, ? magiques, −15 % vitesse 1.5 s. `[8 s]`
- **E — Instant speed** : invisible 0.5 s, +100 % vitesse. `[16 s]`
- **R — Nen Overlord** : explosion autour de lui, ? magiques, immobilise 1.5 s. `[130 s]`

### Kaido
- **P** : perd ? PV/s dans l'eau ; absorbe 10 % de la prochaine compétence reçue `[15 s]`, chaque attaque reçue réduit ce délai de 1 s.
- **A — Ramei Hakke** : frappe à 180°, ? physiques. `[7 s]`
- **Z — Boro Breath** : rayon qui explose au premier ennemi : ? bruts à la cible, ? physiques en zone. `[9 s]`
- **E — Kozanse** : s'envole, 3 tornades en éventail, ? physiques + étourdissement 1 s. `[12 s]`
- **R — Ragnarok** : seulement après E ; plongeon, ? physiques sur la cible et en zone. `[120 s]`

### Shanks
- **P — Haki des rois** : les sbires lui infligent −50 % dégâts.
- **A** : prochaine attaque +? physiques et silence 1.5 s. `[7 s]`
- **Z — Haki de l'armement** : +30 % résistances. `[8 s]`
- **E — Haki des rois** : étourdit autour de lui 1.5 s. `[16 s]`
- **R — Haki de l'observation** : vision de toute la carte pour l'équipe pendant 3 s, sans prévenir l'adversaire. `[80 s]`

### Sasuke Uchiha
- **P — Raiton** : chaque compétence applique « électrisé » 1.5 s.
- **A — Chidori** : se projette ; touché : ? magiques ; raté : silencé 0.75 s. `[7 s]`
- **Z — Téléportation** courte/moyenne portée. `[15 s]`
- **E — Flèche** : tir moyenne portée, ? magiques. `[10 s]`
- **R — Kirin** : frappe tous les ennemis électrisés ; −10 % de dégâts par cible supplémentaire. `[120 s]`

### Guts
- **P — Berserker** : dégâts physiques augmentés selon les PV manquants.
- **A** : élan + coup ; prochaine AA +30 %. `[6 s]`
- **Z** : boulet de canon, ? physiques. `[10 s]`
- **E — Rage** : +30 % vitesse d'attaque 2 s. `[8 s]`
- **R — Armure du berserker** : bond sur la cible, ? physiques ; réutilisable 7 s si la cible meurt. `[140 s]`

### Sung Jin-Woo
- **P** : +5 % d'expérience.
- Deux jeux de compétences (lui / Igris), recharges indépendantes, R bascule de l'un à l'autre.
- **A** : 3 coups de dague ? physiques / Igris : long coup d'épée. `[6 s]`
- **Z** : +15 % vitesse 2 s, prochaine AA à portée augmentée qui le téléporte sur la cible / Igris : échange de position. `[12 s]`
- **E** : invisibilité ≤ 2.5 s (Igris aussi, +20 % vitesse ; révélés s'ils restent 1 s près d'un ennemi) / Igris : −30 % armure 2 s sur la prochaine attaque. `[16 s / 10 s]`
- **R — Igris** : invoque un chevalier contrôlable ; réactivation = bascule des compétences. `[60 s après la disparition d'Igris]`

### Mark Evans — Support / Tank
- **P** : choisit un allié lié en début de partie ; absorbe 10 % des dégâts qu'il subit.
- **A — Poing explosif** : ? AP devant lui + bref ralentissement. `[6 s]`
- **Z** : roulade, ralenti 0.5 s à la sortie. `[12 s]`
- **E — Main céleste** : mur frontal 3 s, +70 % résistance aux compétences qui le touchent. `[14 s]`
- **R — Main magique** : frappe au sol ; centre : étourdit 2 s, onde : immobilise 2 s. `[90 s]`

### Kirito
- **P** : toutes les 3 attaques, dégâts bonus ; chaque attaque pose une marque.
- **A — Attaque circulaire** : coup circulaire, bonus contre hémorragie ; si touche, recharge de E divisée par 2. `[7 s]`
- **Z** : dague empoisonnée, hémorragie 4 s. `[18 s]`
- **E** : petit dash + coup ; si touche, recharge de A divisée par 2. `[8 s]`
- **R — Starburst Stream** : sur une cible à 5 marques, 5 AA qui ignorent 25 % armure. `[65 s]`

### Giorno Giovanna — Support
- **P** : possède un stand (forme) ; régénération bonus dans les buissons.
- **A — Fleurs dorées** (3 charges `[7 s]` chacune) : 1re touche = dégâts + ralentit ; 2e pendant le ralentissement = immobilise 1.5 s ; 3e = étourdit 1.5 s. Stand : hitbox plus grande.
- **Z** : zone qui transfère sa vie aux alliés, durée infinie, gros coût en mana (gratuit avec le stand).
- **E** : +vitesse d'attaque d'un allié `[10 s]` ; stand : repousse les ennemis proches.
- **R** : invoque le stand 10 s et silence 3 s en zone. `[180 s]`

### Jotaro
- **P** : possède un stand ; vision augmentée.
- **A** : renforce la prochaine AA, reset d'AA `[4 s]` ; stand : projectile ? AD + ralentissement.
- **Z** : +résistances 4 s `[18 s]` ; stand : invulnérable 2 s.
- **E** : provoque les ennemis proches `[12 s]` ; stand : ouvre temporairement un passage dans un mur.
- **R** : invoque le stand 13 s et **arrête le temps 3 s** : lui seul peut bouger, sans compétences. `[180 s]`

### Law — Support
- **P** : perd des PV dans l'eau ; augmente la régénération des alliés proches.
- **A** : allonge son épée, ? magiques à la cible. `[6 s]`
- **Z** : aiguille, ? magiques. `[8 s]`
- **E — Shambles** : échange la position de deux unités alliées (hors bâtiments). `[22 s]`
- **R — Room** : renvoie instantanément un allié à la base. `[120 s]`

### Madara — Mage
- **P** : se soigne de ?% des dégâts magiques infligés.
- **A** : canalise 1.25 s puis boule de feu ? magiques ; réactivable une fois.
- **Z — Susanoo** : +50 % résistance physique, −20 % vitesse, 1.5 s. `[12 s]`
- **E — Katon** : cône de feu, ? magiques. `[6 s]`
- **R** : météorite en zone, ? + ? par ennemi supplémentaire. `[90 s]`

### Pain — Tireur
- **P** : jauge qui se charge (temps, attaques et compétences qui touchent ; pleine sur un kill). Pleine → passe au pantin suivant (ordre fixe de 6) ; un kill permet de choisir de rester. Hors combat, passage automatique. Changer de pantin réinitialise les recharges.
- **A** `[5 s]` et **E** `[7 s]` dépendent du pantin : attraction, lance-missiles (zone 10 %, coût continu ?), vol de 20 % des PV actuels rendus 3 s après, chien invoqué, vol de mana, silence 1.5 s / sphère d'attraction, laser, déplacement forcé aléatoire, changement de pantin libre (suivant / précédent), absorption de la prochaine attaque.
- **R** : passif — jauge pleine = changement automatique.
- **Pas de Z** dans le PDF ❓

## 4. Kits partiels (notes de design)

- **Itachi — A Amaterasu** : flamme noire en zone, brûlure 2 s ? magiques/s, prochaine compétence sur la cible +? dégâts. `[2 s]`
- **Aizawa — E** : silence 2 s sur la cible ; pendant ce temps, Aizawa ne peut pas utiliser A, Z, R. `[14 s]`
- **Bojji — P** : n'inflige aucun dégât, n'en subit aucun tant qu'un allié est en vie. ⚠️ équilibrage.
- **Saitama — P** : bonus selon les actions (pompes vues par un ennemi → +AD ; course → 3 paliers de vitesse).
- **Tanjiro** : P marques de saignement cumulables sur les ennemis ; R enflamme les marques.
- **Yugo — R** : invocation.

## 5. Modèle de fiche (pour les nouveaux champions)

```markdown
### <Nom> — <Rôle>
- Ressource : Mana | Aucune | <autre>
- Stats de base : voir `ChampionDefinition` <Nom>.asset
- **P — <nom>** : <effet>.
- **A — <nom>** : <ciblage> ; <effets> ; <valeurs>. `[recharge]` coût ?
- **Z — …**
- **E — …**
- **R — …**
- Systèmes nécessaires : <StatusEffect, Mark, …>
- Questions ouvertes : …
```
