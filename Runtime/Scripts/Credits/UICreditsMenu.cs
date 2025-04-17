using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HHG.UI.Runtime
{
    public class UICreditsMenu : UI
    {
        [SerializeField] private bool autoScroll;
        [SerializeField, ShowIf(nameof(autoScroll), true)] private float scrollSpeed = 50f;
        [SerializeField] private bool showScrollbar;
        [SerializeField] private TextAsset credits;
        [SerializeField] private Transform content;
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private TextMeshProUGUI creditsLabel;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private AutoScroller autoScroller;
        [SerializeField] private ActionEvent onDone;

        private RectTransform rect;
        private GameObject blankSpaceBefore;
        private GameObject blankSpaceAfter;
        private List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();

        protected override void Start()
        {
            base.Start();

            rect = GetComponent<RectTransform>();

            headerLabel.text = string.Empty;
            creditsLabel.text = string.Empty;

            headerLabel.alignment = TextAlignmentOptions.Center;
            creditsLabel.alignment = TextAlignmentOptions.Center;

            if (!showScrollbar)
            {
                scrollRect.vertical = false;

                // Setting the scrollbar inactive does NOT work
                Destroy(scrollRect.verticalScrollbar.gameObject);
            }

            rect.RebuildLayout(); // Do before getting the height

            float height = scrollRect.viewport.rect.height;

            blankSpaceBefore = AddBlankSpace(height);

            string[] lines = credits.text.Split(Environment.NewLine);

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line[0] == '#')
                {
                    CreateHeader(line);
                }
                else
                {
                    CreateCredit(line, labels);
                }
            }

            blankSpaceAfter = AddBlankSpace(height);

            DestroyTemplates();

            // Rebuild after destroying the templates
            rect.RebuildLayout();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            autoScroller.AutoScroll = autoScroll;
            autoScroller.ScrollSpeed = scrollSpeed;
            autoScroller.Restart();
            SetBlankSpaceVisible(autoScroll);
        }

        private GameObject AddBlankSpace(float height)
        {
            GameObject blankSpace = new GameObject("Blank Space", typeof(RectTransform));
            RectTransform rectTransform = blankSpace.GetComponent<RectTransform>();
            rectTransform.SetParent(content, false);
            rectTransform.sizeDelta = new Vector2(0f, height);
            return blankSpace;
        }

        private void CreateHeader(string line)
        {
            string header = line.Substring(1).Trim();
            TextMeshProUGUI label = Instantiate(headerLabel, content);
            label.text = header;
            labels.Clear();
        }

        private void CreateCredit(string line, List<TextMeshProUGUI> labels)
        {
            string[] parts = line.Split('|');

            if (labels.Count == 0)
            {
                GameObject credit = Instantiate(creditsLabel.transform.parent.gameObject, content);
                TextMeshProUGUI creditLabel = credit.GetComponentInChildren<TextMeshProUGUI>(true);
                creditLabel.alignment = GetAlignment(parts.Length, 0);
                labels.Add(creditLabel);

                for (int i = 1; i < parts.Length; i++)
                {
                    TextMeshProUGUI creditLabel2 = Instantiate(creditLabel, credit.transform);
                    creditLabel2.alignment = GetAlignment(parts.Length, i);
                    labels.Add(creditLabel2);
                }
            }

            for (int i = 0; i < parts.Length; i++)
            {
                labels[i].text += parts[i].Trim() + Environment.NewLine;
            }
        }

        private TextAlignmentOptions GetAlignment(int parts, int column)
        {
            return parts != 2 ? TextAlignmentOptions.Center : column == 0 ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        }

        private void DestroyTemplates()
        {
            // Disable so not factored into the rebuild layout
            headerLabel.gameObject.SetActive(false);
            creditsLabel.transform.parent.gameObject.SetActive(false);

            Destroy(headerLabel.gameObject);
            Destroy(creditsLabel.transform.parent.gameObject);
        }

        private void SetBlankSpaceVisible(bool visible)
        {
            blankSpaceBefore.SetActive(visible);
            blankSpaceAfter.SetActive(visible);
            rect.RebuildLayout();
        }
    }
}