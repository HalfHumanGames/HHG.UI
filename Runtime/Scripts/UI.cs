using HHG.Common.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public partial class UI : MonoBehaviour
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

        public object Id { get; } = null;
        public SubjectId SubjectId => new SubjectId(GetType(), Id);
        public OpenState CurrentState => state;
        public FocusState CurrentFocus => focus;
        public OpenState PreviousState => previousState;
        public FocusState PreviousFocus => previousFocus;
        public bool IsOpen => CurrentState == OpenState.Open;
        public bool IsOpening => CurrentState == OpenState.Opening;
        public bool IsClosed => CurrentState == OpenState.Closed;
        public bool IsClosing => CurrentState == OpenState.Closing;
        public bool IsFocused => CurrentFocus == FocusState.Focused;
        public bool IsFocusing => CurrentFocus == FocusState.Focusing;
        public bool IsUnfocused => CurrentFocus == FocusState.Unfocused;
        public bool IsUnfocusing => CurrentFocus == FocusState.Unfocusing;
        public bool IsTransitioning => IsOpening || IsClosing || IsFocusing || IsUnfocusing;
        public bool IsRoot => parent == null;
        public UI Root => root;
        public UI Parent => parent;
        public IReadOnlyList<UI> Children => children;
        public RectTransform RectTransform => rectTransform;
        public Animator Animator => animator;
        public CanvasGroup CanvasGroup => canvasGroup;

        [SerializeField] private bool center;
        [SerializeField, FormerlySerializedAs("SelectOnFocus")] private Selectable select;
        [SerializeField] private OpenState state;
        [SerializeField] private FocusState focus;
        [SerializeField] private bool backEnabled = true;

        public UnityEvent Opened = new UnityEvent();
        public UnityEvent Closed = new UnityEvent();
        public UnityEvent Focused = new UnityEvent();
        public UnityEvent Unfocused = new UnityEvent();

        private UI root;
        private UI parent;
        private List<UI> children = new List<UI>();
        private RectTransform rectTransform;
        private Animator animator;
        private CanvasGroup canvasGroup;
        private bool hasCloseAnimation;
        private bool hasUnfocusAnimation;
        private OpenState previousState;
        private FocusState previousFocus;
        private bool isLayoutDirty;

        private bool wasOpen { get => previousState == OpenState.Open; set => previousState = value ? OpenState.Open : OpenState.Closed; }
        private bool wasClosed { get => previousState == OpenState.Closed; set => previousState = value ? OpenState.Closed : OpenState.Open; }
        private bool wasFocused { get => wasOpen && previousFocus == FocusState.Focused; set => previousFocus = value ? FocusState.Focused : FocusState.Unfocused; }
        private bool wasUnfocused { get => wasOpen && previousFocus == FocusState.Unfocused; set => previousFocus = value ? FocusState.Unfocused : FocusState.Focused; }

        [ContextMenu("Open")]
        public void Open() => OpenInternal(false);
        public void Open(bool instant) => OpenInternal(instant);

        [ContextMenu("Close")]
        public void Close() => CloseInternal(false);
        public void Close(bool instant) => CloseInternal(instant);

        [ContextMenu("Toggle")]
        public void Toggle() => ToggleInternal(false);
        public void Toggle(bool instant) => ToggleInternal(instant);

        [ContextMenu("Focus")]
        public void Focus() => FocusInternal(false);
        public void Focus(bool instant) => FocusInternal(instant);

        [ContextMenu("Unfocus")]
        public void Unfocus() => UnfocusInternal(false);
        public void Unfocus(bool instant) => UnfocusInternal(instant);

        public void RebuildLayout() => isLayoutDirty = true;

        public void EnableBack(bool val) => backEnabled = val;
        public void EnableBack() => backEnabled = true;
        public void DisableBack() => backEnabled = false;

        private Coroutine OpenInternal(bool instant = false) => Transition(OpenCoroutine(instant));
        private Coroutine CloseInternal(bool instant = false) => Transition(CloseCoroutine(instant));
        private Coroutine ToggleInternal(bool instant = false) => IsOpen ? CloseInternal(instant) : OpenInternal(instant);
        private Coroutine FocusInternal(bool instant = false) => Transition(FocusCoroutine(instant));
        private Coroutine UnfocusInternal(bool instant = false) => Transition(UnfocusCoroutine(instant));

        protected virtual void Awake()
        {
            map.Add(SubjectId, this);
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            root = this.GetTopmostComponent<UI>();
            parent = transform.parent.GetComponentInParent<UI>();

            if (parent != null)
            {
                parent.children.Add(this);
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (animator.runtimeAnimatorController is AnimatorOverrideController controller)
            {
                hasCloseAnimation = controller["UI Close"].name != "UI Close";
                animator.SetBool("HasClose", hasCloseAnimation);
                hasUnfocusAnimation = controller["UI Unfocus"].name != "UI Unfocus";
                animator.SetBool("HasUnfocus", hasUnfocusAnimation);
            }

            if (center)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        protected virtual void Start()
        {
            if (IsRoot)
            {
                InitializeRoot();
            }
        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }

        protected virtual void Update()
        {
            if (isLayoutDirty)
            {
                isLayoutDirty = false;
                RectTransform.RebuildLayout();
            }
        }

        private void InitializeRoot()
        {
            children.ForEach(child => child.InitializeChild());

            switch (state)
            {
                case OpenState.Closing:
                case OpenState.Closed:
                    state = OpenState.Open;
                    focus = FocusState.Unfocused;
                    CloseInternal(true);
                    break;
                case OpenState.Opening:
                case OpenState.Open:
                    state = OpenState.Closed;
                    focus = FocusState.Unfocused;
                    Push(GetType(), Id, true);
                    break;
            }
        }

        private void InitializeChild()
        {
            wasOpen = IsOpen || IsOpening;
            state = IsOpen || IsOpening ? OpenState.Open : OpenState.Closed;

            if ((IsOpen || IsOpening) && (parent.IsOpen || parent.IsOpening))
            {
                state = OpenState.Closed;
                focus = FocusState.Unfocused;
                OpenInternal(true);
            }
            else
            {
                state = OpenState.Open;
                focus = FocusState.Unfocused;
                CloseInternal(true);
            }
        }

        protected virtual void OnOpen()
        {

        }

        protected virtual void OnClose()
        {

        }

        protected virtual void OnFocus()
        {
            canvasGroup.interactable = true;

            if (select != null)
            {
                select.Select();
            }
        }

        protected virtual void OnUnfocus()
        {
            canvasGroup.interactable = false;
        }

        private Coroutine Transition(IEnumerator coroutine)
        {
            return StartCoroutine(WaitForAnimationToFinish(coroutine));
        }

        private IEnumerator WaitForAnimationToFinish(IEnumerator coroutine)
        {
            canvasGroup.interactable = false;

            while (IsTransitioning)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return coroutine;

            canvasGroup.interactable = IsOpen && IsFocused;
        }

        private IEnumerator OpenCoroutine(bool instant = false)
        {
            if (IsOpen) yield break;

            RebuildLayout();

            state = OpenState.Opening;
            yield return OpenSelf(instant);
            yield return OpenChildren(instant);
            state = OpenState.Open;

            OnOpen();
            Opened.Invoke();
        }

        private IEnumerator OpenSelf(bool instant)
        {
            if (instant)
            {
                animator.ResetTrigger("Open");
                animator.ResetTrigger("Close");
                animator.Play("Unfocused", -1, 1f);
            }
            else
            {
                animator.ResetTrigger("Close");
                animator.SetTrigger("Open");

                yield return new WaitForAnimatorState(animator, "Unfocused", 0f);
            }
        }

        private IEnumerator OpenChildren(bool instant)
        {
            List<UI> watch = new List<UI>();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].wasOpen)
                {
                    children[i].OpenInternal(instant);
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

            state = OpenState.Closing;
            yield return CloseChildren(instant);
            yield return CloseSelf(instant);
            state = OpenState.Closed;

            ResetAllTriggers();
            OnClose();
            Closed.Invoke();
        }

        private IEnumerator CloseChildren(bool instant)
        {
            List<UI> watch = new List<UI>();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsOpen || children[i].IsOpening)
                {
                    children[i].wasOpen = true;
                    children[i].CloseInternal(instant);
                    watch.Add(children[i]);
                }
                else
                {
                    children[i].wasClosed = true;
                    children[i].CloseInternal(true);
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
                    animator.ResetTrigger("Open");
                    animator.ResetTrigger("Close");
                    animator.Play("Close", 0, 1f);
                }
                else
                {
                    animator.Play("Close (Reverse Open)", 0, 1f);
                }
            }
            else
            {
                if (IsFocusing || IsFocused)
                {
                    animator.ResetTrigger("Focus");
                    animator.SetTrigger("Unfocus");

                    yield return new WaitForAnimatorState(animator, "Unfocused", 1f);
                }

                animator.ResetTrigger("Open");
                animator.SetTrigger("Close");

                yield return new WaitForAnimatorState(animator, "Close", 1f);
            }
        }

        private IEnumerator FocusCoroutine(bool instant = false)
        {
            if (IsClosed || IsFocused) yield break;

            focus = FocusState.Focusing;
            yield return FocusSelf(instant);
            yield return FocusChildren(instant);
            focus = FocusState.Focused;

            ResetAllTriggers();
            OnFocus();
            Focused.Invoke();
        }

        private IEnumerator FocusSelf(bool instant)
        {
            if (instant)
            {
                animator.ResetTrigger("Focus");
                animator.ResetTrigger("Unfocus");
                animator.Play("Focus", 0, 1f);
            }
            else
            {
                animator.ResetTrigger("Unfocus");
                animator.SetTrigger("Focus");

                yield return new WaitForAnimatorState(animator, "Focused", 0f);
            }
        }

        private IEnumerator FocusChildren(bool instant)
        {
            List<UI> watch = new List<UI>();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].wasFocused)
                {
                    children[i].FocusInternal(instant);
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

            focus = FocusState.Unfocusing;
            yield return UnfocusSelf(instant);
            yield return UnfocusChildren(instant);
            focus = FocusState.Unfocused;

            OnUnfocus();
            Unfocused.Invoke();
        }

        private IEnumerator UnfocusSelf(bool instant)
        {
            if (instant)
            {
                if (hasUnfocusAnimation)
                {
                    animator.ResetTrigger("Focus");
                    animator.ResetTrigger("Unfocus");
                    animator.Play("Unfocus", 0, 1f);
                }
                else
                {
                    animator.Play("Unfocus (Reverse Focus)", 0, 1f);
                }
            }
            else
            {
                animator.ResetTrigger("Focus");
                animator.SetTrigger("Unfocus");

                yield return new WaitForAnimatorState(animator, "Unfocused", 1f);
            }
        }

        private IEnumerator UnfocusChildren(bool instant)
        {
            List<UI> watch = new List<UI>();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsFocused || children[i].IsFocusing)
                {
                    children[i].wasFocused = true;
                    children[i].UnfocusInternal(instant);
                    watch.Add(children[i]);
                }
                else
                {
                    children[i].wasFocused = true;
                    children[i].UnfocusInternal(true);
                }
            }
            while (watch.Count > 0 && watch.Any(child => child.IsUnfocusing))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private void ResetAllTriggers()
        {
            animator.ResetTrigger("Open");
            animator.ResetTrigger("Close");
            animator.ResetTrigger("Focus");
            animator.ResetTrigger("Unfocus");
        }

        protected virtual void OnDestroy()
        {
            map.Remove(SubjectId);
        }
    }

    public abstract class UIT : UI, IRefreshableWeak
    {
        public abstract void RefreshWeak(object data);
    }

    public abstract class UI<T> : UIT, IRefreshable<T>
    {
        public override void RefreshWeak(object model)
        {
            Refresh((T)model);
            RebuildLayout();
        }

        public abstract void Refresh(T model);
    }
}