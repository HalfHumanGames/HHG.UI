using System;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.UI.Runtime
{
    [Serializable]
    public class LoadingScreen
    {
        public float MinDuration;
        public List<Sprite> Sprites = new List<Sprite>();
        public List<string> Text = new List<string>();
    }
}