using Sandbox;

namespace SWB_Base;

class LogUtil
{
    public static string Realm => Game.IsClient ? "(CLIENT)" : "(SERVER)";
    public static string Prefix => "[SWB] " + Realm + " ";

    public static void Info(string msg)
    {
        Log.Info(Prefix + msg);
    }

    public static void Info(object obj)
    {
        Info(obj.ToString());
    }

    public static void Error(string msg)
    {
        Log.Error(Prefix + msg);
    }

    public static void Error(object obj)
    {
        Error(obj.ToString());
    }
}
