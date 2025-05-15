using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour
{
    // Enums for actions
    private enum ActionState
    {
        None,
        Diving,
        Crouching,
        Attacking,
        Interacting,
        Menu,
        Inventory
    }
    
    // Local variables
    [Header("Movement")]
    [SerializeField] private GameObject lookAt;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction crouchAction;
    [SerializeField] private InputAction interactAction;
    [SerializeField] private InputAction attackAction;
    [SerializeField] private InputAction inventoryAction;
    [SerializeField] private InputAction menuAction;
    
    [Header("Interacting")]
    [SerializeField] private InputAction submitAction;
    [SerializeField] private InputAction cancelAction;
    [SerializeField] private InputAction navigateAction;
    
    [Header("Menu controller")]
    [SerializeField] private GameMenuDisplayManager gameMenuDisplayManager;
    
    [Header("Menu")]
    [SerializeField] private GameObject menu;
    
    [Header("Inventory")]
    [SerializeField] private GameObject inventory;
    
    // Action states
    [SerializeField] private ActionState actionState;
    
    // Inventory
    [SerializeField] public InventoryManager inventoryManager;
    
    // Physics variables
    private const float Gravity = 9.81f;
    private const float Speed = 5f; // Units per second
    private const float SprintSpeed = 10f;
    private const float JumpSpeed = 8f;
    private const float DiveSpeed = 15f;
    private const float CrouchSpeed = 2.5f;
    private float _vSpeed; // Current vertical speed

    public float gravityMultiplier = 1f;

    // PLAYER -> OBJECT
    // OBJECT </- PLAYER
    // PLAYER WILL ONLY BE ABLE TO AFFECT OBJECT
    // OBJECT WILL NOT BE ABLE TO AFFECT PLAYER
    [FormerlySerializedAs("objectCurrentlyInteractingWith")] public EntityInteract entityCurrentlyInteractingWith;
    public ItemInteract itemCurrentlyInteractingWith;
    public List<GameObject> nearItems = new();
    // This will contain the entities that the player can talk to.
    
    // Attacking variables
    public GameObject attackSphere; // The sphere that will act as a hitbox for the player's attack
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Get required objects
        lookAt = GameObject.FindGameObjectWithTag("LookAt");
        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Player/Move");
        sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        crouchAction = InputSystem.actions.FindAction("Player/Crouch");
        attackAction = InputSystem.actions.FindAction("Player/Attack");
        interactAction = InputSystem.actions.FindAction("Player/Interact");
        inventoryAction = InputSystem.actions.FindAction("Player/Inventory");
        menuAction = InputSystem.actions.FindAction("Player/Menu");
        
        submitAction = InputSystem.actions.FindAction("UI/Submit");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        
        // Set player config
        actionState = ActionState.None;
        inventoryManager = InventoryManager.instance;
        
        // Set player menu states
        menu = transform.GetChild(1).GetChild(0).gameObject;
        gameMenuDisplayManager = menu.GetComponent<GameMenuDisplayManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        ShowLabels();
        switch (actionState)
        {
            case ActionState.Interacting:
                CheckInteractionMovement(
                    cancelAction.IsPressed(), 
                    jumpAction.WasReleasedThisFrame(), 
                    moveAction.ReadValue<Vector2>().y, 
                    moveAction.WasPressedThisFrame()
                );
                break;
            case ActionState.Inventory:
                CheckInventoryMovement(cancelAction.WasReleasedThisFrame(), jumpAction.WasReleasedThisFrame(), moveAction.ReadValue<Vector2>().x, moveAction.ReadValue<Vector2>().y);
                break;
            case ActionState.Menu:
                CheckMainMenuMovement(cancelAction.WasReleasedThisFrame(), jumpAction.WasReleasedThisFrame(), moveAction.ReadValue<Vector2>().x, moveAction.ReadValue<Vector2>().y);
                break;
            default:
                MoveCharacter(sprintAction.IsPressed(), crouchAction.IsPressed(), interactAction.WasReleasedThisFrame(), attackAction.WasPressedThisFrame(), inventoryAction.WasReleasedThisFrame(), menuAction.WasReleasedThisFrame());
                break;
        }
    }

    private void ShowLabels()
    {
        if (nearItems.Count <= 0) return;
        nearItems[0].transform.GetChild(0).gameObject.SetActive(true);
        nearItems[0].transform.GetChild(1).gameObject.SetActive(true);
        for (var i = 1; i < nearItems.Count; i++)
        {   
            nearItems[i].transform.GetChild(0).gameObject.SetActive(false);
            nearItems[i].transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    
    private void MoveCharacter(bool isSprinting, bool isCrouching, bool isInteracting, bool isAttacking, bool isInventory, bool isMenu)
    {
        // Get player input as Vec3
        var vel = characterController.velocity;
        if (characterController.isGrounded)
        {
            // https://discussions.unity.com/t/my-question-is-how-do-i-get-my-player-to-move-in-the-direction-the-camera-is-facing/730970/4
            // Takes care of sprinting and crouching. Can't sprint if already crouched but can crouch if sprinting
            // Get the euler angles of the lookAt GameObject and multiply the normalised movement vectors by them
            // Apply inline boolean operations to set speed
            vel = Quaternion.Euler(0, lookAt.transform.eulerAngles.y, 0) * new Vector3(moveAction.ReadValue<Vector2>().x, 0, moveAction.ReadValue<Vector2>().y) * (isCrouching ? CrouchSpeed : isSprinting ? SprintSpeed : Speed);
            _vSpeed = 0; // Grounded characters have 0 vertical speed
            
            // PC behaviour tree
            if (jumpAction.IsPressed())
            {
                _vSpeed = JumpSpeed;
            }

            if (attackAction.IsPressed())
            {
                actionState = ActionState.Attacking;
                StartCoroutine(StartAttack());
            }
            else
            {
                actionState = ActionState.None;
            }

        }
        else
        {
            // if the PC "sprints" in the air (diving)
            if (actionState != ActionState.Diving && sprintAction.WasPressedThisFrame())
            {
                vel = Quaternion.Euler(0, lookAt.transform.eulerAngles.y, 0) * new Vector3(moveAction.ReadValue<Vector2>().x, 0, moveAction.ReadValue<Vector2>().y) * DiveSpeed;
                actionState = ActionState.Diving;
            }
        }
        
        // apply gravity acceleration to vertical speed
        _vSpeed -= Gravity * (gravityMultiplier * Time.deltaTime);
        vel.y = _vSpeed; // include vertical speed as well
        
        // convert vertical speed to displacement and move character
        characterController.Move(vel * Time.deltaTime);
        
        // Next, we check if the player is near anything and the interaction button is being pressed
        // Since this is the same button as the inventory, we need to return when we call this
        if (isInteracting && nearItems.Count > 0)
        {
            try
            {
                entityCurrentlyInteractingWith = nearItems[0].GetComponent<EntityInteract>();
                entityCurrentlyInteractingWith.StartInteraction();
                actionState = ActionState.Interacting;
                Debug.Log("Interacting with Entity");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                itemCurrentlyInteractingWith = nearItems[0].GetComponent<ItemInteract>();
                if (inventoryManager.AddItem(nearItems[0].transform.parent.name))
                {
                    // This will make the item disappear
                    // because if the player's inventory is too full,
                    // the item should stay on the ground
                    Destroy(itemCurrentlyInteractingWith.transform.parent.gameObject);
                    nearItems.RemoveAt(0);
                    itemCurrentlyInteractingWith = null;
                    return;
                }
                itemCurrentlyInteractingWith.DisplayInventoryFull();
                actionState = ActionState.Interacting;
            }
            // TODO: Also pause any enemies and other objects here
            // TODO: Add picking up items
        }
        
        // Next, we check if the player wants to go to the inventory
        if (isInventory)
        {
            actionState = ActionState.Inventory;
            gameMenuDisplayManager.OpenInventory();
            return;
        }
        
        // Then the main menu
        if (!isMenu) return;
        actionState = ActionState.Menu;
        gameMenuDisplayManager.OpenMainMenu();
    }

    private IEnumerator StartAttack()
    {
        attackSphere.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        attackSphere.SetActive(false);
    }
    
    private void CheckInteractionMovement(bool isCancelling, bool isSubmitting, float y, bool stickMovedNow)
    {
        try
        {
            // first, check if the dialogue box is finished rendering
            if (!entityCurrentlyInteractingWith.finishedRendering) return;

            // secondly, check if the player is submitting
            if (isSubmitting)
            {
                // check if the entity is done
                if (entityCurrentlyInteractingWith.IsFinished())
                {
                    // is so, end the interaction
                    entityCurrentlyInteractingWith.EndInteraction();
                    
                    
                    // wait until player has finished pressing the key so the player
                    // does not jump or start the interaction again
                    actionState = ActionState.None;
                }
                else
                {
                    // otherwise, if the player is the one responding, change it to false
                    if (entityCurrentlyInteractingWith.responding)
                    {
                        entityCurrentlyInteractingWith.SubmitChoice();
                        Debug.Log("HEY");
                    }
                    // and increment the next line
                    entityCurrentlyInteractingWith.NextLine();
                }
            }
            else if (stickMovedNow && entityCurrentlyInteractingWith.responding)
            {
                entityCurrentlyInteractingWith.ChangeSelection(y);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            if (!itemCurrentlyInteractingWith.finishedRendering || !isSubmitting) return;

            if (itemCurrentlyInteractingWith.IsFinished())
            {
                itemCurrentlyInteractingWith.EndInteraction();
                actionState = ActionState.None;
            }
        }
    }

    private void CheckInventoryMovement(bool isCancelling, bool isSubmitting, float x, float y)
    {
        if (!isCancelling) return;
        actionState = ActionState.None;
        gameMenuDisplayManager.CloseInventory();
    }

    private void CheckMainMenuMovement(bool isCancelling, bool isSubmitting, float x, float y)
    {
        if (!isCancelling) return;
        actionState = ActionState.None;
        gameMenuDisplayManager.CloseMainMenu();
    }
    
    
    private void OnTriggerEnter(Collider c) {
        if (c.CompareTag("Entity"))
        {
            nearItems.Add(c.gameObject);
        }
    }

    private void OnTriggerExit(Collider c) {
        if (c.CompareTag("Entity")) 
        {
            nearItems.Remove(c.gameObject);
            c.transform.GetChild(0).gameObject.SetActive(false);
            c.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
