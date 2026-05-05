using HHG.Common.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HHG.UI.Runtime
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
            All = -1
        }

        public object Id { get; set; } = null;
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
        public ActionEvent<UI> OnOpened => onOpened;
        public ActionEvent<UI> OnClosed => onClosed;
        public ActionEvent<UI> OnFocused => onFocused;
        public ActionEvent<UI> OnUnfocused => onUnfocused;

        [SerializeField] protected bool center;
        [SerializeField] protected string customId;
        [SerializeField, FormerlySerializedAs("SelectOnFocus")] protected Selectable select;

        [SerializeField] protected Options options = Options.All;
        [SerializeField] protected OpenState state = OpenState.Open;
        [SerializeField] protected FocusState focus = FocusState.Focused;
        [SerializeField] protected bool backEnabled = true;

        [SerializeField, FormerlySerializedAs("OnOpened")] private ActionEvent<UI> onOpened = new ActionEvent<UI>();
        [SerializeField, FormerlySerializedAs("OnClosed")] private ActionEvent<UI> onClosed = new ActionEvent<UI>();
        [SerializeField, FormerlySerializedAs("OnFocused")] private ActionEvent<UI> onFocused = new ActionEvent<UI>();
        [SerializeField, FormerlySerializedAs("OnUnfocused")] private ActionEvent<UI> onUnfocused = new ActionEvent<UI>();

        private UI root;
        private UI parent;
        private List<UI> children = new List<UI>();
        private Queue<int> transitions = new Queue<int>();
        private RectTransform rectTransform;
        private Animator animator;
        private CanvasGroup canvasGroup;
        private Selectable selectionToRemember;
        private Selectable selectionToRestore;
        private bool hasCloseAnimation;
        private bool hasUnfocusAnimation;
        private OpenState previousState;
        private FocusState previousFocus;

        private bool wasOpen { get => previousState == OpenState.Open; set => previousState = value ? OpenState.Open : OpenState.Closed; }
        private bool wasClosed { get => previousState == OpenState.Closed; set => previousState = value ? OpenState.Closed : OpenState.Open; }
        private bool wasFocused { get => wasOpen && previousFocus == FocusState.Focused; set => previousFocus = value ? FocusState.Focused : FocusState.Unfocused; }
        private bool wasUnfocused { get => wasOpen && previousFocus == FocusState.Unfocused; set => previousFocus = value ? FocusState.Unfocused : FocusState.Focused; }

        [ContextMenu("Open")]
        public Coroutine Open() => StartCoroutineHelper(OpenAsync(false));
        public Coroutine Open(bool instant) => StartCoroutineHelper(OpenAsync(instant));

        [ContextMenu("Close")]
        public Coroutine Close() => StartCoroutineHelper(CloseAsync(false));
        public Coroutine Close(bool instant) => StartCoroutineHelper(CloseAsync(instant));

        [ContextMenu("Toggle")]
        public Coroutine Toggle() => StartCoroutineHelper(ToggleAsync(false));
        public Coroutine Toggle(bool instant) => StartCoroutineHelper(ToggleAsync(instant));

        [ContextMenu("Focus")]
        public Coroutine Focus() => StartCoroutineHelper(FocusAsync(false));
        public Coroutine Focus(bool instant) => StartCoroutineHelper(FocusAsync(instant));

        [ContextMenu("Unfocus")]
        public Coroutine Unfocus() => StartCoroutineHelper(UnfocusAsync(false));
        public Coroutine Unfocus(bool instant) => StartCoroutineHelper(UnfocusAsync(instant));

        [ContextMenu("Push")]
        public Coroutine Push() => Push(GetType(), Id, false);
        public Coroutine Push(bool instant) => Push(GetType(), Id, instant);

        private Coroutine StartCoroutineHelper(IEnumerator routine)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            return StartCoroutine(routine);
        }

        public void MarkLayoutForRebuild(System.Action done = null) => rectTransform.MarkLayoutForRebuild(done);

        public void EnableBack(bool val) => backEnabled = val;
        public void EnableBack() => backEnabled = true;
        public void DisableBack() => backEnabled = false;
        public void ResetSelection() => selectionToRemember = null;

        private IEnumerator OpenAsync(bool instant = false) => WaitForAnimationToFinishAsync(OpenInternalAsync(instant));
        private IEnumerator CloseAsync(bool instant = false) => WaitForAnimationToFinishAsync(CloseInternalAsync(instant));
        private IEnumerator ToggleAsync(bool instant = false) => IsOpen ? CloseAsync(instant) : OpenAsync(instant);
        private IEnumerator FocusAsync(bool instant = false) => WaitForAnimationToFinishAsync(FocusInternalAsync(instant));
        private IEnumerator UnfocusAsync(bool instant = false) => WaitForAnimationToFinishAsync(UnfocusInternalAsync(instant));

        protected virtual void Awake()
        {
            if (!string.IsNullOrEmpty(customId))
            {
                Id = customId;
            }

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

            animator.keepAnimatorStateOnDisable = true;
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

        }

        private void InitializeRoot()
        {
            children.ForEach(child => child.InitializeChild());

            bool instant = state == OpenState.Open || state == OpenState.Closed;

            object data = this is UIT uiT && uiT.WeakAsset != null ? uiT.WeakAsset.WeakData : null;

            switch (state)
            {
                case OpenState.Closing:
                case OpenState.Closed:
                    state = OpenState.Open;
                    focus = FocusState.Unfocused;
                    Close(instant);
                    break;
                case OpenState.Opening:
                case OpenState.Open:
                    state = OpenState.Closed;
                    focus = FocusState.Unfocused;
                    Push(GetType(), Id, data, instant);
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
                Open(true);
            }
            else
            {
                state = OpenState.Open;
                focus = FocusState.Unfocused;
                Close(true);
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
        }

        protected virtual void OnOpen()
        {
            canvasGroup.alpha = 1f;

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
        }

        protected virtual void OnClose()
        {
            canvasGroup.alpha = 0f;

            if (options.HasFlag(Options.ForgetSelectionOnClose))
            {
                ResetSelection();
            }

            if (options.HasFlag(Options.RestorePreviousSelection))
            {
                if (selectionToRestore != null)
                {
                    selectionToRestore.Select();
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        protected virtual void OnFocus()
        {
            canvasGroup.interactable = true;

            if (EventSystem.current.TryGetCurrentSelection(out Selectable selection) && this.IsChild(selection))
            {
                // ReselectSelectedGameObject deselects then reselects
                // the current selected game object, which forces
                // the select event to trigger. We do this since it's
                // possible that it was selected previously, but exited 
                // the select callback due to a IsInteractable check.
                // However, it would still have been selected, so calling
                // Selectable.Select would have done nothing.
                EventSystem.current.ReselectSelectedGameObject();
            }
            else
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

        }

        private IEnumerator WaitForAnimationToFinishAsync(IEnumerator coroutine)
        {
            // Track hash in queue to ensure animations execute
            // in the order in which they were called/enqueued
            int hash = RuntimeHelpers.GetHashCode(coroutine);
            transitions.Enqueue(hash);
            canvasGroup.interactable = false;

            while (IsTransitioning || transitions.Peek() != hash)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return coroutine;

            transitions.Dequeue();
            canvasGroup.interactable = IsOpen && IsFocused;

            // Only deactivate if no queued transitions and closed
            bool deactivate = transitions.Count == 0 && IsClosed;
            if (deactivate) gameObject.SetActive(false);
        }

        private IEnumerator OpenInternalAsync(bool instant = false)
        {
            if (!IsOpen)
            {
                EnsureActive();
                MarkLayoutForRebuild();
                OnWillOpen();

                state = OpenState.Opening;
                yield return OpenSelfAsync(instant);
                yield return OpenChildrenAsync(instant);
                state = OpenState.Open;

                OnOpen();
                onOpened.Invoke(this);
                OnAnyOpened.Invoke(this);
            }
        }

        private IEnumerator OpenSelfAsync(bool instant)
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

        private IEnumerator OpenChildrenAsync(bool instant)
        {
            using CoroutineHandle handle = CoroutineHandle.GetFromPool(this);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].wasOpen)
                {
                    handle.StartCoroutine(children[i].OpenAsync(instant));
                }
            }

            yield return handle;
        }

        private IEnumerator CloseInternalAsync(bool instant = false)
        {
            if (!IsClosed)
            {
                EnsureActive();
                OnWillClose();

                state = OpenState.Closing;
                yield return CloseChildrenAsync(instant);
                yield return CloseSelfAsync(instant);
                state = OpenState.Closed;

                ResetAllTriggers();
                OnClose();
                onClosed.Invoke(this);
                OnAnyClosed.Invoke(this);
            }
        }

        private IEnumerator CloseChildrenAsync(bool instant)
        {
            using CoroutineHandle handle = CoroutineHandle.GetFromPool(this);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsOpen || children[i].IsOpening)
                {
                    children[i].wasOpen = true;
                    handle.StartCoroutine(children[i].CloseAsync(instant));
                }
                else
                {
                    children[i].wasClosed = false;
                }
            }

            yield return handle;
        }

        private IEnumerator CloseSelfAsync(bool instant)
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

        private IEnumerator FocusInternalAsync(bool instant = false)
        {
            if (!IsClosed && !IsFocused)
            {
                EnsureActive();
                OnWillFocus();

                focus = FocusState.Focusing;
                yield return FocusSelfAsync(instant);
                yield return FocusChildrenAsync(instant);
                focus = FocusState.Focused;

                ResetAllTriggers();
                OnFocus();
                onFocused.Invoke(this);
                OnAnyFocused.Invoke(this);
            }
        }

        private IEnumerator FocusSelfAsync(bool instant)
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

        private IEnumerator FocusChildrenAsync(bool instant)
        {
            using CoroutineHandle handle = CoroutineHandle.GetFromPool(this);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].wasFocused)
                {
                    handle.StartCoroutine(children[i].FocusAsync(instant));
                }
            }

            yield return handle;
        }

        private IEnumerator UnfocusInternalAsync(bool instant = false)
        {
            if (!IsClosed && !IsUnfocused)
            {
                EnsureActive();
                OnWillUnfocus();

                focus = FocusState.Unfocusing;
                yield return UnfocusSelfAsync(instant);
                yield return UnfocusChildrenAsync(instant);
                focus = FocusState.Unfocused;

                OnUnfocus();
                onUnfocused.Invoke(this);
                OnAnyUnfocused.Invoke(this);
            }
        }

        private IEnumerator UnfocusSelfAsync(bool instant)
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

        private IEnumerator UnfocusChildrenAsync(bool instant)
        {
            using CoroutineHandle handle = CoroutineHandle.GetFromPool(this);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsFocused || children[i].IsFocusing)
                {
                    children[i].wasFocused = true;
                    handle.StartCoroutine(children[i].UnfocusAsync(instant));
                }
                else
                {
                    children[i].wasFocused = true;
                }
            }

            yield return handle;
        }

        private void EnsureActive()
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
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

        protected virtual void OnValidate()
        {

        }
    }

    public abstract class UIT : UI, IRefreshableWeak
    {
        public abstract UIAssetT WeakAsset { get; set; }

        public abstract void RefreshWeak(object data);
    }

    public abstract class UI<T> : UIT, IRefreshable<T>
    {
        public override UIAssetT WeakAsset { get => asset; set => asset = value as UIAsset<T>; }
        public UIAsset<T> Asset { get => asset; set => asset = value; }

        [SerializeField] private UIAsset<T> asset;

        protected T data;

        public override sealed void RefreshWeak(object data)
        {
            Refresh((T)data);
        }

        public virtual void Refresh(T data)
        {
            this.data = data;
            MarkLayoutForRebuild();
        }
    }
}