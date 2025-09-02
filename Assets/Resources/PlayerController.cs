using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPunCallbacks, Player_InputAction.IGamePlayActions
{
    private Player_InputAction input;
    private Vector2 v2MoveInput;
    private PhotonView pv;

    [SerializeField] private float fMoveSpeed = 6f;
    [SerializeField] private float fFireCooldown = 0.25f;
    private float fLastFireTime;

    private Transform trFirePoint;
    private Transform trLaserSpawn;

    [Header("±Ã±Ø±â °ÔÀÌÁö ¼³Á¤")]
    [SerializeField] private float fChargeSpeed = 1.5f;
    private float fCurrentGauge = 0f;
    private bool bIsCharging = false;
    private bool bIsFevertime = false;

    [Header("±Ã±Ø±â ·¹ÀÌÀú")]
    [SerializeField] private string laserPrefabPath = "UltimateLaser";

    [Header("UI ¿¬°á")]
    [HideInInspector] public UltimateUIManager ultimateUI;
    private ChatManager chatManager;

    public void InitLaserSpawn(Transform tr) => trLaserSpawn = tr;

    private void Awake()
    {
        chatManager = FindFirstObjectByType<ChatManager>();
        input = new Player_InputAction();
        input.GamePlay.SetCallbacks(this);

        pv = GetComponent<PhotonView>();

        trFirePoint = transform.Find("FirePoint");
        if (!trFirePoint)
            Debug.LogError("[PlayerController] FirePoint ¾øÀ½");

        if (trLaserSpawn == null)
            trLaserSpawn = trFirePoint;
    }

    private void Start()
    {
        if (pv.IsMine && ultimateUI == null)
        {
            ultimateUI = FindFirstObjectByType<UltimateUIManager>();

            if (ultimateUI == null)
                Debug.LogWarning("[PlayerController] ±Ã±Ø±â UI ¿¬°á ¾ÈµÊ");
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

    private void Update()
    {
        if (!pv.IsMine || IsChatInputFocused()) 
            return;

        Vector3 v3MoveDir = new Vector3(v2MoveInput.x, v2MoveInput.y, 0f);
        transform.Translate(v3MoveDir * fMoveSpeed * Time.deltaTime, Space.Self);

        ClampPlayerPosition(); 

        if (bIsCharging && !bIsFevertime)
        {
            fCurrentGauge += Time.deltaTime * fChargeSpeed;
            fCurrentGauge = Mathf.Clamp01(fCurrentGauge);

            if (ultimateUI)
                ultimateUI.UpdateGauge(fCurrentGauge);

            if (fCurrentGauge >= 1f)
                TriggerFevertime();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || IsChatInputFocused()) 
            return;

        v2MoveInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || IsChatInputFocused()) 
            return;

        if (context.performed)
        {
            if (Time.time - fLastFireTime < fFireCooldown)
                return;

            fLastFireTime = Time.time;

            if (trFirePoint)
                PhotonNetwork.Instantiate("BulletPrefab", trFirePoint.position, trFirePoint.rotation);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("BulletSound");
        }
    }

    public void OnUltimate(InputAction.CallbackContext context)
    {
        if (!pv.IsMine || IsChatInputFocused()) 
            return;

        if (context.started)
            bIsCharging = true;
        else if (context.canceled)
            bIsCharging = false;
    }

    public void OnChargeButtonPressed()
    {
        bIsCharging = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null); 
    }

    public void OnChargeButtonReleased()
    {
        bIsCharging = false;
    }

    private void ClampPlayerPosition()
    {
        Vector3 v3Pos = transform.position;
        v3Pos.x = Mathf.Clamp(v3Pos.x, -3.5f, 3.5f);
        v3Pos.y = Mathf.Clamp(v3Pos.y, -4.6f, 4.6f);
        transform.position = v3Pos;
    }

    private void TriggerFevertime()
    {
        bIsFevertime = true;

        if (ultimateUI)
        {
            ultimateUI.onCutInFinished = FinishFevertimeAndFire;
            ultimateUI.PlayCutIn(); 
        }
        else
            FinishFevertimeAndFire();
    }

    private void FinishFevertimeAndFire()
    {
        if (!bIsFevertime) 
            return;

        bIsFevertime = false;
        fCurrentGauge = 0f;

        if (ultimateUI)
            ultimateUI.UpdateGauge(0f);

        FireUltimateLaser();
    }

    private void FireUltimateLaser()
    {
        if (!trLaserSpawn || !pv.IsMine) 
            return;

        PhotonNetwork.Instantiate(laserPrefabPath, trLaserSpawn.position, trLaserSpawn.rotation);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("FireSound");
    }

    private bool IsChatInputFocused()
    {
        return EventSystem.current != null &&
               EventSystem.current.currentSelectedGameObject == chatManager?.chatInputField.gameObject;
    }
}
