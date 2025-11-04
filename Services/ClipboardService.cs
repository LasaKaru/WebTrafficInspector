using System;
using System.Collections.Generic;
using System.Linq;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Service for managing clipboard operations on traffic entries
    /// </summary>
    public class ClipboardService
    {
        private List<TrafficEntry> _clipboard = new List<TrafficEntry>();
        private bool _isCutOperation = false;

        public bool HasContent => _clipboard.Count > 0;
        public int ContentCount => _clipboard.Count;
        public bool IsCutOperation => _isCutOperation;

        public event EventHandler ClipboardChanged;

        /// <summary>
        /// Copy entries to clipboard
        /// </summary>
        public void Copy(List<TrafficEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            _clipboard.Clear();
            foreach (var entry in entries)
            {
                _clipboard.Add(CloneEntry(entry));
            }
            _isCutOperation = false;

            OnClipboardChanged();
        }

        /// <summary>
        /// Cut entries to clipboard
        /// </summary>
        public void Cut(List<TrafficEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            _clipboard.Clear();
            foreach (var entry in entries)
            {
                _clipboard.Add(entry); // Store reference for cut operation
            }
            _isCutOperation = true;

            OnClipboardChanged();
        }

        /// <summary>
        /// Paste clipboard contents
        /// Returns the pasted entries
        /// </summary>
        public List<TrafficEntry> Paste()
        {
            if (!HasContent)
                return new List<TrafficEntry>();

            var pastedEntries = new List<TrafficEntry>();

            if (_isCutOperation)
            {
                // For cut operation, use original entries
                pastedEntries = new List<TrafficEntry>(_clipboard);
                _clipboard.Clear();
                _isCutOperation = false;
                OnClipboardChanged();
            }
            else
            {
                // For copy operation, clone entries
                foreach (var entry in _clipboard)
                {
                    pastedEntries.Add(CloneEntry(entry));
                }
            }

            return pastedEntries;
        }

        /// <summary>
        /// Get clipboard contents without removing them
        /// </summary>
        public List<TrafficEntry> GetClipboardContents()
        {
            return _clipboard.Select(CloneEntry).ToList();
        }

        /// <summary>
        /// Clear the clipboard
        /// </summary>
        public void Clear()
        {
            _clipboard.Clear();
            _isCutOperation = false;
            OnClipboardChanged();
        }

        /// <summary>
        /// Get entries that were cut (to remove from source)
        /// </summary>
        public List<TrafficEntry> GetCutEntries()
        {
            return _isCutOperation ? new List<TrafficEntry>(_clipboard) : new List<TrafficEntry>();
        }

        /// <summary>
        /// Clone a traffic entry
        /// </summary>
        private TrafficEntry CloneEntry(TrafficEntry source)
        {
            return new TrafficEntry
            {
                Id = source.Id,
                Timestamp = source.Timestamp,
                Method = source.Method,
                Host = source.Host,
                Path = source.Path,
                Status = source.Status,
                Length = source.Length,
                RawRequest = source.RawRequest,
                RawResponse = source.RawResponse,
                IsPinned = source.IsPinned,
                Tags = source.Tags,
                Notes = source.Notes,
                Color = source.Color
            };
        }

        private void OnClipboardChanged()
        {
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
