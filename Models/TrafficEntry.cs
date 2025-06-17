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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
