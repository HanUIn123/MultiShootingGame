using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPunCallbacks, Player_InputAction.IGamePlayActions
{
    private ChatManager chatManager;

    private Player_InputAction input;
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 6f;

    private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.25f;
    private float lastFireTime;

    [Header("궁극기 게이지 설정")]
    [SerializeField] private float chargeSpeed = 1.5f;
    private float currentGauge = 0f;
    private bool isCharging = false;
    private bool isFevertime = false;

    [Header("궁극기 레이저")]
    [SerializeField] private string laserPrefabPath = "UltimateLaser";
    private Transform _laserSpawnPoint;

    [Header("UI 연결")]
    [HideInInspector]
    public UltimateUIManager ultimateUI;

    private PhotonView pv;

    public void InitLaserSpawn(Transform t) => _laserSpawnPoint = t;

    private void Awake()
    {
        chatManager = FindFirstObjectByType<ChatManager>();

        input = new Player_InputAction();
        input.GamePlay.SetCallbacks(this);
        pv = GetComponent<PhotonView>();

        firePoint = transform.Find("FirePoint");
        if (!firePoint)
            Debug.LogError("[PlayerController] FirePoint 없음");
        if (_laserSpawnPoint == null)
            _laserSpawnPoint = firePoint;
    }

    private void Start()
    {
        if (pv.IsMine)
        {
            if (ultimateUI == null)
            {
                ultimateUI = FindObjectOfType<UltimateUIManager>();
                if (ultimateUI == null)
                    Debug.LogWarning("[PlayerController] 궁극기 UI가 연결되지 않았습니다.");
            }
        }
    }

    private new void OnEnable()
    {
        if (pv && pv.IsMine)
            input.GamePlay.Enable();
    }

    private new void OnDisable()
    {
        if (pv && pv.IsMine)
            input.GamePlay.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || IsChatInputFocused()) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || IsChatInputFocused()) return;

        if (context.performed)
        {
            if (Time.time - lastFireTime < fireCooldown)
                return;

            lastFireTime = Time.time;

            if (firePoint)
                PhotonNetwork.Instantiate("BulletPrefab", firePoint.position, firePoint.rotation);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("BulletSound");
        }
    }

    public void OnUltimate(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || IsChatInputFocused()) return;

        if (context.started)
            isCharging = true;
        else if (context.canceled)
            isCharging = false;
    }

    private void Update()
    {
        if (!pv.IsMine || IsChatInputFocused()) return;

        Vector3 moveDir = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.Self);
        ClampPlayerPosition();

        if (isCharging && !isFevertime)
        {
            currentGauge += Time.deltaTime * chargeSpeed;
            currentGauge = Mathf.Clamp01(currentGauge);

            if (ultimateUI)
                ultimateUI.UpdateGauge(currentGauge);

            if (currentGauge >= 1f)
                TriggerFevertime();
        }
    }

    private void ClampPlayerPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -3.5f, 3.5f);
        pos.y = Mathf.Clamp(pos.y, -4.6f, 4.6f);
        transform.position = pos;
    }

    private void TriggerFevertime()
    {
        isFevertime = true;

        if (ultimateUI)
        {
            ultimateUI.onCutInFinished = FinishFevertimeAndFire;
            ultimateUI.PlayCutIn();
        }
        else
        {
            Debug.LogWarning("[PlayerController] ultimateUI 없음 → 컷인 없이 바로 발사");
            FinishFevertimeAndFire();
        }
    }

    private void FinishFevertimeAndFire()
    {
        if (!isFevertime) return;
        isFevertime = false;

        if (ultimateUI)
            ultimateUI.UpdateGauge(0f);

        currentGauge = 0f;

        FireUltimateLaser();
    }

    private void FireUltimateLaser()
    {
        if (!_laserSpawnPoint || !pv.IsMine) return;

        PhotonNetwork.Instantiate(laserPrefabPath, _laserSpawnPoint.position, _laserSpawnPoint.rotation);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("FireSound");

        Debug.Log("[궁극기] UltimateLaser 발사 완료");
    }

    public void OnChargeButtonPressed()
    {
        isCharging = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnChargeButtonReleased()
    {
        isCharging = false;
    }

    private bool IsChatInputFocused()
    {
        return EventSystem.current != null &&
               EventSystem.current.currentSelectedGameObject == chatManager?.chatInputField.gameObject;
    }
}
