# ADR-0006 — Photon Fusion 2 comme service multijoueur

- Statut : **Accepté** (2026-11-24, MC MEZ)
- Date : 2026-11-24
- Remplace : [ADR-0001](0001-garder-mirror.md)

## Contexte
L'équipe abandonne Mirror et veut un service multijoueur avec calcul et validation côté serveur, et un client qui reste en temps réel grâce à la prédiction ([ADR-0002](0002-serveur-autoritaire.md)). Le jeu est destiné à un groupe d'amis : pas de matchmaking public, pas d'infrastructure à maintenir idéalement.

## Décision
**Photon Fusion 2**, en mode **Host** sur le cloud Photon :
- simulation par ticks avec **prédiction et resimulation** intégrées pour les objets du joueur ;
- **compensation de latence** intégrée (hitboxes) ;
- relais et salons Photon : un ami héberge, les autres rejoignent avec un code, sans ouvrir de ports ;
- mode **Server** dédié possible plus tard sans réécrire la simulation ;
- offre gratuite jusqu'à 100 joueurs simultanés, largement suffisante entre amis.

## Alternatives
- **FishNet** : gratuit, open source, prédiction intégrée ; mais il faut héberger un serveur ou un relais soi-même.
- **Netcode for GameObjects + Unity Relay** : officiel, mais pas de prédiction ni de compensation de latence intégrées : à écrire nous-mêmes.
- **Photon Quantum** : déterministe, idéal pour un MOBA, mais impose un modèle de simulation totalement à part (voir ADR-0002).

## Conséquences
- Dépendance à un SDK propriétaire et au cloud Photon (compte et App ID à créer).
- La simulation suit le modèle Fusion : `NetworkBehaviour`, `[Networked]`, `FixedUpdateNetwork`, `INetworkInput`, `TickTimer`.
- `Core` et `Data` restent indépendants de Fusion pour limiter le coût d'un éventuel changement.
