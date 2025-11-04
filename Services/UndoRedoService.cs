using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Service for managing undo/redo operations on traffic entries
    /// </summary>
    public class UndoRedoService
    {
        private Stack<UndoRedoAction> _undoStack = new Stack<UndoRedoAction>();
        private Stack<UndoRedoAction> _redoStack = new Stack<UndoRedoAction>();
        private const int MaxHistorySize = 50;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public event EventHandler CanUndoRedoChanged;

        /// <summary>
        /// Record an action for undo/redo
        /// </summary>
        public void RecordAction(UndoRedoAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear(); // Clear redo stack when new action is performed

            // Limit stack size
            if (_undoStack.Count > MaxHistorySize)
            {
                var tempStack = new Stack<UndoRedoAction>(_undoStack.Reverse().Skip(1));
                _undoStack = new Stack<UndoRedoAction>(tempStack.Reverse());
            }

            OnCanUndoRedoChanged();
        }

        /// <summary>
        /// Undo the last action
        /// </summary>
        public void Undo(ObservableCollection<TrafficEntry> entries)
        {
            if (!CanUndo) return;

            var action = _undoStack.Pop();
            action.Undo(entries);
            _redoStack.Push(action);

            OnCanUndoRedoChanged();
        }

        /// <summary>
        /// Redo the last undone action
        /// </summary>
        public void Redo(ObservableCollection<TrafficEntry> entries)
        {
            if (!CanRedo) return;

            var action = _redoStack.Pop();
            action.Redo(entries);
            _undoStack.Push(action);

            OnCanUndoRedoChanged();
        }

        /// <summary>
        /// Clear all undo/redo history
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnCanUndoRedoChanged();
        }

        /// <summary>
        /// Get description of the next undo action
        /// </summary>
        public string GetUndoDescription()
        {
            return CanUndo ? _undoStack.Peek().Description : "Nothing to undo";
        }

        /// <summary>
        /// Get description of the next redo action
        /// </summary>
        public string GetRedoDescription()
        {
            return CanRedo ? _redoStack.Peek().Description : "Nothing to redo";
        }

        /// <summary>
        /// Get all undo history descriptions
        /// </summary>
        public List<string> GetUndoHistory()
        {
            return _undoStack.Select(a => a.Description).ToList();
        }

        /// <summary>
        /// Get all redo history descriptions
        /// </summary>
        public List<string> GetRedoHistory()
        {
            return _redoStack.Select(a => a.Description).ToList();
        }

        private void OnCanUndoRedoChanged()
        {
            CanUndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Represents an undoable/redoable action
    /// </summary>
    public abstract class UndoRedoAction
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public abstract void Undo(ObservableCollection<TrafficEntry> entries);
        public abstract void Redo(ObservableCollection<TrafficEntry> entries);
    }

    /// <summary>
    /// Action for moving entries
    /// </summary>
    public class MoveAction : UndoRedoAction
    {
        private List<TrafficEntry> _beforeState;
        private List<TrafficEntry> _afterState;

        public MoveAction(string description, ObservableCollection<TrafficEntry> entries)
        {
            Description = description;
            _beforeState = new List<TrafficEntry>(entries);
        }

        public void CaptureAfterState(ObservableCollection<TrafficEntry> entries)
        {
            _afterState = new List<TrafficEntry>(entries);
        }

        public override void Undo(ObservableCollection<TrafficEntry> entries)
        {
            entries.Clear();
            foreach (var entry in _beforeState)
            {
                entries.Add(entry);
            }
        }

        public override void Redo(ObservableCollection<TrafficEntry> entries)
        {
            entries.Clear();
            foreach (var entry in _afterState)
            {
                entries.Add(entry);
            }
        }
    }

    /// <summary>
    /// Action for deleting entries
    /// </summary>
    public class DeleteAction : UndoRedoAction
    {
        private List<TrafficEntry> _deletedEntries;
        private List<int> _originalIndices;

        public DeleteAction(string description, List<TrafficEntry> deletedEntries,
            ObservableCollection<TrafficEntry> collection)
        {
            Description = description;
            _deletedEntries = new List<TrafficEntry>(deletedEntries);
            _originalIndices = deletedEntries.Select(e => collection.IndexOf(e)).ToList();
        }

        public override void Undo(ObservableCollection<TrafficEntry> entries)
        {
            for (int i = 0; i < _deletedEntries.Count; i++)
            {
                var index = Math.Min(_originalIndices[i], entries.Count);
                entries.Insert(index, _deletedEntries[i]);
            }
        }

        public override void Redo(ObservableCollection<TrafficEntry> entries)
        {
            foreach (var entry in _deletedEntries)
            {
                entries.Remove(entry);
            }
        }
    }

    /// <summary>
    /// Action for adding entries
    /// </summary>
    public class AddAction : UndoRedoAction
    {
        private List<TrafficEntry> _addedEntries;

        public AddAction(string description, List<TrafficEntry> addedEntries)
        {
            Description = description;
            _addedEntries = new List<TrafficEntry>(addedEntries);
        }

        public override void Undo(ObservableCollection<TrafficEntry> entries)
        {
            foreach (var entry in _addedEntries)
            {
                entries.Remove(entry);
            }
        }

        public override void Redo(ObservableCollection<TrafficEntry> entries)
        {
            foreach (var entry in _addedEntries)
            {
                entries.Add(entry);
            }
        }
    }

    /// <summary>
    /// Action for modifying entry properties
    /// </summary>
    public class ModifyAction : UndoRedoAction
    {
        private Dictionary<TrafficEntry, EntrySnapshot> _beforeSnapshots;
        private Dictionary<TrafficEntry, EntrySnapshot> _afterSnapshots;

        public ModifyAction(string description, List<TrafficEntry> entries)
        {
            Description = description;
            _beforeSnapshots = entries.ToDictionary(e => e, e => new EntrySnapshot(e));
        }

        public void CaptureAfterState(List<TrafficEntry> entries)
        {
            _afterSnapshots = entries.ToDictionary(e => e, e => new EntrySnapshot(e));
        }

        public override void Undo(ObservableCollection<TrafficEntry> entries)
        {
            foreach (var kvp in _beforeSnapshots)
            {
                kvp.Value.RestoreTo(kvp.Key);
            }
        }

        public override void Redo(ObservableCollection<TrafficEntry> entries)
        {
            foreach (var kvp in _afterSnapshots)
            {
                kvp.Value.RestoreTo(kvp.Key);
            }
        }

        private class EntrySnapshot
        {
            public bool IsPinned { get; set; }
            public string Tags { get; set; }
            public string Notes { get; set; }
            public string Color { get; set; }

            public EntrySnapshot(TrafficEntry entry)
            {
                IsPinned = entry.IsPinned;
                Tags = entry.Tags;
                Notes = entry.Notes;
                Color = entry.Color;
            }

            public void RestoreTo(TrafficEntry entry)
            {
                entry.IsPinned = IsPinned;
                entry.Tags = Tags;
                entry.Notes = Notes;
                entry.Color = Color;
            }
        }
    }
}
