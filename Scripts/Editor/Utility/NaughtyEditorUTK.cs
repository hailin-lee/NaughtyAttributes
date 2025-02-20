using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NaughtyAttributes.Editor
{
    public class NaughtyEditorUTK
    {
        public static VisualElement NativeProperty_Layout(Object target, PropertyInfo property, List<Action> refreshActions)
        {
            var container = new VisualElement();
            object value = property.GetValue(target, null);

            if (value == null)
            {
                string warning = string.Format("{0} is null. {1} doesn't support reference types with null value", ObjectNames.NicifyVariableName(property.Name), typeof(ShowNativePropertyAttribute).Name);
                container.Add(new HelpBox(warning, HelpBoxMessageType.Warning));
            }
            else if (!Field_Layout(value, ObjectNames.NicifyVariableName(property.Name), () => property.GetValue(target), ref container, ref refreshActions))
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNativePropertyAttribute).Name, property.PropertyType.Name);
                container.Add(new HelpBox(warning, HelpBoxMessageType.Warning));
            }
            return container;
        }

        public static VisualElement NonSerializedField_Layout(Object target, FieldInfo field, List<Action> refreshActions)
        {
            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            object value = field.GetValue(target);

            if (value == null)
            {
                string warning = string.Format("{0} is null. {1} doesn't support reference types with null value", ObjectNames.NicifyVariableName(field.Name), typeof(ShowNonSerializedFieldAttribute).Name);
                container.Add(new HelpBox(warning, HelpBoxMessageType.Warning));
            }
            else if (!Field_Layout(value, ObjectNames.NicifyVariableName(field.Name), () => field.GetValue(target), ref container, ref refreshActions))
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNonSerializedFieldAttribute).Name, field.FieldType.Name);
                container.Add(new HelpBox(warning, HelpBoxMessageType.Warning));
            }
            return container;
        }
        
        public static VisualElement Button(Object target, MethodInfo methodInfo, List<Action> refreshActions)
        {
            var container = new VisualElement();
            bool visible = ButtonUtility.IsVisible(target, methodInfo);
            if (!visible)
            {
                return container;
            }

            if (methodInfo.GetParameters().All(p => p.IsOptional))
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Text;

                var button = new Button(() =>
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    IEnumerator methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;

                    if (!Application.isPlaying && target != null)
                    {
                        // Set target object and scene dirty to serialize changes to disk
                        EditorUtility.SetDirty(target);

                        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (stage != null)
                        {
                            // Prefab mode
                            EditorSceneManager.MarkSceneDirty(stage.scene);
                        }
                        else
                        {
                            // Normal scene
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                    else if (methodResult != null && target is MonoBehaviour behaviour)
                    {
                        behaviour.StartCoroutine(methodResult);
                    }
                })
                {
                    text = buttonText
                };
                container.Add(button);
                refreshActions.Add(() => button.SetEnabled(IsButtonEnable(target, methodInfo)));
            }
            else
            {
                string warning = typeof(ButtonAttribute).Name + " works only on methods with no parameters";
                container.Add(new HelpBox(warning, HelpBoxMessageType.Warning));
                Debug.LogWarning(warning, target);
            }

            return container;
        }

        static bool IsButtonEnable(Object target, MethodInfo methodInfo)
        {
            ButtonAttribute buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
            bool buttonEnabled = ButtonUtility.IsEnabled(target, methodInfo);
            EButtonEnableMode mode = buttonAttribute.SelectedEnableMode;
            
            buttonEnabled &=
                mode == EButtonEnableMode.Always ||
                mode == EButtonEnableMode.Editor && !Application.isPlaying ||
                mode == EButtonEnableMode.Playmode && Application.isPlaying;

            bool methodIsCoroutine = methodInfo.ReturnType == typeof(IEnumerator);
            if (methodIsCoroutine)
            {
                buttonEnabled &= (Application.isPlaying ? true : false);
            }
            return buttonEnabled;
        }
        
        public static bool Field_Layout(object value, string label, Func<object> valueGetter, ref VisualElement container, ref List<Action> refreshActions)
        {
            bool isDrawn = true;
            Type valueType = value.GetType();

            if (valueType == typeof(bool))
            {
                var field = new Toggle(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (bool) valueGetter.Invoke());
            }
            else if (valueType == typeof(short))
            {
                var field = new IntegerField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (short) valueGetter.Invoke());
            }
            else if (valueType == typeof(ushort))
            {
                var field = new IntegerField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (ushort) valueGetter.Invoke());
            }
            else if (valueType == typeof(int))
            {
                var field = new IntegerField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (int) valueGetter.Invoke());
            }
            else if (valueType == typeof(uint))
            {
                var field = new LongField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (uint) valueGetter.Invoke());
            }
            else if (valueType == typeof(long))
            {
                var field = new LongField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (long) valueGetter.Invoke());
            }
            else if (valueType == typeof(ulong))
            {
                var field = new TextField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = ((ulong) valueGetter.Invoke()).ToString());
            }
            else if (valueType == typeof(float))
            {
                var field = new FloatField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (float) valueGetter.Invoke());
            }
            else if (valueType == typeof(double))
            {
                var field = new DoubleField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (double) valueGetter.Invoke());
            }
            else if (valueType == typeof(string))
            {
                var field = new TextField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (string) valueGetter.Invoke());
            }
            else if (valueType == typeof(Vector2))
            {
                var field = new Vector2Field(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Vector2) valueGetter.Invoke());
            }
            else if (valueType == typeof(Vector3))
            {
                var field = new Vector3Field(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Vector3) valueGetter.Invoke());
            }
            else if (valueType == typeof(Vector4))
            {
                var field = new Vector4Field(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Vector4) valueGetter.Invoke());
            }
            else if (valueType == typeof(Vector2Int))
            {
                var field = new Vector2IntField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Vector2Int) valueGetter.Invoke());
            }
            else if (valueType == typeof(Vector3Int))
            {
                var field = new Vector3IntField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Vector3Int) valueGetter.Invoke());
            }
            else if (valueType == typeof(Color))
            {
                var field = new ColorField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Color) valueGetter.Invoke());
            }
            else if (valueType == typeof(Bounds))
            {
                var field = new BoundsField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Bounds) valueGetter.Invoke());
            }
            else if (valueType == typeof(Rect))
            {
                var field = new RectField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Rect) valueGetter.Invoke());
            }
            else if (valueType == typeof(RectInt))
            {
                var field = new RectIntField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (RectInt) valueGetter.Invoke());
            }
            else if (typeof(Object).IsAssignableFrom(valueType))
            {
                var field = new ObjectField(label)
                {
                    objectType = valueType,
                    allowSceneObjects = true
                };
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Object) valueGetter.Invoke());
            }
            else if (valueType.BaseType == typeof(Enum))
            {
                var field = new EnumField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = (Enum) valueGetter.Invoke());
            }
            else if (valueType.BaseType == typeof(TypeInfo))
            {
                var field = new TextField(label);
                field.SetEnabled(false);
                container.Add(field);
                refreshActions.Add(() => field.value = valueGetter.Invoke().ToString());
            }
            else
            {
                isDrawn = false;
            }

            return isDrawn;
        }

    }
}