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
    private Transform firePoint;            // �Ѿ��� ���� ��ġ
    public float fireCooldown = 0.25f;      // ��Ÿ��
    private float lastFireTime;

    private void Awake()
    {
        input = new Player_InputAction();
        input.GamePlay.SetCallbacks(this);
        pv = GetComponent<PhotonView>();

        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError("[PlayerController] �ڽĿ� FirePoint ������Ʈ�� �����ϴ�.");
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

        if (firePoint == null) return;

        // �Ѿ� ���� (��� Ŭ���̾�Ʈ�� ����ȭ��)
        PhotonNetwork.Instantiate("BulletPrefab", firePoint.position, firePoint.rotation);
    }

    private void Update()
    {
        if (pv == null || !pv.IsMine) return;

        Vector3 moveDir = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!pv.IsMine) return;

        //if (collision.CompareTag("Enemy"))
        //{
        //    Debug.Log("�÷��̾� �ǰݵ�!"); // HP �ý��� ����� ���⼭ ���� ó��
        //}
    }
}
