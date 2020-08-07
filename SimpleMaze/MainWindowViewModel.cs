using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleMaze {
    class MainWindowViewModel : INotifyPropertyChanged {
        private int _width, _height, _cellWidth;
        private int _colorA, _colorR, _colorG, _colorB;

        public int MapWidth {
            get => _width;
            set {
                if (value >= 20 && value <= 80) {
                    _width = value;
                    OnPropertyChanged("MapWidth");
                }
            }
        }

        public int MapHeight {
            get => _height;
            set {
                if (value >= 20 && value <= 80) {
                    _height = value;
                    OnPropertyChanged("MapHeight");
                }
            }
        }

        public int MapCellWidth {
            get => _cellWidth;
            set {
                if (value >= 10 && value <= 15) {
                    _cellWidth = value;
                    OnPropertyChanged("MapCellWidth");
                }
            }
        }

        public int ColorA {
            get => _colorA;
            set {
                _colorA = value;
                ColorBrush.Color = Color.FromArgb((byte)_colorA, (byte)_colorR, (byte)_colorG, (byte)_colorB);
            }
        }

        public int ColorR {
            get => _colorR;
            set {
                _colorR = value;
                ColorBrush.Color = Color.FromArgb((byte)_colorA, (byte)_colorR, (byte)_colorG, (byte)_colorB);
            }
        }

        public int ColorG {
            get => _colorG;
            set {
                _colorG = value;
                ColorBrush.Color = Color.FromArgb((byte)_colorA, (byte)_colorR, (byte)_colorG, (byte)_colorB);
            }
        }

        public int ColorB {
            get => _colorB;
            set {
                _colorB = value;
                ColorBrush.Color = Color.FromArgb((byte)_colorA, (byte)_colorR, (byte)_colorG, (byte)_colorB);
            }
        }

        public SolidColorBrush ColorBrush { get; set; }
        public int SelectedMethodIndex { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
