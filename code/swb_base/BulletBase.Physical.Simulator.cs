using System;
using System.Collections.Generic;
using System.Globalization;
using Sandbox;

/* 
 * Simulator for the physical bullets making sure every projectile is lag compensated
*/

namespace SWB_Base
{
    public partial class BulletSimulator
    {
        public List<BulletEntity> List { get; private set; }

        public BulletSimulator()
        {
            List = new();
        }

        public void Add(BulletEntity projectile)
        {
            List.Add(projectile);
        }

        public void Remove(BulletEntity projectile)
        {
            List.Remove(projectile);
        }

        public void Simulate()
        {
            using (Entity.LagCompensation())
            {
                for (int i = List.Count - 1; i >= 0; i--)
                {
                    var projectile = List[i];

                    if (!projectile.IsValid())
                    {
                        List.RemoveAt(i);
                        continue;
                    }

                    if (Prediction.FirstTime)
                        projectile.BulletPhysics();
                }
            }
        }
    }
}
