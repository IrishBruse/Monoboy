namespace Monoboy.Debug;

/// <summary>Stop rules for statement step over, into, and out.</summary>
public static class GblDebugStepping
{
    public static bool StopOver(ushort startSp, int? startPoint, ushort sp, int? point)
    {
        return sp >= startSp && point != startPoint && point.HasValue;
    }

    public static bool StopInto(ushort startSp, int? startPoint, ushort sp, int? point)
    {
        if (sp < startSp)
        {
            return true;
        }

        return StopOver(startSp, startPoint, sp, point);
    }

    public static bool StopOut(ushort startSp, ushort sp, bool onPoint)
    {
        return sp > startSp && onPoint;
    }
}
