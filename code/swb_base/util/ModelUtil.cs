using System.Linq;

namespace SWB.Base;

public class ModelUtil
{
	public static void ParentToBone( GameObject gameObject, SkinnedModelRenderer target, string bone )
	{
		var targetBone = target.Model.Bones.AllBones.FirstOrDefault( b => b.Name == bone );
		if ( targetBone is null )
		{
			Log.Error( $"Could not find bone '{bone}' on {target}" );
			return;
		}

		var holdBoneGO = target.GetBoneObject( targetBone );
		gameObject.SetParent( holdBoneGO );
		gameObject.WorldPosition = holdBoneGO.WorldPosition;
		gameObject.WorldRotation = holdBoneGO.WorldRotation;
		gameObject.Transform.ClearInterpolation();
	}
}
