using Sandbox;
using SWB_Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWB_CSS
{
	[Library( "swb_css_ak47", Title = "AK-47" )]
	partial class AK47Dual : AK47
	{
		public override bool DualWield => true;

		public AK47Dual() : base() {}
	}
}
