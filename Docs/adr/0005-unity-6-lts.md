# ADR-0005 — Passer à Unity 6 LTS

- Statut : **Accepté** (2026-11-24, MC MEZ)
- Date : 2026-11-24

## Contexte
Le projet est sur Unity 2023.1.13f1, une version « tech stream » qui ne reçoit plus de correctifs.

## Décision
Migrer vers la dernière **Unity 6 LTS** à l'étape 1 du refactor, avant d'écrire le nouveau code.

## Conséquences
- Correctifs et support long terme ; Input System et Addressables à jour.
- Quelques API dépréciées à corriger ; vérifier la compatibilité du SDK réseau et des plugins tiers.
