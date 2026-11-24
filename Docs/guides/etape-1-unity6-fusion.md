# Étape 1 — Passer à Unity 6 LTS et installer Photon Fusion 2

Ces actions se font dans l'éditeur et sur le site de Photon : elles ne peuvent pas être automatisées depuis le repo. Compter environ 30 minutes.
Travailler sur la branche de l'étape en cours, puis commiter **tout** ce que Unity modifie (`ProjectSettings/`, `Packages/`, fichiers `.meta`).

## 1. Unity 6 LTS

1. Unity Hub → *Installs* → installer la **dernière Unity 6 LTS** (série `6000.x` marquée LTS), module *Windows Build Support (IL2CPP)* conseillé.
2. Unity Hub → *Projects* → sur MangaLegends, choisir cette version → *Change Editor version* → confirmer la mise à niveau.
3. À l'ouverture :
   - si une fenêtre propose de mettre à jour les API (*API Updater*), accepter ;
   - erreurs attendues : aucune erreur de compilation (l'ancien code est hors de `Assets/`) ; des « Missing Script » dans les scènes et prefabs sont normaux.
4. *Window → Package Manager* : mettre à jour les paquets Unity proposés (TextMeshPro devient « Unity UI » intégré : accepter l'import des *TMP Essentials* si demandé).

## 2. Paquets à ajouter (Package Manager → Unity Registry)

| Paquet | Pourquoi |
|---|---|
| **Input System** | entrées remappables ; accepter le redémarrage et choisir *Both* puis, après la refonte, *Input System Package (New)* |
| **AI Navigation** | NavMesh pour les chemins (mettre à jour s'il est déjà là) |
| **Multiplayer Play Mode** | lancer plusieurs joueurs dans l'éditeur (remplace ParrelSync) |
| **Test Framework** | tests EditMode / PlayMode (déjà présent, mettre à jour) |

## 3. Photon Fusion 2

1. Créer un compte sur [dashboard.photonengine.com](https://dashboard.photonengine.com), puis *Create a new app* → type **Fusion**, SDK **Fusion 2**. Copier l'**App ID**.
2. Télécharger le SDK **Fusion 2** (dernière version stable) depuis la page SDK de Photon et l'importer (`Assets → Import Package → Custom Package…`, tout importer). Il s'installe dans `Assets/Photon/`.
3. Coller l'App ID dans la fenêtre *Fusion Hub* qui s'ouvre (ou `Tools → Fusion → Fusion Hub`). Il est enregistré dans `PhotonAppSettings.asset`. Pour un jeu entre amis, on peut le commiter (il est de toute façon visible dans les builds).
4. *Fusion Hub → Network Project Config* : vérifier que **Assemblies to Weave** contiendra plus tard `MangaLegends.Simulation` et `MangaLegends.Networking` (à faire quand ces assemblies existeront, étape 5).

## 4. Vérifier puis commiter

- *Window → General → Test Runner → EditMode → Run All* : les tests du Core doivent passer.
- `git add -A && git commit -m "chore: Unity 6 LTS, Input System, Fusion 2"` sur la branche, puis prévenir Claude Code pour la suite.
