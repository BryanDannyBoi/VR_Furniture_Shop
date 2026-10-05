using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// CupboardInteraction
/// 
/// Reusable XR interaction script for showroom cupboards (Cupboard_0 to Cupboard_4).
/// Works alongside XRSimpleInteractable to provide:
/// 1. Hover Highlight: Slightly brightens the cupboard materials and adds a soft glow
///    when the player aims the controller ray pointer at it; cleanly reverts when hover exits.
/// 2. Select Auto-Rotation: Pulling the trigger while pointing at the cupboard toggles
///    slow 360-degree rotation in place (30 deg/sec around the world Y vertical axis)
///    so the customer can inspect all sides.
/// 3. Toggle Stop & Reset: Pulling the trigger again stops rotation and snaps the cupboard
///    back to its initial showroom orientation.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class CupboardInteraction : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second around the world vertical axis (Y-axis).")]
    [SerializeField] private float m_RotationSpeed = 30f;

    [Header("Hover Highlight Settings")]
    [Tooltip("Brightness multiplier applied to material base color when hovered.")]
    [SerializeField] private float m_BrightnessMultiplier = 1.3f;

    [Tooltip("Soft emission color added during hover to give an interactive glow.")]
    [SerializeField] private Color m_HoverEmission = new Color(0.12f, 0.12f, 0.16f);

    // References and state
    private XRSimpleInteractable m_Interactable;
    private Quaternion m_InitialRotation;
    private bool m_IsRotating = false;

    // Cache renderers and their original material properties to revert cleanly without leaks
    private struct MaterialData
    {
        public Material material;
        public Color originalBaseColor;
        public Color originalEmission;
        public bool hasBaseColor;
        public bool hasColor;
        public bool hasEmission;
    }

    private readonly List<MaterialData> m_MaterialList = new List<MaterialData>();

    // Shader property IDs for URP / Standard shaders
    private static readonly int s_BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int s_ColorId = Shader.PropertyToID("_Color");
    private static readonly int s_EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        m_Interactable = GetComponent<XRSimpleInteractable>();
        m_InitialRotation = transform.rotation;

        // Cache all child renderers and their material properties
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                MaterialData data = new MaterialData
                {
                    material = mat,
                    hasBaseColor = mat.HasProperty(s_BaseColorId),
                    hasColor = mat.HasProperty(s_ColorId),
                    hasEmission = mat.HasProperty(s_EmissionColorId)
                };

                if (data.hasBaseColor)
                    data.originalBaseColor = mat.GetColor(s_BaseColorId);
                else if (data.hasColor)
                    data.originalBaseColor = mat.GetColor(s_ColorId);
                else
                    data.originalBaseColor = Color.white;

                if (data.hasEmission)
                    data.originalEmission = mat.GetColor(s_EmissionColorId);

                m_MaterialList.Add(data);
            }
        }
    }

    private void OnEnable()
    {
        if (m_Interactable != null)
        {
            m_Interactable.firstHoverEntered.AddListener(OnHoverEntered);
            m_Interactable.lastHoverExited.AddListener(OnHoverExited);
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDisable()
    {
        if (m_Interactable != null)
        {
            m_Interactable.firstHoverEntered.RemoveListener(OnHoverEntered);
            m_Interactable.lastHoverExited.RemoveListener(OnHoverExited);
            m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        // Revert materials and state if disabled
        SetHighlight(false);
    }

    private void Update()
    {
        if (m_IsRotating)
        {
            // Rotate around the world vertical axis (Vector3.up) so the cupboard spins
            // horizontally in place on its showroom pedestal, regardless of the FBX import axis.
            transform.Rotate(Vector3.up, m_RotationSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// Called when the player's controller ray pointer begins hovering over this cupboard.
    /// </summary>
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        SetHighlight(true);
    }

    /// <summary>
    /// Called when the player's controller ray pointer stops pointing at this cupboard.
    /// </summary>
    private void OnHoverExited(HoverExitEventArgs args)
    {
        SetHighlight(false);
    }

    /// <summary>
    /// Called when the player pulls the trigger (Select action) while pointing at this cupboard.
    /// Toggles auto-rotation on and off.
    /// </summary>
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        m_IsRotating = !m_IsRotating;

        if (!m_IsRotating)
        {
            // Reset back to original showroom orientation
            transform.rotation = m_InitialRotation;
        }
    }

    /// <summary>
    /// Applies or reverts the hover highlight tint and emission glow.
    /// </summary>
    private void SetHighlight(bool highlighted)
    {
        for (int i = 0; i < m_MaterialList.Count; i++)
        {
            MaterialData data = m_MaterialList[i];
            if (data.material == null) continue;

            if (highlighted)
            {
                Color highlightColor = data.originalBaseColor * m_BrightnessMultiplier;
                if (data.hasBaseColor)
                    data.material.SetColor(s_BaseColorId, highlightColor);
                if (data.hasColor)
                    data.material.SetColor(s_ColorId, highlightColor);

                if (data.hasEmission)
                {
                    data.material.EnableKeyword("_EMISSION");
                    data.material.SetColor(s_EmissionColorId, data.originalEmission + m_HoverEmission);
                }
            }
            else
            {
                if (data.hasBaseColor)
                    data.material.SetColor(s_BaseColorId, data.originalBaseColor);
                if (data.hasColor)
                    data.material.SetColor(s_ColorId, data.originalBaseColor);

                if (data.hasEmission)
                {
                    data.material.SetColor(s_EmissionColorId, data.originalEmission);
                }
            }
        }
    }
}
