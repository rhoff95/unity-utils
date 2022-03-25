using System.Collections.Generic;
using System.Reflection;
using Plugins.unity_utils.Scripts.DataStructures;
using UnityEditor;
using UnityEngine;

namespace Plugins.unity_utils.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
#if NET_4_6 || NET_STANDARD_2_0
    [CustomPropertyDrawer(typeof(SerializableHashSetBase), true)]
#endif
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        private const string KeysFieldName = "m_keys";
        private const string ValuesFieldName = "m_values";
        private const float IndentWidth = 15f;

        private static readonly GUIContent SIconPlus = IconContent("Toolbar Plus", "Add entry");
        private static readonly GUIContent SIconMinus =
            IconContent("Toolbar Minus", "Remove entry");
        private static readonly GUIContent SWarningIconConflict =
            IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
        private static readonly GUIContent SWarningIconOther =
            IconContent("console.infoicon.sml", "Conflicting key");
        private static readonly GUIContent SWarningIconNull =
            IconContent("console.warnicon.sml", "Null key, this entry will be lost");
        private static readonly GUIStyle SButtonStyle = GUIStyle.none;
        private static readonly GUIContent STempContent = new GUIContent();


        private class ConflictState
        {
            public object ConflictKey;
            public object ConflictValue;
            public int ConflictIndex = -1;
            public int ConflictOtherIndex = -1;
            public bool ConflictKeyPropertyExpanded;
            public bool ConflictValuePropertyExpanded;
            public float ConflictLineHeight;
        }

        private struct PropertyIdentity
        {
            public PropertyIdentity(SerializedProperty property)
            {
                _instance = property.serializedObject.targetObject;
                _propertyPath = property.propertyPath;
            }

            private Object _instance;
            private string _propertyPath;
        }

        private static readonly Dictionary<PropertyIdentity, ConflictState> SConflictStateDict =
            new Dictionary<PropertyIdentity, ConflictState>();

        private enum Action
        {
            None,
            Add,
            Remove
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var buttonAction = Action.None;
            var buttonActionIndex = 0;

            var keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
            var valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);

            var conflictState = GetConflictState(property);

            if (conflictState.ConflictIndex != -1)
            {
                keyArrayProperty.InsertArrayElementAtIndex(conflictState.ConflictIndex);
                var keyProperty =
                    keyArrayProperty.GetArrayElementAtIndex(conflictState.ConflictIndex);
                SetPropertyValue(keyProperty, conflictState.ConflictKey);
                keyProperty.isExpanded = conflictState.ConflictKeyPropertyExpanded;

                if (valueArrayProperty != null)
                {
                    valueArrayProperty.InsertArrayElementAtIndex(conflictState.ConflictIndex);
                    var valueProperty =
                        valueArrayProperty.GetArrayElementAtIndex(conflictState.ConflictIndex);
                    SetPropertyValue(valueProperty, conflictState.ConflictValue);
                    valueProperty.isExpanded = conflictState.ConflictValuePropertyExpanded;
                }
            }

            var buttonWidth = SButtonStyle.CalcSize(SIconPlus).x;

            var labelPosition = position;
            labelPosition.height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
                labelPosition.xMax -= SButtonStyle.CalcSize(SIconPlus).x;

            EditorGUI.PropertyField(labelPosition, property, label, false);
            // property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
            if (property.isExpanded)
            {
                var buttonPosition = position;
                buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
                buttonPosition.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginDisabledGroup(conflictState.ConflictIndex != -1);
                if (GUI.Button(buttonPosition, SIconPlus, SButtonStyle))
                {
                    buttonAction = Action.Add;
                    buttonActionIndex = keyArrayProperty.arraySize;
                }

                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel++;
                var linePosition = position;
                linePosition.y += EditorGUIUtility.singleLineHeight;
                linePosition.xMax -= buttonWidth;

                foreach (var entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
                {
                    var keyProperty = entry.KeyProperty;
                    var valueProperty = entry.ValueProperty;
                    var i = entry.Index;

                    var lineHeight = DrawKeyValueLine(keyProperty, valueProperty, linePosition, i);

                    buttonPosition = linePosition;
                    buttonPosition.x = linePosition.xMax;
                    buttonPosition.height = EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(buttonPosition, SIconMinus, SButtonStyle))
                    {
                        buttonAction = Action.Remove;
                        buttonActionIndex = i;
                    }

                    if (i == conflictState.ConflictIndex && conflictState.ConflictOtherIndex == -1)
                    {
                        var iconPosition = linePosition;
                        iconPosition.size = SButtonStyle.CalcSize(SWarningIconNull);
                        GUI.Label(iconPosition, SWarningIconNull);
                    }
                    else if (i == conflictState.ConflictIndex)
                    {
                        var iconPosition = linePosition;
                        iconPosition.size = SButtonStyle.CalcSize(SWarningIconConflict);
                        GUI.Label(iconPosition, SWarningIconConflict);
                    }
                    else if (i == conflictState.ConflictOtherIndex)
                    {
                        var iconPosition = linePosition;
                        iconPosition.size = SButtonStyle.CalcSize(SWarningIconOther);
                        GUI.Label(iconPosition, SWarningIconOther);
                    }


                    linePosition.y += lineHeight;
                }

                EditorGUI.indentLevel--;
            }

            if (buttonAction == Action.Add)
            {
                Debug.Log($"Add({buttonActionIndex})");
                
                keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
                if (valueArrayProperty != null)
                {
                    valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
                }
                else
                {
                    Debug.Log("valueArrayProperty = null");
                }
            }
            else if (buttonAction == Action.Remove)
            {
                DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
                if (valueArrayProperty != null)
                    DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
            }

            conflictState.ConflictKey = null;
            conflictState.ConflictValue = null;
            conflictState.ConflictIndex = -1;
            conflictState.ConflictOtherIndex = -1;
            conflictState.ConflictLineHeight = 0f;
            conflictState.ConflictKeyPropertyExpanded = false;
            conflictState.ConflictValuePropertyExpanded = false;

            foreach (var entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
            {
                var keyProperty1 = entry1.KeyProperty;
                var i = entry1.Index;
                var keyProperty1Value = GetPropertyValue(keyProperty1);

                if (keyProperty1Value == null)
                {
                    var valueProperty1 = entry1.ValueProperty;
                    SaveProperty(keyProperty1, valueProperty1, i, -1, conflictState);
                    DeleteArrayElementAtIndex(keyArrayProperty, i);
                    if (valueArrayProperty != null)
                        DeleteArrayElementAtIndex(valueArrayProperty, i);

                    break;
                }


                foreach (var entry2 in
                         EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
                {
                    var keyProperty2 = entry2.KeyProperty;
                    var j = entry2.Index;
                    var keyProperty2Value = GetPropertyValue(keyProperty2);

                    if (ComparePropertyValues(keyProperty1Value, keyProperty2Value))
                    {
                        var valueProperty2 = entry2.ValueProperty;
                        SaveProperty(keyProperty2, valueProperty2, j, i, conflictState);
                        DeleteArrayElementAtIndex(keyArrayProperty, j);
                        if (valueArrayProperty != null)
                            DeleteArrayElementAtIndex(valueArrayProperty, j);

                        goto breakLoops;
                    }
                }
            }

            breakLoops:

            EditorGUI.EndProperty();
        }

        private static float DrawKeyValueLine(SerializedProperty keyProperty,
            SerializedProperty valueProperty, Rect linePosition, int index)
        {
            var keyCanBeExpanded = CanPropertyBeExpanded(keyProperty);

            if (valueProperty != null)
            {
                var valueCanBeExpanded = CanPropertyBeExpanded(valueProperty);

                if (!keyCanBeExpanded && valueCanBeExpanded)
                {
                    return DrawKeyValueLineExpand(keyProperty, valueProperty, linePosition);
                }

                var keyLabel = keyCanBeExpanded ? "Key " + index : "";
                var valueLabel = valueCanBeExpanded ? "Value " + index : "";
                return DrawKeyValueLineSimple(keyProperty, valueProperty, keyLabel, valueLabel,
                    linePosition);
            }

            if (!keyCanBeExpanded)
            {
                return DrawKeyLine(keyProperty, linePosition, null);
            }

            {
                var keyLabel = string.Format("{0} {1}",
                    ObjectNames.NicifyVariableName(keyProperty.type), index);
                return DrawKeyLine(keyProperty, linePosition, keyLabel);
            }
        }

        private static float DrawKeyValueLineSimple(SerializedProperty keyProperty,
            SerializedProperty valueProperty, string keyLabel, string valueLabel, Rect linePosition)
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            var labelWidthRelative = labelWidth / linePosition.width;

            var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = labelWidth - IndentWidth;
            EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
            EditorGUI.PropertyField(keyPosition, keyProperty, TempContent(keyLabel), true);

            var valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
            var valuePosition = linePosition;
            valuePosition.height = valuePropertyHeight;
            valuePosition.xMin += labelWidth;
            EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
            EditorGUI.indentLevel--;
            EditorGUI.PropertyField(valuePosition, valueProperty, TempContent(valueLabel), true);
            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = labelWidth;

            return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        }

        private static float DrawKeyValueLineExpand(SerializedProperty keyProperty,
            SerializedProperty valueProperty, Rect linePosition)
        {
            var labelWidth = EditorGUIUtility.labelWidth;

            var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = labelWidth - IndentWidth;
            EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);

            var valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
            var valuePosition = linePosition;
            valuePosition.height = valuePropertyHeight;
            EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);

            EditorGUIUtility.labelWidth = labelWidth;

            return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        }

        private static float DrawKeyLine(SerializedProperty keyProperty, Rect linePosition,
            string keyLabel)
        {
            var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = linePosition.width;

            var keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
            EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);

            return keyPropertyHeight;
        }

        private static bool CanPropertyBeExpanded(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Quaternion:
                    return true;
                default:
                    return false;
            }
        }

        private static void SaveProperty(SerializedProperty keyProperty,
            SerializedProperty valueProperty, int index, int otherIndex,
            ConflictState conflictState)
        {
            conflictState.ConflictKey = GetPropertyValue(keyProperty);
            conflictState.ConflictValue =
                valueProperty != null ? GetPropertyValue(valueProperty) : null;
            var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var valuePropertyHeight =
                valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
            var lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
            conflictState.ConflictLineHeight = lineHeight;
            conflictState.ConflictIndex = index;
            conflictState.ConflictOtherIndex = otherIndex;
            conflictState.ConflictKeyPropertyExpanded = keyProperty.isExpanded;
            conflictState.ConflictValuePropertyExpanded =
                valueProperty != null ? valueProperty.isExpanded : false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                var keysProperty = property.FindPropertyRelative(KeysFieldName);
                var valuesProperty = property.FindPropertyRelative(ValuesFieldName);

                foreach (var entry in EnumerateEntries(keysProperty, valuesProperty))
                {
                    var keyProperty = entry.KeyProperty;
                    var valueProperty = entry.ValueProperty;
                    var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
                    var valuePropertyHeight = valueProperty != null
                        ? EditorGUI.GetPropertyHeight(valueProperty)
                        : 0f;
                    var lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
                    propertyHeight += lineHeight;
                }

                var conflictState = GetConflictState(property);

                if (conflictState.ConflictIndex != -1)
                {
                    propertyHeight += conflictState.ConflictLineHeight;
                }
            }

            return propertyHeight;
        }

        private static ConflictState GetConflictState(SerializedProperty property)
        {
            var propId = new PropertyIdentity(property);
            if (!SConflictStateDict.TryGetValue(propId, out var conflictState))
            {
                conflictState = new ConflictState();
                SConflictStateDict.Add(propId, conflictState);
            }

            return conflictState;
        }

        private static readonly Dictionary<SerializedPropertyType, PropertyInfo>
            SSerializedPropertyValueAccessorsDict;

        static SerializableDictionaryPropertyDrawer()
        {
            var serializedPropertyValueAccessorsNameDict =
                new Dictionary<SerializedPropertyType, string>
                {
                    { SerializedPropertyType.Integer, "intValue" },
                    { SerializedPropertyType.Boolean, "boolValue" },
                    { SerializedPropertyType.Float, "floatValue" },
                    { SerializedPropertyType.String, "stringValue" },
                    { SerializedPropertyType.Color, "colorValue" },
                    { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
                    { SerializedPropertyType.LayerMask, "intValue" },
                    { SerializedPropertyType.Enum, "intValue" },
                    { SerializedPropertyType.Vector2, "vector2Value" },
                    { SerializedPropertyType.Vector3, "vector3Value" },
                    { SerializedPropertyType.Vector4, "vector4Value" },
                    { SerializedPropertyType.Rect, "rectValue" },
                    { SerializedPropertyType.ArraySize, "intValue" },
                    { SerializedPropertyType.Character, "intValue" },
                    { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
                    { SerializedPropertyType.Bounds, "boundsValue" },
                    { SerializedPropertyType.Quaternion, "quaternionValue" },
                };
            var serializedPropertyType = typeof(SerializedProperty);

            SSerializedPropertyValueAccessorsDict =
                new Dictionary<SerializedPropertyType, PropertyInfo>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (var kvp in serializedPropertyValueAccessorsNameDict)
            {
                var propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
                SSerializedPropertyValueAccessorsDict.Add(kvp.Key, propertyInfo);
            }
        }

        private static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        private static GUIContent TempContent(string text)
        {
            STempContent.text = text;
            return STempContent;
        }

        private static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
        {
            var property = arrayProperty.GetArrayElementAtIndex(index);
            // if(arrayProperty.arrayElementType.StartsWith("PPtr<$"))
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = null;
            }

            arrayProperty.DeleteArrayElementAtIndex(index);
        }

        public static object GetPropertyValue(SerializedProperty p)
        {
            PropertyInfo propertyInfo;
            if (SSerializedPropertyValueAccessorsDict.TryGetValue(p.propertyType,
                    out propertyInfo))
            {
                return propertyInfo.GetValue(p, null);
            }

            if (p.isArray)
                return GetPropertyValueArray(p);
            return GetPropertyValueGeneric(p);
        }

        private static void SetPropertyValue(SerializedProperty p, object v)
        {
            if (SSerializedPropertyValueAccessorsDict.TryGetValue(p.propertyType,
                    out var propertyInfo))
            {
                propertyInfo.SetValue(p, v, null);
            }
            else
            {
                if (p.isArray)
                    SetPropertyValueArray(p, v);
                else
                    SetPropertyValueGeneric(p, v);
            }
        }

        private static object GetPropertyValueArray(SerializedProperty property)
        {
            var array = new object[property.arraySize];
            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                array[i] = GetPropertyValue(item);
            }

            return array;
        }

        private static object GetPropertyValueGeneric(SerializedProperty property)
        {
            var dict = new Dictionary<string, object>();
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    var name = iterator.name;
                    var value = GetPropertyValue(iterator);
                    dict.Add(name, value);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }

            return dict;
        }

        private static void SetPropertyValueArray(SerializedProperty property, object v)
        {
            var array = (object[])v;
            property.arraySize = array.Length;
            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                SetPropertyValue(item, array[i]);
            }
        }

        private static void SetPropertyValueGeneric(SerializedProperty property, object v)
        {
            var dict = (Dictionary<string, object>)v;
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    var name = iterator.name;
                    SetPropertyValue(iterator, dict[name]);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }
        }

        private static bool ComparePropertyValues(object value1, object value2)
        {
            if (value1 is Dictionary<string, object> && value2 is Dictionary<string, object>)
            {
                var dict1 = (Dictionary<string, object>)value1;
                var dict2 = (Dictionary<string, object>)value2;
                return CompareDictionaries(dict1, dict2);
            }

            return Equals(value1, value2);
        }

        private static bool CompareDictionaries(Dictionary<string, object> dict1,
            Dictionary<string, object> dict2)
        {
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var kvp1 in dict1)
            {
                var key1 = kvp1.Key;
                var value1 = kvp1.Value;

                object value2;
                if (!dict2.TryGetValue(key1, out value2))
                    return false;

                if (!ComparePropertyValues(value1, value2))
                    return false;
            }

            return true;
        }

        private struct EnumerationEntry
        {
            public readonly SerializedProperty KeyProperty;
            public readonly SerializedProperty ValueProperty;
            public readonly int Index;

            public EnumerationEntry(SerializedProperty keyProperty,
                SerializedProperty valueProperty, int index)
            {
                KeyProperty = keyProperty;
                ValueProperty = valueProperty;
                Index = index;
            }
        }

        private static IEnumerable<EnumerationEntry> EnumerateEntries(
            SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty,
            int startIndex = 0)
        {
            if (keyArrayProperty.arraySize > startIndex)
            {
                var index = startIndex;
                var keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
                var valueProperty = valueArrayProperty != null
                    ? valueArrayProperty.GetArrayElementAtIndex(startIndex)
                    : null;
                var endProperty = keyArrayProperty.GetEndProperty();

                do
                {
                    yield return new EnumerationEntry(keyProperty, valueProperty, index);
                    index++;
                } while (keyProperty.Next(false)
                         && (valueProperty != null ? valueProperty.Next(false) : true)
                         && !SerializedProperty.EqualContents(keyProperty, endProperty));
            }
        }
    }

    [CustomPropertyDrawer(typeof(SerializableDictionaryBase.Storage), true)]
    public class SerializableDictionaryStoragePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
