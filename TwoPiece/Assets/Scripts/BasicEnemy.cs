﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BasicEnemy : MonoBehaviour {
    public int clubHealth = 2;
    public int swordHealth = 1;
    public float walkSpeed = 1.0f;
    public float meleeRange = 1.0f;

    public float timeToSwing = 1.0f;
    public float weaponSwingCooldown = 1.0f;
    // currentWeaponCooldown will be set to timeToSwing + weaponSwingCooldown
    private float currentWeaponCooldown = 0.0f;


    private Controller2D controller;
    private BoxCollider2D collider;
    private SpriteRenderer sprite;

    private RaycastOrigins rayOrigins;
    private EnemyState state = EnemyState.Patrolling;
    public Vector2 direction = Vector2.right;

    public LayerMask collisionMask;
    public LayerMask playerMask;

    enum EnemyState {
        Patrolling,
        PreparingToSwing
    }

	// Use this for initialization
	void Start () {
        controller = GetComponent<Controller2D>();
        collider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
	}

    void Awake()
    {

    }

    void Update()
    {
        UpdateCooldowns();
        if(EnemyState.Patrolling == state)
        {
            UpdateRaycastOrigins();
            if (CheckForPlayerInMeleeRange())
            {
                if(currentWeaponCooldown <= 0.0f)
                {
                    state = EnemyState.PreparingToSwing;
                    StartCoroutine(SwingWeaponAfterTime(timeToSwing));
                }
            }
            else
            {
                CheckForCollisions();
                controller.Move(direction * walkSpeed * Time.deltaTime);
            }
        }
        // else if(EnemyState.PreparingToSwing == state)
    }

    void UpdateCooldowns()
    {
        currentWeaponCooldown -= Time.deltaTime;
    }

    bool CheckForPlayerInMeleeRange()
    {
        int numHorizontalTraces = 4;
        // Bounds.extents/2 as we're only hitting the top half of her hitbox
        float yOffsetPerTrace = (collider.bounds.extents.y / 2) / (numHorizontalTraces - 1);
        for (int i = 0; i <= numHorizontalTraces; i++)
        {
            Vector2 rayOrigin = (direction == Vector2.left) ? rayOrigins.centerLeft : rayOrigins.centerRight;
            rayOrigin += Vector2.up * yOffsetPerTrace * i;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, meleeRange, playerMask);
            Debug.DrawRay(rayOrigin, direction, Color.red);
            if (hit)
            {
                return hit;
            }
        }
        return false;
    }

    void SwingWeapon()
    {
        int numHorizontalTraces = 4;
        // Bounds.extents/2 as we're only hitting the top half of her hitbox
        float yOffsetPerTrace = (collider.bounds.extents.y / 2) / (numHorizontalTraces - 1);
        bool hitFound = false;
        for (int i = 0; i <= numHorizontalTraces && !hitFound; i++)
        {
            Vector2 rayOrigin = (direction == Vector2.left) ? rayOrigins.centerLeft : rayOrigins.centerRight;
            rayOrigin += Vector2.up * yOffsetPerTrace * i;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, meleeRange, playerMask);
            Debug.DrawRay(rayOrigin, direction, Color.red);
            if (hit)
            {
                hit.collider.SendMessage("DamageTaken");
                hitFound = true;
            }
        }
    }

    void CheckForCollisions()
    {
        if( CheckFrontCollisions() || !CheckGroundCollisions() )
        {
            ChangeDirections();
        }
    }

    void ChangeDirections()
    {
        direction = direction * -1;
        FlipSprite();
    }

    // Shoot a ray from the middle of the enemy to distanceToTurnAroundAt in front of them, return true if you hit an obstacle
    bool CheckFrontCollisions()
    {
        const float distanceToTurnAroundAt = 0.5f;
        Vector2 rayOrigin = (direction == Vector2.left) ? rayOrigins.centerLeft : rayOrigins.centerRight;
        // Converted to bool implicitly
        return Physics2D.Raycast(rayOrigin, direction, distanceToTurnAroundAt, collisionMask);
    }

    bool CheckGroundCollisions()
    {
        const float distanceInFrontOfEnemy = 0.1f;
        Vector2 rayOrigin = (direction == Vector2.left) ? rayOrigins.bottomLeft : rayOrigins.bottomRight;
        // OFfset the x to distanceInFrontOfEnemy in front of the enemy
        rayOrigin += direction * distanceInFrontOfEnemy;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, collisionMask);
        return hit;
    }

    void FlipSprite()
    {
        sprite.flipX = !sprite.flipX;
    }

    void OnSwordDamage()
    {
        swordHealth--;
    }

    void OnClubDamage()
    {
        clubHealth--;
    }

    void UpdateRaycastOrigins()
    {
        const float skinWidth = 0.15f;
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        // Use bounds as we're checking for close collisions with these
        rayOrigins.centerLeft = new Vector2(bounds.min.x, bounds.center.y);
        rayOrigins.centerRight = new Vector2(bounds.max.x, bounds.center.y);
        // Use collider.bounds because we're checking in front of the enemy anyways
        rayOrigins.bottomLeft = new Vector2(collider.bounds.min.x, collider.bounds.min.y);
        rayOrigins.bottomRight = new Vector2(collider.bounds.max.x, collider.bounds.min.y);
    }

    struct RaycastOrigins
    {
        public Vector2 centerLeft, centerRight;
        public Vector2 bottomLeft, bottomRight;
    }

    IEnumerator SwingWeaponAfterTime(float time)
    {
        currentWeaponCooldown = time + weaponSwingCooldown;
        yield return new WaitForSeconds(time);

        UpdateRaycastOrigins();
        SwingWeapon();
        state = EnemyState.Patrolling;

        // Code to execute after the delay
    }

}
