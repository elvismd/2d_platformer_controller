using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    PlayerControls playerControls;

    [SerializeField, ReadOnly] float movementInput;
    [SerializeField, ReadOnly] bool jump;
    [SerializeField, ReadOnly] bool dash;
    [SerializeField, ReadOnly] bool walk;
    [SerializeField, ReadOnly] bool _jumpDown;

    public float MovementInput => movementInput;
    public bool Walk => walk;
    public bool Jump => jump;
    public bool JumpDown => _jumpDown;
    public bool Dash => dash;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Move.performed += i => movementInput = i.ReadValue<float>();
            playerControls.PlayerMovement.Move.canceled += i => movementInput = i.ReadValue<float>();

            playerControls.PlayerMovement.Jump.performed += i => _jumpDown = i.ReadValueAsButton();           
            playerControls.PlayerMovement.Jump.canceled += i => _jumpDown = i.ReadValueAsButton();

            playerControls.PlayerMovement.Dash.performed += i => dash = i.ReadValueAsButton();
            playerControls.PlayerMovement.Dash.canceled += i => dash = i.ReadValueAsButton();

            playerControls.PlayerMovement.Walk.performed += i => walk = i.ReadValueAsButton();
            playerControls.PlayerMovement.Walk.canceled += i => walk = i.ReadValueAsButton();
        }

        playerControls.Enable();
    }

    private void Update()
    {
        jump = playerControls.PlayerMovement.Jump.WasPressedThisFrame();
        dash = playerControls.PlayerMovement.Dash.WasPressedThisFrame();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
