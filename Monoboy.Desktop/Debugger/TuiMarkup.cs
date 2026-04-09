namespace Monoboy.Desktop.Debugger;

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>Spectre.Console markup width, clipping, and padding so lines never wrap mid-tag.</summary>
static class TuiMarkup
{
    static readonly Regex SpectreTag = new(@"\[[^\]]*\]", RegexOptions.Compiled);

    internal static string StripSpectreTags(string markup) => SpectreTag.Replace(markup, "");

    /// <summary>Split <paramref name="total"/> (full width incl. gaps between cols) across weighted columns.</summary>
    internal static int[] DistributeWidths(int total, int gap, int[] weights)
    {
        int n = weights.Length;
        int inner = total - (gap * (n - 1));
        inner = Math.Max(0, inner);
        if (inner == 0)
        {
            return new int[n];
        }

        int sum = weights.Sum();
        int[] w = new int[n];
        int assigned = 0;
        for (int i = 0; i < n; i++)
        {
            w[i] = inner * weights[i] / sum;
            assigned += w[i];
        }

        int rem = inner - assigned;
        for (int i = 0; rem > 0; i++)
        {
            w[i % n]++;
            rem--;
        }

        return w;
    }

    internal static string ClipMarkupToVisibleWidth(string markup, int maxVisible)
    {
        int v = VisibleLen(markup);
        if (v <= maxVisible)
        {
            return markup;
        }

        return ClipMarkup(markup, maxVisible);
    }

    internal static int VisibleLen(string markup) => StripSpectreTags(markup).Length;

    internal static string ClipMarkup(string markup, int maxVisible)
    {
        int v = VisibleLen(markup);
        if (v <= maxVisible)
        {
            return markup;
        }

        return ClipMarkupPreserveTags(markup, maxVisible);
    }

    /// <summary>
    /// Truncate to a visible character budget without stripping Spectre markup.
    /// </summary>
    internal static string ClipMarkupPreserveTags(string markup, int maxVisible)
    {
        if (maxVisible <= 0)
        {
            return "";
        }

        var segments = Regex.Split(markup, @"(\[[^\]]+\])");
        var sb = new StringBuilder();
        int visible = 0;
        int styleDepth = 0;

        foreach (string part in segments)
        {
            if (part.Length == 0)
            {
                continue;
            }

            if (part[0] == '[' && part[^1] == ']')
            {
                sb.Append(part);
                if (part == "[/]")
                {
                    styleDepth = Math.Max(0, styleDepth - 1);
                }
                else
                {
                    styleDepth++;
                }

                continue;
            }

            int remaining = maxVisible - visible;
            if (remaining <= 0)
            {
                break;
            }

            int take = Math.Min(part.Length, remaining);
            sb.Append(part.AsSpan(0, take));
            visible += take;
            if (visible >= maxVisible)
            {
                break;
            }
        }

        for (int i = 0; i < styleDepth; i++)
        {
            sb.Append("[/]");
        }

        return sb.ToString();
    }

    internal static string PadMarkup(string markup, int width)
    {
        int v = VisibleLen(markup);
        if (v >= width)
        {
            return ClipMarkup(markup, width);
        }
        return markup + new string(' ', width - v);
    }

    internal static string PadMarkupLeft(string markup, int width)
    {
        int v = VisibleLen(markup);
        if (v >= width)
        {
            return ClipMarkup(markup, width);
        }
        return new string(' ', width - v) + markup;
    }
}
