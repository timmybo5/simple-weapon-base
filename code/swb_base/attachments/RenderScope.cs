
using Sandbox;
using SWB_Base.UI;

namespace SWB_Base.Attachments;

public class RenderScope : Sight
{
    public override string Name => "Render Scope";
    public override string Description => "Used by other attachments to be able to attach to the weapon.";
    public override string[] Positives => new string[]
    {
    };

    public override string[] Negatives => new string[]
    {
    };

    public override StatModifier StatModifier => new StatModifier
    {
    };

    private RenderScopeRT renderScopeRT;

    public override void OnEquip(WeaponBase weapon, AttachmentModel attachmentModel)
    {
        base.OnEquip(weapon, attachmentModel);

        if (Game.IsClient)
        {
            renderScopeRT = new RenderScopeRT(attachmentModel.SceneObject);
            renderScopeRT.Parent = Game.RootPanel;
        }
    }

    public override void OnUnequip(WeaponBase weapon)
    {
        base.Unequip(weapon);

        if (Game.IsClient && renderScopeRT != null)
            renderScopeRT.Delete(true);
    }
}

public class HunterScope : RenderScope
{
    public override string Name => "Hunter Scope x12";
    public override string IconPath => "attachments/swb/sight/scope_hunter/ui/icon.png";
    public override string ModelPath => "attachments/swb/sight/scope_hunter/w_scope_hunter.vmdl";
}
