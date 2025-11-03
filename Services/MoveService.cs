using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced service for moving, reordering, and organizing traffic entries
    /// Provides powerful batch operations and smart filtering capabilities
    /// </summary>
    public class MoveService
    {
        /// <summary>
        /// Move a single entry up in the collection
        /// </summary>
        public bool MoveUp(ObservableCollection<TrafficEntry> entries, TrafficEntry entry)
        {
            if (entry == null || entries == null || entries.Count < 2)
                return false;

            int index = entries.IndexOf(entry);
            if (index <= 0)
                return false;

            entries.Move(index, index - 1);
            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Move a single entry down in the collection
        /// </summary>
        public bool MoveDown(ObservableCollection<TrafficEntry> entries, TrafficEntry entry)
        {
            if (entry == null || entries == null || entries.Count < 2)
                return false;

            int index = entries.IndexOf(entry);
            if (index < 0 || index >= entries.Count - 1)
                return false;

            entries.Move(index, index + 1);
            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Move entry to the top of the collection
        /// </summary>
        public bool MoveToTop(ObservableCollection<TrafficEntry> entries, TrafficEntry entry)
        {
            if (entry == null || entries == null || entries.Count < 2)
                return false;

            int index = entries.IndexOf(entry);
            if (index <= 0)
                return false;

            entries.Move(index, 0);
            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Move entry to the bottom of the collection
        /// </summary>
        public bool MoveToBottom(ObservableCollection<TrafficEntry> entries, TrafficEntry entry)
        {
            if (entry == null || entries == null || entries.Count < 2)
                return false;

            int index = entries.IndexOf(entry);
            if (index < 0 || index >= entries.Count - 1)
                return false;

            entries.Move(index, entries.Count - 1);
            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Move entry to a specific position
        /// </summary>
        public bool MoveToPosition(ObservableCollection<TrafficEntry> entries, TrafficEntry entry, int targetPosition)
        {
            if (entry == null || entries == null || targetPosition < 0 || targetPosition >= entries.Count)
                return false;

            int currentIndex = entries.IndexOf(entry);
            if (currentIndex < 0)
                return false;

            entries.Move(currentIndex, targetPosition);
            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Advanced: Move multiple entries up by one position
        /// Maintains relative order of selected entries
        /// </summary>
        public bool MoveBatchUp(ObservableCollection<TrafficEntry> entries, List<TrafficEntry> selectedEntries)
        {
            if (entries == null || selectedEntries == null || selectedEntries.Count == 0)
                return false;

            // Sort by current index
            var sortedEntries = selectedEntries.OrderBy(e => entries.IndexOf(e)).ToList();

            // Check if topmost entry is already at position 0
            if (entries.IndexOf(sortedEntries[0]) == 0)
                return false;

            // Move each entry up
            foreach (var entry in sortedEntries)
            {
                int index = entries.IndexOf(entry);
                if (index > 0)
                {
                    entries.Move(index, index - 1);
                }
            }

            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Advanced: Move multiple entries down by one position
        /// Maintains relative order of selected entries
        /// </summary>
        public bool MoveBatchDown(ObservableCollection<TrafficEntry> entries, List<TrafficEntry> selectedEntries)
        {
            if (entries == null || selectedEntries == null || selectedEntries.Count == 0)
                return false;

            // Sort by current index in descending order
            var sortedEntries = selectedEntries.OrderByDescending(e => entries.IndexOf(e)).ToList();

            // Check if bottommost entry is already at last position
            if (entries.IndexOf(sortedEntries[0]) == entries.Count - 1)
                return false;

            // Move each entry down
            foreach (var entry in sortedEntries)
            {
                int index = entries.IndexOf(entry);
                if (index < entries.Count - 1)
                {
                    entries.Move(index, index + 1);
                }
            }

            ReassignIds(entries);
            return true;
        }

        /// <summary>
        /// Advanced: Move all entries matching a specific host
        /// </summary>
        public List<TrafficEntry> MoveEntriesByHost(ObservableCollection<TrafficEntry> source,
            ObservableCollection<TrafficEntry> destination, string host, bool remove = false)
        {
            if (source == null || destination == null || string.IsNullOrWhiteSpace(host))
                return new List<TrafficEntry>();

            var matchingEntries = source.Where(e => e.Host.Contains(host, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var entry in matchingEntries)
            {
                destination.Add(entry);
                if (remove)
                {
                    source.Remove(entry);
                }
            }

            ReassignIds(source);
            ReassignIds(destination);
            return matchingEntries;
        }

        /// <summary>
        /// Advanced: Move all entries with a specific status code
        /// </summary>
        public List<TrafficEntry> MoveEntriesByStatus(ObservableCollection<TrafficEntry> source,
            ObservableCollection<TrafficEntry> destination, int statusCode, bool remove = false)
        {
            if (source == null || destination == null)
                return new List<TrafficEntry>();

            var matchingEntries = source.Where(e => e.Status == statusCode).ToList();

            foreach (var entry in matchingEntries)
            {
                destination.Add(entry);
                if (remove)
                {
                    source.Remove(entry);
                }
            }

            ReassignIds(source);
            ReassignIds(destination);
            return matchingEntries;
        }

        /// <summary>
        /// Advanced: Move all entries with a specific HTTP method
        /// </summary>
        public List<TrafficEntry> MoveEntriesByMethod(ObservableCollection<TrafficEntry> source,
            ObservableCollection<TrafficEntry> destination, string method, bool remove = false)
        {
            if (source == null || destination == null || string.IsNullOrWhiteSpace(method))
                return new List<TrafficEntry>();

            var matchingEntries = source.Where(e =>
                e.Method.Equals(method, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var entry in matchingEntries)
            {
                destination.Add(entry);
                if (remove)
                {
                    source.Remove(entry);
                }
            }

            ReassignIds(source);
            ReassignIds(destination);
            return matchingEntries;
        }

        /// <summary>
        /// Advanced: Move all entries within a time range
        /// </summary>
        public List<TrafficEntry> MoveEntriesByTimeRange(ObservableCollection<TrafficEntry> source,
            ObservableCollection<TrafficEntry> destination, DateTime startTime, DateTime endTime, bool remove = false)
        {
            if (source == null || destination == null)
                return new List<TrafficEntry>();

            var matchingEntries = source.Where(e =>
                e.Timestamp >= startTime && e.Timestamp <= endTime).ToList();

            foreach (var entry in matchingEntries)
            {
                destination.Add(entry);
                if (remove)
                {
                    source.Remove(entry);
                }
            }

            ReassignIds(source);
            ReassignIds(destination);
            return matchingEntries;
        }

        /// <summary>
        /// Advanced: Move entries matching custom criteria
        /// </summary>
        public List<TrafficEntry> MoveEntriesByCustomFilter(ObservableCollection<TrafficEntry> source,
            ObservableCollection<TrafficEntry> destination, Func<TrafficEntry, bool> predicate, bool remove = false)
        {
            if (source == null || destination == null || predicate == null)
                return new List<TrafficEntry>();

            var matchingEntries = source.Where(predicate).ToList();

            foreach (var entry in matchingEntries)
            {
                destination.Add(entry);
                if (remove)
                {
                    source.Remove(entry);
                }
            }

            ReassignIds(source);
            ReassignIds(destination);
            return matchingEntries;
        }

        /// <summary>
        /// Advanced: Group and reorder entries by host
        /// </summary>
        public void GroupByHost(ObservableCollection<TrafficEntry> entries)
        {
            if (entries == null || entries.Count < 2)
                return;

            var grouped = entries.OrderBy(e => e.Host).ThenBy(e => e.Timestamp).ToList();
            entries.Clear();

            foreach (var entry in grouped)
            {
                entries.Add(entry);
            }

            ReassignIds(entries);
        }

        /// <summary>
        /// Advanced: Group and reorder entries by method
        /// </summary>
        public void GroupByMethod(ObservableCollection<TrafficEntry> entries)
        {
            if (entries == null || entries.Count < 2)
                return;

            var grouped = entries.OrderBy(e => e.Method).ThenBy(e => e.Timestamp).ToList();
            entries.Clear();

            foreach (var entry in grouped)
            {
                entries.Add(entry);
            }

            ReassignIds(entries);
        }

        /// <summary>
        /// Advanced: Group and reorder entries by status code
        /// </summary>
        public void GroupByStatus(ObservableCollection<TrafficEntry> entries)
        {
            if (entries == null || entries.Count < 2)
                return;

            var grouped = entries.OrderBy(e => e.Status).ThenBy(e => e.Timestamp).ToList();
            entries.Clear();

            foreach (var entry in grouped)
            {
                entries.Add(entry);
            }

            ReassignIds(entries);
        }

        /// <summary>
        /// Advanced: Sort entries by timestamp (chronological)
        /// </summary>
        public void SortByTime(ObservableCollection<TrafficEntry> entries, bool ascending = true)
        {
            if (entries == null || entries.Count < 2)
                return;

            var sorted = ascending ?
                entries.OrderBy(e => e.Timestamp).ToList() :
                entries.OrderByDescending(e => e.Timestamp).ToList();

            entries.Clear();

            foreach (var entry in sorted)
            {
                entries.Add(entry);
            }

            ReassignIds(entries);
        }

        /// <summary>
        /// Advanced: Reverse the order of all entries
        /// </summary>
        public void ReverseOrder(ObservableCollection<TrafficEntry> entries)
        {
            if (entries == null || entries.Count < 2)
                return;

            var reversed = entries.Reverse().ToList();
            entries.Clear();

            foreach (var entry in reversed)
            {
                entries.Add(entry);
            }

            ReassignIds(entries);
        }

        /// <summary>
        /// Extract selected entries to a new collection (for creating new session)
        /// </summary>
        public List<TrafficEntry> ExtractEntries(ObservableCollection<TrafficEntry> source,
            List<TrafficEntry> selectedEntries, bool removeFromSource = false)
        {
            if (source == null || selectedEntries == null || selectedEntries.Count == 0)
                return new List<TrafficEntry>();

            var extracted = new List<TrafficEntry>(selectedEntries);

            if (removeFromSource)
            {
                foreach (var entry in selectedEntries)
                {
                    source.Remove(entry);
                }
                ReassignIds(source);
            }

            return extracted;
        }

        /// <summary>
        /// Merge entries from one collection to another at a specific position
        /// </summary>
        public void MergeAt(ObservableCollection<TrafficEntry> destination,
            List<TrafficEntry> entriesToMerge, int position)
        {
            if (destination == null || entriesToMerge == null || entriesToMerge.Count == 0)
                return;

            position = Math.Max(0, Math.Min(position, destination.Count));

            for (int i = 0; i < entriesToMerge.Count; i++)
            {
                destination.Insert(position + i, entriesToMerge[i]);
            }

            ReassignIds(destination);
        }

        /// <summary>
        /// Reassign sequential IDs to entries after reordering
        /// </summary>
        public void ReassignIds(ObservableCollection<TrafficEntry> entries)
        {
            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Id = i + 1;
            }
        }

        /// <summary>
        /// Get statistics about move operations
        /// </summary>
        public MoveStatistics GetMoveStatistics(List<TrafficEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return new MoveStatistics();

            return new MoveStatistics
            {
                TotalEntries = entries.Count,
                UniqueHosts = entries.Select(e => e.Host).Distinct().Count(),
                UniqueMethods = entries.Select(e => e.Method).Distinct().Count(),
                UniqueStatusCodes = entries.Select(e => e.Status).Distinct().Count(),
                TimeSpan = entries.Count > 1 ?
                    entries.Max(e => e.Timestamp) - entries.Min(e => e.Timestamp) :
                    TimeSpan.Zero
            };
        }
    }

    /// <summary>
    /// Statistics about moved entries
    /// </summary>
    public class MoveStatistics
    {
        public int TotalEntries { get; set; }
        public int UniqueHosts { get; set; }
        public int UniqueMethods { get; set; }
        public int UniqueStatusCodes { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
}
