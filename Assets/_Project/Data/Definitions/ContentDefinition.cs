using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    public abstract class ContentDefinition : ScriptableObject, IContentEntry
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 6)] private string _description;
        [SerializeField] private Sprite _icon;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;

        public virtual void Validate(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(_displayName)) report.Warning(_id, "Display name is empty.");
        }
    }
}
