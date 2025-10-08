/* 
 * Utility class for handling models
*/

namespace SWB.Shared;

public class ModelUtil
{
	public static void ParentToBone( GameObject gameObject, SkinnedModelRenderer target, string bone, int tries = 0 )
	{
		var holdBoneGO = target.GetBoneObject( bone );

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
