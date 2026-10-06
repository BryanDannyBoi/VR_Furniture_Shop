using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

/// <summary>
/// Reusable interactive hinged door for room doorways.
/// Features:
/// 1. Trigger zone in front of the door: displays a world-space "Open" prompt when player approaches.
/// 2. Hides the prompt if the player leaves the trigger zone without opening.
/// 3. Opens smoothly around a hinge pivot when selected via XR Ray Interactor.
/// </summary>
public class InteractiveDoor : MonoBehaviour
{
    [Header("Door Panel & Hinge")]
    [Tooltip("The transform that rotates when the door opens. If null, uses this GameObject.")]
    [SerializeField] private Transform m_HingeTransform;

    [Tooltip("Open angle relative to closed rotation around local Y-axis.")]
    [SerializeField] private float m_OpenAngle = 90f;

    [Tooltip("Duration in seconds for the door swing animation.")]
    [SerializeField] private float m_OpenDuration = 1.0f;

    [Header("UI Prompt")]
    [Tooltip("World-space Canvas containing the prompt UI.")]
    [SerializeField] private Canvas m_PromptCanvas;

    [Tooltip("Text component showing the interaction prompt.")]
    [SerializeField] private TextMeshProUGUI m_PromptText;

    [Tooltip("Default prompt message.")]
    [SerializeField] private string m_PromptMessage = "Open [Ray Select]";

    [Header("Interaction")]
    [SerializeField] private XRSimpleInteractable m_Interactable;

    private Quaternion m_ClosedRotation;
    private Quaternion m_TargetOpenRotation;
    private bool m_IsOpen = false;
    private bool m_IsAnimating = false;
    private int m_PlayerTriggerCount = 0;

    private void Awake()
    {
        if (m_HingeTransform == null)
            m_HingeTransform = transform;

        m_ClosedRotation = m_HingeTransform.localRotation;
        m_TargetOpenRotation = m_ClosedRotation * Quaternion.Euler(0f, m_OpenAngle, 0f);

        if (m_PromptCanvas != null)
            m_PromptCanvas.gameObject.SetActive(false);

        if (m_PromptText != null)
            m_PromptText.text = m_PromptMessage;

        if (m_Interactable == null)
            m_Interactable = GetComponentInChildren<XRSimpleInteractable>();

        if (m_Interactable != null)
        {
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDestroy()
    {
        if (m_Interactable != null)
        {
            m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            m_PlayerTriggerCount++;
            if (!m_IsOpen && m_PromptCanvas != null)
            {
                m_PromptCanvas.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            m_PlayerTriggerCount = Mathf.Max(0, m_PlayerTriggerCount - 1);
            if (m_PlayerTriggerCount == 0 && m_PromptCanvas != null)
            {
                m_PromptCanvas.gameObject.SetActive(false);
            }
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
            return true;

        if (other.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>() != null)
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        return false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!m_IsOpen && !m_IsAnimating)
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (m_IsOpen || m_IsAnimating)
            return;

        StartCoroutine(AnimateOpen());
    }

    private IEnumerator AnimateOpen()
    {
        m_IsAnimating = true;

        if (m_PromptCanvas != null)
            m_PromptCanvas.gameObject.SetActive(false);

        float elapsed = 0f;
        Quaternion startRot = m_HingeTransform.localRotation;

        while (elapsed < m_OpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / m_OpenDuration);
            m_HingeTransform.localRotation = Quaternion.Slerp(startRot, m_TargetOpenRotation, t);
            yield return null;
        }

        m_HingeTransform.localRotation = m_TargetOpenRotation;
        m_IsOpen = true;
        m_IsAnimating = false;

        if (m_Interactable != null)
            m_Interactable.enabled = false;
    }
}
