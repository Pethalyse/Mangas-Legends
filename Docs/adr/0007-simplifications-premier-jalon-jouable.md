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
7. **Carte au tracé de LoL, en primitives** : bases dans les coins opposés avec murs et trois sorties, voies haute, centrale et basse, rivière en diagonale qui rejoint les voies du haut et du bas, fosses de Zodd (haut) et du drake (bas) ouvertes vers la rivière, quatre quadrants de jungle symétriques, 20 buissons, bords de carte ; placements et zones (rivière, buissons) dans `MapLayout`. Décor simple : arbres sur les murs de jungle et autour de la carte, rochers au bord de la rivière, plateformes des fontaines ; pas de relief ni de modèles 3D.
8. **Réglages des touches locaux** : touches et mode de lancement de chaque touche sauvegardés dans les `PlayerPrefs` du poste (comme les réglages du client LoL), pas dans un profil en ligne. La forme des indicateurs de visée est déduite des effets du sort ; les champs d'indicateur de `TargetingData` ne servent qu'à la corriger.
9. **Brouillard de guerre côté client** : chaque poste calcule ce que voit son équipe (rayon `VisionRange` des unités alliées vivantes ; structures toujours visibles) et masque le reste (corps, barres de vie, projectiles, zones, survol et ciblage à la souris). Le serveur n'en tient pas compte : un client modifié pourrait tout voir, risque accepté entre amis. Buissons comme dans LoL (une unité dans un buisson n'est vue que depuis le même buisson) ; les murs (calque `Terrain`) bloquent la vue.

## Alternatives écartées
- Implémenter dès maintenant hitboxes et compensation de latence : coûteux, invisible à moins de 80 ms, à reprendre si les tests en ligne le justifient.
- Scènes séparées pour le salon et la sélection : plus de transitions réseau à gérer pour un gain nul entre amis.

## Conséquences
- Une partie se lance en solo, en hôte ou en client ; tout le flux tient dans deux scènes (`MainMenu`, `Match`).
- À reprendre plus tard : compensation de latence (04 §9), brouillard de guerre, scènes dédiées si besoin, carte définitive.
- `Docs/04-architecture.md` décrit l'état réel (§4, §10, §13).
