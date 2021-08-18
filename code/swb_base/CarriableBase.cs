using Sandbox;
using Sandbox.UI;
using System;

namespace SWB_Base
{
    /// <summary>
    /// An entity that can be carried in the player's inventory and hands.
    /// </summary>
    public class CarriableBase : BaseCarriable
    {
        public override void ActiveStart(Entity ent)
        {
            //base.ActiveStart( ent );

            EnableDrawing = true;

            if (ent is Player player)
            {
                var animator = player.GetActiveAnimator();
                if (animator != null)
                {
                    SimulateAnimator(animator);
                }
            }

            //
            // If we're the local player (clientside) create viewmodel
            // and any HUD elements that this weapon wants
            //
            if (IsLocalPawn)
            {
                DestroyViewModel();
                DestroyHudElements();

                CreateViewModel();
                CreateHudElements();
            }
        }
    }
}
