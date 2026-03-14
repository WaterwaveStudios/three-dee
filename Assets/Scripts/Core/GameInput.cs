using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ThreeDee.Core
{
    public class GameInput : MonoBehaviour
    {
        public static GameInput Instance { get; private set; }

        // Actions
        private InputAction _moveAction;
        private InputAction _scrollAction;
        private InputAction _pointerPositionAction;
        private InputAction _pointerDeltaAction;
        private InputAction _rightClickAction;
        private InputAction _middleClickAction;
        private InputAction _altKeyAction;
        private InputAction _leftClickAction;

        // Public read state
        public Vector2 MoveInput => _moveAction.ReadValue<Vector2>();
        public float ScrollInput => _scrollAction.ReadValue<Vector2>().y;
        public Vector2 PointerPosition => _pointerPositionAction.ReadValue<Vector2>();
        public Vector2 PointerDelta => _pointerDeltaAction.ReadValue<Vector2>();
        public bool IsRightClickHeld => _rightClickAction.IsPressed();
        public bool IsMiddleClickHeld => _middleClickAction.IsPressed();
        public bool IsAltHeld => _altKeyAction.IsPressed();
        public bool IsLeftClickHeld => _leftClickAction.IsPressed();

        // Pan state
        public bool IsPanning => (IsRightClickHeld || IsMiddleClickHeld || (IsLeftClickHeld && IsAltHeld));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance != null) return;
            var go = new GameObject("GameInput");
            Instance = go.AddComponent<GameInput>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnhancedTouchSupport.Enable();
            CreateActions();
        }

        private void CreateActions()
        {
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            _scrollAction = new InputAction("Scroll", InputActionType.Value,
                binding: "<Mouse>/scroll");

            _pointerPositionAction = new InputAction("PointerPosition", InputActionType.Value,
                binding: "<Pointer>/position");

            _pointerDeltaAction = new InputAction("PointerDelta", InputActionType.Value,
                binding: "<Pointer>/delta");

            _rightClickAction = new InputAction("RightClick", InputActionType.Button,
                binding: "<Mouse>/rightButton");

            _middleClickAction = new InputAction("MiddleClick", InputActionType.Button,
                binding: "<Mouse>/middleButton");

            _altKeyAction = new InputAction("Alt", InputActionType.Button,
                binding: "<Keyboard>/leftAlt");

            _leftClickAction = new InputAction("LeftClick", InputActionType.Button,
                binding: "<Mouse>/leftButton");

            _moveAction.Enable();
            _scrollAction.Enable();
            _pointerPositionAction.Enable();
            _pointerDeltaAction.Enable();
            _rightClickAction.Enable();
            _middleClickAction.Enable();
            _altKeyAction.Enable();
            _leftClickAction.Enable();
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _scrollAction?.Dispose();
            _pointerPositionAction?.Dispose();
            _pointerDeltaAction?.Dispose();
            _rightClickAction?.Dispose();
            _middleClickAction?.Dispose();
            _altKeyAction?.Dispose();
            _leftClickAction?.Dispose();

            if (Instance == this) Instance = null;
        }
    }
}
