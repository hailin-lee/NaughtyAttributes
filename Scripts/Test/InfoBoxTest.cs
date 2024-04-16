using UnityEngine;
using UnityEngine.UIElements;

namespace NaughtyAttributes.Test
{
    public class InfoBoxTest : MonoBehaviour
    {
        [InfoBox("None", HelpBoxMessageType.None)]
        public int none;

        public InfoBoxNest1 nest1;
    }

    [System.Serializable]
    public class InfoBoxNest1
    {
        [InfoBox("Info", HelpBoxMessageType.Info)]
        public int info;

        public InfoBoxNest2 nest2;
    }

    [System.Serializable]
    public class InfoBoxNest2
    {
        [InfoBox("Warning", HelpBoxMessageType.Warning)]
        public InfoBoxNest3 nest3;
    }
    
    [System.Serializable]
    public class InfoBoxNest3
    {
        [InfoBox("Error", HelpBoxMessageType.Error)]
        public int error;
    }
}
