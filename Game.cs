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
        private bool paused;
        private Entities.Player player;
        private List<Platform> platforms;
        private Entities.Enemy1 enemy1;
        private Stopwatch frameTimer;
        private Camera camera;
        private Entities.Interface gameInterface;

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

            // Renderer VSYNC = ON
            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine("Renderer could not be created! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                Environment.Exit(1);
            }


            /////////////////////////////////////////////////////////////////////////////////////////// Inicializace entit
            player = new Entities.Player(375, 250, 80, 120);
            player.LoadTextures(renderer);

            enemy1 = new Entities.Enemy1(1800, 571, 79, 129);
            enemy1.LoadTexture(renderer);

            platforms = new List<Platform>
            {
                new Platform(0, 700, 2500, 10),
                new Platform(500, 440, 300, 100),
                new Platform(1050, 300, 100, 400)
            };

            camera = new Camera(1280, 720);
            gameInterface = new Entities.Interface("font.ttf", 24);

            quit = false;
            paused = false;
            frameTimer = new Stopwatch();
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////// Run
        public void Run()
        {
            SDL.SDL_Event e;
            frameTimer.Start();

            while (!quit)
            {
                // Výpočet deltaTime
                frameTimer.Stop();
                float deltaTime = Math.Min((float)frameTimer.Elapsed.TotalSeconds, 0.1f);   // to avoid extreme values
                frameTimer.Restart();

                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                    }
                    // Pause button na 'P'
                    else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_p)
                    {
                        paused = !paused;
                    }
                    // Fix na hybani windowed hry
                    else if (e.type == SDL.SDL_EventType.SDL_WINDOWEVENT)
                    {
                        if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST)
                        {
                            paused = true;
                        }
                        else if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
                        {
                            paused = false;
                        }
                    }
                }

                var keys = SDL.SDL_GetKeyboardState(out int numKeys);

                /////////////////////////////////////////////////////////////// Ovladani, update, enemy logic, kolize, kamera
                if(!paused)
                {
                    player.HandleInput(keys, deltaTime);
                    player.Update(deltaTime);
                    enemy1.Logic(player, keys, deltaTime);

                    player.CheckPlatformCollisions(platforms, deltaTime);

                    // Update camera to follow the player
                    camera.Update(player.X, player.Y, player.Width, player.Height);
                }

                /////////////////////////////////////////////////////////////////////////////////////////////// Render objektu
                SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                SDL.SDL_RenderClear(renderer);
                SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                player.Render(renderer, camera);
                enemy1.Render(renderer, camera);
                foreach (var platform in platforms)
                {
                    platform.Render(renderer, camera);
                }
                gameInterface.RenderHP(renderer, player.HP);

                SDL.SDL_RenderPresent(renderer);
            }

            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            gameInterface.CleanUp();
            SDL.SDL_Quit();
        }
    }
}
