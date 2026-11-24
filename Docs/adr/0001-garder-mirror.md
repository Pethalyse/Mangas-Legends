# ADR-0001 — Garder Mirror comme bibliothèque réseau

- Statut : **Abandonné** (2026-11-24) — remplacé par [ADR-0006](0006-photon-fusion-2.md). Décision de l'équipe : ne plus utiliser Mirror ; on veut un service qui gère l'autorité serveur **et** la prédiction client.
- Date : 2026-11-24

## Contexte
Le projet utilise déjà Mirror 81.4.0 (SyncVar, Command, ClientRpc, KCP, ParrelSync). Le refactor réécrit la logique réseau (voir audit A1–A3), mais pas forcément la bibliothèque.

## Décision
Garder **Mirror**, mis à jour vers la dernière version stable. Topologie hôte pour le développement, serveur dédié headless visé.

## Alternatives écartées
- **Netcode for GameObjects** (Unity) : officiel, mais migration complète sans gain décisif pour un MOBA serveur autoritaire de 10 joueurs.
- **Photon Fusion** : excellent pour la prédiction, mais service payant et dépendance à un cloud tiers.
- **FishNet** : proche de Mirror, migration sans bénéfice clair aujourd'hui.

## Conséquences
- Pas de migration de bibliothèque : l'effort va sur l'architecture.
- L'interest management de Mirror servira au brouillard de guerre.
- À réévaluer si la prédiction client devient nécessaire et trop coûteuse à écrire.
