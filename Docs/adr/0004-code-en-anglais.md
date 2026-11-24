# ADR-0004 — Code en anglais, sans commentaires superflus

- Statut : **Accepté** (2026-11-24, MC MEZ)
- Date : 2026-11-24

## Contexte
L'ancien code mélange français et anglais (`ChampionControleur`, `vie`, `Mouvements`) et contient beaucoup de code commenté.

## Décision
- Types, membres, noms d'assets techniques en **anglais**.
- **Pas de commentaires** sauf quand ils sont nécessaires : expliquer un *pourquoi* non évident (contrainte réseau, contournement d'un bug moteur, formule de design). Le *quoi* doit se lire dans les noms.
- Pas de code commenté, pas de commentaires XML systématiques.
- Docs et messages de commit peuvent être en français.

## Conséquences
- Noms explicites obligatoires (méthodes courtes, bien nommées).
- Glossaire FR ↔ EN dans le GDD si besoin.
