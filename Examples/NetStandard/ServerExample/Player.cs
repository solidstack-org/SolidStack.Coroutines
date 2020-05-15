using SolidStack.Coroutines;
using System;
using System.Collections;
using System.Numerics;

namespace ServerExample
{
    class Player
    {
        private readonly ICoroutineService _coroutineService;

        public Vector2 Position = Vector2.Zero;

        public Player(ICoroutineService coroutineService)
        {
            _coroutineService = coroutineService;
        }

        public void MovePlayer()
        {
            _coroutineService.StartCoroutine(MovePlayerCoro());
        }

        private IEnumerator MovePlayerCoro()
        {
            var origin = Position;

            while(Position.X < 4)
            {
                Position.X += 1;
                Console.WriteLine($"Moving player: {Position}");
                yield return _coroutineService.WaitForSeconds(0.5f);
            }

            Console.WriteLine("Done moving");
        }
    }
}
