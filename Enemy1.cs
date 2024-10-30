using SDL2;
using System;

namespace MyGame.Entities
{
    public class Enemy1
    {
        private SDL.SDL_Rect rect;
        private IntPtr texture1;
        private int HP = 5;
        private float lastHitTime = 0f; // Tracks time since last damage
        private const float damageCooldown = 0.3f;
        private const float playerDamageCooldown = 1.0f;

        // Knockback variables
        private const float knockbackVelocity= 400f; // Adjust as necessary for knockback
        private float knockbackTime = 0.1f; // Time during which knockback is applied
        private float currentKnockbackTimer = 0f; // Timer to apply knockback
        private int knockbackDirection = 0; // 0 = no knockback, -1 = left, 1 = right

        // Idle movement variables
        private const float idleSpeed = 150f; // Slow movement speed for idle
        private const float idleDuration = 3f; // Duration to move in one direction (3 seconds)
        private float idleTimer = 0f; // Timer to track idle movement
        private int idleDirection = -1; // -1 = move left, 1 = move right
        private bool playerSpotted = false;

        // Attack1 variables
        private float stabTimer = 0f;
        private float stabCooldown = 2f;
        private float stabSpeed = 500f;

        private const float stabDuration = 0.2f;  // Duration of the stab (in seconds)
        private bool isStabbing = false;

        // Combo tracking
        private int lastComboStageHit = -1; // Last combo stage that hit the enemy


        public Enemy1(float x, float y, int width, int height)
        {
            rect = new SDL.SDL_Rect { x = (int)x, y = (int)y, w = width, h = height };
        }

        // Method to load the texture
        public void LoadTexture(IntPtr renderer)
        {
            IntPtr loadedTexture = SDL_image.IMG_LoadTexture(renderer, "Textures/marine_red2.png");
            if (loadedTexture == IntPtr.Zero)
            {
                throw new Exception("Failed to load texture: " + SDL.SDL_GetError());
            }
            texture1 = loadedTexture;
        }

        public void FreeTexture()
        {
            if (texture1 != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(texture1);
                texture1 = IntPtr.Zero;
            }
        }

        public SDL.SDL_Rect GetRect()
        {
            return rect;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////// Stav idle
        public void IdleMovement(float deltaTime)
        {
            idleTimer += deltaTime;

            // Move enemy in the current idle direction
            rect.x += (int)(idleDirection * idleSpeed * deltaTime);

            // If the timer exceeds the idle duration, switch direction
            if (idleTimer >= idleDuration)
            {
                idleDirection *= -1; // Reverse direction
                idleTimer = 0f;      // Reset timer
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////// Stav boje
        public void Fight(Player player, float deltaTime)
        {
            SDL.SDL_Rect playerRect = player.GetRect();

            Attack(player, deltaTime);

            /////////////////////////////// Utok na hrace
            if (SDL.SDL_HasIntersection(ref playerRect, ref rect) == SDL.SDL_bool.SDL_TRUE && lastHitTime >= playerDamageCooldown)
            {
                // Perform the attack
                if (player.IsGuarding() == false || player.GetRect().x < rect.x && player.facingLeft || player.GetRect().x > rect.x && player.facingRight)
                {
                    player.TakeDamage(1, rect.x);
                }

                // Reset the cooldown timer
                lastHitTime = 0f;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// Utok
        public void Attack(Player player, float deltaTime)
        {
            stabTimer += deltaTime;

            if (stabTimer >= stabCooldown && !isStabbing)
            {
                // Start the stab
                isStabbing = true;
                stabTimer = 0f;  // Reset the stab timer
            }

            if (isStabbing)
            {
                stabTimer += deltaTime;

                // Perform the stab movement for a short duration
                if (player.GetRect().x > rect.x)
                {
                    rect.x += (int)(stabSpeed * deltaTime);  // Move right toward the player
                }
                else if (player.GetRect().x < rect.x)
                {
                    rect.x -= (int)(stabSpeed * deltaTime);  // Move left toward the player
                }

                // End the stab after the stab duration
                if (stabTimer >= stabDuration)
                {
                    isStabbing = false;
                    stabTimer = 0f;  // Reset the attack timer after the stab
                }
            }
        }


        //////////////////////////////////////////////////////////////////////////////////// Logika nepritele (pohyb, utok, damage)
        public void Logic(Player player, IntPtr keys, float deltaTime)
        {
            if (HP > 0)
            {
                Console.WriteLine(HP);

                lastHitTime += deltaTime;

                ///////////////////////////////////////////////////////////////////////// Pohyb
                if (player.GetRect().x > rect.x - 250 && player.GetRect().x < rect.x + 250)
                {
                    playerSpotted = true;
                    Fight(player, deltaTime);
                }
                else
                {
                    if (!playerSpotted)
                        IdleMovement(deltaTime);
                }

                // Knockback timer
                if (currentKnockbackTimer > 0)
                {
                    currentKnockbackTimer -= deltaTime;
                    rect.x += (int)(knockbackDirection * knockbackVelocity * deltaTime);
                }

                //////////////////////////////////////////////////////////////////////////////////////////////// Utok na enemy
                if (player.IsAttacking() && SDL.SDL_HasIntersection(ref player.weaponRect, ref rect) == SDL.SDL_bool.SDL_TRUE)
                {
                    int currentComboStage = player.currentComboStage;

                    // Check if the current combo stage is greater than the last hit stage
                    if (currentComboStage > lastComboStageHit)
                    {
                        HP--; // Deal damage
                        lastComboStageHit = currentComboStage; // Update last hit stage

                        // Determine knockback direction
                        knockbackDirection = player.GetRect().x < rect.x ? 1 : -1;
                        currentKnockbackTimer = knockbackTime; // Reset knockback timer
                    }
                }

                // Reset logic when the player is not attacking
                if (!player.IsAttacking())
                {
                    lastComboStageHit = -1; // Reset last hit stage
                }
            }
        }


        public void Render(IntPtr renderer, Camera camera)
        {
            if (HP > 0)
            {
                SDL.SDL_Rect cameraAdjustedRect = camera.Apply(rect);
                SDL.SDL_RenderCopy(renderer, texture1, IntPtr.Zero, ref cameraAdjustedRect); // Use SDL_RenderCopy to render the texture
            }
        }
    }
}
