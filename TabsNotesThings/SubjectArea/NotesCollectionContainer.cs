using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

public class NotesCollectionContainer
{
    public static NotesCollectionContainer InstanceFromMinus2To10 =
        new NotesCollectionContainer(new NoteWithOctave(NoteEnum.C, -2), new NoteWithOctave(NoteEnum.B, 10));

    public record NoteInfo(NoteWithOctave NoteWithOctave, int IndexRelativeToA4, decimal Hz);

    public IReadOnlyDictionary<int, NoteInfo> IndexRelativeToA4ToNoteInfoDictionary { get; }

    public NotesCollectionContainer(NoteWithOctave from, NoteWithOctave to)
    {
        var fromIdx = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(from.NoteEnum, from.Octave);
        var toIdx = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(to.NoteEnum, to.Octave);
        IndexRelativeToA4ToNoteInfoDictionary = Enumerable.Range(fromIdx, toIdx - fromIdx+1).Select(idx =>
        {
            var (note, octave) = NoteStrs.Instance.ToNoteAndOctave(idx);
            var hz = NotePitches.Instance.GetFrequency(idx);
            var noteInfo = new NoteInfo(new NoteWithOctave(note, octave), idx, hz);
            var kvp = KeyValuePair.Create(idx, noteInfo);
            return kvp;
        }).ToFrozenDictionary();
    }

    public NoteInfo FindNearest(int anchorIdxRelativeToA4, NoteEnum noteEnum)
    {
        NoteInfo right;
        for (int i = anchorIdxRelativeToA4;; i++)
        {
            if (IndexRelativeToA4ToNoteInfoDictionary[i].NoteWithOctave.NoteEnum == noteEnum)
            {
                right = IndexRelativeToA4ToNoteInfoDictionary[i];
                break;
            }
        }

        NoteInfo left;
        for (int i = anchorIdxRelativeToA4 - 1;; i--)
        {
            if (IndexRelativeToA4ToNoteInfoDictionary[i].NoteWithOctave.NoteEnum == noteEnum)
            {
                left = IndexRelativeToA4ToNoteInfoDictionary[i];
                break;
            }
        }

        if (anchorIdxRelativeToA4 - left.IndexRelativeToA4 < right.IndexRelativeToA4 - anchorIdxRelativeToA4)
        {
            return left;
        }
        else
        {
            return right;
        }
    }
}