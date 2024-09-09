using MyGame.Entities;
using SDL2;
using System;
using System.Diagnostics;

namespace MyGame
{
    public class Game
    {
        private IntPtr window;
        private IntPtr renderer;
        private bool quit;
        private Entities.Player player;
        private List<Platform> platforms;
        private Stopwatch frameTimer;

        public Game()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                Environment.Exit(1);
            }

            window = SDL.SDL_CreateWindow("SDL2 Rectangle Player", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 1280, 720, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (window == IntPtr.Zero)
            {
                Console.WriteLine("Window could not be created! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_Quit();
                Environment.Exit(1);
            }

            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine("Renderer could not be created! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                Environment.Exit(1);
            }

            player = new Entities.Player(375, 250, 50, 50);

            platforms = new List<Platform>
            {
                new Platform(0, 700, 1280, 10),
                new Platform(500, 440, 300, 100),
                new Platform(1050, 300, 100, 400)
            };

            quit = false;
            frameTimer = new Stopwatch();
        }

        public void Run()
        {
            SDL.SDL_Event e;
            frameTimer.Start();

            while (!quit)
            {
                // Výpočet deltaTime
                frameTimer.Stop();
                float deltaTime = (float)frameTimer.Elapsed.TotalSeconds;
                frameTimer.Restart();

                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                    }
                }

                var keys = SDL.SDL_GetKeyboardState(out int numKeys);

                player.HandleInput(keys, deltaTime);
                player.Update(deltaTime);

                player.CheckPlatformCollisions(platforms, deltaTime);

                SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                SDL.SDL_RenderClear(renderer);

                SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                // Render objektu
                player.Render(renderer);
                foreach (var platform in platforms)
                {
                    platform.Render(renderer);
                }

                SDL.SDL_RenderPresent(renderer);
            }

            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
