using System;
using UnityEngine;

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class CustomPropertyDrawer : Attribute { public CustomPropertyDrawer(Type type) { } }
    [AttributeUsage(AttributeTargets.Method)] public sealed class MenuItem : Attribute { public MenuItem(string path) { } }

    public abstract class PropertyDrawer
    {
        public virtual float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0f;
        public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
    }

    public enum SerializedPropertyType { Generic, ManagedReference }
    public enum FocusType { Passive, Keyboard }

    public class SerializedObject
    {
        public void Update() { }
        public bool ApplyModifiedProperties() => true;
    }

    public class SerializedProperty
    {
        public SerializedPropertyType propertyType { get; }
        public object managedReferenceValue { get; set; }
        public string managedReferenceFieldTypename { get; }
        public SerializedObject serializedObject { get; }
    }

    public static class EditorGUI
    {
        public static float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren) => 0f;
        public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren) => true;
        public static bool DropdownButton(Rect position, GUIContent content, FocusType focusType) => false;
    }

    public static class EditorGUIUtility
    {
        public static float labelWidth => 0f;
        public static float singleLineHeight => 0f;
    }

    public static class ObjectNames { public static string NicifyVariableName(string name) => name; }

    public sealed class GenericMenu
    {
        public delegate void MenuFunction();
        public void AddItem(GUIContent content, bool on, MenuFunction func) { }
        public void ShowAsContext() { }
    }

    public static class TypeCache
    {
        public static System.Collections.Generic.IList<Type> GetTypesDerivedFrom(Type parentType) => Array.Empty<Type>();
    }

    public static class AssetDatabase
    {
        public static string[] FindAssets(string filter) => Array.Empty<string>();
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => null;
        public static string GUIDToAssetPath(string guid) => guid;
        public static string GetAssetPath(UnityEngine.Object asset) => string.Empty;
        public static void SaveAssets() { }
    }

    public static class Undo { public static void RecordObject(UnityEngine.Object target, string name) { } }
    public static class EditorUtility { public static void SetDirty(UnityEngine.Object target) { } }
    public static class EditorApplication { public static void Exit(int code) { } }
}
