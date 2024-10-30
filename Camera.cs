using SDL2;

namespace MyGame
{
    public class Camera
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        private int windowWidth;
        private int windowHeight;

        public Camera(int windowWidth, int windowHeight)
        {
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
            X = 0;
            Y = 0;
        }

        /////////////////////////////////////////////////////////////////////////////////// Update the camera to follow the player
        public void Update(float playerX, float playerY, int playerWidth, int playerHeight)
        {
            // Center the camera on the player
            X = playerX + playerWidth / 2 - windowWidth / 2;
            Y = 0;

            // Optional: Clamp X and Y to prevent the camera from moving out of bounds
            // X = Math.Max(0, Math.Min(X, 2000 - windowWidth)); // Assuming a world width of 2000
            // Y = Math.Max(0, Math.Min(Y, 2000 - windowHeight)); // Assuming a world height of 2000
        }

        //////////////////////////////////////////////////////////// Adjust the position of objects based on the camera's position
        public SDL.SDL_Rect Apply(SDL.SDL_Rect objectRect)
        {
            return new SDL.SDL_Rect
            {
                x = objectRect.x - (int)X,
                y = objectRect.y - (int)Y,
                w = objectRect.w,
                h = objectRect.h
            };
        }
    }
}
