using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NaughtyAttributes.Editor
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return GetHelpBoxHeight();
        }

        public override void OnGUI(Rect rect)
        {
            InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)attribute;

            float indentLength = NaughtyEditorGUI.GetIndentLength(rect);
            Rect infoBoxRect = new Rect(
                rect.x + indentLength,
                rect.y,
                rect.width - indentLength,
                GetHelpBoxHeight());

            DrawInfoBox(infoBoxRect, infoBoxAttribute.Text, infoBoxAttribute.Type);
        }

        public override VisualElement CreatePropertyGUI()
        {
            InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)attribute;
            return new HelpBox(infoBoxAttribute.Text, infoBoxAttribute.Type);
        }

        private float GetHelpBoxHeight()
        {
            InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)attribute;
            float minHeight = EditorGUIUtility.singleLineHeight * 2.0f;
            var width = EditorGUIUtility.currentViewWidth;
            float desiredHeight = GUI.skin.box.CalcHeight(new GUIContent(infoBoxAttribute.Text), EditorGUIUtility.currentViewWidth);
            float height = Mathf.Max(minHeight, desiredHeight);

            return height;
        }

        private void DrawInfoBox(Rect rect, string infoText, HelpBoxMessageType infoBoxType)
        {
            MessageType messageType;
            switch (infoBoxType)
            {
                case HelpBoxMessageType.None:
                    messageType = MessageType.None;
                    break;
                case HelpBoxMessageType.Info:
                    messageType = MessageType.Info;
                    break;
                case HelpBoxMessageType.Warning:
                    messageType = MessageType.Warning;
                    break;
                case HelpBoxMessageType.Error:
                    messageType = MessageType.Error;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(infoBoxType), infoBoxType, null);
            }

            NaughtyEditorGUI.HelpBox(rect, infoText, messageType);
        }
    }
}
