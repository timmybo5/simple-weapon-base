using Sandbox.UI;
using SWB.Base;
using System;

namespace SWB.Editor;

public enum DragMode
{
	pos = 0,
	angle = 1,
}

public partial class OffsetEditor
{
	public float X { get; set; } = 0f;
	public float Y { get; set; } = 0f;
	public float Z { get; set; } = 0f;

	public float Pitch { get; set; } = 0f;
	public float Yaw { get; set; } = 0f;
	public float Roll { get; set; } = 0f;
	public float Sensitivity { get; set; } = 0f;

	public float FOV { get; set; } = 75f;

	public Label DragModeLabel { get; set; }

	DragMode dragMode = DragMode.pos;

	float startX;
	float startY;
	float xOrigin;
	float zOrigin;
	float pitchOrigin;
	float yawOrigin;

	Weapon weapon;

	public OffsetEditor( Weapon weapon )
	{
		this.weapon = weapon;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime ) return;

		DragModeLabel.Text = "x/z";
		FOV = weapon.FOV;
	}

	public virtual void OnReset()
	{
		X = 0;
		Y = 0;
		Z = 0;
		Pitch = 0;
		Yaw = 0;
		Roll = 0;
		FOV = weapon.FOV;
	}

	public virtual void OnPrint()
	{
		Log.Info( "-- " + weapon.DisplayName );
		Log.Info( String.Format( "Angle = {0:0.###}f, {1:0.###}f, {2:0.###}f", Pitch, Yaw, Roll ) );
		Log.Info( String.Format( "Pos = {0:0.###}f, {1:0.###}f, {2:0.###}f", X, Y, Z ) );
		Log.Info( String.Format( "FOV = {0:0.###}f", FOV ) );
		Log.Info( "--" );
	}

	// No dragging if not directly on base panel
	public virtual bool CanDragOnPanel( Panel p )
	{
		return p.ElementName == "offseteditor";
	}

	public void SetAimAnimData()
	{
		SetFromAngPos( weapon.AimAnimData, weapon.AimFOV );
	}

	public void SetRunAnimData()
	{
		SetFromAngPos( weapon.RunAnimData, weapon.FOV );
	}

	public void SetCustomizeAnimData()
	{
		SetFromAngPos( weapon.CustomizeAnimData, weapon.FOV );
	}

	private void SetFromAngPos( AngPos angPos, float fov )
	{
		X = angPos.Pos.x;
		Y = angPos.Pos.y;
		Z = angPos.Pos.z;
		Pitch = angPos.Angle.pitch;
		Yaw = angPos.Angle.yaw;
		Roll = angPos.Angle.roll;
		FOV = fov;
	}

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( !HasActive || !CanDragOnPanel( e.Target ) ) return;

		if ( dragMode == DragMode.pos )
		{

			X = xOrigin - (startX - Mouse.Position.x) * 0.001f;
			Z = zOrigin + (startY - Mouse.Position.y) * 0.001f;
		}
		else
		{
			Yaw = yawOrigin + (startX - Mouse.Position.x) * 0.01f;
			Pitch = pitchOrigin - (startY - Mouse.Position.y) * 0.01f;
		}

		SkipTransitions();
		e.StopPropagation();
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		if ( dragMode == DragMode.pos )
		{
			DragModeLabel.Text = "yaw/pitch";
			dragMode = DragMode.angle;
		}
		else
		{
			DragModeLabel.Text = "x/z";
			dragMode = DragMode.pos;
		}
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		if ( !CanDragOnPanel( e.Target ) ) return;

		startX = Mouse.Position.x;
		startY = Mouse.Position.y;

		xOrigin = X;
		zOrigin = Z;
		pitchOrigin = Pitch;
		yawOrigin = Yaw;

		e.StopPropagation();
	}

	public override void Tick()
	{
		if ( weapon is null || weapon.ViewModelHandler is null ) return;

		weapon.ViewModelHandler.EditorOffset = new AngPos
		{
			Angle = new Angles( Pitch, Yaw, Roll ),
			Pos = new Vector3( X, Y, Z )
		};

		weapon.ViewModelHandler.EditorFOV = FOV;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( DateTime.Now.ToString() );
	}
}
