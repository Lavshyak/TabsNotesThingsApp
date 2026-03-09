using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabsNotesThings.ViewModels.Printers;
using TabsNotesThings.ViewModels.ReadOnlyModels;

namespace TabsNotesThings.ViewModels;

public partial class KeyboardViewModel : ViewModelBase
{
    public static KeyboardViewModel Instance { get; } = new KeyboardViewModel();

    private TabViewModel.Tab3 _currentTab = new TabViewModel.Tab3([], []);

    [ObservableProperty]
    private string _view = "";


    private IReadOnlyList<IReadOnlyList<T>> SplitByNull<T>(IReadOnlyList<T?> roots)
    {
        List<List<T>> splitted = [];
        List<T> current = [];
        foreach (var root in roots)
        {
            if (root == null)
            {
                if (current.Count != 0)
                {
                    splitted.Add(current);
                    current = [];
                }
            }
            else
            {
                current.Add(root);
            }
        }

        if (current.Count != 0)
        {
            splitted.Add(current);
        }

        return splitted;
    }

    [RelayCommand]
    private void HandleInput(TabViewModel.Tab2 inputTab)
    {
        var currentRootsNotNull = _currentTab.Roots.Where(r => r != null).ToImmutableArray();

        var inputRootIdxNotNull = -1;

        var newRoots = new List<NoteWithOctave?>();
        int inputStringIdx = -1;

        foreach (var inputRoot in inputTab.Roots)
        {
            if (inputRoot == null)
            {
                newRoots.Add(null);
                inputStringIdx = -1;
                continue;
            }
            else
            {
                inputStringIdx += 1;
                inputRootIdxNotNull += 1;

                if (inputRoot.Octave.HasValue)
                {
                    newRoots.Add(new NoteWithOctave(inputRoot.NoteEnum, inputRoot.Octave.Value));
                    continue;
                }
                else
                {
                    NoteWithOctave? currentRoot = null;
                    if (inputRootIdxNotNull < currentRootsNotNull.Length)
                    {
                        currentRoot = currentRootsNotNull[inputRootIdxNotNull];
                    }

                    if (currentRoot != null)
                    {
                        newRoots.Add(new NoteWithOctave(inputRoot.NoteEnum, currentRoot.Octave));
                        continue;
                    }
                    else
                    {
                        newRoots.Add(
                            DefaultGuitarNotes.Instance.GetDefaultNoteWithOctave(inputStringIdx, inputRoot.NoteEnum));
                        continue;
                    }
                }
            }
        }

        var newTab = new TabViewModel.Tab3(newRoots, inputTab.Columns);
        SetNewTab(newTab);
    }

    private void SetNewTab(TabViewModel.Tab3 newTab)
    {
        _currentTab = newTab;
        var tabStr = TabPrinter.Instance.PrintTabRootNotesFretNotes(newTab);
        View = tabStr;

        SelectedHalfToneIdxAndNoteStrs = newTab.Roots.Select(r =>
        {
            SelectableHalfToneIdxAndNoteStr hui;
            if (r == null)
            {
                hui = SelectableHalfToneIdxAndNoteStr.CreateDisabled();
            }
            else
            {
                var htIdx = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(r.NoteEnum, r.Octave);
                var selectedIdx = HalfToneIdxAndNoteStrs.Index().First(t => t.Item.HalfToneIdx == htIdx).Index;
                hui = SelectableHalfToneIdxAndNoteStr.CreateNormal(selectedIdx, HalfToneIdxAndNoteStrs);
            }

            hui.PropertyChanged += (obj, args) => OnRootKeyChanged();
            return hui;
        }).ToImmutableArray();
    }

    private void OnRootKeyChanged()
    {
        var htIdxs = SelectedHalfToneIdxAndNoteStrs.Select(s =>
        {
            if (s.SelectedIndex == -1)
            {
                return (int?)null;
            }
            else
            {
                return HalfToneIdxAndNoteStrs[s.SelectedIndex].HalfToneIdx;
            }
        });

        var notesWithOctaves = htIdxs.Select(htIdx =>
        {
            if (htIdx == null)
            {
                return null;
            }
            else
            {
                var noteOctave = NoteStrs.Instance.ToNoteAndOctave(htIdx.Value);
                var noteWithOctave = new NoteWithOctave(noteOctave.note, noteOctave.octave);
                return noteWithOctave;
            }
        }).ToArray();

        var newTab = new TabViewModel.Tab3(notesWithOctaves, _currentTab.Columns);
        SetNewTab(newTab);
    }

    public ObservableCollection<string> Log { get; } = [];

    public KeyboardViewModel()
    {
        var from = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(NoteEnum.C, -2);
        var to = NoteStrs.Instance.ToHalfToneIdxRelativeToA4(NoteEnum.B, 10);
        HalfToneIdxAndNoteStrs = Enumerable.Range(from, to - from+1).Select(htIdx =>
                new HalfToneIdxAndNoteStr(htIdx, NoteStrs.Instance.ToStringNoteOctaveFromHalfToneIdx(htIdx)))
            .ToImmutableArray();

        if (Design.IsDesignMode)
        {
            HandleInputCommand.Execute(new TabViewModel.Tab2([
                        new NoteMayBeWithOctave(NoteEnum.FSharp, 2), null, new NoteMayBeWithOctave(NoteEnum.B, null)
                    ],
                    [
                        [18, null, null],
                        [5, null, 5],
                        [null, null, 13]
                    ]
                )
            );
        }
    }


    [ObservableProperty]
    private IReadOnlyList<SelectableHalfToneIdxAndNoteStr>
        _selectedHalfToneIdxAndNoteStrs = [];

    private IReadOnlyList<HalfToneIdxAndNoteStr> HalfToneIdxAndNoteStrs { get; }
}

public record HalfToneIdxAndNoteStr(int HalfToneIdx, string NoteStr);

public partial class SelectableHalfToneIdxAndNoteStr : ViewModelBase
{
    public static SelectableHalfToneIdxAndNoteStr CreateNormal(int selectedIndex,
        IReadOnlyList<HalfToneIdxAndNoteStr> variants)
    {
        var instance = new SelectableHalfToneIdxAndNoteStr(selectedIndex, variants, true);
        return instance;
    }

    public static SelectableHalfToneIdxAndNoteStr CreateDisabled()
    {
        var instance = new SelectableHalfToneIdxAndNoteStr(-1, [], false);
        return instance;
    }

    private SelectableHalfToneIdxAndNoteStr(int selectedIndex, IReadOnlyList<HalfToneIdxAndNoteStr> variants,
        bool isEnabled)
    {
        _selectedIndex = selectedIndex;
        Variants = variants;
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }

    [ObservableProperty]
    private int _selectedIndex;
    public IReadOnlyList<HalfToneIdxAndNoteStr> Variants { get; }
}