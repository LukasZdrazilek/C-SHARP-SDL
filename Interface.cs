using SDL2;
using System;

namespace MyGame.Entities
{
    public class Interface
    {
        private IntPtr font;
        private IntPtr cachedMessageTexture;  // Cache the texture
        private string cachedText = "";       // Track the last rendered text
        private int cachedHP = -1;            // Track the last HP value
        private SDL.SDL_Color white = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };  // Cached color

        public Interface(string fontPath, int fontSize)
        {
            // Initialize SDL_ttf
            if (SDL_ttf.TTF_Init() == -1)
            {
                throw new Exception("Failed to initialize SDL_ttf: " + SDL.SDL_GetError());
            }

            // Load the font once in the constructor
            font = SDL_ttf.TTF_OpenFont(fontPath, fontSize);
            if (font == IntPtr.Zero)
            {
                throw new Exception("Failed to load font: " + SDL.SDL_GetError());
            }

            cachedMessageTexture = IntPtr.Zero; // Initialize the cache texture to empty
        }

        // Method to render the HP text in the top-left corner
        public void RenderHP(IntPtr renderer, int hp)
        {
            // Only create new texture if HP changed
            if (hp != cachedHP)
            {
                // Clean up previous texture if any
                if (cachedMessageTexture != IntPtr.Zero)
                {
                    SDL.SDL_DestroyTexture(cachedMessageTexture);
                    cachedMessageTexture = IntPtr.Zero;
                }

                cachedHP = hp;
                cachedText = "HP: " + hp;

                // Render the text to a surface
                IntPtr surfaceMessage = SDL_ttf.TTF_RenderText_Solid(font, cachedText, white);

                // Convert surface to a texture and cache it
                cachedMessageTexture = SDL.SDL_CreateTextureFromSurface(renderer, surfaceMessage);
                SDL.SDL_FreeSurface(surfaceMessage); // Free the surface after texture creation
            }

            // Set the rectangle where the text will be displayed (top-left corner)
            SDL.SDL_Rect messageRect = new SDL.SDL_Rect { x = 0, y = 0, w = 100, h = 50 };

            // Render the cached texture
            SDL.SDL_RenderCopy(renderer, cachedMessageTexture, IntPtr.Zero, ref messageRect);
        }

        // Clean up method to close font and quit SDL_ttf
        public void CleanUp()
        {
            // Destroy cached texture
            if (cachedMessageTexture != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(cachedMessageTexture);
                cachedMessageTexture = IntPtr.Zero;
            }

            if (font != IntPtr.Zero)
            {
                SDL_ttf.TTF_CloseFont(font);
                font = IntPtr.Zero;
            }

            SDL_ttf.TTF_Quit();
        }
    }
}
