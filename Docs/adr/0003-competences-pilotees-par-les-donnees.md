# ADR-0003 — Compétences pilotées par les données

- Statut : **Accepté** (2026-11-24, MC MEZ)
- Date : 2026-11-24

## Contexte
Plus de 40 champions sont prévus, aux mécaniques variées mais qui reposent sur des briques communes (dégâts, contrôles, boucliers, dashs, zones, projectiles, invocations, formes). Aujourd'hui chaque compétence est une classe avec UI, input et valeurs en dur.

## Décision
Une compétence = un `AbilityDefinition` (ScriptableObject) : ciblage + liste d'effets génériques (`[SerializeReference]`) + valeurs par rang. Un `AbilityBehaviour` C# optionnel traite les cas particuliers. Les jeux de compétences (`AbilitySet`) gèrent les formes.

## Alternatives écartées
- Une classe par compétence : rapide pour 1 champion, ingérable pour 40.
- Graphes visuels (Visual Scripting) : difficile à relire, versionner et tester.

## Conséquences
- Ajouter un champion consiste surtout à créer des assets.
- L'équilibrage se fait sans toucher au code.
- Il faut un outil de validation des données et une bonne couverture de tests des effets.
