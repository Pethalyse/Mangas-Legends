# CLAUDE.md — MangaLegends

> Les docs de conception sont dans `Docs/`.

## Le projet

MOBA 5v5 en Unity (C#), multijoueur entre amis (**Photon Fusion 2** en mode Host, voir `Docs/adr/0006`), champions issus de mangas, **Unity 6 LTS**. L'ancien code (Mirror, 2023) est remplacé par une **refonte** ; il reste consultable dans `Legacy/` jusqu'à l'étape 10. Langue de travail : **français** (docs, échanges) ; code en **anglais**.

## Avant de coder : lire

1. `Docs/06-plan-refactor.md` — à quelle étape de la refonte on est, ce qui reste.
2. `Docs/04-architecture.md` — architecture cible (assemblies, unités, compétences, réseau).
3. `Docs/05-conventions-code.md` — règles obligatoires.
4. Selon la tâche : `Docs/01-gdd.md`, `Docs/02-champions.md`, `Docs/03-objets.md`, `Docs/adr/`.
5. Les 2–3 dernières entrées de `Docs/journal.md`.

## Règles non négociables

- **Serveur autoritaire, client prédictif** : dégâts, soins, stats, or, objets, recharges, morts sont décidés sous `HasStateAuthority`. Le client prédit son champion et affiche des retours provisoires (anim, VFX, faux dégâts) remplacés par le résultat serveur.
- **Simulation rejouable** : état en `[Networked]`, logique dans `FixedUpdateNetwork()`, temps en ticks (`TickTimer`), aucun `Time.time`, coroutine, `UnityEngine.Random`, VFX ou log dans la simulation.
- Le client parle par `INetworkInput` ; RPC seulement pour des événements ponctuels non prédits.
- **Tout est configurable** : aucune valeur d'équilibrage, formule ni nom de personnage en dur ; tout passe par les ScriptableObjects (`*Definition`, `GameRulesConfig`).
- **Pas de** `GameObject.Find`, singletons statiques, lecture d'entrées hors `PlayerInputCollector`, `new void Update()`.
- **Pas de commentaires** sauf pour un *pourquoi* non évident ; pas de code commenté.
- **Respect des assemblies** : Core et Data ne dépendent ni de Fusion, ni de l'UI, ni de la présentation.
- **Originalité** : le jeu suit les conventions du genre MOBA mais ne reprend aucun contenu d'un jeu existant (noms, objets, carte, assets, textes de League of Legends ou autre).
- Fichiers en UTF-8.

## Mettre à jour les docs — obligatoire à chaque avancée

Une tâche n'est **pas terminée** tant que les docs ne reflètent pas le code. Dans le **même commit / la même PR** que le code :

| Si tu… | Mets à jour |
|---|---|
| termines ou avances une étape du refactor | `Docs/06-plan-refactor.md` : cocher les cases, ajouter une entrée dans « Notes d'avancement » |
| ajoutes/changes un système, une assembly, un composant, un flux réseau | `Docs/04-architecture.md` (diagrammes compris) |
| prends une décision technique non triviale (bibliothèque, pattern, compromis) | nouvel ADR dans `Docs/adr/` (copier `0000-modele.md`, numéro suivant) ; ne jamais réécrire un ADR accepté, le remplacer |
| implémentes ou modifies un champion | `Docs/02-champions.md` : colonne « Code » et toute valeur fixée |
| implémentes ou modifies un objet | `Docs/03-objets.md` |
| changes une règle de jeu, découvres une question de design | `Docs/01-gdd.md` (§10 pour les questions ouvertes) |
| proposes de changer une convention | ADR + `Docs/05-conventions-code.md` après validation |
| finis une session de travail | `Docs/journal.md` : nouvelle entrée en haut (fait, décisions, docs mis à jour, prochaine étape, bloquants) |

Ne **jamais** inventer une règle de game design pour combler un trou : l'ajouter aux questions ouvertes du GDD et demander.

## Façon de travailler

- Une branche par sujet (`refactor/…`, `feat/…`, `fix/…`, `docs/…`), Conventional Commits.
- Commits et PR **au nom du compte du propriétaire uniquement** (`Pethalyse`) : aucune mention d'outil ou d'IA, pas de `Co-Authored-By`, pas de lien de session.
- Chaque étape laisse le projet **compilable** ; à partir de l'étape 5, une partie doit rester lançable en hôte.
- Écrire les tests EditMode des règles Core avant ou avec le code.
- Demander avant : supprimer du contenu de jeu (assets, scènes), changer une décision d'un ADR accepté, pousser sur `main`.
- Unity ne peut pas tourner dans tous les environnements : si tu ne peux pas compiler ni lancer, dis-le explicitement dans la PR et le journal, et liste ce que l'humain doit vérifier dans l'éditeur.

## Commandes utiles

- Tests du Core sans Unity : `dotnet test Tools/CoreTests/Tests` (nécessite le SDK .NET 8). À lancer après toute modification de `Assets/_Project/Core` ou de ses tests.
- Tout nouveau fichier ou dossier sous `Assets/` doit avoir son `.meta` (GUID unique) : le générer si Unity n'est pas disponible.
- Build serveur headless : à compléter à l'étape 5.
