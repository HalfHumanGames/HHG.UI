using HHG.Common.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: Add support for rebind prefab
namespace HHG.UI.Runtime
{
    [RequireComponent(typeof(UIBinding))]
    [RequireComponent(typeof(UITabNavigation))]
    public class UIOptionsMenu : UI
    {
        [SerializeField] private ScriptableSavable optionsAsset;
        [SerializeField] private GameObject dropdownPrefab;
        [SerializeField] private GameObject inputFieldPrefab;
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private GameObject sliderPrefab;
        [SerializeField] private GameObject togglePrefab;

        private UIBinding binding;
        private UITabNavigation tabNavigation;
        private Transform tabContainer;
        private Transform contentContainer;
        private GameObject tabTemplate;
        private GameObject contentTemplate;

        protected override void Awake()
        {
            base.Awake();

            binding = GetComponent<UIBinding>();
            tabNavigation = GetComponent<UITabNavigation>();
            tabContainer = tabNavigation.TabContainer;
            contentContainer = tabNavigation.ContentContainer;
            tabTemplate = tabContainer.GetChild(0).gameObject;
            contentTemplate = contentContainer.GetChild(0).gameObject;
            tabTemplate.SetActive(false);
            contentTemplate.SetActive(false);

            for (int i = 1; i < tabContainer.childCount; i++)
            {
                Destroy(tabContainer.GetChild(i).gameObject);
            }

            for (int i = 1; i < contentContainer.childCount; i++)
            {
                Destroy(contentContainer.GetChild(i).gameObject);
            }

            Initialize();

            // Set parent to null to prevent from being retrieved later on,
            // since object destruction occurs at the end of the frame
            tabTemplate.transform.SetParent(null);
            contentTemplate.transform.SetParent(null);

            Destroy(tabTemplate);
            Destroy(contentTemplate);
        }

        protected override void Start()
        {
            base.Start();

            // Do in start in case it initialized
            // before this script initializes
            tabNavigation.Initialize();

            // Select the first tab by default
            select = tabNavigation.Tabs.First().Button;
        }

        private void Initialize()
        {
            PropertyInfo[] properties = optionsAsset.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            string category = string.Empty;

            Dictionary<string, List<PropertyInfo>> propertiesByCategory = new Dictionary<string, List<PropertyInfo>>();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<OptionsAttribute>() != null)
                {
                    if (property.TryGetCustomAttribute(out OptionsCategoryAttribute categoryAttribute))
                    {
                        category = categoryAttribute.Category;
                    }

                    if (!propertiesByCategory.ContainsKey(category))
                    {
                        propertiesByCategory[category] = new List<PropertyInfo>();
                    }

                    propertiesByCategory[category].Add(property);
                }
            }

            foreach (var kvpair in propertiesByCategory)
            {
                InitializeCategory(kvpair.Key, kvpair.Value.ToArray());
            }
        }

        private void InitializeCategory(string category, PropertyInfo[] properties)
        {
            GameObject tab = Instantiate(tabTemplate, tabContainer);
            GameObject content = Instantiate(contentTemplate, contentContainer);

            tab.name = $"{tabTemplate.name} - {category}";
            content.name = $"{contentTemplate.name} - {category}";

            tab.SetActive(true);
            content.SetActive(true);

            if (tab.TryGetComponentInChildren(out Button button))
            {
                button.onClick.RemoveAllListeners();
            }

            if (tab.TryGetComponentInChildren(out TMP_Text label))
            {
                label.text = category;
            }

            content.DestroyChildren();

            foreach (PropertyInfo property in properties)
            {
                if (property.TryGetCustomAttribute(out OptionsDropdownAttribute dropdownAttribute))
                {
                    var options = optionsAsset.GetValueByPath<IEnumerable<string>>(dropdownAttribute.OptionsMemberName);
                    InitializeDropdown(property, options, content.transform);
                }
                else if (property.TryGetCustomAttribute(out OptionsInputFieldAttribute inputFieldAttribute))
                {
                    InitializeInputField(property, content.transform);
                }
                else if (property.TryGetCustomAttribute(out OptionsSliderAttribute sliderAttribute))
                {
                    InitializeSlider(property, content.transform);
                }
                else if (property.TryGetCustomAttribute(out OptionsToggleAttribute toggleAttribute))
                {
                    InitializeToggle(property, content.transform);
                }
            }
        }

        private void InitializeToggle(PropertyInfo property, Transform content)
        {
            GameObject created = Instantiate(togglePrefab, content);

            if (created.TryGetComponentInChildren(out TMP_Text label))
            {
                label.text = property.Name.ToNicified();
            }

            if (created.TryGetComponentInChildren(out Toggle toggle))
            {
                binding.AddBinding(optionsAsset, property.Name, toggle);
            }
        }

        private void InitializeSlider(PropertyInfo property, Transform content)
        {
            GameObject created = Instantiate(sliderPrefab, content);

            if (created.TryGetComponentInChildren(out TMP_Text label))
            {
                label.text = property.Name.ToNicified();
            }

            if (created.TryGetComponentInChildren(out Slider slider))
            {
                binding.AddBinding(optionsAsset, property.Name, slider);
            }
        }

        private void InitializeDropdown(PropertyInfo property, IEnumerable<string> dropdownOptions, Transform content)
        {
            GameObject created = Instantiate(dropdownPrefab, content);

            if (created.TryGetComponentInChildren(out TMP_Text label))
            {
                label.text = property.Name.ToNicified();
            }

            if (created.TryGetComponentInChildren(out TMP_Dropdown dropdown))
            {
                dropdown.options.Clear();

                foreach (string dropdownOption in dropdownOptions)
                {
                    dropdown.options.Add(new TMP_Dropdown.OptionData(dropdownOption));
                }

                binding.AddBinding(optionsAsset, property.Name, dropdown);
            }
        }

        private void InitializeInputField(PropertyInfo property, Transform content)
        {
            GameObject created = Instantiate(inputFieldPrefab, content);

            if (created.TryGetComponentInChildren(out TMP_Text label))
            {
                label.text = property.Name.ToNicified();
            }

            if (created.TryGetComponentInChildren(out TMP_InputField inputField))
            {
                binding.AddBinding(optionsAsset, property.Name, inputField);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            optionsAsset.Save();
        }
    }
}
