﻿using System.Windows.Input;

namespace MapViewer {
    public static class CustomCommands {
        public static readonly RoutedUICommand CreateSymbolPlayer = new RoutedUICommand("Player", "Player", typeof(CustomCommands), null);
        public static readonly RoutedUICommand CreateSymbolNonPlayer = new RoutedUICommand("NonPlayer", "NonPlayer", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DuplicateSymbol = new RoutedUICommand("Duplicate", "Duplicate", typeof(CustomCommands), null);
        public static readonly RoutedUICommand CreateSymbolImage = new RoutedUICommand("Load Icon", "LoadIcon", typeof(CustomCommands), null);
        public static readonly RoutedUICommand CreateSpellCircular7m = new RoutedUICommand("Spell r=7m", "Spell_r=7m", typeof(CustomCommands), null);
        public static readonly RoutedUICommand CreateSpellCircular3m = new RoutedUICommand("Spell r=3m", "Spell_r=3m", typeof(CustomCommands), null);
        public static readonly RoutedUICommand CreateSpellCircular2m = new RoutedUICommand("Spell r=2m", "Spell_r=2m", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DeleteSymbol = new RoutedUICommand("Delete", "Delete", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SetSymbolColor = new RoutedUICommand("Set Color", "SetColor", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SendSymbolToFront = new RoutedUICommand("Bring to Front", "BringToFront", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SendSymbolToBack = new RoutedUICommand("Send to Back", "SendToBack", typeof(CustomCommands), null);
        public static readonly RoutedUICommand MoveSymbolUp = new RoutedUICommand("Move Up", "MoveUp", typeof(CustomCommands), null);
        public static readonly RoutedUICommand MoveSymbolDown = new RoutedUICommand("Move Down", "MoveDown", typeof(CustomCommands), null);
        public static readonly RoutedUICommand RotateSymbolCW = new RoutedUICommand("Rotate CW", "RotateCW", typeof(CustomCommands), null);
        public static readonly RoutedUICommand RotateSymbolCCW = new RoutedUICommand("Rotate CCW", "RotateCCW", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SetSymbolCross = new RoutedUICommand("Show Cross", "SetCross", typeof(CustomCommands), null);

        public static readonly RoutedUICommand SelectSymbols = new RoutedUICommand("Select", "SelectRectangle", typeof(CustomCommands), null);

        public static readonly RoutedUICommand FullMask = new RoutedUICommand("Full Mask", "FullMask", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ShowPublicCursorTemporary = new RoutedUICommand("Show Cursor Temp.", "ShowCursorTemp", typeof(CustomCommands), null);

        public static readonly RoutedUICommand OpenImage = new RoutedUICommand("Open Image", "OpenImage", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ExitApp = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands), null);
        public static readonly RoutedUICommand PublishMap = new RoutedUICommand("Publish", "Publish", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ClearMask = new RoutedUICommand("Clear Mask", "ClearMask", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ClearOverlay = new RoutedUICommand("Clear Symbols", "ClearSymbols", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ScaleToFit= new RoutedUICommand("Scale to Fit", "ScaleToFit", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ZoomIn = new RoutedUICommand("Zoom In", "ZoomIn", typeof(CustomCommands), null);
        public static readonly RoutedUICommand ZoomOut = new RoutedUICommand("Zoom Out", "ZoomOut", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelDown = new RoutedUICommand("Level Down", "LevelDown", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelUp = new RoutedUICommand("Level Up", "LevelUp", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelDownPublish = new RoutedUICommand("Level Down Publish", "LevelDownPublish", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelUpPublish = new RoutedUICommand("Level Up Publish", "LevelUpPublish", typeof(CustomCommands), null);

        public static readonly RoutedUICommand RotateMap = new RoutedUICommand("Rotate Map", "RotateMap", typeof(CustomCommands), null);
        public static readonly RoutedUICommand AddDisplay = new RoutedUICommand("Add Display", "AddDisplay", typeof(CustomCommands), null);
        public static readonly RoutedUICommand RemoveDisplay = new RoutedUICommand("Remove Display", "RemoveDisplay", typeof(CustomCommands), null);

        public static readonly RoutedUICommand Save = new RoutedUICommand("Save", "Save", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SaveSymbolsAs = new RoutedUICommand("Save Symbols as...", "SaveSymbolsAs", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LoadSymbols = new RoutedUICommand("Load Symbols", "LoadSymbols", typeof(CustomCommands), null);
        public static readonly RoutedUICommand CalibrateDisplay = new RoutedUICommand("Calibrate Display", "CalibrateDisplay", typeof(CustomCommands), null);
        public static readonly RoutedUICommand OpenLastImage = new RoutedUICommand("Open Last Image", "OpenLastImage", typeof(CustomCommands), null);

        public static readonly RoutedUICommand Calibrate = new RoutedUICommand("Calibrate", "Calibrate", typeof(CustomCommands), null);
        public static readonly RoutedUICommand Measure = new RoutedUICommand("Measure", "Measure", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DrawLine = new RoutedUICommand("Draw Line", "DrawLine", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DrawCone = new RoutedUICommand("Draw Cone", "DrawCone", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DrawCircle = new RoutedUICommand("Draw Circle", "DrawCircle", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DrawRectangle = new RoutedUICommand("Draw Rectangle", "DrawRectangle", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DrawText = new RoutedUICommand("Draw Text", "DrawText", typeof(CustomCommands), null);

        public static readonly RoutedUICommand MaskRectangle = new RoutedUICommand("Mask Rectangle", "MaskRectangle", typeof(CustomCommands), null);
        public static readonly RoutedUICommand UnmaskRectangle = new RoutedUICommand("Unmask Rectangle", "UnmaskRectangle", typeof(CustomCommands), null);
        public static readonly RoutedUICommand MaskCircle = new RoutedUICommand("Mask Circle", "MaskCircle", typeof(CustomCommands), null);
        public static readonly RoutedUICommand UnmaskCircle = new RoutedUICommand("Unmask Circle", "UnmaskCircle", typeof(CustomCommands), null);
        public static readonly RoutedUICommand MaskPolygon = new RoutedUICommand("Mask Polygon", "MaskPolygon", typeof(CustomCommands), null);
        public static readonly RoutedUICommand UnmaskPolygon = new RoutedUICommand("Unmask Polygon", "UnmaskPolygon", typeof(CustomCommands), null);
        public static readonly RoutedUICommand UnmaskLineOfSight20m = new RoutedUICommand("Unmask LOS 20m", "UnmaskLOS20m", typeof(CustomCommands), null);
    }
}