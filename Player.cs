using SDL2;
using System;
using System.Collections.Generic;

namespace MyGame.Entities
{
    public class Player
    {
        private SDL.SDL_Rect rect;
        public SDL.SDL_Rect weaponRect;
        private SDL.SDL_Rect cameraAdjustedRect;  // Reused camera-adjusted rectangle
        private SDL.SDL_Rect cameraAdjustedWeaponRect;  // Reused weapon rectangle
        private Dictionary<string, IntPtr> textures; // Dictionary to store textures

        private float posX, posY;
        private float velocityX, velocityY;
        private float speed = 500;
        private float jumpSpeed = 500;
        private float gravity = 5000;
        private bool isJumping = false;
        private bool isFalling = false;
        private bool isColliding = false;
        private float jumpStartY;
        private float maxJumpHeight = 120;

        public bool facingRight = true;
        public bool facingLeft = false;
        private bool walkingRight = false;
        private bool walkingLeft = false;
        private bool attackingRight = false;
        private bool attackingLeft = false;
        private bool wasAttacking = false; // Tracks if the attack key was pressed in the last frame

        private float attackDuration = 0.5f; // How long the attack is visible
        private float attackCooldown = 0.5f;  // Cooldown between attacks
        private float attackTimer = 0f;     // Timer to control attack visibility
        private bool canAttack = true;      // Whether the player can attack
        public int HP { get; private set; } = 3; // Starting HP
        private bool guarding = false;

        public int currentComboStage = 0; // Current stage of the attack combo
        private float comboWindow = 0.5f;   // Time window to press for the next attack
        private float comboTimer = 0f;       // Timer for the combo window
        private float comboCooldown = 1f;     // Cooldown after completing a combo

        private float currentKnockbackTimer = 0f; // Timer for knockback
        private const float knockbackTime = 0.1f; // Duration of knockback
        private float knockbackVelocity = 800f; // Speed of knockback
        private int knockbackDirection = 0; // -1 for left, 1 for right

        private float walkTimer = 0f;
        private float walkFrameDuration = 0.3f; // Duration of each frame
        private int walkFrame = 1; // Current walking frame (1, 2, or 3)


        public float X => posX;  // Public getter for X position
        public float Y => posY;  // Public getter for Y position
        public int Width => rect.w;
        public int Height => rect.h;

        public Player(float x, float y, int width, int height)
        {
            this.posX = x;
            this.posY = y;
            rect = new SDL.SDL_Rect { x = (int)x, y = (int)y, w = width, h = height };
            weaponRect = new SDL.SDL_Rect { x = (int)x, y = (int)y, w = width, h = height };
            cameraAdjustedRect = new SDL.SDL_Rect();        // Initialize the reusable rectangle
            cameraAdjustedWeaponRect = new SDL.SDL_Rect();  // Initialize the reusable rectangle
            textures = new Dictionary<string, IntPtr>(); // Initialize the texture dictionary
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// Textures
        public void LoadTextures(IntPtr renderer)
        {
            // Load each texture and store it in the dictionary
            textures["idleRight"] = LoadTexture(renderer, "Textures/playerIdleRight.png");
            textures["idleLeft"] = LoadTexture(renderer, "Textures/playerIdleLeft.png");
            textures["attackRight"] = LoadTexture(renderer, "Textures/playerAttackRight.png");
            textures["attackLeft"] = LoadTexture(renderer, "Textures/playerAttackLeft.png");
            textures["weaponRight"] = LoadTexture(renderer, "Textures/playerWeaponRight.png");
            textures["weaponLeft"] = LoadTexture(renderer, "Textures/playerWeaponLeft.png");
            textures["guardingRight"] = LoadTexture(renderer, "Textures/playerShieldRight.png");    //
            textures["guardingLeft"] = LoadTexture(renderer, "Textures/playerShieldLeft.png");      //
            textures["walk1Right"] = LoadTexture(renderer, "Textures/playerWalk1Right.png");
            textures["walk1Left"] = LoadTexture(renderer, "Textures/playerWalk1Left.png");
            textures["walk2Right"] = LoadTexture(renderer, "Textures/playerWalk2Right.png");
            textures["walk2Left"] = LoadTexture(renderer, "Textures/playerWalk2Left.png");
            textures["walk3Right"] = LoadTexture(renderer, "Textures/playerWalk3Right.png");
            textures["walk3Left"] = LoadTexture(renderer, "Textures/playerWalk3Left.png");

            textures["swordIdleRight"] = LoadTexture(renderer, "Textures/playerSwordIdleRight.png");
            textures["swordIdleLeft"] = LoadTexture(renderer, "Textures/playerSwordIdleLeft.png");

            textures["combo1Right"] = LoadTexture(renderer, "Textures/playerCombo1Right.png");
            textures["combo1Left"] = LoadTexture(renderer, "Textures/playerCombo1Left.png");
            textures["swordCombo1Right"] = LoadTexture(renderer, "Textures/playerSwordCombo1Right.png");
            textures["swordCombo1Left"] = LoadTexture(renderer, "Textures/playerSwordCombo1Left.png");

            textures["combo2Right"] = LoadTexture(renderer, "Textures/playerCombo2Right.png");
            textures["combo2Left"] = LoadTexture(renderer, "Textures/playerCombo2Left.png");
            textures["swordCombo2Right"] = LoadTexture(renderer, "Textures/playerSwordCombo2Right.png");
            textures["swordCombo2Left"] = LoadTexture(renderer, "Textures/playerSwordCombo2Left.png");

            textures["combo3Right"] = LoadTexture(renderer, "Textures/playerCombo3Right.png");
            textures["combo3Left"] = LoadTexture(renderer, "Textures/playerCombo3Left.png");
            textures["swordCombo3Right"] = LoadTexture(renderer, "Textures/playerSwordCombo3Right.png");
            textures["swordCombo3Left"] = LoadTexture(renderer, "Textures/playerSwordCombo3Left.png");
        }

        // Helper method to load a texture
        private IntPtr LoadTexture(IntPtr renderer, string filePath)
        {
            IntPtr texture = SDL_image.IMG_LoadTexture(renderer, filePath);
            if (texture == IntPtr.Zero)
            {
                throw new Exception("Failed to load texture: " + SDL.SDL_GetError());
            }
            return texture;
        }

        public void FreeTextures()
        {
            foreach (var texture in textures.Values)
            {
                if (texture != IntPtr.Zero)
                {
                    SDL.SDL_DestroyTexture(texture);
                }
            }
            textures.Clear(); // Clear the dictionary after freeing the textures
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////// Returns player texture
        private IntPtr GetCurrentTexture()
        {

            if (guarding && facingRight) return textures["guardingRight"];
            else if (guarding && facingLeft) return textures["guardingLeft"];

            // Check for combo states, only return if combo state is active
            if (currentComboStage > 0) 
            {
                if (attackingRight) return textures[$"combo{currentComboStage}Right"];
                if (attackingLeft) return textures[$"combo{currentComboStage}Left"];
            }

            if (walkingRight)
            {
                switch (walkFrame)
                {
                    case 1: return textures["walk1Right"];
                    case 2: return textures["walk2Right"];
                    case 3: return textures["walk3Right"];
                    case 4: return textures["walk2Right"];
                }
            }

            if (walkingLeft)
            {
                switch (walkFrame)
                {
                    case 1: return textures["walk1Left"];
                    case 2: return textures["walk2Left"];
                    case 3: return textures["walk3Left"];
                    case 4: return textures["walk2Left"];
                }
            }

            if (facingRight) return textures["idleRight"];
            if (facingLeft) return textures["idleLeft"];

            return textures["idleRight"];
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////// Get weapon texture
        private IntPtr GetCurrentWeaponTexture()
        {
            // Return weapon texture based on the current combo stage
            if (currentComboStage > 0) // Only return combo textures if in a combo state
            {
                if (attackingRight) return textures[$"swordCombo{currentComboStage}Right"];
                if (attackingLeft) return textures[$"swordCombo{currentComboStage}Left"];
            }

            // Return the default idle weapon texture when not attacking
            if (facingRight) return textures["swordIdleRight"];
            else if (facingLeft) return textures["swordIdleLeft"];

            // Default to idle weapon texture on the right side
            return textures["swordIdleRight"];
        }


        public SDL.SDL_Rect GetRect()
        {
            return rect;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////// Player input
        public void HandleInput(IntPtr keys, float deltaTime)
        {
            unsafe
            {
                byte* keyState = (byte*)keys;

                ////////////////////////////////////////////////////////////////////////////////////////////// Horizontal movement
                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
                {
                    velocityX = -speed;
                    facingRight = false;
                    facingLeft = true;
                    walkingLeft = true;
                    walkingRight = false;
                }
                else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
                {
                    velocityX = speed;
                    facingRight = true;
                    facingLeft = false;
                    walkingRight = true;
                    walkingLeft = false;
                }
                else
                {
                    velocityX = 0;
                    walkingLeft = false;
                    walkingRight = false;
                }

                //////////////////////////////////////////////////////////////////////////////////////////////////// Jumping logic
                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE] == 1 && !isJumping && !isFalling)
                {
                    isJumping = true;
                    jumpStartY = posY;
                    velocityY = -jumpSpeed;
                }

                if (isJumping)
                {
                    if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE] == 1 && posY > jumpStartY - maxJumpHeight)
                    {
                        velocityY -= gravity * deltaTime;
                    }
                    else
                    {
                        isJumping = false;
                        isFalling = true;
                    }
                }

                ///////////////////////////////////////////////////////////////////////////////////////////////////// Attack press
                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1 && canAttack && !wasAttacking)
                {
                    // Reset combo if cooldown is active or the time window has passed
                    if (currentComboStage == 0 || comboTimer <= 0)
                    {
                        currentComboStage = 1; // Start from the first attack
                    }
                    else
                    {
                        if (!isJumping && !isFalling)
                            currentComboStage++; // Increment the combo stage
                    }

                    // Execute the attack based on the current combo stage
                    if (currentComboStage <= 3)
                    {
                        if (facingRight)
                        {
                            attackingRight = true;
                            attackingLeft = false;
                        }
                        else if (facingLeft)
                        {
                            attackingRight = false;
                            attackingLeft = true;
                        }

                        // Set the attack timer and reset the combo timer
                        attackTimer = attackDuration; // Set attack duration
                        comboTimer = comboWindow;      // Reset combo timer

                        // If reaching the combo limit, set the cooldown
                        if (currentComboStage == 3)
                        {
                            canAttack = false; // Start cooldown after the third attack
                        }
                    }
                    else // Reset to idle after the third attack
                    {
                        currentComboStage = 0; // Reset to idle state
                        attackingRight = false;
                        attackingLeft = false;
                    }
                }

                // Update wasAttacking
                wasAttacking = keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1;

                ////////////////////////////////////////////////////////////////////////////////////////////////////// Guard press
                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_C] == 1 && canAttack && !wasAttacking && !walkingLeft && !walkingRight && !isJumping)
                {
                    guarding = true;
                }
                else
                {
                    guarding = false;
                }
            }
        }

        public bool IsGuarding()
        {
            return guarding;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Utok
        public bool IsAttacking()
        {
            return attackTimer > 0; // Return true if the attack timer is active
        }

        public void TakeDamage(int damage, int enemyX)
        {
            HP = Math.Max(HP - damage, 0); // Reduce HP but don't go below 0

            // Determine knockback direction
            knockbackDirection = (enemyX < rect.x) ? 1 : -1;
            currentKnockbackTimer = knockbackTime; // Start knockback
        }

        /////////////////////////////////////////////////////////////////////////////////////////////// Kolize hrace s platformami
        public void CheckPlatformCollisions(List<Platform> platforms, float deltaTime)
        {
            SDL.SDL_Rect playerBounds = new SDL.SDL_Rect
            {
                x = (int)posX,
                y = (int)posY,
                w = rect.w,
                h = rect.h
            };

            SDL.SDL_Rect previousPlayerBounds = new SDL.SDL_Rect
            {
                x = (int)(posX - velocityX * deltaTime),
                y = (int)(posY - velocityY * deltaTime),
                w = rect.w,
                h = rect.h
            };

            foreach (var platform in platforms)
            {
                SDL.SDL_Rect platformRect = platform.GetRect();

                // Check for intersection with previous and current bounds
                if (SDL.SDL_HasIntersection(ref playerBounds, ref platformRect) == SDL.SDL_bool.SDL_TRUE ||
                    SDL.SDL_HasIntersection(ref previousPlayerBounds, ref platformRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    // Kolize zvrchu
                    if (velocityY > 0 && previousPlayerBounds.y + previousPlayerBounds.h <= platformRect.y && playerBounds.y + playerBounds.h > platformRect.y)
                    {
                        posY = platformRect.y - playerBounds.h;
                        velocityY = 0.0f;
                        isFalling = false;
                        isJumping = false;  // Allow jumping again
                        isColliding = true;
                    }    
                    // Kolize zespodu
                    else if (velocityY < 0 && playerBounds.y < platformRect.y + platformRect.h && playerBounds.y + playerBounds.h > platformRect.y + platformRect.h)
                    {
                        posY = platformRect.y + platformRect.h;
                        velocityY = (gravity + jumpSpeed + 40000) * deltaTime;
                        isFalling = true;
                    }
                    // Kolize zleva
                    else if (velocityX > 0 && playerBounds.x + playerBounds.w > platformRect.x && playerBounds.x < platformRect.x && playerBounds.y < platformRect.y + platformRect.h && playerBounds.y + playerBounds.h > platformRect.y)
                    {
                        posX = platformRect.x - playerBounds.w;
                        velocityX = 0.0f;
                    }
                    // Kolize zprava
                    else if (velocityX < 0 && playerBounds.x < platformRect.x + platformRect.w && playerBounds.x + playerBounds.w > platformRect.x + platformRect.w && playerBounds.y < platformRect.y + platformRect.h && playerBounds.y + playerBounds.h > platformRect.y)
                    {
                        posX = platformRect.x + platformRect.w;
                        velocityX = 0.0f;
                    }

                    // Update player bounds after vertical movement
                    rect.x = (int)posX;
                    rect.y = (int)posY;
                }
            }

            // If no collisions are detected and player is not jumping, set falling to true
            if (!isColliding && !isJumping)
            {
                isFalling = true;
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////// Gravity logic
            if (isFalling)
            {
                velocityY += gravity * deltaTime;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////// Update player position and attack logic
        public void Update(float deltaTime)
        {
            // Walking animation
            if (walkingRight || walkingLeft)
            {
                walkTimer += deltaTime;

                if (walkTimer >= walkFrameDuration)
                {
                    walkTimer = 0f;

                    // Modify frame update logic to switch between 1, 2, 3, 2, 1
                    if (walkFrame == 1)
                    {
                        walkFrame = 2;
                    }
                    else if (walkFrame == 2)
                    {
                        walkFrame = 3;
                    }
                    else if (walkFrame == 3)
                    {
                        walkFrame = 4; // After frame 3, go back to 2
                    }
                    else if (walkFrame == 4) // After frame 2 again, go back to 1
                    {
                        walkFrame = 1;
                    }
                }
            }
            else
            {
                // Reset to the first frame if not walking
                walkFrame = 1;
            }

            // Apply knockback or regular movement
            if (currentKnockbackTimer > 0)
            {
                posX += knockbackDirection * knockbackVelocity * deltaTime;
                currentKnockbackTimer -= deltaTime;
            }
            else
            {
                posX += velocityX * deltaTime;
            }
            posY += velocityY * deltaTime;

            rect.x = (int)posX;
            rect.y = (int)posY;

            // Attack timer - visibility update
            if (attackTimer > 0)
            {
                attackTimer -= deltaTime;
            }
            else
            {
                attackingRight = false;
                attackingLeft = false;
            }

            // Manage combo timer for the attack
            if (comboTimer > 0)
            {
                comboTimer -= deltaTime;

                // Reset combo stage if the time window has passed
                if (comboTimer <= 0)
                {
                    currentComboStage = 0; // Reset to the first stage
                }
            }

            // Manage cooldown for the attack
            if (!canAttack)
            {
                comboCooldown -= deltaTime; // Decrease cooldown timer
                if (comboCooldown <= 0)
                {
                    canAttack = true; // Reset canAttack after cooldown
                    comboCooldown = 1f; // Reset cooldown duration
                    currentComboStage = 0; // Reset combo stage after cooldown
                }
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////// Update weapon hitbox
        private void UpdateWeaponHitbox()
        {
            if (attackTimer > 0) // Weapon is in attacking state
            {
                if (attackingRight)
                {
                    weaponRect.x = (int)(rect.x + rect.w); // Position hitbox to the right when attacking right
                    weaponRect.y = (int)rect.y;
                }
                else if (attackingLeft)
                {
                    weaponRect.x = (int)(rect.x - weaponRect.w); // Position hitbox to the left when attacking left
                    weaponRect.y = (int)rect.y;
                }
            }
            else // Idle state (walking or standing)
            {
                if (facingRight)
                {
                    weaponRect.x = (int)(rect.x - rect.w); // Position to the right of the player when idle
                    weaponRect.y = (int)rect.y;
                }
                else if (facingLeft)
                {
                    weaponRect.x = (int)(rect.x + weaponRect.w); // Position to the left of the player when idle
                    weaponRect.y = (int)rect.y;
                }
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////// Render player and attack
        public void Render(IntPtr renderer, Camera camera)
        {
            // Reuse cameraAdjustedRect
            SDL.SDL_Rect cameraAdjustedRect = camera.Apply(rect);

            // Render player
            IntPtr currentTexture = GetCurrentTexture(); // Get the correct texture for rendering
            SDL.SDL_RenderCopy(renderer, currentTexture, IntPtr.Zero, ref cameraAdjustedRect);

            // Update the weapon hitbox when changing weapon state
            UpdateWeaponHitbox();

            // Reuse cameraAdjustedWeaponRect
            SDL.SDL_Rect cameraAdjustedWeaponRect = camera.Apply(weaponRect);

            IntPtr weaponTexture = GetCurrentWeaponTexture();
            if (weaponTexture != IntPtr.Zero)
            {
                SDL.SDL_RenderCopy(renderer, weaponTexture, IntPtr.Zero, ref cameraAdjustedWeaponRect);
            }
        }

    }
}
