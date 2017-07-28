using System.Collections.Generic;
using HouraiTeahouse.HouraiInput;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HouraiTeahouse.SmashBrew.UI {

    public interface IPlayerClickable {

        void Click(Player player);

    }

    public class CharacterSelectInputModule : PointerInputModule {

        [SerializeField]
        InputTarget _cancel = InputTarget.Action2;

        [SerializeField]
        InputTarget _changeLeft = InputTarget.Action3;

        [SerializeField]
        InputTarget _changeRight = InputTarget.Action4;

        PointerEventData _eventData;

        [SerializeField]
        InputTarget _horizontal = InputTarget.LeftStickX;

        List<PlayerPointer> _pointers;

        [SerializeField]
        InputTarget _submit = InputTarget.Action1;

        [SerializeField]
        InputTarget _vertical = InputTarget.LeftStickY;

        internal static CharacterSelectInputModule Instance { get; private set; }

        internal void AddPointer(PlayerPointer pointer) {
            if (pointer)
                _pointers.Add(pointer);
        }

        internal void RemovePointer(PlayerPointer pointer) { _pointers.Remove(pointer); }

        protected override void Awake() {
            base.Awake();
            Instance = this;
            _pointers = new List<PlayerPointer>();
        }

        public override void Process() {
            for (var i = 0; i < _pointers.Count; i++) {
                PlayerPointer pointer = _pointers[i];
                Player player = pointer.Player;
                InputDevice controller = player.Controller;
                if (controller == null)
                    continue;
                // Move the controller
                pointer.Move(new Vector2(controller[_horizontal], controller[_vertical]));
                ProcessPointerSubmit(pointer, i, controller);
                CharacterChange(player, controller);
            }
        }

        void ProcessPointerSubmit(PlayerPointer pointer, int i, InputDevice controller) {
            GetPointerData(i, out _eventData, true);
            _eventData.position = Camera.main.WorldToScreenPoint(pointer.transform.position);
            EventSystem.current.RaycastAll(_eventData, m_RaycastResultCache);
            RaycastResult result = FindFirstRaycast(m_RaycastResultCache);
            ProcessMove(_eventData);
            var success = false;
            _eventData.clickCount = 0;
            if (controller[_submit].WasPressed) {
                _eventData.pressPosition = _eventData.position;
                _eventData.clickCount = 1;
                _eventData.clickTime = Time.unscaledTime;
                _eventData.pointerPressRaycast = result;
                if (m_RaycastResultCache.Count > 0) {
                    _eventData.selectedObject = result.gameObject;
                    _eventData.pointerPress = ExecuteEvents.ExecuteHierarchy(result.gameObject,
                        _eventData,
                        ExecuteEvents.submitHandler);
                    _eventData.rawPointerPress = result.gameObject;
                    foreach (IPlayerClickable clickable in
                        result.gameObject.GetComponentsInParent<IPlayerClickable>())
                        clickable.Click(pointer.Player);
                    success = true;
                }
            }
            if (!success) {
                _eventData.clickCount = 0;
                _eventData.eligibleForClick = false;
                _eventData.pointerPress = null;
                _eventData.rawPointerPress = null;
            }
        }

        void CharacterChange(Player player, InputDevice controller) {
            PlayerSelection selection = player.Selection;
            if (!selection.Character)
                return;
            if (controller[_changeRight].WasPressed)
                selection.Pallete--;
            if (controller[_changeLeft].WasPressed)
                selection.Pallete++;
            if (controller[_cancel].WasPressed) {
            }
        }

    }

}