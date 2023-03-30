using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Html;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SWB_Editor;

/// <summary>
/// A UI control which provides multiple options via a dropdown box
/// </summary>
//[Library("swb_Select"), Alias("swb_Dropdown")]
public class DropDown : PopupButton
{
    protected IconPanel DropdownIndicator;

    /// <summary>
    /// The options to show on click. You can edit these directly via this property.
    /// </summary>
    public List<Option> Options { get; } = new();

    Option selected;

    /// <summary>
    /// The current string value. This is useful to have if Selected is null.
    /// </summary>
    public string Value { get; protected set; }

    /// <summary>
    /// The currently selected option
    /// </summary>
    public Option Selected
    {
        get => selected;
        set
        {
            if (selected == value) return;

            selected = value;

            if (selected != null)
            {
                Value = $"{selected.Value}";
                Icon = selected.Icon;
                Text = selected.Title;

                CreateEvent("onchange");
                CreateValueEvent("value", selected?.Value);
            }
        }
    }

    public DropDown()
    {
        StyleSheet.Load("/swb_player/editor/ui/components/DropDown.scss");

        AddClass("dropdown");
        DropdownIndicator = Add.Icon("expand_more", "dropdown_indicator");
    }

    public DropDown(Panel parent) : this()
    {
        Parent = parent;
    }

    public override void SetPropertyObject(string name, object value)
    {
        if (name == "value")
        {
            // If we have no options, try to populate them using the type
            // of the object
            if (Options.Count == 0 && value != null)
                PopulateOptionsFromType(value.GetType());

            Select(value?.ToString(), false);
            return;
        }

        base.SetPropertyObject(name, value);
    }

    /// <summary>
    /// Given the type, populate options. This is useful if you're an enum type.
    /// </summary>
    private void PopulateOptionsFromType(Type type)
    {
        if (type == typeof(bool))
        {
            Options.Add(new Option("True", true));
            Options.Add(new Option("False", false));
            return;
        }

        if (type.IsEnum)
        {
            var names = type.GetEnumNames();
            var values = type.GetEnumValues();

            for (int i = 0; i < names.Length; i++)
            {
                Options.Add(new Option(names[i], values.GetValue(i).ToString()));
            }

            return;
        }

        Log.Info($"Dropdown Type: {type}");
    }

    /// <summary>
    /// Open the dropdown
    /// </summary>
    public override void Open()
    {
        Popup = new Popup(this, Popup.PositionMode.BelowStretch, 0.0f);
        Popup.AddClass("flat-top");

        foreach (var option in Options)
        {
            var o = Popup.AddOption(option.Title, option.Icon, () => Select(option));
            if (Selected != null && option.Value == Selected.Value)
            {
                o.AddClass("active");
            }
        }
    }

    /// <summary>
    /// Select an option
    /// </summary>
    protected virtual void Select(Option option, bool triggerChange = true)
    {
        if (!triggerChange)
        {
            selected = option;

            if (option != null)
            {
                Value = $"{option.Value}";
                Icon = option.Icon;
                Text = option.Title;
            }
        }
        else
        {
            Selected = option;
        }
    }

    /// <summary>
    /// Select an option by value string
    /// </summary>
    protected virtual void Select(string value, bool triggerChange = true)
    {
        if (Value == value) return;
        Value = value;

        Select(Options.FirstOrDefault(x => string.Equals(x.Value.ToString(), value, StringComparison.OrdinalIgnoreCase)), triggerChange);
    }


    /// <summary>
    /// Give support for option elements in html template
    /// </summary>
    public override bool OnTemplateElement(INode element)
    {
        Options.Clear();

        foreach (var child in element.Children)
        {
            if (!child.IsElement) continue;

            //
            // 	<select> <-- this DropDown control
            //		<option value="#f00">Red</option> <-- option
            //		<option value="#ff0">Yellow</option> <-- option
            //		<option value="#0f0">Green</option> <-- option
            // </select>
            //
            if (child.Name.Equals("option", StringComparison.OrdinalIgnoreCase))
            {
                var o = new Option();

                o.Title = child.InnerHtml;
                o.Value = child.GetAttribute("value", o.Title);
                o.Icon = child.GetAttribute("icon", null);

                Options.Add(o);
            }
        }

        Select(Value);
        return true;
    }
}
