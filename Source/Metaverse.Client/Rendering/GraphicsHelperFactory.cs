
namespace OSMP
{
    class GraphicsHelperFactory
    {
        static IGraphicsHelper instance = new GraphicsHelperGl();
        public static IGraphicsHelper GetInstance()
        {
            return instance;
        }
        public static void SetGraphicsHelper( IGraphicsHelper newinstance )
        {
            instance = newinstance;
        }
    }
}

