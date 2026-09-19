using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    /// <summary>Device input only; physics remains independent of the binding scheme.</summary>
    public sealed class VehicleInput : MonoBehaviour
    {
        public float Throttle { get; private set; }
        public float BrakeReverse { get; private set; }
        public float Steering { get; private set; }
        bool resetRequested;
        InputAction throttle, brake, steering, reset;
        public bool UsingGamepad { get; private set; }
        public string ResetControlLabel
        {
            get
            {
                if(!UsingGamepad)return reset.GetBindingDisplayString(1);
                string label=Gamepad.current?.buttonNorth.displayName;
                return string.IsNullOrEmpty(label)||label=="Button North"?"Y":label;
            }
        }
        void Used(InputAction.CallbackContext context) { UsingGamepad=context.control.device is Gamepad; }

        void Awake()
        {
            throttle = new InputAction("Throttle", InputActionType.Value);
            throttle.AddBinding("<Gamepad>/rightTrigger");
            throttle.AddBinding("<Keyboard>/w");
            throttle.AddBinding("<Keyboard>/upArrow");
            brake = new InputAction("BrakeReverse", InputActionType.Value);
            brake.AddBinding("<Gamepad>/leftTrigger");
            brake.AddBinding("<Keyboard>/s");
            brake.AddBinding("<Keyboard>/downArrow");
            steering = new InputAction("Steering", InputActionType.Value);
            steering.AddBinding("<Gamepad>/leftStick/x").WithProcessor("axisDeadzone(min=0.12,max=0.95)");
            steering.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
            steering.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            reset = new InputAction("Reset", InputActionType.Button);
            reset.AddBinding("<Gamepad>/buttonNorth");
            reset.AddBinding("<Keyboard>/r");
            throttle.performed+=Used; brake.performed+=Used; steering.performed+=Used; reset.performed+=Used;
        }

        void OnEnable() { throttle.Enable(); brake.Enable(); steering.Enable(); reset.Enable(); }
        void Update()
        {
            Throttle = throttle.ReadValue<float>();
            BrakeReverse = brake.ReadValue<float>();
            Steering = steering.ReadValue<float>();
            resetRequested |= reset.WasPressedThisFrame();
        }
        public bool ConsumeReset() { bool result = resetRequested; resetRequested = false; return result; }
        void OnDisable()
        {
            throttle.Disable(); brake.Disable(); steering.Disable(); reset.Disable();
            Throttle = BrakeReverse = Steering = 0; resetRequested = false;
        }
        void OnDestroy() { throttle.Dispose(); brake.Dispose(); steering.Dispose(); reset.Dispose(); }
    }
}
