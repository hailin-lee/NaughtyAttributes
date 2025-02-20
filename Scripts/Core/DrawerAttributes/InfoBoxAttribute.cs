using System;


namespace NaughtyAttributes
{
    public enum InfoBoxType
    {
        /// <summary>
        ///        <para>
        /// Neutral message.
        /// </para>
        ///      </summary>
        None,
        /// <summary>
        ///        <para>
        /// Info message.
        /// </para>
        ///      </summary>
        Info,
        /// <summary>
        ///        <para>
        /// Warning message.
        /// </para>
        ///      </summary>
        Warning,
        /// <summary>
        ///        <para>
        /// Error message.
        /// </para>
        ///      </summary>
        Error,
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class InfoBoxAttribute : DrawerAttribute
    {
        public string Text { get; private set; }
        public InfoBoxType Type { get; private set; }

        public InfoBoxAttribute(string text, InfoBoxType type = InfoBoxType.Info)
        {
            Text = text;
            Type = type;
        }
    }
}