using System;
using UnityEngine.UIElements;

namespace NaughtyAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class InfoBoxAttribute : DrawerAttribute
    {
        public string Text { get; private set; }
        public HelpBoxMessageType Type { get; private set; }

        public InfoBoxAttribute(string text, HelpBoxMessageType type = HelpBoxMessageType.None)
        {
            Text = text;
            Type = type;
        }
    }
}
