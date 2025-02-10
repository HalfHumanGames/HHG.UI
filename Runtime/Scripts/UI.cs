using HHG.Common.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HHG.UISystem.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
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

        [System.Flags]
        public enum Options
        {
            RememberSection = 1 << 0,
            ForgetSelectionOnClose = 1 << 1,
            RestorePreviousSelection = 1 << 2,

            // This prevents Unity from serializing an enum with all
            // of the currently available flags enabled as -1, which
            // means any new flags added at a later time would also
            // become enabled by default, which we do not want.
            _ = 1 << 31
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
        public ActionEvent OnOpened => onOpened;
        public ActionEvent OnClosed => onClosed;
        public ActionEvent OnFocused => onFocused;
        public ActionEvent OnUnfocused => onUnfocused;

        [SerializeField] protected bool center;
        [SerializeField, FormerlySerializedAs("SelectOnFocus")] protected Selectable select;
        // TODO: Deprecate these
        [SerializeField, HideInInspector] protected bool rememberSelection;
        [SerializeField, HideInInspector] protected bool resetSelectionOnClose;
        [SerializeField, HideInInspector] protected bool restoreSelection;
        // END_TODO
        [SerializeField] protected Options options;
        [SerializeField] protected OpenState state;
        [SerializeField] protected FocusState focus;
        [SerializeField] protected bool backEnabled = true;

        [SerializeField, FormerlySerializedAs("OnOpened")] private ActionEvent onOpened = new ActionEvent();
        [SerializeField, FormerlySerializedAs("OnClosed")] private ActionEvent onClosed = new ActionEvent();
        [SerializeField, FormerlySerializedAs("OnFocused")] private ActionEvent onFocused = new ActionEvent();
        [SerializeField, FormerlySerializedAs("OnUnfocused")] private ActionEvent onUnfocused = new ActionEvent();

        private UI root;
        private UI parent;
        private List<UI> children = new List<UI>();
        private RectTransform rectTransform;
        private Animator animator;
        private CanvasGroup canvasGroup;
        private Selectable selectionToRemember;
        private Selectable selectionToRestore;
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
        public Coroutine Open() => OpenInternal(false);
        public Coroutine Open(bool instant) => OpenInternal(instant);

        [ContextMenu("Close")]
        public Coroutine Close() => CloseInternal(false);
        public Coroutine Close(bool instant) => CloseInternal(instant);

        [ContextMenu("Toggle")]
        public Coroutine Toggle() => ToggleInternal(false);
        public Coroutine Toggle(bool instant) => ToggleInternal(instant);

        [ContextMenu("Focus")]
        public Coroutine Focus() => FocusInternal(false);
        public Coroutine Focus(bool instant) => FocusInternal(instant);

        [ContextMenu("Unfocus")]
        public Coroutine Unfocus() => UnfocusInternal(false);
        public Coroutine Unfocus(bool instant) => UnfocusInternal(instant);

        public void RebuildLayout() => isLayoutDirty = true;

        public void EnableBack(bool val) => backEnabled = val;
        public void EnableBack() => backEnabled = true;
        public void DisableBack() => backEnabled = false;
        public void ResetSelection() => selectionToRemember = null;

        private Coroutine OpenInternal(bool instant = false) => Transition(OpenCoroutine(instant));
        private Coroutine CloseInternal(bool instant = false) => Transition(CloseCoroutine(instant));
        private Coroutine ToggleInternal(bool instant = false) => IsOpen ? CloseInternal(instant) : OpenInternal(instant);
        private Coroutine FocusInternal(bool instant = false) => Transition(FocusCoroutine(instant));
        private Coroutine UnfocusInternal(bool instant = false) => Transition(UnfocusCoroutine(instant));

        protected virtual void Awake()
        {
            SyncOptions();

            map.Add(SubjectId, this);
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
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

        protected virtual void OnWillOpen()
        {

        }

        protected virtual void OnWillClose()
        {

        }

        protected virtual void OnWillFocus()
        {

        }

        protected virtual void OnWillUnfocus()
        {

        }

        protected virtual void OnOpen()
        {
            canvasGroup.alpha = 1f;
        }

        protected virtual void OnClose()
        {
            canvasGroup.alpha = 0f;

            if (options.HasFlag(Options.ForgetSelectionOnClose))
            {
                ResetSelection();
            }
        }

        protected virtual void OnFocus()
        {
            canvasGroup.interactable = true;

            Selectable selection = EventSystem.current.GetCurrentSelectable();

            if (options.HasFlag(Options.RestorePreviousSelection))
            {
                if (selection && !this.IsChild(selection))
                {
                    selectionToRestore = selection;
                }
                else
                {
                    selectionToRestore = null;
                }
            }

            if (!selection || !this.IsChild(selection))
            {
                if (options.HasFlag(Options.RememberSection) && selectionToRemember)
                {
                    selectionToRemember.Select();
                }
                else if (select)
                {
                    select.Select();
                }
            }
        }

        protected virtual void OnUnfocus()
        {
            canvasGroup.interactable = false;

            if (options.HasFlag(Options.RememberSection))
            {
                if (EventSystem.current.TryGetCurrentSelection(out Selectable selection) && this.IsChild(selection))
                {
                    selectionToRemember = selection;
                }
                else
                {
                    selectionToRemember = null;
                }
            }

            if (selectionToRestore != null)
            {
                selectionToRestore.Select();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
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
            OnWillOpen();

            state = OpenState.Opening;
            yield return OpenSelf(instant);
            yield return OpenChildren(instant);
            state = OpenState.Open;

            OnOpen();
            onOpened.Invoke(this);
            OnAnyOpened.Invoke(this);
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

            OnWillClose();

            state = OpenState.Closing;
            yield return CloseChildren(instant);
            yield return CloseSelf(instant);
            state = OpenState.Closed;

            ResetAllTriggers();
            OnClose();
            onClosed.Invoke(this);
            OnAnyClosed.Invoke(this);
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

            OnWillFocus();

            focus = FocusState.Focusing;
            yield return FocusSelf(instant);
            yield return FocusChildren(instant);
            focus = FocusState.Focused;

            ResetAllTriggers();
            OnFocus();
            onFocused.Invoke(this);
            OnAnyFocused.Invoke(this);
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

            OnWillUnfocus();

            focus = FocusState.Unfocusing;
            yield return UnfocusSelf(instant);
            yield return UnfocusChildren(instant);
            focus = FocusState.Unfocused;

            OnUnfocus();
            onUnfocused.Invoke(this);
            OnAnyUnfocused.Invoke(this);
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

        private void SyncOptions()
        {
            if (rememberSelection)
            {
                rememberSelection = false;
                options |= Options.RememberSection;
            }

            if (resetSelectionOnClose)
            {
                resetSelectionOnClose = false;
                options |= Options.ForgetSelectionOnClose;
            }

            if (restoreSelection)
            {
                restoreSelection = false;
                options |= Options.RestorePreviousSelection;
            }
        }

        protected virtual void OnDestroy()
        {
            // Make sure the map contains this UI instance
            // otherwise you may remove other instances
            if (map.ContainsValue(this))
            {
                map.Remove(SubjectId);
            }
        }

        protected virtual void OnValidate()
        {
            SyncOptions();
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