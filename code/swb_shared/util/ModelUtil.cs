/* 
 * Utility class for handling models
*/

namespace SWB.Shared;

public class ModelUtil
{
	public static void ParentToBone( GameObject gameObject, SkinnedModelRenderer target, string bone, int tries = 0, bool deleteOnFail = true, System.Action<GameObject> onFail = null )
	{
		if ( !gameObject.IsValid() ) return;

		void Fail( string message )
		{
			Log.Warning( message );
			onFail?.Invoke( gameObject );

			if ( deleteOnFail && gameObject.IsValid() )
				gameObject.Destroy();
		}

		if ( !target.IsValid() )
		{
			Fail( $"Could not parent {gameObject} to bone '{bone}' because target renderer is invalid" );
			return;
		}

		var holdBoneGO = target.GetBoneObject( bone );

		if ( !holdBoneGO.IsValid() || holdBoneGO.Scene is null )
		{
			// Try again 1 frame later, viewmodel edge case
			async void retry()
			{
				await GameTask.Delay( 1 );
				ParentToBone( gameObject, target, bone, tries + 1, deleteOnFail, onFail );
			}

			if ( tries < 10 )
				retry();
			else
				Fail( $"Could not get bone object from '{bone}' on {target}" );

			return;
		}

		try
		{
			gameObject.SetParent( holdBoneGO );
		}
		catch
		{
			//Log.Info( target.Model.ResourcePath );
			//Log.Info( holdBoneGO.Scene );
			//Log.Info( holdBoneGO.Scene == null );
			Fail( $"Could not parent {gameObject} to {holdBoneGO}" );
			return;
		}

		if ( !gameObject.IsValid() ) return;

		gameObject.WorldPosition = holdBoneGO.WorldPosition;
		gameObject.WorldRotation = holdBoneGO.WorldRotation;
		gameObject.Transform.ClearInterpolation();
	}
}
