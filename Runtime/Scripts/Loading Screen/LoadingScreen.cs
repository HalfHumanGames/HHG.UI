using System;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.UISystem.Runtime
{
    [Serializable]
    public class LoadingScreen
    {
        public bool IsDefault;
        public float MinDuration;
        public List<Sprite> Sprites = new List<Sprite>();
        public List<string> Text = new List<string>();
    }
}