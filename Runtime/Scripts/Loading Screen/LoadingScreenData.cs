using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HHG.UI.Runtime
{
    [Serializable]
    public class LoadingScreenData
    {
        public float MinDuration;
        public List<Sprite> Sprites = new List<Sprite>();
        [FormerlySerializedAs("Text")] public List<string> Texts = new List<string>();

        public LoadingScreenData(List<Sprite> sprites, List<string> texts = null, float minDuration = 0f)
        {
            Sprites = sprites;

            if (texts != null)
            {
                Texts = texts;
            }

            MinDuration = minDuration;
        }

        public LoadingScreenData(IEnumerable<Sprite> sprites, IEnumerable<string> texts = null, float minDuration = 0f)
        {
            Sprites.AddRange(sprites);

            if (texts != null)
            {
                Texts.AddRange(texts);
            }

            MinDuration = minDuration;
        }
    }
}