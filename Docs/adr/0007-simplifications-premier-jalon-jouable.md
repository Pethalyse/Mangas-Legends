# ADR-0007 — Simplifications du premier jalon jouable

- Statut : Proposé
- Date : 2026-11-25
- Décideurs : Claude Code (à valider par Pethalyse)

## Contexte
Les étapes 5 à 10 du plan ont été écrites d'un bloc pour obtenir une partie jouable. Plusieurs points de l'architecture cible (04 §3, §8, §9, §10) demandent beaucoup de travail pour un gain faible tant qu'on joue entre amis avec peu de latence. Il fallait trancher pour livrer un jeu qui tourne.

## Décision
1. **Impacts sans compensation de latence** : les projectiles, zones et attaques testent les positions actuelles des unités (géométrie plane : cercles et segments), côté autorité. `Runner.LagCompensation` et les `Hitbox` ne sont pas utilisés ; la compensation reste désactivée dans `NetworkProjectConfig`.
2. **Le salon et le choix du champion vivent dans la scène `Match`** (phase `Lobby` de `MatchController`), au lieu des scènes `Lobby` et `ChampionSelect`. Le choix passe par des RPC ponctuels de `PlayerSlot` (champion, sorts, équipe, pseudo), autorisés par les conventions.
3. **Services par runner** : `MatchServices` est un composant du prefab du runner (`PF_Runner`), atteint par `MatchServices.Of(runner)`. Il remplace la composition root par scène.
4. **Un composant `Unit` orchestre** les autres (`Stats`, `Status`, `Health`, `Resource`, cerveau, `Caster`, `Attack`, `Motor`) dans un ordre fixe à chaque tick ; ces composants n'ont pas leur propre `FixedUpdateNetwork`.
5. **État hôte non répliqué** : la file des corps à retirer (`MatchController`) est une liste C# côté hôte, l'hôte ne resimulant jamais.
6. **Interface construite en code** (UGUI, police intégrée) plutôt qu'en prefabs.
7. **Carte de test** : sol plat, voies, rivière et murs de jungle en primitives ; placements dans `MapLayout`.

## Alternatives écartées
- Implémenter dès maintenant hitboxes et compensation de latence : coûteux, invisible à moins de 80 ms, à reprendre si les tests en ligne le justifient.
- Scènes séparées pour le salon et la sélection : plus de transitions réseau à gérer pour un gain nul entre amis.

## Conséquences
- Une partie se lance en solo, en hôte ou en client ; tout le flux tient dans deux scènes (`MainMenu`, `Match`).
- À reprendre plus tard : compensation de latence (04 §9), brouillard de guerre, scènes dédiées si besoin, carte définitive.
- `Docs/04-architecture.md` décrit l'état réel (§4, §10, §13).
