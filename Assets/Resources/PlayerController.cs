using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, Player_InputAction.IGamePlayActions
{
    private Player_InputAction input;
    private Vector2 moveInput;
    public float moveSpeed = 6f;

    private PhotonView pv;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;         // 연결할 총알 프리팹
    private Transform firePoint;            // 총알이 나갈 위치
    public float fireCooldown = 0.25f;      // 쿨타임
    private float lastFireTime;

    private void Awake()
    {
        input = new Player_InputAction();
        input.GamePlay.SetCallbacks(this);
        pv = GetComponent<PhotonView>();

        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError("[PlayerController] 자식에 FirePoint 오브젝트가 없습니다.");
        }
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

    public void OnMove(InputAction.CallbackContext context)
    {
        if (pv == null || !pv.IsMine) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed || pv == null || !pv.IsMine) return;
        if (Time.time - lastFireTime < fireCooldown) return;

        lastFireTime = Time.time;

        if (bulletPrefab == null || firePoint == null) return;

        // 총알 생성 (모든 클라이언트에 동기화됨)
        PhotonNetwork.Instantiate(bulletPrefab.name, firePoint.position, firePoint.rotation);
    }

    private void Update()
    {
        if (pv == null || !pv.IsMine) return;

        Vector3 moveDir = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.Self);
    }
}
