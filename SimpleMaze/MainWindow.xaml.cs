using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using System.Windows.Data;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace SimpleMaze {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Grid _view;
        private MazeMap map;
        private readonly MainWindowViewModel Info;

        public MainWindow() {
            Info = new MainWindowViewModel {
                MapWidth = 50,
                MapHeight = 50,
                MapCellWidth = 10,
                ColorBrush = new SolidColorBrush(),
                SelectedMethodIndex = 0,
                ColorA = 128,
                ColorR = 100,
                ColorG = 100,
                ColorB = 100,
            };

            InitializeComponent();
            mainView.DataContext = this.Info;
        }

        private void mainView_Loaded(object sender, RoutedEventArgs e) {
            _view = new Grid() {
                Margin = new Thickness(20, 0, 20, 10),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            ((StackPanel)sender).Children.Add(_view);
        }

        private void GenerateMap(object sender, RoutedEventArgs e) {
            _view.Children.Clear();

            var rd = new Random();
            var inPoint = rd.Next(Info.MapHeight) + 1;
            var outPoint = rd.Next(Info.MapHeight) + 1;
            map = new MazeMap(Info.MapWidth, Info.MapHeight, inPoint, outPoint);
            GetGenerateMethod()?.Invoke();
            foreach (var item in map) {
                _view.Children.Add(item.GetElement(Info.MapCellWidth));
            }
        }

        private Action GetGenerateMethod() => Info.SelectedMethodIndex switch
        {
            0 => map.GenerateWallRb,
            1 => map.GenerateWallRp,
            2 => map.GenerateWallRd,
            _ => null,
        };

        private void MoveToOutPoint(object sender, RoutedEventArgs e) {
            if (_view.Children.Count <= 0) {
                return;
            }

            var mapUnitCount = Info.MapWidth * Info.MapHeight;
            while (_view.Children.Count > mapUnitCount) {
                _view.Children.RemoveAt(mapUnitCount);
            }

            map.GetRouteFromInToOut();

            foreach (var location in map.OutRoute) {
                var ellipse = new Ellipse() {
                    Width = Info.MapCellWidth - 4,
                    Height = Info.MapCellWidth - 4,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(((location.X - 1) * Info.MapCellWidth) + 2, ((location.Y - 1) * Info.MapCellWidth) + 2, 0, 0),
                };
                ellipse.SetBinding(Ellipse.FillProperty, new Binding("ColorBrush") { Source = Info });
                _view.Children.Add(ellipse);
            }
        }

        private void ExportMazeData(object sender, RoutedEventArgs e) {
            if (_view.Children.Count <= 0) {
                return;
            }

            var fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog(this) ?? false) {
                var sb = new System.Text.StringBuilder();
                foreach (var item in map) {
                    sb.AppendLine($@"X{item.Location.X.ToString().PadLeft(3, '0')}Y{item.Location.Y.ToString().PadLeft(3, '0')}L{(item.Walls[Direction.Left] ? 1 : 0)}T{(item.Walls[Direction.Top] ? 1 : 0)}R{(item.Walls[Direction.Right] ? 1 : 0)}B{(item.Walls[Direction.Bottom] ? 1 : 0)}");
                }
                System.IO.File.WriteAllText(fileDialog.FileName, sb.ToString());
                CustomizeMessageBox.Show(this, "Export successfully.", "Info");
            }
        }

        private void ImportMazeData(object sender, RoutedEventArgs e) {
            var fileDialog = new OpenFileDialog() {
                Multiselect = false,
            };
            if (fileDialog.ShowDialog() ?? false) {
                var mapData = new List<string>();
                mapData.AddRange(System.IO.File.ReadAllLines(fileDialog.FileName));
                var isInvalidInput = mapData.Any(e => !Regex.IsMatch(e, @"X\d{3}Y\d{3}L(1|0)T(1|0)R(1|0)B(1|0)"));
                if (isInvalidInput) {
                    CustomizeMessageBox.Show(this, "Illegal data file.", "Info");
                    return;
                }

                var mapInfo = mapData.Select(e => new {
                    X = int.Parse(e.Substring(1, 3)),
                    Y = int.Parse(e.Substring(5, 3)),
                    L = int.Parse(e.Substring(9, 1)) != 0,
                    T = int.Parse(e.Substring(11, 1)) != 0,
                    R = int.Parse(e.Substring(13, 1)) != 0,
                    B = int.Parse(e.Substring(15, 1)) != 0,
                });

                var width = mapInfo.Max(e => e.X);
                var height = mapInfo.Max(e => e.Y);
                var inPoint = mapInfo.Where(e => e.X == 1 && e.L == false).Select(e => e.Y).First();
                var outPoint = mapInfo.Where(e => e.X == width && e.R == false).Select(e => e.Y).First();

                Info.MapHeight = height;
                Info.MapWidth = width;

                map = new MazeMap(width, height, inPoint, outPoint, (mapInfo.Select(e => ((e.X, e.Y), (e.L, e.T, e.R, e.B))).ToList()));

                _view.Children.Clear();
                foreach (var item in map) {
                    _view.Children.Add(item.GetElement(Info.MapCellWidth));
                }
            }
        }
    }

    static class Exetension {
        public static Border GetElement(this MazeCell cell, int width) {
            var b = new Border() {
                Width = width,
                Height = width,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness((cell.Location.X - 1) * width, (cell.Location.Y - 1) * width, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                BorderThickness = new Thickness(cell.Walls[Direction.Left] ? 1 : 0, cell.Walls[Direction.Top] ? 1 : 0, cell.Walls[Direction.Right] ? 1 : 0, cell.Walls[Direction.Bottom] ? 1 : 0)
            };

            return b;
        }
    }
}
