using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace SWB.Editor;

/// <summary>
/// A horizontal slider with a text entry on the right
/// </summary>
public class SliderEntry : Panel
{
	public Slider Slider { get; protected set; }
	public TextEntry TextEntry { get; protected set; }

	public float sharedValue;

	public float MinValue
	{
		get => Slider.MinValue;
		set
		{
			Slider.MinValue = value;
			TextEntry.MinValue = value;
		}
	}

	public float MaxValue
	{
		get => Slider.MaxValue;
		set
		{
			Slider.MaxValue = value;
			TextEntry.MaxValue = value;
		}
	}

	public float Step
	{
		get => Slider.Step;
		set => Slider.Step = value;
	}

	public string Format
	{
		get => TextEntry.NumberFormat;
		set => TextEntry.NumberFormat = value;
	}

	public SliderEntry()
	{
		StyleSheet.Load( "/swb_editor/components/SliderEntry.cs.scss" );

		Slider = AddChild<Slider>();
		TextEntry = AddChild<TextEntry>();
		TextEntry.Numeric = true;
		TextEntry.NumberFormat = "0.###";

		Slider.AddEventListener( "value.changed", () => OnValueChanged( Slider.Value ) );
		TextEntry.AddEventListener( "value.changed", () => OnValueChanged( TextEntry.Text ) );
	}

	protected void OnValueChanged( object value )
	{
		float newValue = 0;
		if ( value is float floatValue )
		{
			newValue = (float)Math.Round( floatValue, 2 );
		}
		else if ( value is string stringValue )
		{
			newValue = float.Parse( stringValue );
		}

		Value = newValue;
		CreateValueEvent( "value", value );
	}

	/// <summary>
	/// The actual value. Setting the value will snap and clamp it.
	/// </summary>
	public float Value
	{
		get => sharedValue;
		set
		{
			sharedValue = value;
			Slider.Value = value;
			TextEntry.Value = value.ToString();
			TextEntry.Text = value.ToString();
		}
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name )
		{
			case "sensitivity":
			case "name":
			case "step":
			case "value":
			case "max":
			case "min":
				Slider.SetProperty( name, value );
				return;
		}

		if ( name == "format" )
		{
			TextEntry.NumberFormat = value;
			return;
		}

		base.SetProperty( name, value );
	}
}

public static class SliderWithEntryConstructor
{
	public static SliderEntry SliderWithEntry( this PanelCreator self, float min, float max, float step )
	{
		var control = self.panel.AddChild<SliderEntry>();
		control.MinValue = min;
		control.MaxValue = max;
		control.Step = step;

		return control;
	}
}
