# MangaLegends — Objets

> Statut : brouillon. Un seul objet est esquissé dans les PDF. Le repo contient déjà des objets en ScriptableObject (`Assets/Resources/Items/{Uniques,Secondaires,Terciaires}`) : **à inventorier lors de l'étape 1 du refactor** et à reporter ici.

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
