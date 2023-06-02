using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    PlayerControls playerControls;

    [SerializeField]
    float movementInput;
    [SerializeField]
    bool jump;
    [SerializeField]
    bool dash;
    [SerializeField]
    bool walk;

    public float MovementInput => movementInput;
    public bool Walk { get { return walk; } set { walk = value; } }
    public bool Jump { get { return jump; } set { jump = value; } }
    public bool Dash { get { return dash; } set { dash = value; } }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Move.performed += i => movementInput = i.ReadValue<float>();
            playerControls.PlayerMovement.Move.canceled += i => movementInput = i.ReadValue<float>();

            playerControls.PlayerMovement.Jump.performed += i => jump = i.ReadValueAsButton();
            playerControls.PlayerMovement.Jump.canceled += i => jump = i.ReadValueAsButton();

            playerControls.PlayerMovement.Dash.performed += i => dash = i.ReadValueAsButton();
            playerControls.PlayerMovement.Dash.canceled += i => dash = i.ReadValueAsButton();

            playerControls.PlayerMovement.Walk.performed += i => walk = i.ReadValueAsButton();
            playerControls.PlayerMovement.Walk.canceled += i => walk = i.ReadValueAsButton();
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
