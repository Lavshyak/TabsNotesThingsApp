using System;
using System.Collections.Generic;

namespace TabsNotesThings.ViewModels;

public sealed class NotePitches
{
    public static NotePitches Instance { get; } = new NotePitches();

    public decimal A4PitchHz { get; }

    public NotePitches(decimal a4PitchHz = 440m)
    {
        if (a4PitchHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(a4PitchHz));

        A4PitchHz = a4PitchHz;
    }

    /// <summary>
    /// Возвращает частоту ноты относительно A4.
    /// halfToneOffset:
    ///  0  -> A4
    ///  1  -> A#4
    /// -1  -> G#4
    /// 12  -> A5
    /// </summary>
    public decimal GetFrequency(int halfToneOffset)
    {
        double factor = Math.Pow(2.0, halfToneOffset / 12.0);
        return A4PitchHz * (decimal)factor;
    }

    public IEnumerable<decimal> FromA4Up()
    {
        for (int i = 0;; i++)
            yield return GetFrequency(i);
    }

    public IEnumerable<decimal> FromA4Down()
    {
        for (int i = 0;; i--)
            yield return GetFrequency(i);
    }
}