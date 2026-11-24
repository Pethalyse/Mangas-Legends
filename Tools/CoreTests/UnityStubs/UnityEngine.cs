using System;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
        public static implicit operator bool(Object o) => o != null;
    }

    public class ScriptableObject : Object { }
    public sealed class GameObject : Object { }
    public sealed class Sprite : Object { }

    public abstract class PropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeReference : Attribute { }
    [AttributeUsage(AttributeTargets.Class)] public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string menuName { get; set; }
        public string fileName { get; set; }
        public int order { get; set; }
    }
    public sealed class HeaderAttribute : PropertyAttribute { public HeaderAttribute(string header) { } }
    public sealed class TextAreaAttribute : PropertyAttribute
    {
        public TextAreaAttribute() { }
        public TextAreaAttribute(int minLines, int maxLines) { }
    }
    public sealed class MinAttribute : PropertyAttribute { public MinAttribute(float min) { } }
    public sealed class RangeAttribute : PropertyAttribute { public RangeAttribute(float min, float max) { } }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogError(object message) { }
    }

    public struct Rect
    {
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
        public float x, y, width, height;
    }

    public class GUIContent
    {
        public GUIContent(string text) { }
    }
}
