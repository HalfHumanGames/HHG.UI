using System.Collections.Generic;
using UnityEngine;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class Carousel
    {
        public List<CarouselItem> Items = new List<CarouselItem>();
    }

    [System.Serializable]
    public class CarouselItem
    {
        public List<Sprite> Sprites = new List<Sprite>();
        public List<string> Texts = new List<string>();
    }
}