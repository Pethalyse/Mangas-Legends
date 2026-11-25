# MangaLegends — Objets

> Statut : brouillon. Un seul objet est esquissé dans les PDF ; les objets de l'ancien prototype sont inventoriés au §4 (étape 1 de la refonte, 2026-11-24).

## 1. Règles 🟡

- 6 emplacements ; un objet complet peut être acheté si ses composants libèrent de la place.
- Prix d'un objet complet = prix de la recette + prix des composants manquants.
- Revente : 70 % du prix total (valeur actuelle du code).
- Objets **uniques** : un seul exemplaire par champion.
- Effets : stats, **passifs** (à l'impact, vitesse, soin…), **actifs** (touches 1–6).
- Toute transaction est validée **par le serveur** (le client n'envoie jamais un prix).

## 2. Hiérarchie

| Niveau | Nom dans le repo | Exemple |
|---|---|---|
| Tier 1 — basique | `Terciaires` | Lame (+10 AD), Dague (+10 % vitesse) |
| Tier 2 — composant | `Secondaires` | Lame d'acier supérieure |
| Tier 3 — complet | `Uniques` | Wado Ichimonji |

## 3. Objets décrits dans les PDF

### Wado Ichimonji (complet, unique)
- +70 AD, +20 % vitesse d'attaque, +10 % vitesse de déplacement.
- **Passif — Observation** : rayon de vision et de détection des unités cachées augmenté.
- Recette : 750 · **Prix total : 3 100** ❓ (le PDF additionne 1 100 + 1 000 + 200 + 500 : à revérifier).
- Composants :
  - Pierre magique de renforcement (+40 AD, 1 100) ← Minerai étrange (+30 AD)
  - Lame d'acier supérieure (+25 AD, +15 % AS, 1 050) ← Dague (+10 % MS), Lame (+10 AD) ×2
  - Dague (+10 % AS, 200)

❓ Incohérence : « Dague » vaut +10 % MS dans une recette et +10 % AS dans une autre.

## 4. Objets de l'ancien prototype (inventaire du 2026-11-24)

Source : `Assets/Resources/Items/**.asset` (YAML lisible, scripts d'origine dans `Legacy/`). Ce sont des valeurs de départ à migrer vers `ItemDefinition` (étape 9), pas des décisions d'équilibrage.
Le « prix recette » est le coût propre de l'objet ; le prix total ajoute celui des composants.

**Migré le 2026-11-25** : les 14 objets existent en `ItemDefinition` (`Assets/_Project/Content/Items/`, ids du guide de reprise), plus `item.odm_recruit` (bottes de base, +0,25 m/s). Choix : la Dague donne +10 % de vitesse d'attaque ; la Pierre de régénération +50 % de régénération de mana (`PercentAdd 0.5`) ; Wado Ichimonji coûte 3 300 au total (recette du prototype). Passifs et actifs reliés le 2026-11-25 : un passif déclare un déclencheur (`OnHit`, `OnAbilityHit`, `OnHeal`, `OnKill`, `OnDamageTaken`, `Periodic`), une recharge et des effets ; un actif se lance avec les touches 1, 2, 3, 5, 6, 7 (comme LoL) et le mode de lancement choisi dans les options. **Sceptre de la lune** : actif « Nébuleuse noire » relié (126 dégâts magiques, cible ennemie à 6 m, recharge 90 s) ; le passif « Eclipse » reste à définir (texte tronqué). **Wado Ichimonji** : « Observation » = +2 m de rayon de vision (la détection des unités cachées attendra l'invisibilité).

### Tier 1 — basiques

| Objet | Stats | Prix |
|---|---|---|
| Bouclier en cuir | +15 Armure | 300 |
| Bracelet de précision | +15 % Critique | 600 |
| Circuit de mana | +250 Mana | 350 |
| Dague | +10 % Vitesse d'attaque | 300 |
| Grimoire des arcanes | +25 AP | 450 |
| Lame | +10 AD | 350 |
| Minerai étrange | +30 AD | 770 |
| Orbe de résistance | +20 Résistance magique | 400 |
| Pierre de régénération | +50 Régénération de mana | 300 |
| Veine de vie | +150 PV | 400 |

### Tier 2 — composants

| Objet | Stats | Recette | Composants | Prix total |
|---|---|---|---|---|
| Lame d'acier de qualité supérieure | +25 AD, +15 % Vitesse d'attaque | 150 | Lame, Lame, Dague | 1 150 |
| Pierre magique de renforcement | +40 AD | 330 | Minerai étrange | 1 100 |

### Tier 3 — complets

| Objet | Stats | Recette | Composants | Effets | Prix total |
|---|---|---|---|---|---|
| Wado Ichimonji (unique) | +70 AD, +20 % Vitesse d'attaque, +10 % Vitesse de déplacement | 750 | Lame d'acier sup., Pierre magique de renf., Dague | passif de test (vide) | 3 300 |
| Sceptre de la lune (unique) | +150 PV, +80 AP, +350 Mana, +10 Réduction de recharge | 3 100 | aucun | passif « Eclipse » : soins 10 % plus efficaces sous condition sur l’allié ciblé (texte tronqué, non implémenté) ; actif « Nébuleuse noire » : 126 dégâts au champion ciblé, recharge 90 s (défini mais pas relié à l'objet) | 3 100 |

❓ Le PDF annonce 3 100 pour Wado Ichimonji, le prototype donne 3 300 ; la Dague y vaut +10 % vitesse d'attaque (le PDF hésite avec la vitesse de déplacement).

## 5. Objets système (bottes, support, vision) ✅

Choisis par le propriétaire le 2026-11-24 ; rôles et valeurs de référence : [07-regles-de-jeu.md](07-regles-de-jeu.md). Valeurs choisies par Claude à l'équilibrage.

### 5.1 Bottes : équipement tridimensionnel (L'Attaque des Titans)

Une version de base, six variantes (niveau 2), et une version améliorée de chacune (niveau 3), obtenue par la quête du Mid (07 §9). La version améliorée renforce l'effet de sa variante.

| Id | Nom | Effet | Rôle dans le genre |
|---|---|---|---|
| `item.odm_recruit` | Équipement tridimensionnel de recrue | vitesse de déplacement | bottes de base |
| `item.odm_levi` | Équipement tridimensionnel de Livaï | + vitesse d'attaque | bottes de vitesse d'attaque |
| `item.odm_hange` | Équipement tridimensionnel de Hansi | + pénétration magique | bottes de mage |
| `item.odm_garrison` | Équipement tridimensionnel de la Garnison | + armure, réduit les dégâts des attaques de base | bottes d'armure |
| `item.odm_mikasa` | Équipement tridimensionnel de Mikasa | + résistance magique, + ténacité | bottes de résistance magique |
| `item.odm_armin` | Équipement tridimensionnel d'Armin | + accélération de compétence et des sorts d'invocateur | bottes de recharge |
| `item.odm_kenny` | Équipement tridimensionnel anti-personnel de Kenny | grande vitesse, réduit les ralentissements | bottes de vitesse |

Les versions améliorées prennent l'id de la variante suivi de `_mk2` (ex. `item.odm_levi_mk2`).

### 5.2 Objet de support

| Id | Nom | Effet |
|---|---|---|
| `item.nami_treasure_map` | Carte au trésor de Nami (One Piece) | revenu d'or du support (07 §9) ; débloque la quête du Support et le stockage des Den Den Mushi |

### 5.3 Vision

| Id | Nom | Rôle |
|---|---|---|
| `item.shino_insects` | Insectes de Shino (Naruto) | balise furtive, bijou gratuit |
| `item.den_den_mushi_watch` | Den Den Mushi de surveillance (One Piece) | balise de contrôle achetable |
| `item.observation_haki` | Haki de l'observation (One Piece) | bijou de détection (révèle balises et unités cachées) |

### 5.4 Compagnons du jungler : fées de Zelda ✅

Le jungler achète une fée au départ ; elle accompagne Smite et évolue avec les monstres tués (07 §7.2). Trois variantes, comme dans le genre (choix des fées et effets par Claude) :

| Id | Nom | Effet une fois évoluée | Rôle dans le genre |
|---|---|---|---|
| `item.fairy_navi` | Navi | bonus de vitesse en entrant dans la jungle ou la rivière, et après avoir tué un monstre | compagnon de vitesse |
| `item.fairy_great` | Fée de la Grande Fée (fée en bouteille) | bouclier après avoir tué un monstre ou hors combat | compagnon défensif |
| `item.fairy_tatl` | Taya | Smite et les attaques sur champion brûlent et ralentissent | compagnon offensif |

