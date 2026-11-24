# MangaLegends — Documentation de conception

Cette suite de docs est la **source de vérité** du projet. Elle est tenue à jour par l'équipe **et par Claude Code** à chaque avancée (voir [`CLAUDE.md`](../CLAUDE.md) à la racine du repo).

| Doc | Rôle | Qui le modifie |
|---|---|---|
| [01-gdd.md](01-gdd.md) | Game Design Document : vision, règles du jeu, carte, économie, progression | Game design (Claude Code propose, l'humain valide) |
| [02-champions.md](02-champions.md) | Fiches des champions : kits, valeurs, état d'implémentation | Game design + Claude Code (colonne « état ») |
| [03-objets.md](03-objets.md) | Objets, composants, recettes, prix | Game design |
| [04-architecture.md](04-architecture.md) | Architecture technique Unity / C# / réseau | Claude Code à chaque changement structurel |
| [05-conventions-code.md](05-conventions-code.md) | Règles de code obligatoires | Humain (Claude Code propose via ADR) |
| [06-plan-refactor.md](06-plan-refactor.md) | Plan de refactor du code existant, par étapes | Claude Code à chaque étape terminée |
| [07-regles-de-jeu.md](07-regles-de-jeu.md) | Règles du genre (LoL 2026) traduites pour MangaLegends : or, sbires, structures, jungle, sorts, quêtes | Claude Code à chaque règle codée ou changée |
| [adr/](adr/) | Architecture Decision Records : une décision = un fichier | Claude Code + humain |
| [journal.md](journal.md) | Journal de développement daté | Claude Code à chaque session |
| [guides/](guides/) | Guides pas à pas : étape 1 (Unity 6 + Fusion), reprise par Claude Code connecté à Unity | Claude Code |
| [CLAUDE.md](../CLAUDE.md) | Instructions pour Claude Code (racine du repo) | Humain |

## Où vivent ces docs

Dans `Docs/` à la racine du repo (hors de `Assets/` pour qu'Unity ne génère pas de `.meta`). Le repo fait foi.

## Règle d'or

> Un changement de code qui contredit un doc **met le doc à jour dans le même commit**. Un doc qui n'est plus vrai est un bug.
