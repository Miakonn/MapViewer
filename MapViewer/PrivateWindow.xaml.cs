using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Bluegrams.Application;
using log4net;
using MapViewer.Dialogs;
using MapViewer.Maps;
using MapViewer.Properties;
using MapViewer.Symbols;
using MapViewer.Tools;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MapViewer {
    public partial class PrivateWindow :INotifyPropertyChanged {

        private enum CursorAction {
            None, MovingPublicMapPos, MovingPrivateMap, MovingSymbol
        }


        #region Attributes

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public PrivateMaskedMap MapPrivate;
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

        private UIElement _lastClickedElem;
        private Symbol _lastClickedSymbol;

        private CursorAction _cursorAction;
        private Point _mouseDownPoint;  // Canvas
        private Point _mouseDownPointFirst;  // Canvas
        private Point _mouseDownPointWindow;
        private ICanvasTool _activeTool;
        private bool writtenLogSetting;

        public bool IsImageCalibrated => MapPrivate?.IsCalibrated ?? false;

        private double _characterDistanceMoved;

        public ICanvasTool ActiveTool {
            get => _activeTool;

            set {
                _activeTool?.Deactivate();
                _activeTool = value;
                _cursorAction = CursorAction.None;
                MapPrivate?.SymbolsPM.RaiseSymbolsChanged();
                if (MapPublic != null && MapPublic.ShowPublicCursorTemporary) {
                    MapPublic.ShowPublicCursor = false;
                    MapPublic.ShowPublicCursorTemporary = false;
                }
            }
        }

        public bool PublicNeedsRescaling { get; private set; }
        private int _level;

        #endregion

        public PrivateWindow()
        {
            InitializeComponent();
            InitSettings();

            Title = $"Miakonn's MapViewer {FileVersion} - Private map";

            Log.Info($"STARTING MapViewer {FileVersion} ******************************************");

            MapPublic = PublicWindow.Map;
            MapPublic.ScreenScaleMMperM = 20.0;
            MapPublic.MapData.LastFigureScaleUsed = 50;
            MapPublic.IsLinked = false;
            MapPublic.Create();

            PrivateContextMenu.Opened += ContextMenu_OnOpened;

            InitTimer();

            ComboBoxPublicScale.DataContext = this;
        }

        private void HandleImageScaleChanged(object sender, EventArgs e) {
            OnPropertyChanged(nameof(IsImageCalibrated));
        }
        
        public void SetScale(int scale)
        {
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
                return fvi.FileVersion;
            }
        }

        private void MovePublic(Vector vector)
        {
            MapPublic.Translate(vector / MapPublic.ScaleDpiFix);
            if (MapPublic.IsLinked || MapPublic.MapId != MapPrivate.MapId) {
                MapPrivate.RemoveElement(MaskedMap.PublicPositionUid);
            }
            else {
                MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
            }
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
            MapPrivate?.ScaleToWindow(LayerMap);
        }


        private void GetLastClickedSymbol() {
            _lastClickedElem = MapPrivate.CanvasOverlay.FindElementHit();

            if (_lastClickedElem == null) {
                _lastClickedSymbol = null;
                return;
            }

            _lastClickedSymbol = MapPrivate.SymbolsPM.FindSymbolFromUid(_lastClickedElem.Uid);
        }

        private void PrivateWinMouseDown(object sender, MouseButtonEventArgs e) {
            if (ActiveTool != null) {
                ActiveTool.MouseDown(sender, e);
                return;
            }

            GetLastClickedSymbol();

            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1) {
                if (shiftPressed) {
                    if (_lastClickedSymbol != null) {
                        MapPrivate.SymbolsPM.ChangeSymbolSelection(_lastClickedSymbol);
                    }
                    else {
                        MapPrivate.SymbolsPM.ClearSymbolSelection();
                    }
                }
                else if (_lastClickedSymbol != null) {
                    _mouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    _mouseDownPointFirst = _mouseDownPoint;
                    _cursorAction = CursorAction.MovingSymbol;

                    _characterDistanceMoved = 0;
                }
                else if (_lastClickedElem != null && _lastClickedElem.Uid == MaskedMap.PublicPositionUid) {
                    _mouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    _mouseDownPointFirst = _mouseDownPoint;
                    _cursorAction =  CursorAction.MovingPublicMapPos;

                    _characterDistanceMoved = 0;
                }
                else {
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
                    MapPrivate.SymbolsPM.OpenEditor(_lastClickedSymbol, PointToScreen(_mouseDownPointWindow));
                }
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1) {
                _mouseDownPoint = e.GetPosition(MapPrivate.CanvasMap);
                _mouseDownPointWindow = e.GetPosition(this);
                _mouseDownPointFirst = _mouseDownPoint;
            }
        }

        private void PrivateWinMouseMove(object sender, MouseEventArgs e)
        {
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
                    DisplayPopup($"{DistanceFromStart(curMouseDownPoint),5:N1} Track: {DistanceTrack(move),5:N1}");
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

        private void PrivateWinMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ActiveTool != null) {
                ActiveTool.MouseUp(sender, e);
                return;
            }

            HidePopup(3);
            _cursorAction = CursorAction.None;
        }

        private void PrivateWinMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scale = (1.0 + e.Delta / 600.0);
            MapPrivate.Zoom(scale, e.GetPosition(this));
        }

        private void PrivateWindow_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ComboBoxPublicScale_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ComboBoxItem)ComboBoxPublicScale.SelectedItem;
            if (selected != null && MapPublic != null && MapPrivate != null) {
                PublicNeedsRescaling = true;
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

        private void ComboBoxPlayerSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ComboBoxItem)ComboBoxPlayerSize.SelectedItem;
            if (selected == null || MapPublic == null || MapPrivate == null) {
                return;
            }
            switch (selected.Uid) {
                case "PlayerSize_0.7":
                    MapPrivate.SymbolsPM.ChangePlayerSizes(0.7);
                    break;
                case "PlayerSize_0.8":
                    MapPrivate.SymbolsPM.ChangePlayerSizes(0.8);
                    break;
                case "PlayerSize_0.9":
                    MapPrivate.SymbolsPM.ChangePlayerSizes(0.9);
                    break;
                case "PlayerSize_1.0":
                    MapPrivate.SymbolsPM.ChangePlayerSizes(1.0);
                    break;
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
                    MapPublic.PlayerSizePixel = 20;
                    MapPrivate.PlayerSizePixel = 20;
                    break;
                case "PlayerMinSize_20p":
                    MapPublic.PlayerSizePixel = 20;
                    MapPrivate.PlayerSizePixel = 20;
                    break;
                case "PlayerMinSize_25p":
                    MapPublic.PlayerSizePixel = 25;
                    MapPrivate.PlayerSizePixel = 25;
                    break;
                case "PlayerMinSize_30p":
                    MapPublic.PlayerSizePixel = 30;
                    MapPrivate.PlayerSizePixel = 30;
                    break;
                case "PlayerMinSize_35p":
                    MapPublic.PlayerSizePixel = 35;
                    MapPrivate.PlayerSizePixel = 35;
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

        #endregion

        #region Public methods

        public void InitSettings() {
            //string user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //PortableSettingsProvider.SettingsFileName = $"user.{user}.config".Replace("\\", "_");
            PortableSettingsProvider.ApplyProvider(Settings.Default);
            if (!writtenLogSetting) {
                Log.Debug("Uses settings file:" + PortableSettingsProvider.SettingsFileName);
                writtenLogSetting = true;
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