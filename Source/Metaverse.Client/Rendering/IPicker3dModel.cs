
namespace OSMP
{
    public class HitTarget
    {
    }
        
    public interface IPicker3dModel
    {
        void AddHitTarget( HitTarget hittarget );
        void EndHitTarget();
        HitTarget GetClickedHitTarget( IRenderable renderable, int MouseX, int MouseY );
        HitTarget GetClickedHitTarget( int iWindowX, int iWindowY );
    }
}
