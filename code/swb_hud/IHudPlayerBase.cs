
using SWB.Shared;

namespace SWB.HUD;

public interface IHudPlayerBase : IPlayerBase
{
	public IInventory Inventory { get; }
	public int Health { get; }
}
