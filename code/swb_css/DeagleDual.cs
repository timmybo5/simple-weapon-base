using Sandbox;
using SWB_Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWB_CSS
{
	[Library( "swb_css_deagle_dual", Title = "Desert Eagle" )]
	partial class DeagleDual : Deagle
	{
		public override bool DualWield => true;

		public DeagleDual() : base()
		{}
	}
}
