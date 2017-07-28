using UnityEngine;

namespace HouraiTeahouse.HouraiInput {

    public class UnityButtonSource : InputSource {

        static string[,] _buttonQueries;
        readonly int _buttonId;

        public UnityButtonSource(int buttonId) {
            _buttonId = buttonId;
            SetupButtonQueries();
        }

        public float GetValue(InputDevice inputDevice) { return GetState(inputDevice) ? 1.0f : 0.0f; }

        public bool GetState(InputDevice inputDevice) {
            var unityInputDevice = inputDevice as UnityInputDevice;
            if (unityInputDevice == null)
                return false;
            int joystickId = unityInputDevice.JoystickId;
            string buttonKey = GetButtonKey(joystickId, _buttonId);
            return Input.GetKey(buttonKey);
        }

        static void SetupButtonQueries() {
            if (_buttonQueries != null)
                return;
            _buttonQueries = new string[UnityInputDevice.MaxDevices, UnityInputDevice.MaxButtons];

            for (var joystickId = 1; joystickId <= UnityInputDevice.MaxDevices; joystickId++)
                for (var buttonId = 0; buttonId < UnityInputDevice.MaxButtons; buttonId++)
                    _buttonQueries[joystickId - 1, buttonId] = "joystick {0} button {1}".With(joystickId, buttonId);
        }

        static string GetButtonKey(int joystickId, int buttonId) { return _buttonQueries[joystickId - 1, buttonId]; }

    }

}