# Journal de développement

> Une entrée par session de travail, la plus récente en haut. Format :
> `## AAAA-MM-JJ — <titre>` puis **Fait**, **Décisions**, **Docs mis à jour**, **Prochaine étape**, **Bloquants**.

## 2026-11-24 — Correction des rôles

**Fait** : le rôle « Tireur » n'existe pas ; les rôles sont Top, Jungle, Mid, Bot, Support (le champion du Bot est l'ADC). GDD §4.2, fiche de Pain et enum `ChampionRole` (`Marksman` → `Bot`) corrigés.

**Fait (2)** : invocations précisées. Seul Sung Jin-Woo contrôle une invocation (Igris) ; les clones de Naruto (E, R) et le chien de Pain sont des animations d'attaques (dash, zone, ligne). GDD §4.3 et fiches mises à jour ; aucune donnée à changer (`SummonEffect` reste réservé aux vraies unités).

**Fait (3)** : pas de type de dégâts « chaos » : ce sont des dégâts magiques (GDD §4.4 et §10, fiche d'Ainz). Le code n'a que Physical, Magical, True.

**Fait (4)** : jeu de fans privé assumé, les noms de licences restent tels quels (GDD §1.2, plus aucune proposition de renommage) ; monstres de jungle définis (GDD §7.1) avec leur rôle et la compétence absorbée par Rimuru ; récompenses et positions en questions ouvertes. Zodd et Ogrest partagent volontairement la même compétence (peut évoluer).

**Fait (5)** : structures de la carte (GDD §6.1) : 3 tours par voie, un inhibiteur par voie à l'entrée de la base, 2 tours devant le Cœur, le Cœur. Les types `Tower` et `Structure` de `UnitKind` suffisent, aucun code à changer ; règles de destruction en questions ouvertes.

**Fait (6)** : règles des structures (GDD §6.1) : ordre de destruction, méga sbires après la chute d'un inhibiteur, or des structures, tours renforcées en début de partie. `GameRulesConfig` : sections *Structures* et *Minion waves* + `Validate` ; DataCheck compile. Valeurs à équilibrer en questions ouvertes.

## 2026-11-24 — Étape 4 : modèle de données (sans Unity)

**Fait** : identifiants et index de contenu, validation, prix des objets (Core, 52 tests verts) ; assembly Data (toutes les définitions et les effets) ; assembly Editor (sélecteur de type d'effet, validation, reconstruction du registre) ; compilation vérifiée contre des bouchons d'API Unity ; guide de reprise pour Claude Code connecté à Unity.

**Décisions** : effets en `[SerializeReference]` avec sélecteur de type ; durée de statut surchargeable par compétence ; index réseau du contenu = ordre des ids.

**Docs mis à jour** : plan, architecture §11, CLAUDE.md, README des docs, guide de reprise.

**Prochaine étape** : dans Unity, vérifications et création des assets (guide de reprise), puis étape 5.

**Bloquants** : Unity indisponible pour l'instant chez l'humain ; étape 1 en attente.

## 2026-11-24 — Étapes 1 à 3 : préparation moteur et Core

**Fait** : paquets inutiles retirés, objets de l'ancien prototype inventoriés, guide Unity 6 + Fusion 2 pour l'humain, assembly `MangaLegends.Core` (stats, dégâts, boucliers, statuts, mise à l'échelle, ticks, aléatoire déterministe) et 35 tests EditMode verts hors Unity.

**Décisions** : commits et PR au nom de Pethalyse uniquement, sans attribution d'outil.

**Docs mis à jour** : plan, architecture (§4 arborescence, §6 unités des stats, §7.1 formules), objets (§4), CLAUDE.md (commandes), guide d'étape 1.

**Prochaine étape** : l'humain fait l'étape 1 dans l'éditeur ; ensuite étape 4 (données).

**Bloquants** : compilation dans Unity non vérifiée.

## 2026-11-24 — Étape 0 : hygiène du dépôt

**Fait** : builds retirés du suivi, Mirror/ParrelSync/icône LoL supprimés, ancien code déplacé dans `Legacy/` (UTF-8), symboles de compilation nettoyés, `.editorconfig`, `.gitattributes`, `.gitignore`, docs et `CLAUDE.md` ajoutés au repo.

**Docs mis à jour** : plan (étape 0 cochée + notes), README des docs, README du repo.

**Prochaine étape** : étape 1 (Unity 6 LTS, Photon Fusion 2, Input System).

**Bloquants** : ouverture du projet dans Unity à vérifier par l'humain (scripts manquants attendus) ; décision Git LFS.

## 2026-11-24 — Décisions techniques de l'équipe

**Décisions** (MC MEZ)
- Abandon de Mirror. Service multijoueur avec calcul et validation côté serveur, client prédictif (animations et faux dégâts immédiats).
- Tout gameplay configurable en données ; code en anglais sans commentaires superflus ; Unity 6 LTS ; jeu réservé à un cercle d'amis.
- ADR 0002, 0003, 0004, 0005 acceptés ; 0001 abandonné ; 0006 (Photon Fusion 2) accepté.

**Docs mis à jour** : architecture (v0.2, modèle par ticks et prédiction), conventions (§1, §5, exemple), plan (refonte au lieu de refactor, nouvelle étape 5 de prototype réseau), GDD (§1, §9), CLAUDE.md.

**Prochaine étape** : étape 0 (hygiène du dépôt), qui demande un accès en écriture au repo.

## 2026-11-24 — Création de la documentation de conception

**Fait**
- Lecture des deux PDF de conception (kits des champions, un objet).
- Audit du repo `Pethalyse/Mangas-Legends` (commit `2366901`) : 11 problèmes d'architecture, 11 bugs, hygiène du dépôt.
- Rédaction de : GDD, champions, objets, architecture cible, conventions, plan de refactor, ADR 0001–0005, CLAUDE.md.

**Décisions** (toutes « Proposé », à valider par l'équipe)
- Garder Mirror, serveur autoritaire, compétences pilotées par les données, code en anglais, Unity 6 LTS.

**Prochaine étape** : validation des docs, puis étape 0 du refactor (hygiène du dépôt).

**Bloquants / questions** : GDD §10 (questions ouvertes) ; divergences entre les deux PDF (02-champions §2).
