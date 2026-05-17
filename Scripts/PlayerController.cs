using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
public CharacterController charCon;
public float MoveSpeed;
public float gravityModifier = 4f; 
public InputActionReference moveAction;
Vector3 currentMovment;
public InputActionReference lookAction  ;
public InputActionReference jumpAction;
 public float jumpPower;
private Vector2 rotStore;
public float lookSpeed;
public InputActionReference shootAction;
public InputActionReference sprintAction;
public float runSpeed;
public Camera theCam;
public float camZoomNormal , camZoomOut, camZoomSpeed;

public float minViewAngle , maxViewAngle;
public WeaponController weaponCon;
public InputActionReference reloadAction;

void OnEnable()
{
    moveAction.action.Enable();
        lookAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
            shootAction.action.Enable();
                reloadAction.action.Enable();
}
void OnDisable()
{        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
        shootAction.action.Disable();
        reloadAction.action.Disable();
}
// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
{
    //Cursor.lockState = CursorLockMode.Locked;
}

// Update is called once per frame
void Update()
{
        float yStore = currentMovment.y;
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        // Debug.Log(moveInput);
        // currentMovment = new Vector3(moveInput.x * MoveSpeed, 0f, moveInput.y * MoveSpeed) ;
        Vector3 moveForward = transform.forward * moveInput.y ;
        Vector3 moveSideways = transform.right * moveInput.x ;
        //handle sprinting
        if (sprintAction.action.IsPressed())
        {
             currentMovment = (moveForward + moveSideways) * runSpeed;
             if(currentMovment != Vector3.zero)//hwe w2f ma be ser effect t3lt running
            {
               theCam.fieldOfView = Mathf.Lerp(theCam.fieldOfView, camZoomOut, Time.deltaTime * camZoomSpeed);  
            }
            
        }
        else
        {
              currentMovment = (moveForward + moveSideways) * MoveSpeed;
              theCam.fieldOfView = Mathf.Lerp(theCam.fieldOfView, camZoomNormal, Time.deltaTime * camZoomSpeed);
        }
      
        if (charCon.isGrounded)//handle gravity
        {
            yStore = 0f;
        }
        currentMovment.y = yStore + (Physics.gravity.y * Time.deltaTime * gravityModifier);//bynzal lplayer iza kn fu2
        //handle jump
        if (jumpAction.action.WasPressedThisFrame() && charCon.isGrounded)
        {
            currentMovment.y = jumpPower;
        }
    charCon.Move( currentMovment * Time.deltaTime); 
    //handle looking around
    Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
    lookInput.y = -lookInput.y;//bytl3 fu2 tht mzbt
    rotStore = rotStore + lookInput * lookSpeed * Time.deltaTime;
    rotStore.y = Mathf.Clamp(rotStore.y, minViewAngle, maxViewAngle);//limt lal angle ahsa ma ytsh2lb 
    transform.rotation = Quaternion.Euler(0f, rotStore.x, 0f);
    theCam.transform.localRotation = Quaternion.Euler(rotStore.y, 0f, 0f);
    //handle shooting
    if (shootAction.action.WasPressedThisFrame())
    {        weaponCon.shoot();
    }
    if(shootAction.action.IsPressed())
    {
        weaponCon.ShootHeld();
    }
    if (reloadAction.action.WasPressedThisFrame())
    {
       weaponCon.Reload();
    }
}
}