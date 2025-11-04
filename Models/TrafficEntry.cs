using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTrafficInspector.Models
{
    public class TrafficEntry : INotifyPropertyChanged
    {
        private int _id;
        private DateTime _timestamp;
        private string _method;
        private string _host;
        private string _path;
        private int _status;
        private long _length;
        private string _rawRequest;
        private string _rawResponse;
        private double _duration;
        private string _contentType;

        // Enhanced features
        private bool _isPinned;
        private string _tags;
        private string _notes;
        private string _color;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(nameof(Timestamp)); }
        }

        public string Method
        {
            get => _method;
            set { _method = value; OnPropertyChanged(nameof(Method)); }
        }

        public string Host
        {
            get => _host;
            set { _host = value; OnPropertyChanged(nameof(Host)); }
        }

        public string Path
        {
            get => _path;
            set { _path = value; OnPropertyChanged(nameof(Path)); }
        }

        public int Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public long Length
        {
            get => _length;
            set { _length = value; OnPropertyChanged(nameof(Length)); }
        }

        public string RawRequest
        {
            get => _rawRequest;
            set { _rawRequest = value; OnPropertyChanged(nameof(RawRequest)); }
        }

        public string RawResponse
        {
            get => _rawResponse;
            set { _rawResponse = value; OnPropertyChanged(nameof(RawResponse)); }
        }

        public double Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(nameof(Duration)); }
        }

        public string ContentType
        {
            get => _contentType;
            set { _contentType = value; OnPropertyChanged(nameof(ContentType)); }
        }

        // Computed/Alias properties for compatibility
        public string Url => $"http://{Host}{Path}";
        public long Size => Length;
        public int StatusCode => Status;

        // Enhanced properties
        public bool IsPinned
        {
            get => _isPinned;
            set { _isPinned = value; OnPropertyChanged(nameof(IsPinned)); }
        }

        public string Tags
        {
            get => _tags;
            set { _tags = value; OnPropertyChanged(nameof(Tags)); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
