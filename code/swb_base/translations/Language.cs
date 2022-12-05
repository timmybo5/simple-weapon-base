using System;
using System.Collections.Generic;

namespace SWB_Base.Translations;

[Serializable]
public class Language
{
    public string Name { get; set; }
    public Dictionary<string, string> Translations { get; set; }
}
