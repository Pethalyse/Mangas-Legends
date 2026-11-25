# MangaLegends — Champions

> Source : PDF « PROJET SECRET OMG » (liste complète, brouillon) et « clean doc » (version présentée, 9 champions).
> Les valeurs `?` sont à équilibrer : elles vivent dans les données (`AbilityDefinition`), jamais dans le code.
> `[x s]` = temps de recharge. Jeu de fans privé : les noms des personnages sont utilisés tels quels ([01-gdd.md §1.2](01-gdd.md)).

## 1. Tableau de suivi

État design : ⬜ vide · 🟨 partiel · ✅ kit complet. État code : — rien · 🔧 en cours · ✅ jouable · 🧪 testé.
**Claude Code met à jour la colonne « Code » à chaque champion implémenté.**

| Champion | Rôle | Design | Code | Systèmes nécessaires |
|---|---|---|---|---|
| Ainz Ooal Gown | Support | ✅ | ✅ jouable (testé en solo le 2026-11-25), modèle 3D animé | Shield, Charges, Zone de soin |
| Luffy | Jungle | ✅ | 🔧 jouable, simplifié (Z portée) | TerrainZone (eau), AA chargée, Canalisation |
| Zoro Roronoa | Top | ✅ | 🔧 jouable, simplifié (Z, R) | Stack, Shield, Dash |
| Naruto Uzumaki | Mid | ✅ | 🔧 jouable, simplifié (P, A raté) | Dash, Knockback, Zone (clones = visuel) |
| Gon Freecss | ❓ | ✅ | — | Mark ×3, Synergy (Killua) |
| Rimuru | Jungle | ✅ | — | AbilitySet dynamique (monstre absorbé) |
| Killua Zoldyck | ❓ | ✅ | — | Invisibilité, Synergy (Gon) |
| Kaido | ❓ | ✅ | — | Compétence conditionnelle (R après E) |
| Shanks | ❓ | ✅ | — | GlobalEffect (vision totale) |
| Sasuke Uchiha | ❓ | ✅ | — | Mark (électrisation), Téléport |
| Guts | ❓ | ✅ | — | Bonus selon PV manquants, Reset sur kill |
| Sung Jin-Woo | ❓ | ✅ | — | Summon contrôlable, AbilitySet ×2 |
| Mark Evans | Support/Tank | ✅ | ✅ jouable, modèle 3D (testé en solo le 2026-11-25) | Synergy (allié lié), Mur |
| Kirito | Top | ✅ | 🔧 jouable, simplifié (R) | Mark ×5, réduction de recharge croisée |
| Giorno Giovanna | Support | ✅ | ✅ jouable, modèle 3D + stand (testé en solo le 2026-11-25) | Charges, Forme (stand) |
| Jotaro | Top | ✅ | 🔧 jouable, simplifié (E stand), modèle 3D + stand (testé en solo le 2026-11-25) | Forme (stand), GlobalEffect (arrêt du temps), mur destructible |
| Law | Support | ✅ | — | Échange de position, téléport allié |
| Madara | Mage | ✅ | — | Canalisation, recast |
| Pain | Bot (ADC) | ✅ | ✅ jouable, modèle 3D de Tendô (testé en solo le 2026-11-25) | 6 AbilitySets, jauge de passif |
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
| Ainz A | dégâts magiques, slow 20 % 2 s | dégâts magiques (« chaos » = magiques ✅), valeurs ? |
| Ainz Z | durée 2.5 s, [17 s] | [15 s] |
| Ainz E | 3 charges, [12 s / charge] | zone unique, [10 s] |
| Luffy R | Gear 4 | Gear 5 |
| Naruto A | [7 s] | [8 s] |

Proposition : la « clean doc » fait foi quand elle donne une valeur, sinon le brouillon. 🟡

## 3. Kits complets

Format : **Touche — Nom** : effet. `[recharge]`

### Ainz Ooal Gown — Support
- **P — Bénédiction d'Albedo** : hors combat (pas de dégâts ni de compétence ennemie reçus récemment), régénère ?% du mana max par seconde.
- **A — Mantle of Chaos** : projectile ciblé, ? dégâts magiques, ralentit ?% pendant ? s. `[7 s]`
- **Z — Infinity Wall** : bouclier sur un allié qui absorbe dégâts magiques **et contrôles** jusqu'à ? dégâts, expire après ? s. `[15 s]`
- **E — Life Essence** : zone de soin ? PV/s pendant ? s (variante : 3 charges posables à longue portée). `[10 s]`
- **R — Penetrate Up** : projectile très longue portée, dégâts de zone et −?% résistances pendant ? s. `[100 s]`

**Valeurs choisies (2026-11-25, référence : supports mages du genre)** : stats PV 560 (+99), mana 480 (+23,5), régén. mana 1,6/s, AD 52, armure 19 (+4,4), RM 30, vitesse d'attaque 0,625, vitesse 3,3 m/s, portée 5,5 m (attaque à distance). P : 2 % du mana max par seconde hors combat. A : 80/120/160/200/240 (+60 % AP) magiques, ralentit 20/25/30/35/40 % pendant 2 s, portée 9 m, coût 60–80. Z : bouclier magique 80/115/150/185/220 (+50 % AP), bloque les contrôles, 2,5 s, recharge 15/14/13/12/11 s, portée 8 m. E : zone de 2,5 m pendant 3 s, soigne 12/18/24/30/36 (+5 % AP) toutes les 0,5 s ; la version à 3 charges du brouillon n'est pas retenue. R (3 rangs) : projectile de 30 m, impact de 3,5 m, 200/325/450 (+75 % AP) magiques, −20/30/40 % d'armure et de RM pendant 4 s, recharge 100/85/70 s.

### Luffy — Jungle
- **P** : perd ? PV/s dans l'eau ; résistance physique accrue ; AA chargée qui ralentit un champion `[20 s]`.
- **A — Rifle** : coup à distance, ralentit. `[8 s]`
- **Z — Gatling** : canalise 2 s sur une cible (cible verrouillée), lourds dégâts ; portée qui croît avec l'AD. `[12 s]`
- **E — Rocket** : s'agrippe à la cible et fonce vers elle. `[10 s]`
- **R — Gear** : augmente les dégâts des compétences selon l'AD. `[90 s]`

**Implémentation (2026-11-25)** : P = toutes les 20 s, la prochaine attaque de base sur un champion inflige 30 (+30 % AD bonus) et ralentit de 30 % pendant 1,5 s ; « résistance physique accrue » = armure de base 40 (+4,8 par niveau) au lieu de 32–36 ; fruit du démon : perd 1,5 % de ses PV max par seconde dans la rivière (jamais mortel). A = poing étiré de 6 m (projectile), 60→180 (+90 % AD bonus), ralentit de 25 % pendant 1,5 s, recharge 8→6 s. Z = 8 coups en 2 s sur la cible (12→36 (+15 % AD bonus) chacun, soit 96→288 (+120 %)), Luffy ne peut pas bouger pendant ce temps, portée 3,5 m (la portée qui croît avec l'AD n'est pas codée), recharge 12→10 s. E = fonce sur la cible jusqu'à 8 m, 50→170 (+60 % AD bonus), recharge 10→8 s. R = Gear 5 pendant 10 / 13 / 16 s selon le rang : +20/30/40 % d'AD et +10 % de vitesse, recharge 90/80/70 s. Changements du 2026-11-25 (propriétaire) : A = coup de poing très rapide (projectile à 70 m/s) ; passif armé = portée d'attaque +0,7 m au niveau 1 à +2,4 m au niveau 18.

**Modèle (2026-11-25)** : modèle 3D texturé déjà présent dans le dépôt (`Assets/Sprites/Origines/ainz_ooal_gown___fbx_3d_model_by_azureberylblue_dewaw2c`, modèle de fan d'azureberylblue sur DeviantArt), rig Biped importé en Humanoid ; animations reprises de l'ancien projet (`Idle`, `Running`, attaque d'Ainz) dans `Art/Animations/ANIM_Champion_Humanoid.controller`. L'autre modèle (`Sprites/Origines/ainzOoalGown.fbx`, sans squelette ni textures) n'est plus utilisé. Aucun modèle pour les autres champions dans le dépôt.

**Changements du 2026-11-25 (propriétaire)** : Q (Manteau du chaos) vise l'ennemi le plus proche du curseur ; E (Essence de vie) devient une zone autour d'Ainz qui le suit, 3 / 3,5 / 4 / 4,5 / 5 m selon le rang ; Z (Mur de l'infini) = bouclier contre les seuls dégâts magiques qui bloque les contrôles, affiché en violet.

**Correction du 2026-11-25 (propriétaire)** : le Q n'a pas de ciblage spécial, il part sur l'ennemi sous la souris (lancement rapide ou normal). Le « Q-clic » dont parlait le propriétaire est l'attaque-déplacement de LoL (07 §11). **Animations** : Ainz ne marche pas, il flotte (`UnitView._floats` : pas d'animation de course, modèle à 0,35 m du sol qui ondule et se penche dans le sens du déplacement) ; l'évènement `aaCreation` de son clip d'attaque, devenu inutile, est retiré.

### Zoro Roronoa
- **P — Haki de l'armement** : chaque AA sur un champion donne +1 AD (cumul).
- **A — Nitoryu slash** : ? AD `[1 s]`. Chaque coup qui touche charge ; au 3e, devient **Demon slash** (150 %) `[1.5 s]`.
- **Z — Parade** : canalise ≤ 1.5 s, bloque 40 % du premier coup ; si bloqué, bouclier = 40 % AD + dégâts bloqués. `[11 s]`
- **E — Calibre 36** : lame d'air courte portée, ? dégâts physiques + projection. `[8 s]`
- **R — Ashura** : grand dash, ? dégâts physiques, +10 % par ennemi touché. `[90 s]`
**Implémentation (2026-11-25, rôle Top choisi par Claude)** : A = coup en cône de 3 m, 40/65/90/115/140 (+100 % AD bonus), recharge 5→3 s ; chaque utilisation qui touche charge Zoro (6 s) et la 3e devient **Demon slash** : cône de 4 m et 120°, 60→210 (+150 % AD bonus), recharge 1,5 s, puis retour à Nitoryu (jeu de compétences alternatif) ; Z = bouclier de 60→200 (+40 % AD bonus) pendant 1,5 s (pas de blocage de 40 % du premier coup) ; E = lame d'air de 6 m, 60→200 (+80 % AD bonus), projection de 2 m, traverse 3 cibles ; R = dash de 7 m puis zone de 3 m, 150/275/400 (+120 % AD bonus) (le +10 % par ennemi n'est pas codé). P = +1 AD par attaque de base sur un champion (`on_hit_champion`), **30 cumuls au plus, perdus à la mort** (choix d'équilibrage : sans plafond, le bonus devient illimité en fin de partie).

### Naruto Uzumaki
- **P** : +10 % vitesse dans une zone de carte dédiée.
- **A — Rasengan** : fonce sur l'ennemi ; touché : ? magiques + projection ; raté : ralenti 1 s. `[7–8 s]`
- **Z — Shurikens** : 3 shurikens en ligne fixe, ? physiques, −15 % vitesse par shuriken. `[8 s]`
- **E — Clone** : dash dans une direction choisie ; le clone qui le propulse est une animation. `[15 s]`
- **R — Multi Rasen-Shuriken** : Rasen-Shuriken lancé sur la zone visée, ? dégâts magiques en zone. Les 2 clones qui apparaissent sont une animation, pas des unités. `[90 s]`
**Implémentation (2026-11-25, rôle Mid choisi par Claude)** : A = fonce sur la cible (6 m), 70→230 (+60 % AP) magiques, projection de 2,5 m, recharge 8→6 s (le ralentissement si raté n'est pas codé) ; Z = 3 shurikens en éventail de 18°, 30→90 (+40 % AD bonus) physiques, −15 % de vitesse par shuriken (cumulable 3 fois, 2 s) ; E = dash de 4,5 m vers le point visé, recharge 15→11 s ; R = zone de 4 m au point visé après 0,6 s, 200/325/450 (+80 % AP) magiques. P non codé (demande des zones de terrain).

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
- **R — Belzébuth** `[60–90 s selon le monstre]` : estomac vide → ?–? dégâts bruts à un monstre ; s'il meurt, il est stocké. Estomac plein → se transforme et lance la compétence du monstre absorbé.
  → Correspondance monstre ↔ compétence : [01-gdd.md §7.1](01-gdd.md).

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

**Implémentation (2026-11-25)** : stats PV 640 (+108), mana 310 (+45), AD 56, armure 42 (+4,7), RM 32, VA 0,644, vitesse 3,35 m/s, mêlée. P = Mark encaisse 10 % des dégâts que subit son allié lié (avant ses propres résistances). L'allié se choisit avec l'objet **Gants de David Evans** (`03-objets.md`), que Mark a dès le début : actif ciblé sur un allié à 10 m, **utilisable une seule fois** (l'objet disparaît), le lien dure toute la partie (gardé à la mort). A = cône de 3,5 m et 70°, 70→250 (+60 % AP) magiques, ralentit de 40 % pendant 1 s, coût 50→70. Z = roulade de 4 m vers le point visé, puis ralenti de 30 % pendant 0,8 s (≈ 0,5 s après la sortie), recharge 12→10 s. E = mur de 4 m vers la direction visée, qui suit Mark quand il marche (sa direction reste celle du lancement) pendant 3 s ; il n'arrête rien, mais les compétences dont la source est devant le mur (cône de 120°) font 70 % de dégâts en moins à Mark ; recharge 14→10 s. R = frappe au point visé (6 m) après 0,5 s : onde de 4,5 m, 150/250/350 (+70 % AP) magiques et immobilisation 2 s ; centre de 1,6 m, étourdissement 2 s ; recharge 90/80/70 s. **Simplifié** : « touche le mur » = la source du sort est devant le mur. Modèle : `Mark Evans/Mark Evans.fbx` (ballon masqué), animé par `ProceduralRig`.

### Kirito
- **P** : toutes les 3 attaques, dégâts bonus ; chaque attaque pose une marque.
- **A — Attaque circulaire** : coup circulaire, bonus contre hémorragie ; si touche, recharge de E divisée par 2. `[7 s]`
- **Z** : dague empoisonnée, hémorragie 4 s. `[18 s]`
- **E** : petit dash + coup ; si touche, recharge de A divisée par 2. `[8 s]`
- **R — Starburst Stream** : sur une cible à 5 marques, 5 AA qui ignorent 25 % armure. `[65 s]`

**Implémentation (2026-11-25, rôle Top choisi par Claude)** : P = chaque attaque de base pose une marque (5 au plus, 6 s) et la 3e attaque d'une série (fenêtre de 4 s) inflige 25 (+50 % AD bonus) physiques. A = cercle de 2,5 m autour de Kirito, 50→170 (+80 % AD bonus), +20→60 contre une cible en hémorragie, la recharge restante de E est divisée par 2 si un ennemi est touché, recharge 7→5 s. Z = dague de 8 m, 40→120 (+40 % AD bonus) et hémorragie de 4 s (10→30 (+10 % AD bonus) par seconde), recharge 18→14 s. E = dash de 3 m puis coup de 1,5 m de rayon, 40→140 (+60 % AD bonus), la recharge restante de A est divisée par 2 si un ennemi est touché, recharge 8→6 s. R = cible à 2,5 m : armure réduite de 25 % pendant 3 s puis 100/175/250 (+100 % AD bonus) **+40/60/80 par marque**, marques consommées, recharge 65/60/55 s. Simplifié : le R se lance sans exiger 5 marques (il en tire ses dégâts bonus) et frappe en un seul coup au lieu de 5 attaques.

### Giorno Giovanna — Support
- **P** : possède un stand (forme) ; régénération bonus dans les buissons.
- **A — Fleurs dorées** (3 charges `[7 s]` chacune) : 1re touche = dégâts + ralentit ; 2e pendant le ralentissement = immobilise 1.5 s ; 3e = étourdit 1.5 s. Stand : hitbox plus grande.
- **Z** : zone qui transfère sa vie aux alliés, durée infinie, gros coût en mana (gratuit avec le stand).
- **E** : +vitesse d'attaque d'un allié `[10 s]` ; stand : repousse les ennemis proches.
- **R** : invoque le stand 10 s et silence 3 s en zone. `[180 s]`

**Implémentation (2026-11-25)** : stats PV 560 (+104), mana 400 (+40), AD 52, armure 26, RM 30, VA 0,625, vitesse 3,3 m/s, distance 5,25 m. P = dans un buisson, +0,5 PV/s par cumul, cumuls = 4 + niveau (2,5 PV/s au niveau 1, 11 PV/s au niveau 18). A = 3 charges (7 s chacune), fleur de 9 m, 50→150 (+40 % AP) magiques, ralentit de 30 % pendant 2 s et marque la cible 2 s ; 2e fleur sur une cible marquée : immobilise 1,5 s ; 3e : étourdit 1,5 s. Z = sort activable : toutes les 0,5 s, soigne les alliés à 3,5 m de 8→24 (+4 % AP) et coûte 6→18 PV à Giorno ; 30 de mana à l'activation puis 12→20 mana par seconde. E = +25→45 % de vitesse d'attaque pendant 4 s sur un allié (ou lui-même) à 7 m. R = Gold Experience pendant 10 s et silence 3 s sur les ennemis à 4,5 m, recharge 180/160/140 s. **Avec Gold Experience** (jeu de compétences alternatif) : fleurs à zone de touche de 1,2 m au lieu de 0,5 m, Z sans mana, E repousse de 3 m les ennemis à 3 m. Le stand apparaît derrière lui (`UnitView` : compagnon). Les charges de fleurs sont pleines au changement de forme.

### Jotaro
- **P** : possède un stand ; vision augmentée.
- **A** : renforce la prochaine AA, reset d'AA `[4 s]` ; stand : projectile ? AD + ralentissement.
- **Z** : +résistances 4 s `[18 s]` ; stand : invulnérable 2 s.
- **E** : provoque les ennemis proches `[12 s]` ; stand : ouvre temporairement un passage dans un mur.
- **R** : invoque le stand 13 s et **arrête le temps 3 s** : lui seul peut bouger, sans compétences. `[180 s]`

**Implémentation (2026-11-25, rôle Top choisi par Claude)** : stats PV 650 (+105), mana 300, AD 64 (+3,5), armure 36 (+4,8), RM 32, VA 0,65, vitesse 3,45 m/s, mêlée, **vision 15 m** au lieu de 12 (passif). A = réinitialise l'attaque ; la prochaine attaque de base dans les 5 s inflige 20→120 (+40 % AD bonus) physiques en plus. Z = +15→43 d'armure et de RM pendant 4 s, recharge 18→14 s. E = provoque les ennemis à 3,5 m pendant 1→1,6 s (ils n'attaquent que Jotaro et ne lancent pas de sort). R = Star Platinum pendant 13 s et **temps arrêté 3 s** sur toutes les autres unités de la carte, alliées comprises (ni déplacement, ni attaque, ni sort) ; Jotaro peut seulement se déplacer ; recharge 180/160/140 s. Déséquilibré exprès : non bloqué par les boucliers anti-contrôle, non purifiable. **Avec Star Platinum** : A = Star Finger, doigt de 6 m qui traverse 3 cibles, 60→200 (+70 % AD bonus) physiques, ralentit de 30 % pendant 1,5 s ; Z = insensible aux dégâts pendant 2 s ; E = Jotaro traverse le mur (ruée de 4 m qui passe les murs). **Simplifié** : le E du stand ne creuse pas de passage pour les autres.

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

### Pain — Bot (ADC)
- **P** : jauge qui se charge (temps, attaques et compétences qui touchent ; pleine sur un kill). Pleine → passe au pantin suivant (ordre fixe de 6) ; un kill permet de choisir de rester. Hors combat, passage automatique. Changer de pantin réinitialise les recharges.
- **A** `[5 s]` et **E** `[7 s]` dépendent du pantin : attraction, lance-missiles (zone 10 %, coût continu en mana par seconde ✅, `CostPerSecond`), vol de 20 % des PV actuels rendus 3 s après, chien qui fonce tout droit (attaque en ligne non contrôlée, le chien est visuel), vol de mana, silence 1.5 s / sphère d'attraction, laser, déplacement forcé aléatoire, changement de pantin libre (suivant / précédent), absorption de la prochaine attaque.
- **R** : passif — jauge pleine = changement automatique.
- **Pas de Z** dans le PDF ❓

**Implémentation (2026-11-25)** : stats PV 600 (+100), mana 350 (+45), AD 60 (+2,8), armure 26, RM 30, VA 0,658, vitesse 3,3 m/s, distance 5,5 m. P = jauge de 100 : +2 par seconde, +5 par attaque de base, +5 par ennemi touché par une compétence ; pleine, Pain passe tout de suite au pantin suivant et ses recharges sont réinitialisées (la jauge est visible comme un effet à cumuls). **Après un kill**, la jauge est pleine et Pain choisit : R (Ondes de chakra, disponible dès le niveau 1, sans point de compétence) le fait passer au pantin suivant, sinon il garde son pantin et change automatiquement dès qu'il sort du combat (5 s). A `[5 s]` et E `[7 s]` par pantin : **Tendô** A attire de 3 m une cible à 6 m, 60→200 (+80 % AD bonus) physiques ; E sphère de 3 m au point visé (8 m) qui attire et inflige 4 × 15→55 (+15 %) en 1 s. **Shuradô** A lance-missiles activable : +1,5 m de portée, chaque attaque inflige 10 % de l'AD en zone (2,5 m), 20 de mana puis 8→12 par seconde ; E laser de 9 m sur 1,2 m, 80→260 (+90 %) physiques. **Ningendô** A retire 20 % des PV actuels de la cible (dégâts bruts), qui les récupère 3 s plus tard ; E pousse la cible de 3 m dans une direction aléatoire en 1 s. **Chikushôdô** A chien géant (projectile à tête chercheuse, 8 m), 70→230 (+80 %) physiques ; E passe au pantin suivant. **Gakidô** A absorbe 15→27 % du mana actuel de la cible ; E absorbe la prochaine attaque de base reçue (4 s). **Jigokudô** A silence 1,5 s ; E revient au pantin précédent. Pas de Z (confirmé par le propriétaire). Un seul modèle (Tendô) pour les six pantins.

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

## 6. Changements du 2026-11-26 (retours du propriétaire)

- **Ainz** : R = sort au sol. On vise un point avec la souris (l'indicateur est un cercle centré sur la souris), portée 40 m ; hors de portée, Ainz avance jusqu'à être à portée puis le lance, et un autre ordre (déplacement, attaque) annule l'attente. L'impact au point visé fait les dégâts et les effets, 0,8 s après le lancement (la boule d'énergie n'est qu'un visuel). Z : seulement sur un champion allié. E : soigne seulement les champions alliés. Passif : allumé quand il est actif ; en combat, le HUD montre le temps restant avant qu'il ne le redevienne.
- **Giorno** : mêlée (1,75 m). Z : soigne seulement les champions alliés et ne coûte des PV que s'il soigne quelqu'un. A : les fleurs traversent et touchent tout ce qu'elles rencontrent (sbires compris) ; ralentissement 20→40 %, immobilisation et étourdissement 1→1,5 s selon le rang. R : Gold Experience 8 / 10 / 12 s.
- **Kirito** : R seulement sur un champion ennemi qui porte 5 marques (portée 6 m) ; sans cible valide, il prend le champion marqué 5 fois le plus proche, sinon il ne part pas (pas de recharge). Les réductions de recharge entre A et E ne jouent que si un champion ennemi est touché.
- **Mark Evans** : R, zone et durées qui grandissent avec les rangs : onde 3,5 / 4,25 / 5 m, centre 1,2 / 1,5 / 1,8 m, étourdissement et immobilisation 1 / 1,5 / 2 s.
- **Naruto** : A = il tient son Rasengan 2,5 s et court 30 % plus vite, le joueur le dirige ; le premier ennemi touché prend les dégâts et est projeté ; s'il ne touche personne, il est ralenti (30 %, 1 s). Il peut lancer son E pendant ce temps.
- **Pain** : pas de mana (la barre sous ses PV sera la jauge de pantin). A et E montent jusqu'au rang 9 (pas de Z ni de R à monter : 18 points au total) ; les valeurs des rangs 6 à 9 prolongent celles du rang 5 jusqu'à +30 %. 5 s au moins entre deux changements de pantin (R, E de Chikushôdô et de Jigokudô bloqués pendant ce temps) : c'est le décompte affiché sur le passif, qui s'allume quand le changement est possible. Lance-missiles de Shuradô : bonus de 4 s (plus de coût en mana par seconde, puisqu'il n'a pas de mana).
