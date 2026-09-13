using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private PlayerMovementAdvanced pm;

    [Header("Sliding")]
    public float maxSlideTime = 0.8f;
    public float slideDrag = 8f;
    public float slideSteerAcceleration = 5f;
    private float slideTimer;

    [Header("Altura do Collider Durante o Slide")]
    public float slideHeight = 1f;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    [Header("Slide Buffer")]
    public float slideBufferTime = 0.15f;
    private float slideBufferTimer;

    [Header("Cancelamento por Pulo")]
    [Tooltip("Fração da velocidade do slide preservada ao pular no meio dele (0 = comportamento antigo de cortar tudo, 1 = mantém 100%).")]
    [Range(0f, 1f)] public float slideJumpMomentumRetention = 0.9f;

    [Tooltip("Teto de velocidade após o slide-jump. Se <= 0, usa pm.slideSpeed como teto.")]
    public float maxSlideJumpSpeed = -1f;

    private float originalStepOffset;

    private void Start()
    {
        pm = GetComponent<PlayerMovementAdvanced>();
        pm.OnJumped += HandleJumped;
        originalStepOffset = pm.controller.stepOffset;
    }

    private void OnDestroy()
    {
        if (pm != null)
            pm.OnJumped -= HandleJumped;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        HandleSlideInput();

        if (pm.sliding)
            SlidingMovement();
    }

    private void HandleSlideInput()
    {
        if (Input.GetKeyDown(slideKey) && !pm.sliding)
        {
            if (pm.grounded && (horizontalInput != 0 || verticalInput != 0))
            {
                StartSlide();
            }
            else
            {
                slideBufferTimer = slideBufferTime;
            }
        }

        if (slideBufferTimer > 0f)
        {
            slideBufferTimer -= Time.deltaTime;

            if (pm.grounded && !pm.sliding && (horizontalInput != 0 || verticalInput != 0))
            {
                StartSlide();
                slideBufferTimer = 0f;
            }
        }
    }

    private void HandleJumped()
    {
        if (!pm.sliding) return;

        StopSlide();

        float speed = pm.horizontalVelocity.magnitude;
        if (speed <= 0.01f) return;

        // Preserva uma fração do momentum do slide em vez de cortar direto
        // para sprintSpeed. O teto evita que slide-jumps encadeados acumulem
        // velocidade infinita.
        float ceiling = maxSlideJumpSpeed > 0f ? maxSlideJumpSpeed : pm.slideSpeed;
        float preservedSpeed = Mathf.Min(speed * slideJumpMomentumRetention, ceiling);

        pm.horizontalVelocity = pm.horizontalVelocity.normalized * preservedSpeed;
    }

    private void StartSlide()
    {
        pm.sliding = true;

        pm.controller.stepOffset = 0f;

        slideTimer = maxSlideTime;

        Vector3 flatForward = orientation.forward;
        flatForward.y = 0f;

        pm.horizontalVelocity = flatForward.normalized * pm.slideSpeed;
    }

    private void SlidingMovement()
    {
        pm.RequestHeight(slideHeight);

        Vector3 inputDir = (orientation.right * horizontalInput + orientation.forward * verticalInput).normalized;

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Vector3 steeredTarget = inputDir * pm.horizontalVelocity.magnitude;
            pm.horizontalVelocity = Vector3.MoveTowards(pm.horizontalVelocity, steeredTarget, slideSteerAcceleration * Time.deltaTime);
        }

        pm.horizontalVelocity = Vector3.MoveTowards(pm.horizontalVelocity, Vector3.zero, slideDrag * Time.deltaTime);

        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0f)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;

        pm.controller.stepOffset = originalStepOffset;
    }
}