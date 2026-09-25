using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// XRHeadCollisionDriver
/// 
/// Purpose:
/// 1. Constrains the XR Origin rig's Y position to constantly remain at a fixed height
///    (default -0.5m) so the player's eye level is comfortably aligned with the cupboards,
///    and prevents the CharacterController from popping up onto the floor.
/// 2. Aligns the CharacterController capsule so its bottom rests exactly on the floor (Y = 0)
///    while its top extends up to the player's camera height.
/// 3. In room-scale VR and XR Device Simulator, tracks horizontal head movement each frame
///    and uses CharacterController.Move() to detect physical obstacles (furniture and walls).
/// 4. If an obstacle blocks movement, offsets the XR Origin so the camera stops right at
///    the obstacle surface rather than clipping through into the interior geometry.
/// 5. Seamlessly handles teleportation by detecting external position jumps and updating
///    tracking baselines without false pushbacks.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(150)] // Runs after TrackedPoseDriver updates camera position
public class XRHeadCollisionDriver : MonoBehaviour
{
    [Header("Height Locking")]
    [Tooltip("Constantly locked world Y position for the XR Origin rig.")]
    [SerializeField] private float m_FixedY = -0.5f;

    [Header("Controller Settings")]
    [Tooltip("Default height of the character controller capsule.")]
    [SerializeField] private float m_DefaultHeight = 1.8f;

    [Tooltip("Radius of the character controller capsule.")]
    [SerializeField] private float m_Radius = 0.2f;

    [Tooltip("Minimum allowed capsule height when ducking.")]
    [SerializeField] private float m_MinHeight = 0.8f;

    [Tooltip("Maximum allowed capsule height.")]
    [SerializeField] private float m_MaxHeight = 2.2f;

    private CharacterController m_CharacterController;
    private XROrigin m_XROrigin;
    private Camera m_Camera;

    private Vector3 m_LastOriginPos;
    private Vector3 m_LastCamLocalPos;
    private bool m_IsInitialized;

    public float FixedY
    {
        get => m_FixedY;
        set => m_FixedY = value;
    }

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_XROrigin = GetComponent<XROrigin>();

        // Lock Y position immediately on awake
        Vector3 pos = transform.position;
        pos.y = m_FixedY;
        transform.position = pos;

        if (m_CharacterController != null)
        {
            m_CharacterController.height = m_DefaultHeight;
            m_CharacterController.radius = m_Radius;
            // Floor is at world Y = 0. When rig is at m_FixedY (-0.5),
            // floor in local space is at -m_FixedY (+0.5).
            float floorLocalY = -m_FixedY;
            m_CharacterController.center = new Vector3(0f, floorLocalY + m_DefaultHeight * 0.5f, 0f);
            m_CharacterController.skinWidth = 0.05f;
            m_CharacterController.minMoveDistance = 0.001f;
            m_CharacterController.stepOffset = 0.3f;
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Enforce fixed Y
        Vector3 pos = transform.position;
        pos.y = m_FixedY;
        transform.position = pos;

        if (m_Camera == null)
        {
            if (m_XROrigin != null && m_XROrigin.Camera != null)
            {
                m_Camera = m_XROrigin.Camera;
            }
            else
            {
                m_Camera = GetComponentInChildren<Camera>();
            }
        }

        if (m_Camera != null)
        {
            m_LastOriginPos = transform.position;
            m_LastCamLocalPos = m_Camera.transform.localPosition;
            m_IsInitialized = true;
        }
    }

    private void Update()
    {
        if (!m_IsInitialized || m_Camera == null || m_CharacterController == null)
        {
            Initialize();
            return;
        }

        // Always lock Y position to m_FixedY
        Vector3 currentOriginPos = transform.position;
        currentOriginPos.y = m_FixedY;
        transform.position = currentOriginPos;

        Vector3 currentCamLocalPos = m_Camera.transform.localPosition;

        // Dynamically match capsule height to current head height
        float camY = currentCamLocalPos.y;
        float targetHeight = Mathf.Clamp(camY > 0.2f ? camY : m_DefaultHeight, m_MinHeight, m_MaxHeight);
        m_CharacterController.height = targetHeight;

        // In local coordinates, the floor (world Y=0) is at -m_FixedY (+0.5).
        // Center the capsule so its bottom rests on the floor (no floor penetration).
        float floorLocalY = -m_FixedY;
        float centerLocalY = floorLocalY + targetHeight * 0.5f;

        // Detect if the XR Origin was moved by an external system (e.g. Teleportation).
        // We compare only horizontal distance to avoid any vertical teleport discrepancy.
        Vector2 originXZ = new Vector2(currentOriginPos.x, currentOriginPos.z);
        Vector2 lastOriginXZ = new Vector2(m_LastOriginPos.x, m_LastOriginPos.z);
        if (Vector2.Distance(originXZ, lastOriginXZ) > 0.01f)
        {
            m_LastOriginPos = currentOriginPos;
            m_LastCamLocalPos = currentCamLocalPos;
            m_CharacterController.center = new Vector3(currentCamLocalPos.x, centerLocalY, currentCamLocalPos.z);
            return;
        }

        // Calculate physical/simulated head movement delta in world space (horizontal only)
        Vector3 deltaLocal = currentCamLocalPos - m_LastCamLocalPos;
        Vector3 deltaWorld = transform.TransformDirection(deltaLocal);
        deltaWorld.y = 0f; // Purely horizontal collision checking; rig Y is constant

        // Filter out micro-jitter (< 1e-6) and extreme frame jumps (> 1.5m)
        if (deltaWorld.sqrMagnitude > 1e-6f && deltaWorld.magnitude < 1.5f)
        {
            // Position the capsule at the previous head position before sweeping
            m_CharacterController.center = new Vector3(m_LastCamLocalPos.x, centerLocalY, m_LastCamLocalPos.z);

            // Move the CharacterController horizontally by the head's delta
            Vector3 posBefore = transform.position;
            posBefore.y = m_FixedY;
            m_CharacterController.Move(deltaWorld);

            Vector3 actualMove = transform.position - posBefore;
            actualMove.y = 0f; // Ignore any vertical push from PhysX

            // If an obstacle stopped or deflected movement (actualMove != deltaWorld),
            // offset the XR Origin horizontally so the camera stops at the collider surface.
            Vector3 pushBack = actualMove - deltaWorld;
            pushBack.y = 0f;

            Vector3 newPos = posBefore + pushBack;
            newPos.y = m_FixedY; // Guarantee Y remains constantly -0.5
            transform.position = newPos;
            Physics.SyncTransforms();

            // Place capsule center at new camera location
            m_CharacterController.center = new Vector3(currentCamLocalPos.x, centerLocalY, currentCamLocalPos.z);
        }
        else
        {
            // Align capsule center with camera when standing still
            m_CharacterController.center = new Vector3(currentCamLocalPos.x, centerLocalY, currentCamLocalPos.z);
        }

        m_LastOriginPos = transform.position;
        m_LastCamLocalPos = currentCamLocalPos;
    }

    private void LateUpdate()
    {
        // Enforce fixed Y after all other updates and locomotion phases
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.y - m_FixedY) > 0.0001f)
        {
            pos.y = m_FixedY;
            transform.position = pos;
            m_LastOriginPos = pos;
        }
    }
}
