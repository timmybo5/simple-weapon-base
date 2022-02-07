using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace SWB_Base.Editor
{
    public partial class Popup
    {
        static List<Popup> AllPopups = new();

        public static void CloseAll(Panel exceptThisOne = null)
        {
            foreach (var panel in AllPopups.ToArray())
            {
                if (panel == exceptThisOne) continue;

                panel.Delete();
            }
        }

        [Event("ui.closepopups")]
        public static void ClosePopupsEvent(object obj)
        {
            Popup floater = null;

            if (obj is Panel panel)
            {
                floater = panel.AncestorsAndSelf.OfType<Popup>().FirstOrDefault();
            }

            CloseAll(floater);
        }
    }


}
