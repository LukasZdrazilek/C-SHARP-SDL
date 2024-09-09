using SDL2;
using System;

namespace MyGame.Entities
{
    public class Platform
    {
        private SDL.SDL_Rect rect;

        public Platform(float x, float y, int width, int height)
        {
            rect = new SDL.SDL_Rect { x = (int)x, y = (int)y, w = width, h = height };
        }

        public SDL.SDL_Rect GetRect()
        {
            return rect;
        }

        public void Render(IntPtr renderer)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255); // Green color for the platforms
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }
    }
}
