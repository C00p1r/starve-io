using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace StarveIO.Input
{
    // 將檔案名稱改為與你的資源匹配
    [CreateAssetMenu(fileName = "InputReader", menuName = "StarveIO/Input/InputReader")]
    public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions // 使用你找到的介面名
    {
        // 定義事件供其他系統訂閱
        public event UnityAction<Vector2> MoveEvent = delegate { };
        public event UnityAction<Vector2> LookEvent = delegate { };
        public event UnityAction AttackEvent = delegate { };
        public event UnityAction InteractEvent = delegate { };

        private InputSystem_Actions _inputActions; // 改用你的類別名

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new InputSystem_Actions();
                _inputActions.Player.SetCallbacks(this); // 這裡對應你的 Player Action Map
            }
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        // --- 實作 IPlayerActions 介面中的方法 ---
        // 註：具體方法名稱取決於你在 Input Action 視窗中定義的 Action 名字

        public void OnLook(InputAction.CallbackContext context)
        {
            // 讀取滑鼠在螢幕上的座標 (Screen Position)
            LookEvent.Invoke(context.ReadValue<Vector2>());
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 moveVal = context.ReadValue<Vector2>();

            // 除錯用：確認放開按鍵時是否有印出 (0.0, 0.0)
            Debug.Log($"Move Input: {moveVal}, Phase: {context.phase}");

            MoveEvent.Invoke(moveVal);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                AttackEvent.Invoke();
        }

        // 如果你的 Action 叫 Interact，就會有這個方法
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                InteractEvent.Invoke();
        }

        // 注意：如果介面提示你缺少其他方法（例如 OnLook, OnJump），
        // 即使沒用到也必須寫出來，可以留空，否則會報錯。
        public void OnJump(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
    }
}