﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using MapViewer.Dialogs;
using MapViewer.Properties;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MapViewer {
    public partial class PrivateWindow {

		private enum CursorAction {
			None, MovingPublicMapPos, MovingPrivateMap, MovingElement
		}


		#region Attributes

		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MaskedMap MapPrivate;
		public MaskedMap MapPublic;
        public List<MaskedMap> MapList = new List<MaskedMap>();
        public int Level {
            get => _level;
            set => _level = Math.Max(Math.Min(value, LevelNumber - 1), 0);
        }

        public int LevelNumber => MapList.Count;

		public readonly PublicWindow PublicWindow = new PublicWindow();

		private UIElement _lastClickedElem;
		private CursorAction _cursorAction;
		private Point _mouseDownPoint;
        private Point _mouseDownPointFirst;
        private ICanvasTool _activeTool;

		public ICanvasTool ActiveTool {
			get => _activeTool;

            set {
                _activeTool?.Deactivate();
                _activeTool = value;
				_cursorAction = CursorAction.None;
				if (MapPublic != null && MapPublic.ShowPublicCursorTemporary) {
					MapPublic.ShowPublicCursor = false;
					MapPublic.ShowPublicCursorTemporary = false;
				}
			}
		}

        public bool PublicNeedsRescaling { get; private set; }
        private int _level;

        #endregion

		public PrivateWindow() {
			InitializeComponent();
			InitSettings();

			Title = $"Miakonn's MapViewer {FileVersion} - Private map";

			Log.Info("STARTING MapViewer ******************************************");

            MapPublic = PublicWindow.Map;
            MapPublic.ScreenScaleMMperM = 20.0;
			MapPublic.MapData.LastFigureScaleUsed = 50;
			MapPublic.IsLinked = false;
            MapPublic.Create();
        }

		public void SetScale(int scale) {
			if (scale == 0) {
				ComboBoxPublicScale.Text = "Linked";
			}
			else {
				var str =  "1:" + scale;
				ComboBoxPublicScale.Text = str;
			}
		}

		#region Private methods

		private static string FileVersion {
			get {
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
				return fvi.FileVersion;
			}
		}

		private void MovePublic(Vector vector) {
			MapPublic.Translate(vector / MapPublic.ScaleDpiFix);
			if (MapPublic.IsLinked || !string.Equals(MapPublic.ImageFilePath, MapPrivate.ImageFilePath)) {
				MapPrivate.DeleteShape(MaskedMap.PublicPositionUid);
			}
			else {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		#endregion

		#region Events

		private void Border_Loaded(object sender, RoutedEventArgs e) {
			var window = GetWindow(this);
			if (window != null) {
				window.KeyDown += PrivateWinKeyDown;
			}
		}

		private void PrivateWinKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
                if (_cursorAction == CursorAction.MovingElement) {
                    var move = new Vector((_mouseDownPoint.X - _mouseDownPointFirst.X), (_mouseDownPoint.Y - _mouseDownPointFirst.Y));
                    MapPrivate.MoveElement(_lastClickedElem, move);
                    MapPublic.MoveElement(_lastClickedElem.Uid, move);

                }
                HidePopup(0);
                ActiveTool = null;
				_cursorAction = CursorAction.None;
			}
			if (e.Key == Key.F12 && Publish_CanExecute()) {
				PublishMap_Execute(null, null);
			}

            ActiveTool?.KeyDown(sender, e);
        }

		private void PrivateWinSizeChanged(object sender, SizeChangedEventArgs e) {
			MapPrivate?.ScaleToWindow(MapPresenterMain2);
		}

		private void PrivateWinMouseDown(object sender, MouseButtonEventArgs e) {

			if (ActiveTool != null) {
				ActiveTool.MouseDown(sender, e);
				return;
			}
			_lastClickedElem = MapPrivate.CanvasOverlay.FindElementHit();

			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1) {
				if (_lastClickedElem != null) {
					_mouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
                    _mouseDownPointFirst = _mouseDownPoint;
                    _cursorAction = _lastClickedElem.Uid == MaskedMap.PublicPositionUid
						? CursorAction.MovingPublicMapPos
						: CursorAction.MovingElement;
				}
				else {
					_mouseDownPoint = e.GetPosition(this);
                    _mouseDownPointFirst = _mouseDownPoint;
					_cursorAction = CursorAction.MovingPrivateMap;
				}
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1) {
				_mouseDownPoint = e.GetPosition(MapPrivate.CanvasMapMask);
                _mouseDownPointFirst = _mouseDownPoint;
			}
        }

		private void PrivateWinMouseMove(object sender, MouseEventArgs e) {

			if (ActiveTool != null) {
				if (ActiveTool.ShowPublicCursor()) {
					MapPublic.MovePublicCursor(e.GetPosition(MapPrivate.CanvasOverlay));					
				}
				ActiveTool.MouseMove(sender, e);
				return;
			}

			MapPublic.MovePublicCursor(e.GetPosition(MapPrivate.CanvasOverlay));

			if (_cursorAction == CursorAction.MovingPublicMapPos) {
				var curMouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
				MovePublic(new Vector((_mouseDownPoint.X - curMouseDownPoint.X), (_mouseDownPoint.Y - curMouseDownPoint.Y)) * MapPublic.Scale * MapPublic.ScaleDpiFix);
				_mouseDownPoint = curMouseDownPoint;
                e.Handled = true;
			}
			else if (_cursorAction == CursorAction.MovingElement) {
				var curMouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
				var move = new Vector((_mouseDownPoint.X - curMouseDownPoint.X), (_mouseDownPoint.Y - curMouseDownPoint.Y));
				MapPrivate.MoveElement(_lastClickedElem, move);
				MapPublic.MoveElement(_lastClickedElem.Uid, move);
				_mouseDownPoint = curMouseDownPoint;
                DisplayPopup(CalculateDistanceToLastPoint(curMouseDownPoint) + " " + MapPrivate.Unit);
                e.Handled = true;
			}
			else if (_cursorAction == CursorAction.MovingPrivateMap) {
				var curMouseDownPoint = e.GetPosition(this);
				var move = curMouseDownPoint - _mouseDownPoint;
				MapPrivate.Translate(move);
				_mouseDownPoint = curMouseDownPoint;
				e.Handled = true;
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
			MapPrivate.Zoom(scale, e.GetPosition(this));
		}

		private void PrivateWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			Application.Current.Shutdown();
		}

		private void ComboBoxPublicScale_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
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
		}

        private void ComboBoxPlayerSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = (ComboBoxItem)ComboBoxPlayerSize.SelectedItem;
            if (selected != null && MapPublic != null && MapPrivate != null) {
                switch (selected.Uid) {
                    case "PlayerSize_0.7m":
                        MapPublic.PlayerSizeMeter = 0.7;
                        MapPublic.PlayerSizePixel = 0;
                        MapPrivate.PlayerSizeMeter = 0.7;
                        MapPrivate.PlayerSizePixel = 0;
                        break;
                    case "PlayerSize_1m":
                        MapPublic.PlayerSizeMeter = 1;
                        MapPublic.PlayerSizePixel = 0;
                        MapPrivate.PlayerSizeMeter = 1;
                        MapPrivate.PlayerSizePixel = 0;
                        break;
                    case "PlayerSize_20p":
                        MapPublic.PlayerSizeMeter = 0;
                        MapPublic.PlayerSizePixel = 20;
                        MapPrivate.PlayerSizeMeter = 0;
                        MapPrivate.PlayerSizePixel = 20;
                        break;
                    case "PlayerSize_25p":
                        MapPublic.PlayerSizeMeter = 0;
                        MapPublic.PlayerSizePixel = 25;
                        MapPrivate.PlayerSizeMeter = 0;
                        MapPrivate.PlayerSizePixel = 25;
                        break;
                    case "PlayerSize_30p":
                        MapPublic.PlayerSizeMeter = 0;
                        MapPublic.PlayerSizePixel = 30;
                        MapPrivate.PlayerSizeMeter = 0;
                        MapPrivate.PlayerSizePixel = 30;
                        break;
                    case "PlayerSize_35p":
                        MapPublic.PlayerSizeMeter = 0;
                        MapPublic.PlayerSizePixel = 35;
                        MapPrivate.PlayerSizeMeter = 0;
                        MapPrivate.PlayerSizePixel = 35;
                        break;
                }
                MapPrivate.UpdatePlayerElementSizes();
                MapPublic.UpdatePlayerElementSizes();
            }
        }

        private void Tab_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			ActiveTool = null;
		}
	
		private void HelpButton_OnClick(object sender, RoutedEventArgs e) {
			var dialog = new DialogAbout {Owner = this};
			dialog.ShowDialog();
		}

		#endregion

		#region Public methods

		public void InitSettings() {
			var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			Settings.Default.SettingsKey = Path.Combine(path, "Miakonn\\MapViewer\\user.config");			
		}

		public void DisplayPopup(string text) {
			PopupDisplay.IsOpen = true;
            if (PopupDisplay.Child is TextBlock popupText) {
				popupText.Text = text;
			}
		}

		public void HidePopup(int delay) {
			var time = new DispatcherTimer { Interval = TimeSpan.FromSeconds(delay) };
			time.Start();
			time.Tick += delegate {
				PopupDisplay.IsOpen = false;
				time.Stop();
			};
		}

        private string CalculateDistanceToLastPoint(Point pntNow) {
            var length = new Vector(_mouseDownPointFirst.X - pntNow.X, _mouseDownPointFirst.Y - pntNow.Y).Length;
            var dist = MapPrivate.ImageScaleMperPix * length;
            return dist.ToString("N1");
        }

        public void AddToMru(string path) {
			Settings.Default.MRU = path;
			Settings.Default.Save();
		}

		#endregion region

    }
}