using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Bluegrams.Application;
using log4net;
using MapViewer.Dialogs;
using MapViewer.Maps;
using MapViewer.Properties;
using MapViewer.Symbols;
using MapViewer.Tools;
using MapViewer.Utilities;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MapViewer {
    public partial class PrivateWindow :INotifyPropertyChanged {

        private enum CursorAction {
            None, MovingPublicMapPos, MovingPrivateMap, MovingSymbol
        }


        #region Attributes

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private PrivateMaskedMap _mapPrivate;

        public PrivateMaskedMap MapPrivate {
            get => _mapPrivate;
            set {
                if (_mapPrivate != null) {
                    _mapPrivate.SymbolsPM.SymbolsChanged -= SymbolsPM_SymbolsChanged;
                }
                _mapPrivate = value;
                if (_mapPrivate != null) {
                    OnPropertyChanged(nameof(MultiLevelVisibility));
                    _mapPrivate.SymbolsPM.SymbolsChanged += SymbolsPM_SymbolsChanged;
                }
            }
        }

        public List<PrivateMaskedMap> MapList = new List<PrivateMaskedMap>();
        public PublicMaskedMap MapPublic;
        public int Level {
            get => _level;
            set => _level = Math.Max(Math.Min(value, LevelNumber - 1), 0);
        }

        public int LevelNumber => MapList.Count;

        public PrivateMaskedMap MapAbove => Level < LevelNumber - 1 ? MapList[Level + 1] : null;

        public PrivateMaskedMap MapBelow => Level > 0 ? MapList[Level - 1] : null;

        public readonly PublicWindow PublicWindow = new PublicWindow();

        public ObservableCollection<Symbol> SymbolCollection { get; } = new ObservableCollection<Symbol>();

        public bool IsSymbolsCollectionNotEmpty => (SymbolCollection?.Count > 0);

        public Visibility MultiLevelVisibility => (LevelNumber > 1 ? Visibility.Visible : Visibility.Collapsed);


        private UIElement _lastClickedElem;
        private Symbol _lastClickedSymbol;

        private CursorAction _cursorAction;
        private Point _mouseDownPoint;  // Canvas
        private Point _mouseDownPointFirst;  // Canvas
        private Point _mouseDownPointWindow;
        private CanvasTool _activeTool;
        private bool _writtenLogSetting;

        public bool IsImageCalibrated => MapPrivate?.IsCalibrated ?? false;

        private double _characterDistanceMoved;

        public CanvasTool ActiveTool {
            get => _activeTool;

            set {
                _activeTool?.Deactivate();
                _activeTool = value;
                _cursorAction = CursorAction.None;
                
                MapPrivate?.SymbolsPM.RaiseSymbolsChanged();
               
                MapPrivate?.SetCursor((value != null) ?  Cursors.Cross : null);

                if (MapPublic != null && MapPublic.ShowPublicCursorTemporary) {
                    MapPublic.ShowPublicCursor = false;
                    MapPublic.ShowPublicCursorTemporary = false;
                }
            }
        }

        private int _level;

        #endregion

        public PrivateWindow()
        {
            InitializeComponent();
 
            Title = $"Miakonn's MapViewer {FileVersion}   —   Private map";
            Log.Info($"STARTING MapViewer {FileVersion} ******************************************");
            
            InitSettings();

            MapPublic = PublicWindow.Map;
            MapPublic.ScreenScaleMMperM = 20.0;
            MapPublic.MapData.LastFigureScaleUsed = 50;
            MapPublic.IsLinked = false;
            MapPublic.Create();

            DropboxHandler.AddWorkingPath(System.Reflection.Assembly.GetEntryAssembly()?.Location);

            InitTimer();

            ComboBoxPublicScale.DataContext = this;
            PrivateContextMenu.DataContext = this;
        }

        private void HandleImageScaleChanged(object sender, EventArgs e) {
            OnPropertyChanged(nameof(IsImageCalibrated));
        }

        private void HandleZoomChanged(object sender, EventArgs e) {
            MapPrivate.UpdateVisibleRectangle(MapPublic);
        }

        public void SetScale(int scale) {
            if (scale == 0) {
                ComboBoxPublicScale.Text = "Linked";
            }
            else {
                var str = "1:" + scale;
                ComboBoxPublicScale.Text = str;
            }
        }

        #region Private methods

        private static string FileVersion {
            get {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var parts = fvi.FileVersion.Split('.');
                return $"{parts[0]}.{parts[1]}";
            }
        }

        private void MovePublic(Vector vector)
        {
            MapPublic.Translate(vector / MapPublic.ScaleDpiFix);
            MapPrivate.UpdateVisibleRectangle(MapPublic);
            MapPrivate.SystemSymbolsPM.RaiseSymbolsChanged();
        }

        #endregion

        #region Events

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var window = GetWindow(this);
            if (window != null) {
                window.KeyDown += PrivateWinKeyDown;
            }
        }

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            ActiveTool = null;
        }

        private void PrivateWinKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                if (_cursorAction == CursorAction.MovingSymbol) {
                    var move = new Vector((_mouseDownPoint.X - _mouseDownPointFirst.X), (_mouseDownPoint.Y - _mouseDownPointFirst.Y));
                    if (_lastClickedSymbol != null) {
                        MapPrivate.SymbolsPM.MoveSymbolPosition(_lastClickedSymbol, move);
                    }
                }

                MapPrivate.SymbolsPM.ClearSymbolSelection();
                HidePopup(0);
                ActiveTool = null;
            }
            else if (e.Key == Key.F12 && Publish_CanExecute()) {
                PublishMap_Execute(null, null);
            }
            else if (e.Key == Key.Space) {
                ActiveTool?.KeyDown(sender, e);
            }
            else {
                ActiveTool = null;
            }
        }

        private void PrivateWinSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MapPrivate?.ScaleToWindow(Layer1_Map);
        }

        private void GetLastClickedSymbol() {
            _lastClickedElem = MapPrivate.CanvasOverlay.FindElementHit();

            if (_lastClickedElem == null) {
                _lastClickedElem = MapPrivate.CanvasOverlay.FindElementHit();
            }

            if (_lastClickedElem == null) {
                _lastClickedSymbol = null;
                return;
            }

            _lastClickedSymbol = MapPrivate.SymbolsPM.FindSymbolFromUid(_lastClickedElem.Uid);
        }

        private void PrivateWinMouseDown(object sender, MouseButtonEventArgs e) {
            if (ActiveTool != null) {
                ActiveTool.MouseDown(sender, e);
                _lastClickedSymbol = MapPrivate.SymbolsPM.GetSelected();
                return;
            }

            GetLastClickedSymbol();

            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            Debug.WriteLine($"PrivateWinMouseDown: {shiftPressed}");
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1) {
                if (shiftPressed) {
                    if (_lastClickedSymbol != null) {
                        MapPrivate.SymbolsPM.ChangeSymbolSelection(_lastClickedSymbol);
                        if (!_lastClickedSymbol.IsSelected) {
                            _lastClickedSymbol = MapPrivate.SymbolsPM.GetSelected();
                        }
                    }
                }
                else if (_lastClickedSymbol != null) {
                    _mouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    _mouseDownPointFirst = _mouseDownPoint;
                    _cursorAction = CursorAction.MovingSymbol;
                    MapPrivate.SymbolsPM.SaveState();
                    if (!_lastClickedSymbol.IsSelected) {
                        MapPrivate.SymbolsPM.NewSymbolSelection(_lastClickedSymbol);
                    }

                    _characterDistanceMoved = 0;
                }
                else if (_lastClickedElem != null && _lastClickedElem.Uid == MapPrivate.PublicPositionUid) {
                    _mouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    _mouseDownPointFirst = _mouseDownPoint;
                    _cursorAction =  CursorAction.MovingPublicMapPos;

                    _characterDistanceMoved = 0;
                }
                else {
                    MapPrivate.SymbolsPM.ClearSymbolSelection();
                    _mouseDownPointWindow = e.GetPosition(this);
                    _cursorAction = CursorAction.MovingPrivateMap;
                }
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) {
                if (_lastClickedSymbol != null) {
                    _mouseDownPointWindow = e.GetPosition(this);
                    _cursorAction = CursorAction.None;
                    MapPrivate.SymbolsPM.ClearSymbolSelection();

                    var posDialog = ScaleWithWindowsDpi(PointToScreen(_mouseDownPointWindow));
                    MapPrivate.SymbolsPM.OpenEditor(this, posDialog, _lastClickedSymbol);
                }
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1) {
                _mouseDownPoint = e.GetPosition(MapPrivate.CanvasMap);
                _mouseDownPointWindow = e.GetPosition(this);
                _mouseDownPointFirst = _mouseDownPoint;
            }
        }

        public Point ScaleWithWindowsDpi(Point pos) {
            // Scale with windows dpi scale
            var dpiScale = VisualTreeHelper.GetDpi(this);
            pos.X /= dpiScale.DpiScaleX;
            pos.Y /= dpiScale.DpiScaleY;

            // Move inside window
            var windowSize = this.RenderSize;
            pos.X = Math.Min(pos.X, windowSize.Width - 200);
            pos.Y = Math.Min(pos.Y, windowSize.Height - 200);

            return pos;
        }

        private void PrivateWinMouseMove(object sender, MouseEventArgs e) {
            if (ActiveTool != null) {
                if (ActiveTool.ShowPublicCursor()) {
                    MapPublic.MovePublicCursor(e.GetPosition(MapPrivate.CanvasOverlay), MapPrivate.MapId);
                }
                ActiveTool.MouseMove(sender, e);
                return;
            }

            MapPublic.MovePublicCursor(e.GetPosition(MapPrivate.CanvasOverlay), MapPrivate.MapId);

            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                _cursorAction = CursorAction.None;
                return;
            }

            switch (_cursorAction) {
                case CursorAction.MovingPublicMapPos: {
                    var curMouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    MovePublic(new Vector((_mouseDownPoint.X - curMouseDownPoint.X), (_mouseDownPoint.Y - curMouseDownPoint.Y)) * MapPublic.ZoomScale * MapPublic.ScaleDpiFix);
                    _mouseDownPoint = curMouseDownPoint;
                    e.Handled = true;
                    break;
                }
                case CursorAction.MovingSymbol: {
                    var curMouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    var move = new Vector((_mouseDownPoint.X - curMouseDownPoint.X), (_mouseDownPoint.Y - curMouseDownPoint.Y));
                    Debug.Assert(_lastClickedSymbol != null);

                    MapPrivate.SymbolsPM.MoveSymbolPosition(_lastClickedSymbol, move);
                    _mouseDownPoint = curMouseDownPoint;
                    DisplayPopup($"{DistanceFromStart(curMouseDownPoint),5:N1}{MapPrivate.Unit} Track: {DistanceTrack(move),5:N1}{MapPrivate.Unit}");
                    e.Handled = true;
                    break;
                }
                case CursorAction.MovingPrivateMap: {
                    var curMouseDownPoint = e.GetPosition(this);
                    var move = curMouseDownPoint - _mouseDownPointWindow;
                    MapPrivate.Translate(move);
                    _mouseDownPointWindow = curMouseDownPoint;
                    e.Handled = true;
                    break;
                }
            }
        }

        private void PrivateWinMouseUp(object sender, MouseButtonEventArgs e) {
            if (ActiveTool != null) {
                ActiveTool.MouseUp(sender, e);
                return;
            }

            HidePopup(3);
            _cursorAction = CursorAction.None;
        }

        private void PrivateWinMouseWheel(object sender, MouseWheelEventArgs e) {
            var scale = (1.0 + e.Delta / 600.0);
            MapPrivate.Zoom(scale, e.GetPosition(MapPrivate.CanvasOverlay));
        }

        private void PrivateWindow_Closing(object sender, CancelEventArgs e) {
            Application.Current.Shutdown();
        }

        private void ComboBoxPublicScale_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = (ComboBoxItem)ComboBoxPublicScale.SelectedItem;
            if (selected != null && MapPublic != null && MapPrivate != null) {
                var value = int.Parse(selected.Uid.Substring(6));
                MapPrivate.MapData.LastFigureScaleUsed = value;
                if (value == 0) {
                    MapPrivate.IsLinked = true;
                    MapPublic.IsLinked = true;
                }
                else {
                    MapPublic.ScreenScaleMMperM = 1000.0 / value;
                    MapPrivate.IsLinked = false;
                    MapPublic.IsLinked = false;
                }
            }
            else {
                ComboBoxPublicScale.Text = "Linked";
            }
        }

        private void ComboBoxPlayerSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = (ComboBoxItem)ComboBoxPlayerSize.SelectedItem;
            if (selected == null || MapPublic == null || MapPrivate == null) {
                return;
            }

            var parts = selected.Uid.Split('_');
            if (parts.Length == 2 && double.TryParse(parts[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var size)) {
                MapPrivate.SymbolsPM.ChangePlayerSizes(size);
            }
            MapPrivate.SymbolsPM.RaiseSymbolsChanged();
        }

        private void ComboBoxPlayerMinSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
             var selected = (ComboBoxItem)ComboBoxPlayerMinSize.SelectedItem;
             if (selected == null || MapPublic == null || MapPrivate == null) {
                 return;
             }
             switch (selected.Uid) {
                case "PlayerMinSize_10p":
                    MapPublic.PlayerMinSizePixel = 10;
                    MapPrivate.PlayerMinSizePixel = 10;
                    break;
                case "PlayerMinSize_15p":
                    MapPublic.PlayerMinSizePixel = 15;
                    MapPrivate.PlayerMinSizePixel = 15;
                    break;
                case "PlayerMinSize_20p":
                    MapPublic.PlayerMinSizePixel = 20;
                    MapPrivate.PlayerMinSizePixel = 20;
                    break;
                case "PlayerMinSize_25p":
                    MapPublic.PlayerMinSizePixel = 25;
                    MapPrivate.PlayerMinSizePixel = 25;
                    break;
                case "PlayerMinSize_30p":
                    MapPublic.PlayerMinSizePixel = 30;
                    MapPrivate.PlayerMinSizePixel = 30;
                    break;
                case "PlayerMinSize_35p":
                    MapPublic.PlayerMinSizePixel = 35;
                    MapPrivate.PlayerMinSizePixel = 35;
                    break;
            }
            MapPrivate.SymbolsPM.RaiseSymbolsChanged();
        }

        private void Tab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveTool = null;
            if (MapPrivate != null && !MapPrivate.IsCalibrated) {
                DisplayPopupWarning("Image is not calibrated!", 10);
            }
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogAbout { Owner = this };
            dialog.ShowDialog();
        }

        private void SymbolsPM_SymbolsChanged(object sender, EventArgs e) {

            SymbolCollection.Clear();

            foreach (var symbol in MapPrivate.SymbolsPM.Symbols.Values) {
                if (!string.IsNullOrWhiteSpace(symbol.DisplayName)) {
                    SymbolCollection.Add(symbol);
                }
            }

            OnPropertyChanged(nameof(SymbolCollection));
            OnPropertyChanged(nameof(IsSymbolsCollectionNotEmpty));
        }

        #endregion

        #region Public methods

        public void InitSettings() {
            //string user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //PortableSettingsProvider.SettingsFileName = $"user.{user}.config".Replace("\\", "_");
            PortableSettingsProvider.ApplyProvider(Settings.Default);
            if (!_writtenLogSetting) {
                Log.Debug("Uses settings file:" + PortableSettingsProvider.SettingsFileName);
                _writtenLogSetting = true;
            }
        }

        public void DisplayPopup(string text)
        {
            PopupDisplay.IsOpen = true;
            if (PopupDisplay.Child is TextBlock popupText) {
                popupText.Text = text;
            }
        }

        public void HidePopup(int delay)
        {
            if (delay == 0) {
                PopupDisplay.IsOpen = false;
                return;
            }
            var time = new DispatcherTimer { Interval = TimeSpan.FromSeconds(delay) };
            time.Start();
            time.Tick += delegate {
                PopupDisplay.IsOpen = false;
                time.Stop();
            };
        }

        public void DisplayPopupWarning(string text, int delay)
        {
            PopupWarning.IsOpen = true;
            if (PopupWarning.Child is TextBlock popupText) {
                popupText.Text = text;
            }
            HideWarning(delay);
        }

        public void HideWarning(int delay) {
            var time = new DispatcherTimer { Interval = TimeSpan.FromSeconds(delay) };
            time.Start();
            time.Tick += delegate {
                PopupWarning.IsOpen = false;
                time.Stop();
            };
        }


        private double DistanceFromStart(Point pntNow)
        {
            var length = new Vector(_mouseDownPointFirst.X - pntNow.X, _mouseDownPointFirst.Y - pntNow.Y).Length;
            var dist = MapPrivate.ImageScaleMperPix * length;
            return dist;
        }

        private double DistanceTrack(Vector move)
        {
            var dist = MapPrivate.ImageScaleMperPix * move.Length;
            _characterDistanceMoved += dist;
            return _characterDistanceMoved;
        }

        public void AddCurrentFilesToMru()
        {
            var mru = MapList.Aggregate("", (current, map) => current + map.ImageFilePath + ";");
            if (string.IsNullOrWhiteSpace(mru)) {
                return;
            }
            Settings.Default.MRU = mru;
            Settings.Default.Save();
        }

        #endregion region


        #region Save Timer
        private Timer _saveTimer;
 
        public void InitTimer()
        {
            _saveTimer = new Timer();
            _saveTimer.Tick += Save_Execute;
            _saveTimer.Interval = 30000; // in miliseconds
            _saveTimer.Start();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}