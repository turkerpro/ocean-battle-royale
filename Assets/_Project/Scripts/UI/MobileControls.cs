using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace OceanBattleRoyale.UI
{
    public class MobileControls : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _leftStickArea;
        [SerializeField] private RectTransform _leftStickKnob;
        [SerializeField] private RectTransform _rightStickArea;
        [SerializeField] private RectTransform _rightStickKnob;
        [SerializeField] private GameObject _fireButton;
        [SerializeField] private GameObject _mineButton;
        [SerializeField] private GameObject _weaponSwitchButton;

        [Header("Settings")]
        [SerializeField] private float _stickRadius = 80f;
        [SerializeField] private float _deadZone = 0.15f;

        private Vector2 _moveInput;
        private Vector2 _aimInput;
        private bool _firePressed;
        private bool _minePressed;
        private int _pointerIdLeft = -1;
        private int _pointerIdRight = -1;
        private int _pointerIdFire = -1;
        private int _pointerIdMine = -1;
        private int _pointerIdWeapon = -1;

        public Vector2 MoveInput => _moveInput;
        public Vector2 AimInput => _aimInput;
        public bool FirePressed => _firePressed;
        public bool MinePressed => _minePressed;
        public bool WeaponSwitchPressed { get; private set; }

        private void Awake()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            gameObject.SetActive(false);
#endif
        }

        private void Update()
        {
            ProcessTouchInput();
            ProcessMouseInput(); // Editor testing
        }

        private void ProcessTouchInput()
        {
            if (Touchscreen.current == null) return;

            var touches = Touchscreen.current.touches;
            _firePressed = false;
            _minePressed = false;
            WeaponSwitchPressed = false;

            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (!touch.isInProgress) continue;

                Vector2 pos = touch.position.ReadValue();

                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(_leftStickArea, pos))
                    {
                        _pointerIdLeft = touch.touchId.ReadValue();
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(_rightStickArea, pos))
                    {
                        _pointerIdRight = touch.touchId.ReadValue();
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(_fireButton.GetComponent<RectTransform>(), pos))
                    {
                        _pointerIdFire = touch.touchId.ReadValue();
                        _firePressed = true;
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(_mineButton.GetComponent<RectTransform>(), pos))
                    {
                        _pointerIdMine = touch.touchId.ReadValue();
                        _minePressed = true;
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(_weaponSwitchButton.GetComponent<RectTransform>(), pos))
                    {
                        _pointerIdWeapon = touch.touchId.ReadValue();
                        WeaponSwitchPressed = true;
                    }
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                         touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    int id = touch.touchId.ReadValue();

                    if (id == _pointerIdLeft)
                    {
                        UpdateStick(_leftStickArea, _leftStickKnob, pos, ref _moveInput);
                    }
                    else if (id == _pointerIdRight)
                    {
                        UpdateStick(_rightStickArea, _rightStickKnob, pos, ref _aimInput);
                    }
                    else if (id == _pointerIdFire)
                    {
                        _firePressed = true;
                    }
                    else if (id == _pointerIdMine)
                    {
                        _minePressed = true;
                    }
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    int id = touch.touchId.ReadValue();

                    if (id == _pointerIdLeft)
                    {
                        _pointerIdLeft = -1;
                        _moveInput = Vector2.zero;
                        _leftStickKnob.anchoredPosition = Vector2.zero;
                    }
                    else if (id == _pointerIdRight)
                    {
                        _pointerIdRight = -1;
                        _aimInput = Vector2.zero;
                        _rightStickKnob.anchoredPosition = Vector2.zero;
                    }
                    else if (id == _pointerIdFire)
                    {
                        _pointerIdFire = -1;
                    }
                    else if (id == _pointerIdMine)
                    {
                        _pointerIdMine = -1;
                    }
                    else if (id == _pointerIdWeapon)
                    {
                        _pointerIdWeapon = -1;
                    }
                }
            }
        }

        private void ProcessMouseInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Mouse.current == null) return;

            _moveInput = Vector2.zero;
            _aimInput = Vector2.zero;
            _firePressed = false;
            _minePressed = false;
            WeaponSwitchPressed = false;

            if (Keyboard.current.wKey.isPressed) _moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) _moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) _moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) _moveInput.x += 1;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            _aimInput = (mousePos - screenCenter).normalized;

            if (Mouse.current.leftButton.isPressed) _firePressed = true;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) _minePressed = true;
            if (Keyboard.current.tabKey.wasPressedThisFrame) WeaponSwitchPressed = true;
#endif
        }

        private void UpdateStick(RectTransform area, RectTransform knob, Vector2 screenPos, ref Vector2 output)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPos, null, out Vector2 localPos);
            Vector2 direction = localPos.normalized;
            float distance = Mathf.Min(localPos.magnitude, _stickRadius);
            knob.anchoredPosition = direction * distance;

            output = direction * (distance / _stickRadius);
            if (output.magnitude < _deadZone) output = Vector2.zero;
        }

        public void SetStickAreas(RectTransform leftArea, RectTransform leftKnob, RectTransform rightArea, RectTransform rightKnob)
        {
            _leftStickArea = leftArea;
            _leftStickKnob = leftKnob;
            _rightStickArea = rightArea;
            _rightStickKnob = rightKnob;
        }

        public void SetButtons(GameObject fire, GameObject mine, GameObject weaponSwitch)
        {
            _fireButton = fire;
            _mineButton = mine;
            _weaponSwitchButton = weaponSwitch;
        }
    }
}
