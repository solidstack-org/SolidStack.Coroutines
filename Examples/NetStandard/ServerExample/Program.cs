using SolidStack.Coroutines;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ServerExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CoroutineService coroutineService = new CoroutineService();
            ServerCoroutineServiceRunner coroutineRunner = new ServerCoroutineServiceRunner(coroutineService);

            Player player1 = new Player(coroutineService);

            player1.MovePlayer();

            // Run server logic
            while (true)
            {
                // Tick Coroutines
                coroutineRunner.Tick();

                // Assuming server ticks 66 times a second
                await Task.Delay(15);
            }
        }
    }
}
