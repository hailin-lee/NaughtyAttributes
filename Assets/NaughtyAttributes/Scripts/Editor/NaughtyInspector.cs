using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NaughtyAttributes.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class NaughtyInspector : UnityEditor.Editor
    {
        private List<SerializedProperty> _serializedProperties = new List<SerializedProperty>();
        private IEnumerable<FieldInfo> _nonSerializedFields;
        private IEnumerable<PropertyInfo> _nativeProperties;
        private IEnumerable<MethodInfo> _methods;
        private Dictionary<string, SavedBool> _foldouts = new Dictionary<string, SavedBool>();
        List<Action> _refreshActions = new();

        protected virtual void OnEnable()
        {
            _nonSerializedFields = ReflectionUtility.GetAllFields(
                target, f => f.GetCustomAttributes(typeof(ShowNonSerializedFieldAttribute), true).Length > 0);

            _nativeProperties = ReflectionUtility.GetAllProperties(
                target, p => p.GetCustomAttributes(typeof(ShowNativePropertyAttribute), true).Length > 0);

            _methods = ReflectionUtility.GetAllMethods(
                target, m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);
        }

        protected virtual void OnDisable()
        {
            ReorderableListPropertyDrawer.Instance.ClearCache();
        }

        public override VisualElement CreateInspectorGUI()
        {
            GetSerializedProperties(ref _serializedProperties);
        
            bool anyNaughtyAttribute = _serializedProperties.Any(p => PropertyUtility.GetAttribute<INaughtyAttribute>(p) != null);
            
            var container = !anyNaughtyAttribute ? DrawDefaultInspector_UTK() : DrawSerializedProperties_UTK();
            container.Add(DrawNonSerializedFields_UTK());
            container.Add(DrawNativeProperties_UTK());
            container.Add(DrawButtons_UTK());
            // container.schedule.Execute(() =>
            // {
            //     serializedObject.ApplyModifiedProperties();
            // }).Every(200);
            RefreshNonSerializedFields();
            container.schedule.Execute(RefreshNonSerializedFields).Every(200);
            return container;
        }

        public VisualElement DrawDefaultInspector_UTK()
        {
            
            var container = new VisualElement();
            InspectorElement.FillDefaultInspector(container, serializedObject, this);
            return container;
        }

        void RefreshNonSerializedFields()
        {
            foreach (var x in _refreshActions)
            {
                x.Invoke();
            }
        }

        public override void OnInspectorGUI()
        {
            GetSerializedProperties(ref _serializedProperties);
        
            bool anyNaughtyAttribute = _serializedProperties.Any(p => PropertyUtility.GetAttribute<INaughtyAttribute>(p) != null);
            if (!anyNaughtyAttribute)
            {
                DrawDefaultInspector();
            }
            else
            {
                DrawSerializedProperties();
            }
        
            DrawNonSerializedFields();
            DrawNativeProperties();
            DrawButtons();
        }

        protected void GetSerializedProperties(ref List<SerializedProperty> outSerializedProperties)
        {
            outSerializedProperties.Clear();
            using (var iterator = serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        outSerializedProperties.Add(serializedObject.FindProperty(iterator.name));
                    }
                    while (iterator.NextVisible(false));
                }
            }
        }

        VisualElement DrawSerializedProperties_UTK()
        {
             serializedObject.Update();
             var container = new VisualElement();
 
             // Draw non-grouped serialized properties
             foreach (var property in GetNonGroupedProperties(_serializedProperties))
             {
                 if (property.name.Equals("m_Script", System.StringComparison.Ordinal))
                 {
                     var pf = new PropertyField(property);
                     pf.SetEnabled(false);
                     container.Add(pf);
                 }
                 else
                 {
                     container.Add(new PropertyField(property));
                 }
             }
 
             // Draw grouped serialized properties
             foreach (var group in GetGroupedProperties(_serializedProperties))
             {
                 IEnumerable<SerializedProperty> visibleProperties = group.Where(p => PropertyUtility.IsVisible(p));
                 if (!visibleProperties.Any())
                 {
                     continue;
                 }

                 var box = new Box();
                 container.Add(box);
                 box.Add(new Label(group.Key));
                 foreach (var property in visibleProperties)
                 {
                     box.Add(new PropertyField(property));
                 }
             }
 
             // Draw foldout serialized properties
             foreach (var group in GetFoldoutProperties(_serializedProperties))
             {
                 IEnumerable<SerializedProperty> visibleProperties = group.Where(p => PropertyUtility.IsVisible(p));
                 if (!visibleProperties.Any())
                 {
                     continue;
                 }
 
                 if (!_foldouts.ContainsKey(group.Key))
                 {
                     _foldouts[group.Key] = new SavedBool($"{target.GetInstanceID()}.{group.Key}", false);
                 }

                 var foldout = new Foldout();
                 container.Add(foldout);
                 foldout.value = _foldouts[group.Key].Value;
                 foldout.RegisterValueChangedCallback(evt => _foldouts[group.Key].Value = evt.newValue);
                 foreach (var property in visibleProperties)
                 {
                     foldout.Add(new PropertyField(property));
                 }
             }
             
             return container;
        }

        protected void DrawSerializedProperties()
        {
            serializedObject.Update();

            // Draw non-grouped serialized properties
            foreach (var property in GetNonGroupedProperties(_serializedProperties))
            {
                if (property.name.Equals("m_Script", System.StringComparison.Ordinal))
                {
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        EditorGUILayout.PropertyField(property);
                    }
                }
                else
                {
                    NaughtyEditorGUI.PropertyField_Layout(property, includeChildren: true);
                }
            }

            // Draw grouped serialized properties
            foreach (var group in GetGroupedProperties(_serializedProperties))
            {
                IEnumerable<SerializedProperty> visibleProperties = group.Where(p => PropertyUtility.IsVisible(p));
                if (!visibleProperties.Any())
                {
                    continue;
                }

                NaughtyEditorGUI.BeginBoxGroup_Layout(group.Key);
                foreach (var property in visibleProperties)
                {
                    NaughtyEditorGUI.PropertyField_Layout(property, includeChildren: true);
                }

                NaughtyEditorGUI.EndBoxGroup_Layout();
            }

            // Draw foldout serialized properties
            foreach (var group in GetFoldoutProperties(_serializedProperties))
            {
                IEnumerable<SerializedProperty> visibleProperties = group.Where(p => PropertyUtility.IsVisible(p));
                if (!visibleProperties.Any())
                {
                    continue;
                }

                if (!_foldouts.ContainsKey(group.Key))
                {
                    _foldouts[group.Key] = new SavedBool($"{target.GetInstanceID()}.{group.Key}", false);
                }

                _foldouts[group.Key].Value = EditorGUILayout.Foldout(_foldouts[group.Key].Value, group.Key, true);
                if (_foldouts[group.Key].Value)
                {
                    foreach (var property in visibleProperties)
                    {
                        NaughtyEditorGUI.PropertyField_Layout(property, true);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        VisualElement DrawNonSerializedFields_UTK(bool drawHeader = false)
        {
            var container = new VisualElement();
             if (_nonSerializedFields.Any())
             {
                 if (drawHeader)
                 {
                     container.Add(new Label());
                     container.Add(new Label("Non-Serialized Fields"));
                 }
 
                 foreach (var field in _nonSerializedFields)
                 {
                     container.Add(NaughtyEditorUTK.NonSerializedField_Layout(serializedObject.targetObject, field, _refreshActions));
                 }
             }
             return container;
        }

        protected void DrawNonSerializedFields(bool drawHeader = false)
        {
            if (_nonSerializedFields.Any())
            {
                if (drawHeader)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Non-Serialized Fields", GetHeaderGUIStyle());
                    NaughtyEditorGUI.HorizontalLine(
                        EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
                }

                foreach (var field in _nonSerializedFields)
                {
                    NaughtyEditorGUI.NonSerializedField_Layout(serializedObject.targetObject, field);
                }
            }
        }

        VisualElement DrawNativeProperties_UTK(bool drawHeader = false)
        {
            var container = new VisualElement();
             if (_nativeProperties.Any())
             {
                 if (drawHeader)
                 {
                     container.Add(new Label());
                     container.Add(new Label("Native Properties"));
                 }
 
                 foreach (var property in _nativeProperties)
                 {
                     container.Add(NaughtyEditorUTK.NativeProperty_Layout(serializedObject.targetObject, property, _refreshActions));
                 }
             }
             return container;
        }

        protected void DrawNativeProperties(bool drawHeader = false)
        {
            if (_nativeProperties.Any())
            {
                if (drawHeader)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Native Properties", GetHeaderGUIStyle());
                    NaughtyEditorGUI.HorizontalLine(
                        EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
                }

                foreach (var property in _nativeProperties)
                {
                    NaughtyEditorGUI.NativeProperty_Layout(serializedObject.targetObject, property);
                }
            }
        }

        VisualElement DrawButtons_UTK(bool drawHeader = false)
        {
            var container = new VisualElement();
            if (_methods.Any())
            {
                if (drawHeader)
                {
                    container.Add(new Label());
                    container.Add(new Label("Buttons"));
                }

                foreach (var method in _methods)
                {
                    container.Add(NaughtyEditorUTK.Button(serializedObject.targetObject, method, _refreshActions));
                }
            }
            return container;
        }

        protected void DrawButtons(bool drawHeader = false)
        {
            if (_methods.Any())
            {
                if (drawHeader)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Buttons", GetHeaderGUIStyle());
                    NaughtyEditorGUI.HorizontalLine(
                        EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
                }

                foreach (var method in _methods)
                {
                    NaughtyEditorGUI.Button(serializedObject.targetObject, method);
                }
            }
        }

        private static IEnumerable<SerializedProperty> GetNonGroupedProperties(IEnumerable<SerializedProperty> properties)
        {
            return properties.Where(p => PropertyUtility.GetAttribute<IGroupAttribute>(p) == null);
        }

        private static IEnumerable<IGrouping<string, SerializedProperty>> GetGroupedProperties(IEnumerable<SerializedProperty> properties)
        {
            return properties
                .Where(p => PropertyUtility.GetAttribute<BoxGroupAttribute>(p) != null)
                .GroupBy(p => PropertyUtility.GetAttribute<BoxGroupAttribute>(p).Name);
        }

        private static IEnumerable<IGrouping<string, SerializedProperty>> GetFoldoutProperties(IEnumerable<SerializedProperty> properties)
        {
            return properties
                .Where(p => PropertyUtility.GetAttribute<FoldoutAttribute>(p) != null)
                .GroupBy(p => PropertyUtility.GetAttribute<FoldoutAttribute>(p).Name);
        }

        private static GUIStyle GetHeaderGUIStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;

            return style;
        }
    }
}
