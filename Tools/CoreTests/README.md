# Tests du Core hors Unity

Compile `MangaLegends.Core` (C# 9, .NET Standard 2.1, comme Unity) et lance les tests EditMode avec NUnit, sans ouvrir Unity.

```
dotnet test Tools/CoreTests/Tests
```

Seuls le Core et ses tests sont concernés : tout code qui dépend de `UnityEngine` ou de Fusion se teste dans le Test Runner d'Unity.
