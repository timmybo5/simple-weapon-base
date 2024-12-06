using System.Linq;

namespace SWB.Base;

public class ModelUtil
{
	public static void ParentToBone( GameObject gameObject, SkinnedModelRenderer target, string bone, int tries = 0 )
	{
		var targetBone = target.Model.Bones.AllBones.FirstOrDefault( b => b.Name == bone );
		if ( targetBone is null )
		{
			Log.Error( $"Could not find bone '{bone}' on {target}" );
			return;
		}

		var holdBoneGO = target.GetBoneObject( targetBone );
		if ( holdBoneGO is null )
		{
			// Try again 1 frame later, viewmodel edge case
			async void retry()
			{
				await GameTask.Delay( 1 );
				ParentToBone( gameObject, target, bone, tries++ );
			}

			if ( tries < 10 )
				retry();
			else
				Log.Error( $"Could not get bone object from '{bone}' on {target}" );

			return;
		}

		gameObject.SetParent( holdBoneGO );
		gameObject.WorldPosition = holdBoneGO.WorldPosition;
		gameObject.WorldRotation = holdBoneGO.WorldRotation;
		gameObject.Transform.ClearInterpolation();
	}
}
