using HHG.Common.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.UI.Runtime
{
    [System.Serializable]
    public class CarouselData
    {
        public List<CarouselItemData> Items = new List<CarouselItemData>();
        public ActionEvent OnContinue;

        public CarouselData(List<CarouselItemData> items, ActionEvent onContinue = null)
        {
            Items = items;

            if (onContinue != null)
            {
                OnContinue = onContinue;
            }
        }

        public CarouselData(IEnumerable<CarouselItemData> items, ActionEvent onContinue = null)
        {
            Items.AddRange(items);

            if (onContinue != null)
            {
                OnContinue = onContinue;
            }
        }
    }

    [System.Serializable]
    public class CarouselItemData
    {
        public List<Sprite> Sprites = new List<Sprite>();
        public List<string> Texts = new List<string>();

        public CarouselItemData(List<Sprite> sprites, List<string> texts = null)
        {
            Sprites = sprites;
            
            if (texts != null)
            {
                Texts = texts;
            }
        }

        public CarouselItemData(IEnumerable<Sprite> sprites, IEnumerable<string> texts = null)
        {
            Sprites.AddRange(sprites);
            
            if (texts != null)
            {
                Texts.AddRange(texts);
            }
        }
    }
}