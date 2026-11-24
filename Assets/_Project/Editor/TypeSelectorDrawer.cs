using System;
using System.Collections.Generic;
using System.Linq;
using MangaLegends.Data.Serialization;
using UnityEditor;
using UnityEngine;

namespace MangaLegends.Editor
{
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    public sealed class TypeSelectorDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, Type[]> CandidatesByField = new Dictionary<string, Type[]>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            Rect buttonRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - 2f,
                EditorGUIUtility.singleLineHeight);

            string current = property.managedReferenceValue?.GetType().Name ?? "(none)";
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(ObjectNames.NicifyVariableName(current)), FocusType.Keyboard))
                ShowMenu(property);

            EditorGUI.PropertyField(position, property, label, true);
        }

        private static void ShowMenu(SerializedProperty property)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("(none)"), property.managedReferenceValue == null, () => Assign(property, null));
            foreach (Type type in Candidates(property.managedReferenceFieldTypename))
            {
                Type captured = type;
                bool selected = property.managedReferenceValue?.GetType() == type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), selected, () => Assign(property, captured));
            }
            menu.ShowAsContext();
        }

        private static void Assign(SerializedProperty property, Type type)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
        }

        // Unity encodes the field type as "AssemblyName Namespace.TypeName".
        private static Type[] Candidates(string fieldTypename)
        {
            if (CandidatesByField.TryGetValue(fieldTypename, out Type[] cached)) return cached;

            string[] parts = fieldTypename.Split(' ');
            Type baseType = parts.Length == 2 ? Type.GetType($"{parts[1]}, {parts[0]}") : null;
            Type[] candidates = baseType == null
                ? Array.Empty<Type>()
                : TypeCache.GetTypesDerivedFrom(baseType)
                    .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.Name)
                    .ToArray();

            CandidatesByField[fieldTypename] = candidates;
            return candidates;
        }
    }
}
