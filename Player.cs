using SDL2;
using System;

namespace MyGame.Entities
{
    public class Player
    {
        private SDL.SDL_Rect rect;
        private float posX, posY;
        private float velocityX, velocityY;
        private float speed = 700; // horizontal speed
        private float jumpSpeed = 500; // initial jump velocity
        private float gravity = 5000; // gravity strength
        private bool isJumping = false;
        private bool isFalling = false;
        private float jumpStartY;
        private float maxJumpHeight = 120; // maximum jump height

        public Player(float x, float y, int width, int height)
        {
            this.posX = x;
            this.posY = y;
            rect = new SDL.SDL_Rect { x = (int)x, y = (int)y, w = width, h = height };
        }

        public void HandleInput(IntPtr keys, float deltaTime)
        {
            unsafe
            {
                byte* keyState = (byte*)keys;

                // Horizontal movement
                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_A] == 1)
                {
                    velocityX = -speed;
                }
                else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_D] == 1)
                {
                    velocityX = speed;
                }
                else
                {
                    velocityX = 0;
                }

                // Jumping mechanics
                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE] == 1 && !isJumping && !isFalling)
                {
                    isJumping = true;
                    jumpStartY = posY;
                    velocityY = -jumpSpeed; // Set initial jump velocity
                }

                if (isJumping)
                {
                    // Continue jumping while Spacebar is held and within max height
                    if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_SPACE] == 1 && posY > jumpStartY - maxJumpHeight)
                    {
                        velocityY -= gravity * deltaTime; // Decrease upward velocity due to gravity
                    }
                    else
                    {
                        isJumping = false;
                        isFalling = true;
                    }
                }

                // Apply gravity if falling
                if (isFalling)
                {
                    velocityY += gravity * deltaTime; // Increase downward velocity due to gravity
                }
            }
        }

        public void Update(float deltaTime)
        {
            // Apply velocity to position before checking collisions
            posX += velocityX * deltaTime;
            posY += velocityY * deltaTime;

            // Update the SDL_Rect with the new position
            rect.x = (int)posX;
            rect.y = (int)posY;

            // Check collisions
            //CheckPlatformCollisions(platforms, deltaTime);
        }

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

            bool wasColliding = false;

            foreach (var platform in platforms)
            {
                SDL.SDL_Rect platformRect = platform.GetRect();

                // Check for intersection with previous and current bounds
                if (SDL.SDL_HasIntersection(ref playerBounds, ref platformRect) == SDL.SDL_bool.SDL_TRUE ||
                    SDL.SDL_HasIntersection(ref previousPlayerBounds, ref platformRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    wasColliding = true;

                    // Kolize zvrchu
                    if (velocityY > 0 && previousPlayerBounds.y + previousPlayerBounds.h <= platformRect.y && playerBounds.y + playerBounds.h > platformRect.y)
                    {
                        posY = platformRect.y - playerBounds.h;
                        velocityY = 0.0f;
                        isFalling = false;
                        isJumping = false;  // Allow jumping again
                    }
                    // Kolize zespodu
                    else if (velocityY < 0 && playerBounds.y < platformRect.y + platformRect.h && playerBounds.y + playerBounds.h > platformRect.y + platformRect.h)
                    {
                        posY = platformRect.y + platformRect.h;
                        velocityY = (gravity + jumpSpeed + 400000) * deltaTime;
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
                }
            }

            // If no collisions are detected and player is not jumping, set falling to true
            if (!wasColliding && !isJumping)
            {
                isFalling = true;
            }
        }





        public void Render(IntPtr renderer)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255); // Red color for the player
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }
    }
}
