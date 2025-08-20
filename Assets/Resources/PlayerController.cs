using UnityEngine;
using Photon.Pun; // << 이거 중요!

public class PlayerController : MonoBehaviour, Player_InputAction.IGamePlayActions
{
    private Player_InputAction input;
    private Vector2 moveInput;
    public float moveSpeed = 6f;

    private PhotonView pv;

    private void Awake()
    {
        input = new Player_InputAction();
        input.GamePlay.SetCallbacks(this);

        pv = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        if (pv != null && pv.IsMine)
        {
            input.GamePlay.Enable();
        }
    }

    private void OnDisable()
    {
        if (pv != null && pv.IsMine)
        {
            input.GamePlay.Disable();
        }
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (pv == null || !pv.IsMine) return; // 내가 조작하는 오브젝트 아니면 무시
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (pv == null || !pv.IsMine) return;

        Vector3 moveDir = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }
}
