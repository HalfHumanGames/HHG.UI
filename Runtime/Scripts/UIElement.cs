using HHG.Common.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HHG.UISystem.Runtime
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIElement : MonoBehaviour, IRefreshable
    {
        public enum OpenState
        {
            Closed,
            Closing,
            Open,
            Opening
        }

        public enum FocusState
        {
            Focused,
            Focusing,
            Unfocused,
            Unfocusing
        }

        [Serializable]
        public class Callbacks
        {
            public UnityEvent Opened;
            public UnityEvent Closed;
            public UnityEvent Focused;
            public UnityEvent Unfocused;
        }

        [SerializeField, FormerlySerializedAs("InitialState")] protected OpenState state;
        [SerializeField] protected FocusState focus;
        public Callbacks Events;

        public virtual object Id { get; } = null;
        public SubjectId ViewId => new SubjectId(GetType(), Id);
        public OpenState CurrentState { get => state; private set => state = value; }
        public FocusState CurrentFocus { get => focus; private set => focus = value; }
        public OpenState FormerState { get; private set; }
        public FocusState FormerFocus { get; private set; }
        public bool IsOpen => CurrentState == OpenState.Open;
        public bool IsOpening => CurrentState == OpenState.Opening;
        public bool IsClosed => CurrentState == OpenState.Closed;
        public bool IsClosing => CurrentState == OpenState.Closing;
        public bool IsFocused => CurrentFocus == FocusState.Focused;
        public bool IsFocusing => CurrentFocus == FocusState.Focusing;
        public bool IsUnfocused => CurrentFocus == FocusState.Unfocused;
        public bool IsUnfocusing => CurrentFocus == FocusState.Unfocusing;
        public bool WasOpen { get => FormerState == OpenState.Open; set => FormerState = value ? OpenState.Open : OpenState.Closed; }
        public bool WasClosed { get => FormerState == OpenState.Closed; set => FormerState = value ? OpenState.Closed : OpenState.Open; }
        public bool WasFocused { get => WasOpen && FormerFocus == FocusState.Focused; set => FormerFocus = value ? FocusState.Focused : FocusState.Unfocused; }
        public bool WasUnfocused { get => WasOpen && FormerFocus == FocusState.Unfocused; set => FormerFocus = value ? FocusState.Unfocused : FocusState.Focused; }
        public bool IsTransitioning => IsOpening || IsClosing || IsFocusing || IsUnfocusing;
        public RectTransform RectTransform { get; private set; }
        public Animator Animator { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public UIView View { get; private set; }
        public UIElement Parent { get; private set; }
        public ReadOnlyCollection<UIElement> Children { get; private set; }

        protected List<UIElement> children { get; private set; }  = new List<UIElement>();
        protected bool hasCloseAnimation { get; private set; }
        protected bool hasUnfocusAnimation { get; private set; }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnFocus() { }
        protected virtual void OnUnfocus() { }

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            Animator = GetComponent<Animator>();
            CanvasGroup = GetComponent<CanvasGroup>();
            View = GetComponentInParent<UIView>();
            Parent = transform.parent.GetComponentInParent<UIElement>();
            if (Parent != null)
            {
                Parent.children.Add(this);
            }
            if (Animator == null)
            {
                Animator = GetComponent<Animator>();
            }
            if (Animator.runtimeAnimatorController is AnimatorOverrideController controller)
            {
                hasCloseAnimation = controller["UI Close"].name != "UI Close";
                Animator.SetBool("HasClose", hasCloseAnimation);
                hasUnfocusAnimation = controller["UI Unfocus"].name != "UI Unfocus";
                Animator.SetBool("HasUnfocus", hasUnfocusAnimation);
            }
            Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        protected virtual void Start()
        {
            Children = new ReadOnlyCollection<UIElement>(children);
        }

        internal virtual void InitializeState()
        {
            WasOpen = IsOpen || IsOpening;
            state = IsOpen || IsOpening ? OpenState.Open : OpenState.Closed;
            if ((IsOpen || IsOpening) && (Parent.IsOpen || Parent.IsOpening)) {
                state = OpenState.Closed;
                focus = FocusState.Unfocused;
                Open(true);
            } else {
                state = OpenState.Open;
                focus = FocusState.Unfocused;
                Close(true);
            }
        }

        public void Toggle() => Toggle(false);
        public void Open() => Open(false);
        public void Close() => Close(false);
        public void Focus() => Focus(false);
        public void Unfocus() => Unfocus(false);
       
        public virtual Coroutine Toggle(bool instant = false) => IsOpen ? Close(instant) : Open(instant);
        public virtual Coroutine Open(bool instant = false) => Transition(OpenCoroutine(instant));
        public virtual Coroutine Close(bool instant = false) => Transition(CloseCoroutine(instant));
        public virtual Coroutine Focus(bool instant = false) => Transition(FocusCoroutine(instant));
        public virtual Coroutine Unfocus(bool instant = false) => Transition(UnfocusCoroutine(instant));

        private Coroutine Transition(IEnumerator coroutine)
        {
            return StartCoroutine(WaitForAnimationToFinish(coroutine));
        }

        private IEnumerator WaitForAnimationToFinish(IEnumerator coroutine)
        {
            CanvasGroup.interactable = false;
            while (IsTransitioning)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return coroutine;
            CanvasGroup.interactable = IsOpen && IsFocused;
        }

        private IEnumerator OpenCoroutine(bool instant = false)
        {
            if (IsOpen) yield break;
            CurrentState = OpenState.Opening;
            yield return OpenSelf(instant);
            yield return OpenChildren(instant);
            CurrentState = OpenState.Open;
            OnOpen();
            Events.Opened.Invoke();
        }

        private IEnumerator OpenSelf(bool instant)
        {
            if (instant)
            {
                Animator.ResetTrigger("Open");
                Animator.ResetTrigger("Close");
                Animator.Play("Unfocused", -1, 1f);
            }
            else
            {
                Animator.ResetTrigger("Close");
                Animator.SetTrigger("Open");
                yield return new WaitForAnimatorState(Animator, "Unfocused", 0f);
            }
        }

        private IEnumerator OpenChildren(bool instant)
        {
            List<UIElement> watch = new List<UIElement>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].WasOpen)
                {
                    children[i].Open(instant);
                    watch.Add(children[i]);
                }
            }
            while (watch.Count > 0 && watch.Any(child => child.IsOpening))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator CloseCoroutine(bool instant = false)
        {
            if (IsClosed) yield break;
            CurrentState = OpenState.Closing;
            yield return CloseChildren(instant);
            yield return CloseSelf(instant);
            CurrentState = OpenState.Closed;
            ResetAllTriggers();
            OnClose();
            Events.Closed.Invoke();
        }

        private IEnumerator CloseChildren(bool instant)
        {
            List<UIElement> watch = new List<UIElement>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsOpen || children[i].IsOpening)
                {
                    children[i].WasOpen = true;
                    children[i].Close(instant);
                    watch.Add(children[i]);
                }
                else
                {
                    children[i].WasClosed = true;
                    children[i].Close(true);
                }
            }
            while (watch.Count > 0 && watch.Any(child => child.IsClosing))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator CloseSelf(bool instant)
        {
            if (instant)
            {
                if (hasCloseAnimation)
                {
                    Animator.ResetTrigger("Open");
                    Animator.ResetTrigger("Close");
                    Animator.Play("Close", 0, 1f);
                }
                else
                {
                   Animator.Play("Close (Reverse Open)", 0, 1f);
                }
            }
            else
            {
                if (IsFocusing || IsFocused)
                {
                    Animator.ResetTrigger("Focus");
                    Animator.SetTrigger("Unfocus");
                    yield return new WaitForAnimatorState(Animator, "Unfocused", 1f);
                }
                Animator.ResetTrigger("Open");
                Animator.SetTrigger("Close");
                yield return new WaitForAnimatorState(Animator, "Close", 1f);
            }
        }

        private IEnumerator FocusCoroutine(bool instant = false)
        {
            if (IsClosed || IsFocused) yield break;
            CurrentFocus = FocusState.Focusing;
            yield return FocusSelf(instant);
            yield return FocusChildren(instant);
            CurrentFocus = FocusState.Focused;
            ResetAllTriggers();
            OnFocus();
            Events.Focused.Invoke();
        }

        private IEnumerator FocusSelf(bool instant)
        {
            if (instant)
            {
                Animator.ResetTrigger("Focus");
                Animator.ResetTrigger("Unfocus");
                Animator.Play("Focus", 0, 1f);
            }
            else
            {
                Animator.ResetTrigger("Unfocus");
                Animator.SetTrigger("Focus");
                yield return new WaitForAnimatorState(Animator, "Focused", 0f);
            }
        }

        private IEnumerator FocusChildren(bool instant)
        {
            List<UIElement> watch = new List<UIElement>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].WasFocused)
                {
                    children[i].Focus(instant);
                    watch.Add(children[i]);
                }
            }
            while (watch.Count > 0 && watch.Any(child => child.IsFocusing))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator UnfocusCoroutine(bool instant = false)
        {
            if (IsClosed || IsUnfocused) yield break;
            CurrentFocus = FocusState.Unfocusing;
            yield return UnfocusSelf(instant);
            yield return UnfocusChildren(instant);
            CurrentFocus = FocusState.Unfocused;
            OnUnfocus();
            Events.Unfocused.Invoke();
        }

        private IEnumerator UnfocusSelf(bool instant)
        {
            if (instant)
            {
                if (hasUnfocusAnimation)
                {
                    Animator.ResetTrigger("Focus");
                    Animator.ResetTrigger("Unfocus");
                    Animator.Play("Unfocus", 0, 1f);
                }
                else
                {
                    Animator.Play("Unfocus (Reverse Focus)", 0, 1f);
                }
            }
            else
            {
                Animator.ResetTrigger("Focus");
                Animator.SetTrigger("Unfocus");
                yield return new WaitForAnimatorState(Animator, "Unfocused", 1f);
            }
        }

        private IEnumerator UnfocusChildren(bool instant)
        {
            List<UIElement> watch = new List<UIElement>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsFocused || children[i].IsFocusing)
                {
                    children[i].WasFocused = true;
                    children[i].Unfocus(instant);
                    watch.Add(children[i]);
                }
                else
                {
                    children[i].WasFocused = true;
                    children[i].Unfocus(true);
                }
            }
            while (watch.Count > 0 && watch.Any(child => child.IsUnfocusing))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private void ResetAllTriggers()
        {
            Animator.ResetTrigger("Open");
            Animator.ResetTrigger("Close");
            Animator.ResetTrigger("Focus");
            Animator.ResetTrigger("Unfocus");
        }

        public virtual void Refresh()
        {
            
        }
    }

    public class UIElement<T> : UIElement, IRefreshable<T>
    {
        public virtual void Refresh(T model)
        {
            
        }
    }
}