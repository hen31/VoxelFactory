using Stride.Engine;

namespace VoxelFactory
{
    class VoxelFactoryApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
