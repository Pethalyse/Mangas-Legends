# MangaLegends — Conventions de code

> Ces règles sont **obligatoires**. Pour en changer une, écrire un ADR.

## 1. Langue et encodage

- Code (types, méthodes, variables, assets techniques) : **anglais**. ([ADR-0004](adr/0004-code-en-anglais.md))
- **Pas de commentaires**, sauf quand ils sont nécessaires pour expliquer un *pourquoi* non évident (contrainte réseau, contournement moteur, formule de design). Le *quoi* se lit dans les noms. Pas de code commenté, pas de `///` systématique.
- Docs et messages de commit : français accepté.
- Fichiers en **UTF-8**, fins de ligne LF, appliqués par `.editorconfig` et `.gitattributes`.

## 2. Nommage (C#)

| Élément | Style | Exemple |
|---|---|---|
| Namespace | `MangaLegends.<Assembly>[.<Domaine>]` | `MangaLegends.Gameplay.Abilities` |
| Classe, struct, enum, méthode, propriété, événement | PascalCase | `AbilityCaster`, `TryCast()`, `Died` |
| Interface | `I` + PascalCase | `IDamageable` |
| Champ privé | `_camelCase` | `_currentHealth` |
| Champ sérialisé | `[SerializeField] private` + `_camelCase` | `[SerializeField] private float _baseCooldown;` |
| Constante | PascalCase | `MaxInventorySlots` |
| Paramètre, variable locale | camelCase | `damageRequest` |
| Booléen | question | `IsDead`, `CanCast`, `HasTarget` |
| ScriptableObject | suffixe `Definition` / `Config` | `AbilityDefinition`, `GameRulesConfig` |
| RPC Fusion | préfixe `Rpc` + cible | `RpcAllPlayPing` |
| Propriété réseau | `[Networked]` PascalCase | `[Networked] public float CurrentHealth { get; private set; }` |
| Assets | `<Type>_<Nom>` | `CHAMP_Ainz.asset`, `ABIL_Ainz_MantleOfChaos.asset`, `PF_Minion_Melee.prefab` |

Un fichier = un type public, même nom que le type.

## 3. Structure d'une classe

Ordre : constantes → champs sérialisés → champs privés → propriétés `[Networked]` → événements → propriétés → méthodes Unity (`Awake`, `OnEnable`, `OnDisable`, `OnDestroy`) → callbacks Fusion (`Spawned`, `FixedUpdateNetwork`, `Render`, `Despawned`) → méthodes publiques → méthodes privées.

## 4. Règles Unity

- ❌ `GameObject.Find`, `FindObjectOfType`, `FindGameObjectsWithTag` dans le code de jeu (tolérés dans les outils d'éditeur).
- ❌ Singletons statiques (`public static X instance`). Utiliser l'injection : références sérialisées, ou un `CompositionRoot` par scène qui passe les dépendances.
- ❌ `GetComponent` dans `Update` ; mettre en cache dans `Awake`.
- ❌ `Resources.Load` pour le nouveau code.
- ❌ Chaînes magiques pour tags, layers, axes, noms d'animations : constantes centralisées ou `Animator.StringToHash` mis en cache.
- ❌ Nombres magiques de gameplay dans le code : ils vont dans les données.
- ❌ `new` pour masquer `Awake`/`Start`/`Update` d'une classe parente. Si un parent a besoin du cycle de vie : `protected virtual` + `override` + appel à `base`.
- ✅ Champs publics interdits : `[SerializeField] private` + propriété en lecture si nécessaire.
- ✅ `[RequireComponent]` pour les dépendances de composants.
- ✅ Coroutines et `Update()` pour la présentation seulement. La logique de jeu tourne dans `FixedUpdateNetwork()` et mesure le temps en ticks (`TickTimer`), jamais avec `Time.time`, `Time.deltaTime` ou `WaitForSeconds` (sinon la resimulation casse).
- ✅ Événements C# (`event Action<T>`) pour la communication ; se désabonner dans `OnDisable`/`OnDestroy`.

## 5. Règles réseau (Photon Fusion 2)

- Tout état de jeu qui doit être prédit ou répliqué est une propriété `[Networked]`. Pas d'état de jeu dans des champs C# ordinaires d'un `NetworkBehaviour`.
- Toute logique de jeu vit dans `FixedUpdateNetwork()` (ou est appelée depuis). Elle doit pouvoir être **rejouée** plusieurs fois pour un même tick sans effet de bord : pas d'`Instantiate`, de son, de VFX, de `Debug.Log` ni d'événement UI dans la simulation. Les effets visuels se déclenchent dans `Render()` ou via des détecteurs de changement.
- Les décisions définitives (dégâts, mort, or, objets, tirages aléatoires) sont prises sous `HasStateAuthority`. Le client peut les **estimer** pour l'affichage, jamais les appliquer.
- Le client communique par `INetworkInput` (structure d'entrée par tick). Les RPC sont réservés aux événements ponctuels non prédits ; jamais pour changer l'état de jeu depuis un client.
- Aléatoire : générateur déterministe dérivé du tick et d'une graine `[Networked]`, jamais `UnityEngine.Random` dans la simulation.
- Entités référencées par `NetworkId` / `PlayerRef` / `NetworkBehaviourId`, jamais par nom de GameObject.
- Impacts et ciblage côté autorité via `Runner.LagCompensation`.

## 6. Qualité

- Méthodes courtes (≈ 30 lignes max), classes centrées sur une responsabilité (≈ 300 lignes max ; au-delà, se poser la question).
- Pas de code commenté laissé dans le dépôt : git garde l'historique.
- `Debug.Log` interdit dans le code livré ; utiliser un `GameLog` avec catégories, désactivable.
- Les avertissements du compilateur sont traités comme des erreurs dans nos assemblies.
- Toute logique Core a des tests EditMode. Tout bug corrigé a un test qui le reproduit quand c'est possible.

## 7. Git

- Branches : `main` (stable, lance une partie), `feat/<sujet>`, `fix/<sujet>`, `refactor/<sujet>`, `docs/<sujet>`.
- Commits : [Conventional Commits](https://www.conventionalcommits.org/fr/) — `feat(abilities): ajouter DashEffect`.
- Une PR = un objectif, projet qui compile et se lance, docs mis à jour.
- Ne jamais commiter : builds (`Run/`, `Builds/`), `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`.
- Binaires lourds (`.fbx`, `.png` > 1 Mo, `.wav`) via Git LFS.

## 8. Exemple de référence

```csharp
using System;
using Fusion;
using MangaLegends.Core;

namespace MangaLegends.Simulation.Combat
{
    public sealed class Health : NetworkBehaviour
    {
        private Unit _unit;
        private ChangeDetector _changes;

        [Networked] public float Current { get; private set; }

        public event Action<float, float> Changed;
        public event Action<DamageResult> DamageApplied;

        public float Max => _unit.Stats.Get(StatType.MaxHealth);
        public bool IsDead => Current <= 0f;

        public override void Spawned()
        {
            _unit = GetComponent<Unit>();
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
            if (HasStateAuthority) Current = Max;
        }

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            if (!HasStateAuthority || IsDead) return DamageResult.None;
            DamageResult result = DamageCalculator.Resolve(request, _unit.Stats, _unit.StatusEffects);
            Current = MathF.Max(0f, Current - result.FinalAmount);
            return result;
        }

        public override void Render()
        {
            foreach (string _ in _changes.DetectChanges(this))
                Changed?.Invoke(Current, Max);
        }
    }
}
```
