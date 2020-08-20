using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerMovement : MonoBehaviour
{
    [Header("MovementAttribute")]
    public Collider2D standColl;
    public Collider2D crouchColl;
    public Rigidbody2D rb;
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;
    public Animator ani;

    [Header("RunAttribute")]
    public float normalSpeed;
    public float movementSpeed;
    public bool canSpecialJump;
    public Vector2 direction;

    [Header("JumpAttribute")]
    public float jumpForce;
    public float delayTimeFall;
    public bool isJumping;
    public bool isGround;
    public float jumpTimeLimit;
    public float jumpTimeCount;
    public Transform feetPosition;
    public float checkFeetRadius;

    [Header("SomersaultbAttribute")]
    public bool isSomersault;
    public float somersaultForce;
    public bool canSomersault;
    public bool stackSault;

    [Header("FallAttribute")]
    public float fallMultiphlier = 2.5f;
    public float lowMultiphlier = 2;
    public float gravity;

    [Header("WallSlideAttribute")]
    public bool isWall;
    public bool isLeftWall;
    public bool isRightWall;
    public int lastCheckWall;
    public float friction;
    public Transform leftHand;
    public Transform rightHand;

    [Header("WallGrabAttribute")]
    public bool isWallGrab;

    [Header("CrouchAttribute")]
    public bool canStand;
    public bool isCrouch;
    public float crouchMovementSpeed;
    public Transform head;
    public float checkHeadRadius;

    [Header("WallJumpAttribute")]
    public bool canWallJump;
    public bool isWallJump;
    public float wallJumpForce;
    public Vector2 wallJumpDirection;
    public float delayTimeWallJump;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        movementSpeed = normalSpeed;
    }
    private void Update()
    {
        Flip();
        Jump();
        
        WallJump();

        Somersault();

        WallSlide();
        WallGrab1();
        //WallGrab();
        Run();

        Crouch();


    }
    void FixedUpdate()
    {   

        
    }

    public void Flip()
    {
        if (!isWallGrab)
        {
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }

    }
    public void Jump()
    {
        isGround = Physics2D.OverlapCircle(feetPosition.position, checkFeetRadius, whatIsGround);
        //ani
        ani.SetBool("isGround", isGround);
        if (isGround)
        {
            canSomersault = false;
            stackSault = true;
            ani.SetBool("isSomersault", false);

            ani.SetBool("fall", false);
            rb.gravityScale = gravity;
        }
        if (isGround && Input.GetKeyDown(KeyCode.J))
        {
            rb.velocity = Vector2.up * jumpForce;
            //rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            isJumping = true;


            jumpTimeCount = jumpTimeLimit;

            //ani
            ani.SetTrigger("jump");
        }
        if (Input.GetKey(KeyCode.J) && isJumping)
        {
            if (jumpTimeCount > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                //rb.AddForce(Vector2.up * jumpForce,ForceMode2D.Force);
                jumpTimeCount -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
                //canSomersault = true;//khi đứng sát tường nhảy sẽ là nhảy x2
                if (!isWall)
                {
                    canSomersault = true;//khi đứng sát tường thì nhảy sẽ là nhảy bật tường
                }
                //ani
                ani.SetBool("fall", true);
                //StartCoroutine(WaitFall());
            }
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            isJumping = false;
            //canSomersault = true;//khi đứng sát tường nhảy sẽ là nhảy x2
            if (!isWall)
            {
                canSomersault = true;//khi đứng sát tường thì nhảy sẽ là nhảy bật tường
            }
            //ani
            ani.SetBool("fall", true);
            //StartCoroutine(WaitFall());
        }
        // fix gravity
        if (rb.velocity.y < 0 && !isWallJump && !isWallGrab && !isWall)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiphlier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.J) && !isWallJump && !isWallGrab && !isWall)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowMultiphlier - 1) * Time.deltaTime;
        }
    }
    public void Somersault()
    {
        if (stackSault && canSomersault && Input.GetKeyDown(KeyCode.J))
        {
            stackSault = false;
            rb.velocity = Vector2.up * somersaultForce;

            //ani
            ani.SetBool("isSomersault", true);
        }
    }
    public void Run()
    {
        float fixInput = Input.GetAxisRaw("Horizontal");
        if ((isLeftWall && fixInput == -1) || (isRightWall && fixInput == 1) || fixInput == 0)
        {
            fixInput = 0;
        }
        if (!isWallJump && !isWallGrab)
        {
            direction = new Vector2(fixInput * movementSpeed, rb.velocity.y);
            rb.velocity = direction;
            //ani
            ani.SetBool("isRunning", fixInput != 0);
        }
        if (!isGround && !isWall)
        {
            canSomersault = true;
        }

    }
    public void WallSlide()// xem lai 
    {
        isLeftWall = Physics2D.OverlapBox(leftHand.position, new Vector2(0.1f, 0.5f),0 , whatIsWall); // nen xem lai cach su dung
        isRightWall = Physics2D.OverlapBox(rightHand.position, new Vector2(0.1f, 0.5f), 0, whatIsWall);
        isWall = isLeftWall || isRightWall;
        if (isWall && rb.velocity.y < 0)
        {
            rb.gravityScale = friction;
            canSomersault = false;
            stackSault = true;
            ani.SetBool("isSomersault", false);
        }
        else if (!isWallGrab && isWall)
        {
            rb.gravityScale = gravity;
        }



        if (isRightWall)
        {
            lastCheckWall = 1;
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (isLeftWall)
        {
            lastCheckWall = -1;
            GetComponent<SpriteRenderer>().flipX = true;
        }
        //ani
        ani.SetBool("isWall", isWall);
        ani.SetFloat("velocityY", rb.velocity.y);
    }
    public void WallGrab()
    {
        if (isWall && !isWallJump && Input.GetKey(KeyCode.K))
        {
            isWallGrab = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            //ani
            ani.SetBool("isWallGrab", true);
        }
        if (isWallGrab && Input.GetKeyUp(KeyCode.K))
        {
            isWallGrab = false;
            rb.gravityScale = gravity;

            //ani
            ani.SetBool("isWallGrab", false);
        }
    }
    public void WallGrab1()
    {
        if (isWall && !isWallJump && Input.GetKeyDown(KeyCode.K))
        {
            isWallGrab = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            //ani
            ani.SetBool("isWallGrab", true);
        }
        if (isWallGrab && Input.GetKeyUp(KeyCode.K))
        {
            isWallGrab = false;
            rb.gravityScale = gravity;

            //ani
            ani.SetBool("isWallGrab", false);
        }
    }
    public void Crouch()
    {
        canStand = !Physics2D.OverlapCircle(head.position, checkHeadRadius, whatIsGround); 
        if (Input.GetKeyDown(KeyCode.S) && isGround)
        {
            isCrouch = true;
            standColl.isTrigger = true;
            movementSpeed = crouchMovementSpeed;

            //ani
            ani.SetBool("isCrouch", true);
        }
        if (Input.GetKeyUp(KeyCode.S) && isCrouch)
        {
            isCrouch = false;
        }
        if (isGround && canStand && !isCrouch)
        {
            standColl.isTrigger = false;
            movementSpeed = normalSpeed;

            //ani
            ani.SetBool("isCrouch", false);
        }
    }
    public void Drop()
    {
        if (Input.GetKeyDown(KeyCode.S) && isGround)
        {
            //standColl.gameObject.layer = 9;
            crouchColl.gameObject.layer = 9;
            Debug.Log("wait");
            StartCoroutine(resetLayer(0.5f));
        }
    }
    public IEnumerator resetLayer(float time) // xem lai thoi gian
    {
        yield return new WaitForSeconds(time);
        standColl.gameObject.layer = 10;
        crouchColl.gameObject.layer = 10;
    }
    public void WallJump()
    {
        canWallJump = !isGround && isWall;
        if (canWallJump && Input.GetKeyDown(KeyCode.J))
        {
            
            isWallJump = true;


            isWallGrab = false;
            rb.gravityScale = gravity;

            //ani
            ani.SetBool("isWallGrab", false);

            Vector2 fixDirection = new Vector2(wallJumpDirection.x * (isLeftWall ? 1 : -1), wallJumpDirection.y);
            rb.velocity = fixDirection * wallJumpForce ;
            //ani
            ani.SetBool("isWallJump", true);
            StartCoroutine(WaitWallJump());
        }
    }
    public void QuickFlip()
    {
        if (lastCheckWall == 1)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (lastCheckWall == -1)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }
    public IEnumerator WaitWallJump()
    {
        yield return new WaitForSeconds(delayTimeWallJump);
        isWallJump = false;
        canSomersault = true;
        //ani
        ani.SetBool("isWallJump", false);
    }
}
