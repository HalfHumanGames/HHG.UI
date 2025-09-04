using HHG.Common.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace HHG.UI.Runtime
{
    public partial class UI
    {
        public static ActionEvent OnAnyOpened => onAnyOpened;
        public static ActionEvent OnAnyClosed => onAnyClosed;
        public static ActionEvent OnAnyFocused => onAnyFocused;
        public static ActionEvent OnAnyUnfocused => onAnyUnfocused;
        public static ActionEvent OnBack => onBack;

        public static UI Current => opened.Count > 0 ? opened.Peek() : null;
        public static int Count => opened.Count;
        public static bool TryGet<T>(out T ui) where T : UI => TryGet<T>(null, out ui);
        public static bool TryGet<T>(object id, out T ui) where T : UI => TryGet(typeof(T), id, out UI val) && (ui = val as T) != null || (ui = null) is T;
        public static bool TryGet(System.Type type, out UI ui) => TryGet(type, null, out ui);
        public static bool TryGet(System.Type type, object id, out UI ui) => map.TryGetValue(new SubjectId(type, id), out ui);
        public static void Refresh<T, TData>(object id, TData data) where T : UI<TData> => RefreshInternal(new SubjectId(typeof(T), id), data);
        public static void Refresh<T, TData>(TData data) where T : UI<TData> => RefreshInternal(new SubjectId(typeof(T), null), data);
        public static void Refresh(System.Type type, object id, object data) => RefreshInternal(new SubjectId(type, id), data);
        public static void Refresh(System.Type type, object data) => RefreshInternal(new SubjectId(type, null), data);
        public static void RegisterDataProvider<T>(object id, IDataProvider dataProvider) where T : UI => RegisterDataProviderInternal(typeof(T), id, dataProvider);
        public static void RegisterDataProvider<T>(IDataProvider dataProvider) where T : UI => RegisterDataProviderInternal(typeof(T), null, dataProvider);
        public static void RegisterDataProvider(System.Type type, object id, IDataProvider dataProvider) => RegisterDataProviderInternal(type, id, dataProvider);
        public static void RegisterDataProvider(System.Type type, IDataProvider dataProvider) => RegisterDataProviderInternal(type, null, dataProvider);
        public static void UnregisterDataProvider<T>(object id) where T : UI => UnregisterDataProviderInternal(typeof(T), id);
        public static void UnregisterDataProvider<T>() where T : UI => UnregisterDataProviderInternal(typeof(T), null);
        public static void UnregisterDataProvider(System.Type type, object id) => UnregisterDataProviderInternal(type, id);
        public static void UnregisterDataProvider(System.Type type) => UnregisterDataProviderInternal(type, null);
        public static Coroutine Open<T, TData>(object id, TData data, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(OpenAsync(typeof(T), id, instant, data));
        public static Coroutine Open<T, TData>(TData data, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(OpenAsync(typeof(T), null, instant, data));
        public static Coroutine Open<TData>(System.Type type, object id, TData data, bool instant = false) => CoroutineUtil.StartCoroutine(OpenAsync(type, id, instant, data));
        public static Coroutine Open<TData>(System.Type type, TData data, bool instant = false) => CoroutineUtil.StartCoroutine(OpenAsync(type, null, instant, data));
        public static Coroutine Open<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(OpenAsync(typeof(T), id, instant));
        public static Coroutine Open<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(OpenAsync(typeof(T), null, instant));
        public static Coroutine Open(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(OpenAsync(type, id, instant));
        public static Coroutine Open(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(OpenAsync(type, null, instant));
        public static Coroutine Close<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(CloseAsync(typeof(T), id, instant));
        public static Coroutine Close<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(CloseAsync(typeof(T), null, instant));
        public static Coroutine Close(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(CloseAsync(type, id, instant));
        public static Coroutine Close(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(CloseAsync(type, null, instant));
        public static Coroutine Focus<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(FocusAsync(typeof(T), id, instant));
        public static Coroutine Focus<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(FocusAsync(typeof(T), null, instant));
        public static Coroutine Focus(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(FocusAsync(type, id, instant));
        public static Coroutine Focus(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(FocusAsync(type, null, instant));
        public static Coroutine Unfocus<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(UnfocusAsync(typeof(T), id, instant));
        public static Coroutine Unfocus<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(UnfocusAsync(typeof(T), null, instant));
        public static Coroutine Unfocus(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(UnfocusAsync(type, id, instant));
        public static Coroutine Unfocus(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(UnfocusAsync(type, null, instant));
        public static Coroutine GoTo<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(GoToAsync(typeof(T), id, instant));
        public static Coroutine GoTo<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(GoToAsync(typeof(T), null, instant));
        public static Coroutine GoTo(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(GoToAsync(type, id, instant));
        public static Coroutine GoTo(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(GoToAsync(type, null, instant));
        public static Coroutine Push<T, TData>(object id, TData data, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(PushAsync(typeof(T), id, instant, data));
        public static Coroutine Push<T, TData>(TData data, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(PushAsync(typeof(T), null, instant, data));
        public static Coroutine Push<TData>(System.Type type, object id, TData data, bool instant = false) => CoroutineUtil.StartCoroutine(PushAsync(type, id, instant, data));
        public static Coroutine Push<TData>(System.Type type, TData data, bool instant = false) => CoroutineUtil.StartCoroutine(PushAsync(type, null, instant, data));
        public static Coroutine Push<TData>(TData data, bool instant = false) => Push(FindTypeByModel<TData>(), data, instant);
        public static Coroutine Push<TData>(object id, TData data, bool instant = false) => Push(FindTypeByModel<TData>(), id, data, instant);
        public static Coroutine Push<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(PushAsync(typeof(T), id, instant));
        public static Coroutine Push<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(PushAsync(typeof(T), null, instant));
        public static Coroutine Push(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(PushAsync(type, id, instant));
        public static Coroutine Push(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(PushAsync(type, null, instant));
        public static Coroutine Pop(bool instant = false) => CoroutineUtil.StartCoroutine(PopAsync(instant));
        public static Coroutine Pop(int amount, bool instant = false) => CoroutineUtil.StartCoroutine(PopAsync(amount, instant));
        public static Coroutine Clear(bool instant = false) => CoroutineUtil.StartCoroutine(ClearAsync(instant));
        public static Coroutine Swap<T>(object id = null, bool instant = false) where T : UI => CoroutineUtil.StartCoroutine(SwapAsync(typeof(T), id, instant));
        public static Coroutine Swap<T>(bool instant) where T : UI => CoroutineUtil.StartCoroutine(SwapAsync(typeof(T), null, instant));
        public static Coroutine Swap(System.Type type, object id = null, bool instant = false) => CoroutineUtil.StartCoroutine(SwapAsync(type, id, instant));
        public static Coroutine Swap(System.Type type, bool instant) => CoroutineUtil.StartCoroutine(SwapAsync(type, null, instant));

        private static ActionEvent onAnyOpened = new ActionEvent();
        private static ActionEvent onAnyClosed = new ActionEvent();
        private static ActionEvent onAnyFocused = new ActionEvent();
        private static ActionEvent onAnyUnfocused = new ActionEvent();
        private static ActionEvent onBack = new ActionEvent();

        private static Dictionary<SubjectId, UI> map = new Dictionary<SubjectId, UI>();
        private static Dictionary<SubjectId, IDataProvider> dataProviders = new Dictionary<SubjectId, IDataProvider>();
        private static Stack<UI> opened = new Stack<UI>();
        private static InputAction back;

        static UI()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            // Do not clear the map since that breaks any persistent UIs
            // Clear since not required to pop all UIs before loading a new scene
            // This assumes that no persistent UIs are open when changing scenes
            opened.Clear();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // The current input system's action asset can
            // change for multiplayer games, so instead of
            // subscribing to the asset's event, we subscribe
            // to this global event and check if it is the 
            // modules current input actions asset
            InputSystem.onActionChange -= OnActionChange;
            InputSystem.onActionChange += OnActionChange;
        }

        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (CanGoBack(obj, change))
            {
                Back();
            }
        }

        private static bool CanGoBack(object obj, InputActionChange change)
        {
            return EventSystem.current != null &&
                   change == InputActionChange.ActionPerformed &&
                   obj is InputAction action &&
                   EventSystem.current.currentInputModule is InputSystemUIInputModule module &&
                   action == module.cancel.action;
        }

        private static void Back()
        {
            if (IsInDropdown())
            {
                // Do nothing since cancel closes dropdowns
            }
            else if (IsInInputField(out TMP_InputField inputField))
            {
                // Unfocus since cancel does NOT close dropdowns
                inputField.DeactivateInputField();
            }
            else if (Current && Current.backEnabled && !Current.IsTransitioning)
            {
                // Use temp since after we pop, current
                // can be null if the stack size is 1
                UI temp = Current;
                Pop();
                OnBack?.Invoke(temp);
            }
        }

        private static bool IsInDropdown()
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;
            return current != null && current.transform.parent.TryGetComponentInParent<TMP_Dropdown>(out _);
        }

        private static bool IsInInputField(out TMP_InputField inputField)
        {
            inputField = null;
            GameObject current = EventSystem.current.currentSelectedGameObject;
            return current != null && current.TryGetComponent(out inputField) && inputField.isFocused;
        }

        private static System.Type FindTypeByModel<T>()
        {
            return FindTypeByModel(typeof(T));
        }

        private static System.Type FindTypeByModel(System.Type modelType)
        {
            foreach (System.Type type in map.Keys.Select(k => k.Type))
            {
                if (type.IsSubclassOfGeneric(typeof(UI<>), out System.Type genericType) && genericType.GetGenericArguments()[0] == modelType)
                {
                    return type;
                }
            }

            throw new System.ArgumentException($"No type found with model of type: {modelType.FullName}", nameof(modelType));
        }

        private static void RefreshInternal(SubjectId key, object data)
        {
            if (map.TryGetValue(key, out UI ui) && ui is IRefreshableWeak refreshable)
            {
                if (dataProviders.TryGetValue(ui.SubjectId, out IDataProvider dataProvider))
                {
                    data = dataProvider.GetDataWeak(data);
                }

                if (data != null)
                {
                    refreshable.RefreshWeak(data);
                }
            }
        }

        private static void RegisterDataProviderInternal(System.Type type, object id, IDataProvider dataProvider)
        {
            SubjectId key = new SubjectId(type, id);
            dataProviders[key] = dataProvider;
        }

        private static void UnregisterDataProviderInternal(System.Type type, object id)
        {
            SubjectId key = new SubjectId(type, id);
            dataProviders.Remove(key);
        }

        private static IEnumerator OpenAsync(System.Type type, object id = null, bool instant = false, object data = null)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.TryGetValue(key, out UI ui))
            {
                RefreshInternal(key, data);
                yield return ui.OpenAsync(instant);
            }
        }

        private static IEnumerator CloseAsync(System.Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.TryGetValue(key, out UI ui))
            {
                yield return ui.CloseAsync(instant);
            }
        }

        private static IEnumerator FocusAsync(System.Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.TryGetValue(key, out UI ui))
            {
                yield return ui.FocusAsync(instant);
            }
        }

        private static IEnumerator UnfocusAsync(System.Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.TryGetValue(key, out UI ui))
            {
                yield return ui.UnfocusAsync(instant);
            }
        }

        // TODO: Need to not focus when popping deeper
        private static IEnumerator GoToAsync(System.Type type, object id = null, bool instant = false)
        {
            SubjectId key = new SubjectId(type, id);

            while (opened.Count > 0 && opened.Peek().SubjectId != key)
            {
                yield return PopAsync(instant);
            }

            if (opened.Count == 0)
            {
                yield return PushAsync(type, id, instant);
            }
        }

        private static IEnumerator PushAsync(System.Type type, object id = null, bool instant = false, object data = null)
        {
            SubjectId key = new SubjectId(type, id);

            if (map.TryGetValue(key, out UI ui))
            {
                if (opened.Count > 0)
                {
                    yield return opened.Peek().UnfocusAsync(instant);
                }

                RefreshInternal(key, data);

                opened.Push(ui);

                yield return ui.OpenAsync(instant);
                yield return ui.FocusAsync(instant);
            }
        }

        private static IEnumerator PopAsync(bool instant = false)
        {
            if (opened.Count > 0)
            {
                UI popped = opened.Pop();

                yield return popped.UnfocusAsync(instant);
                yield return popped.CloseAsync(instant);

                if (opened.Count > 0)
                {
                    yield return opened.Peek().FocusAsync(instant);
                }
            }
        }

        private static IEnumerator PopAsync(int amount, bool instant = false)
        {
            if (amount > opened.Count)
            {
                amount = opened.Count;
            }
            for (int i = 0; i < amount; i++)
            {
                yield return PopAsync(instant);
            }
        }

        private static IEnumerator ClearAsync(bool instant = false)
        {
            while (opened.Count > 0)
            {
                yield return PopAsync(instant);
            }
        }

        private static IEnumerator SwapAsync(System.Type type, object id = null, bool instant = false)
        {
            yield return PopAsync(instant);
            yield return PushAsync(type, id, instant);
        }
    }
}