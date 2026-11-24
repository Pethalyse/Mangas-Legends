# ADR-0002 — Serveur autoritaire, client prédictif

- Statut : **Accepté** (2026-11-24, MC MEZ)
- Date : 2026-11-24

## Contexte
L'ancien code calculait dégâts, soins et stats dans des `ClientRpc` et laissait le client détecter les impacts et envoyer les prix (audit A1–A3) : les clients divergeaient et la triche était triviale. L'équipe veut que le serveur fasse les vrais calculs et la validation, tout en gardant une sensation de temps réel côté client.

## Décision
- Le serveur (l'hôte) est seul maître de l'état : dégâts, soins, recharges, mana, or, objets, morts.
- Le client envoie ses entrées par tick et **prédit** son propre champion (déplacement, lancement, projectiles). Il affiche des **retours provisoires** (animations, VFX, « faux » dégâts) remplacés par le résultat serveur.
- Les autres unités sont interpolées.
- Les impacts sont vérifiés côté serveur avec compensation de latence.

## Alternatives écartées
- Serveur autoritaire sans prédiction : ~1 aller-retour de latence sur chaque action, mauvaise sensation pour un MOBA.
- Autorité client : impossible à garder cohérente.
- Lockstep déterministe (type Photon Quantum) : très adapté aux MOBA mais impose un moteur de simulation déterministe à part (ECS, maths fixes) et une courbe d'apprentissage forte.

## Conséquences
- La simulation doit être **rejouable** : état en `[Networked]`, temps en ticks, pas de coroutines ni `Time.time` dans la logique, pas d'aléatoire non synchronisé.
- La présentation doit savoir annuler un retour provisoire.
- Partie entre amis : on fait confiance à l'hôte.
