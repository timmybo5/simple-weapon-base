using System.Linq;

namespace SWB.Base;

public class ModelUtil
{
	public static void ParentToBone( GameObject gameObject, SkinnedModelRenderer target, string bone )
	{
		var targetBone = target.Model.Bones.AllBones.FirstOrDefault( b => b.Name == bone );
		var holdBoneGO = target.GetBoneObject( targetBone );

		gameObject.SetParent( holdBoneGO );
		gameObject.Transform.Position = holdBoneGO.Transform.Position;
		gameObject.Transform.Rotation = holdBoneGO.Transform.Rotation;
	}
}

