﻿using System.Windows;

namespace FocLauncher.Theming
{
    public static class EnvironmentColors
    {
        private static ComponentResourceKey _backgroundColor;
        private static ComponentResourceKey _backgroundImage;

        private static ComponentResourceKey _captionText;
        private static ComponentResourceKey _windowText;


        private static ComponentResourceKey _listBoxBackground;
        private static ComponentResourceKey _listBoxBorder;

        private static ComponentResourceKey _listBoxItemBackground;
        private static ComponentResourceKey _listBoxItemBorder;
        private static ComponentResourceKey _listBoxItemText;
        private static ComponentResourceKey _listBoxItemHoverBackground;
        private static ComponentResourceKey _listBoxItemHoverBorder;
        private static ComponentResourceKey _listBoxItemHoverText;
        private static ComponentResourceKey _listBoxItemSelectedBackground;
        private static ComponentResourceKey _listBoxItemSelectedBorder;
        private static ComponentResourceKey _listBoxItemSelectedText;
        private static ComponentResourceKey _listBoxItemInactiveBackground;
        private static ComponentResourceKey _listBoxItemInactiveBorder;
        private static ComponentResourceKey _listBoxItemInactiveText;


        private static ComponentResourceKey _statusBarDefaultBackground;
        private static ComponentResourceKey _statusBarDefaultText;

        private static ComponentResourceKey _statusBarRunningBackground;
        private static ComponentResourceKey _statusBarRunningText;

        private static ComponentResourceKey _waitWindowBackground;
        private static ComponentResourceKey _waitWindowBorder;
        private static ComponentResourceKey _waitWindowText;
        private static ComponentResourceKey _waitWindowCaptionBackground;
        private static ComponentResourceKey _waitWindowCaptionText;

        private static ComponentResourceKey _scrollBarArrowBackground;
        private static ComponentResourceKey _scrollBarArrowDisabledBackground;
        private static ComponentResourceKey _scrollBarArrowGlyph;
        private static ComponentResourceKey _scrollBarArrowGlyphDisabled;
        private static ComponentResourceKey _scrollBarArrowGlyphMouseOver;
        private static ComponentResourceKey _scrollBarArrowGlyphPressed;
        private static ComponentResourceKey _scrollBarArrowMouseOverBackground;
        private static ComponentResourceKey _scrollBarArrowPressedBackground;
        private static ComponentResourceKey _scrollBarBackground;
        private static ComponentResourceKey _scrollBarBorder;
        private static ComponentResourceKey _scrollBarDisabledBackground;
        private static ComponentResourceKey _scrollBarThumbBackground;
        private static ComponentResourceKey _scrollBarThumbBorder;
        private static ComponentResourceKey _scrollBarThumbDisabled;
        private static ComponentResourceKey _scrollBarThumbGlyph;
        private static ComponentResourceKey _scrollBarThumbGlyphMouseOverBorder;
        private static ComponentResourceKey _scrollBarThumbGlyphPressedBorder;
        private static ComponentResourceKey _scrollBarThumbMouseOverBackground;
        private static ComponentResourceKey _scrollBarThumbMouseOverBorder;
        private static ComponentResourceKey _scrollBarThumbPressedBackground;
        private static ComponentResourceKey _scrollBarThumbPressedBorder;
        private static ComponentResourceKey _autoHideResizeGrip;

        private static ComponentResourceKey _playButtonBackground;
        private static ComponentResourceKey _playButtonBorder;
        private static ComponentResourceKey _playButtonText;
        private static ComponentResourceKey _playButtonBackgroundHover;
        private static ComponentResourceKey _playButtonBorderHover;
        private static ComponentResourceKey _playButtonTextHover;
        private static ComponentResourceKey _playButtonBackgroundPressed;
        private static ComponentResourceKey _playButtonBorderPressed;
        private static ComponentResourceKey _playButtonTextPressed;
        private static ComponentResourceKey _playButtonBackgroundDisabled;
        private static ComponentResourceKey _playButtonBorderDisabled;
        private static ComponentResourceKey _playButtonTextDisabled;

        private static ComponentResourceKey _checkBoxBackground;
        private static ComponentResourceKey _checkBoxBorder;
        private static ComponentResourceKey _checkBoxGlyph;
        private static ComponentResourceKey _checkBoxText;
        private static ComponentResourceKey _checkBoxBackgroundHover;
        private static ComponentResourceKey _checkBoxBorderHover;
        private static ComponentResourceKey _checkBoxGlyphHover;
        private static ComponentResourceKey _checkBoxTextHover;
        private static ComponentResourceKey _checkBoxBackgroundDown;
        private static ComponentResourceKey _checkBoxBorderDown;
        private static ComponentResourceKey _checkBoxGlyphDown;
        private static ComponentResourceKey _checkBoxTextDown;
        private static ComponentResourceKey _checkBoxBackgroundDisabled;
        private static ComponentResourceKey _checkBoxBorderDisabled;
        private static ComponentResourceKey _checkBoxGlyphDisabled;
        private static ComponentResourceKey _checkBoxTextDisabled;

        private static ComponentResourceKey _mainWindowActiveShadowAndBorderColor;
        private static ComponentResourceKey _mainWindowInactiveShadowAndBorderColor;
        private static ComponentResourceKey _mainWindowTitleBarBackground;
        private static ComponentResourceKey _mainWindowTitleBarForeground;
        private static ComponentResourceKey _mainWindowTitleBarForegroundInactive;

        private static ComponentResourceKey _windowTitleBarButtonBackground;
        private static ComponentResourceKey _windowTitleBarButtonBorder;
        private static ComponentResourceKey _windowTitleBarButtonForeground;
        private static ComponentResourceKey _windowTitleBarButtonHoverBackground;
        private static ComponentResourceKey _windowTitleBarButtonHoverBorder;
        private static ComponentResourceKey _windowTitleBarButtonHoverForeground;
        private static ComponentResourceKey _windowTitleBarButtonDownBackground;
        private static ComponentResourceKey _windowTitleBarButtonDownBorder;
        private static ComponentResourceKey _windowTitleBarButtonDownForeground;
        private static ComponentResourceKey _windowTitleBarButtonInactiveBackground;
        private static ComponentResourceKey _windowTitleBarButtonInactiveBorder;
        private static ComponentResourceKey _windowTitleBarButtonInactiveForeground;
        private static ComponentResourceKey _windowTitleBarButtonInactiveHoverBackground;
        private static ComponentResourceKey _windowTitleBarButtonInactiveHoverBorder;
        private static ComponentResourceKey _windowTitleBarButtonInactiveHoverForeground;
       
        private static ComponentResourceKey _textBoxBackground;
        private static ComponentResourceKey _textBoxBorder;
        private static ComponentResourceKey _textBoxWatermark;
        private static ComponentResourceKey _textBoxTextDisabled;
        private static ComponentResourceKey _textBoxBackgroundFocused;
        private static ComponentResourceKey _textBoxBorderFocused;
        private static ComponentResourceKey _textBoxBackgroundHover;
        private static ComponentResourceKey _textBoxBorderHover;
        private static ComponentResourceKey _textBoxBackgroundDisabled;
        private static ComponentResourceKey _textBoxBorderDisabled;
        private static ComponentResourceKey _textBoxTextUnfocused;
        private static ComponentResourceKey _textBoxTextFocused;
        private static ComponentResourceKey _textBoxTextHover;
        private static ComponentResourceKey _textBoxSelection;
        private static ComponentResourceKey _textBoxWatermarkHover;


        public static ComponentResourceKey BackgroundColor =>
            _backgroundColor ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundColor));

        public static ComponentResourceKey BackgroundImage =>
            _backgroundImage ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundImage));

        public static ComponentResourceKey CaptionText =>
            _captionText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(CaptionText));

        public static ComponentResourceKey WindowText =>
            _windowText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(WindowText));


        public static ComponentResourceKey ListBoxBackground =>
            _listBoxBackground ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxBackground));

        public static ComponentResourceKey ListBoxBorder =>
            _listBoxBorder ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxBorder));



        public static ComponentResourceKey ListBoxItemBackground =>
            _listBoxItemBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemBackground));

        public static ComponentResourceKey ListBoxItemBorder =>
            _listBoxItemBorder ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemBorder));

        public static ComponentResourceKey ListBoxItemText =>
            _listBoxItemText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemText));

        public static ComponentResourceKey ListBoxItemHoverBackground =>
            _listBoxItemHoverBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemHoverBackground));

        public static ComponentResourceKey ListBoxItemHoverBorder =>
            _listBoxItemHoverBorder ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemHoverBorder));

        public static ComponentResourceKey ListBoxItemHoverText =>
            _listBoxItemHoverText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemHoverText));

        public static ComponentResourceKey ListBoxItemSelectedBackground =>
            _listBoxItemSelectedBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemSelectedBackground));

        public static ComponentResourceKey ListBoxItemSelectedBorder =>
            _listBoxItemSelectedBorder ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemSelectedBorder));

        public static ComponentResourceKey ListBoxItemSelectedText =>
            _listBoxItemSelectedText ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemSelectedText));

        public static ComponentResourceKey ListBoxItemInactiveBackground =>
            _listBoxItemInactiveBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemInactiveBackground));

        public static ComponentResourceKey ListBoxItemInactiveBorder =>
            _listBoxItemInactiveBorder ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemInactiveBorder));

        public static ComponentResourceKey ListBoxItemInactiveText =>
            _listBoxItemInactiveText ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(ListBoxItemInactiveText));





        public static ComponentResourceKey StatusBarDefaultBackground =>
            _statusBarDefaultBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarDefaultBackground));

        public static ComponentResourceKey StatusBarDefaultText =>
            _statusBarDefaultText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarDefaultText));

        public static ComponentResourceKey StatusBarRunningBackground =>
            _statusBarRunningBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarRunningBackground));

        public static ComponentResourceKey StatusBarRunningText =>
            _statusBarRunningText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarRunningText));


        public static ComponentResourceKey WaitWindowBackground =>
            _waitWindowBackground ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowBackground));

        public static ComponentResourceKey WaitWindowBorder =>
            _waitWindowBorder ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowBorder));

        public static ComponentResourceKey WaitWindowText =>
            _waitWindowText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowText));

        public static ComponentResourceKey WaitWindowCaptionBackground =>
            _waitWindowCaptionBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowCaptionBackground));

        public static ComponentResourceKey WaitWindowCaptionText =>
            _waitWindowCaptionText ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowCaptionText));



        public static ComponentResourceKey ScrollBarArrowBackground => _scrollBarArrowBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowBackground));

        public static ComponentResourceKey ScrollBarArrowDisabledBackground => _scrollBarArrowDisabledBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowDisabledBackground));

        public static ComponentResourceKey ScrollBarArrowGlyph => _scrollBarArrowGlyph ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowGlyph));

        public static ComponentResourceKey ScrollBarArrowGlyphDisabled => _scrollBarArrowGlyphDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowGlyphDisabled));

        public static ComponentResourceKey ScrollBarArrowGlyphMouseOver => _scrollBarArrowGlyphMouseOver ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowGlyphMouseOver));

        public static ComponentResourceKey ScrollBarArrowGlyphPressed => _scrollBarArrowGlyphPressed ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowGlyphPressed));

        public static ComponentResourceKey ScrollBarArrowMouseOverBackground => _scrollBarArrowMouseOverBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowMouseOverBackground));

        public static ComponentResourceKey ScrollBarArrowPressedBackground => _scrollBarArrowPressedBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarArrowPressedBackground));

        public static ComponentResourceKey ScrollBarBackground => _scrollBarBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarBackground));

        public static ComponentResourceKey ScrollBarBorder => _scrollBarBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarBorder));

        public static ComponentResourceKey ScrollBarDisabledBackground => _scrollBarDisabledBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarDisabledBackground));


        public static ComponentResourceKey ScrollBarThumbBackground => _scrollBarThumbBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbBackground));

        public static ComponentResourceKey ScrollBarThumbBorder => _scrollBarThumbBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbBorder));

        public static ComponentResourceKey ScrollBarThumbDisabled => _scrollBarThumbDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbDisabled));

        public static ComponentResourceKey ScrollBarThumbGlyph => _scrollBarThumbGlyph ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbGlyph));

        public static ComponentResourceKey ScrollBarThumbGlyphMouseOverBorder => _scrollBarThumbGlyphMouseOverBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbGlyphMouseOverBorder));

        public static ComponentResourceKey ScrollBarThumbGlyphPressedBorder => _scrollBarThumbGlyphPressedBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbGlyphPressedBorder));

        public static ComponentResourceKey ScrollBarThumbMouseOverBackground => _scrollBarThumbMouseOverBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbMouseOverBackground));

        public static ComponentResourceKey ScrollBarThumbMouseOverBorder => _scrollBarThumbMouseOverBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbMouseOverBorder));

        public static ComponentResourceKey ScrollBarThumbPressedBackground => _scrollBarThumbPressedBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbPressedBackground));

        public static ComponentResourceKey ScrollBarThumbPressedBorder => _scrollBarThumbPressedBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(ScrollBarThumbPressedBorder));

        public static ComponentResourceKey AutoHideResizeGrip => _autoHideResizeGrip ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(AutoHideResizeGrip));


        public static ComponentResourceKey PlayButtonBackground =>
            _playButtonBackground ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBackground));

        public static ComponentResourceKey PlayButtonBorder =>
            _playButtonBorder ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBorder));

        public static ComponentResourceKey PlayButtonText =>
            _playButtonText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonText));

        public static ComponentResourceKey PlayButtonBackgroundHover =>
            _playButtonBackgroundHover ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBackgroundHover));

        public static ComponentResourceKey PlayButtonBorderHover =>
            _playButtonBorderHover ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBorderHover));

        public static ComponentResourceKey PlayButtonTextHover =>
            _playButtonTextHover ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonTextHover));

        public static ComponentResourceKey PlayButtonBackgroundPressed =>
            _playButtonBackgroundPressed ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBackgroundPressed));

        public static ComponentResourceKey PlayButtonBorderPressed =>
            _playButtonBorderPressed ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBorderPressed));

        public static ComponentResourceKey PlayButtonTextPressed =>
            _playButtonTextPressed ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonTextPressed));

        public static ComponentResourceKey PlayButtonBackgroundDisabled =>
            _playButtonBackgroundDisabled ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBackgroundDisabled));

        public static ComponentResourceKey PlayButtonBorderDisabled =>
            _playButtonBorderDisabled ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonBorderDisabled));

        public static ComponentResourceKey PlayButtonTextDisabled =>
            _playButtonTextDisabled ??=
                new ComponentResourceKey(typeof(EnvironmentColors), nameof(PlayButtonTextDisabled));



        public static ComponentResourceKey CheckBoxBackground => _checkBoxBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBackground));

        public static ComponentResourceKey CheckBoxBorder =>
            _checkBoxBorder ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBorder));

        public static ComponentResourceKey CheckBoxGlyph =>
            _checkBoxGlyph ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxGlyph));

        public static ComponentResourceKey CheckBoxText =>
            _checkBoxText ??= new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxText));

        public static ComponentResourceKey CheckBoxBackgroundHover => _checkBoxBackgroundHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBackgroundHover));

        public static ComponentResourceKey CheckBoxBorderHover => _checkBoxBorderHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBorderHover));

        public static ComponentResourceKey CheckBoxGlyphHover => _checkBoxGlyphHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxGlyphHover));

        public static ComponentResourceKey CheckBoxTextHover => _checkBoxTextHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxTextHover));

        public static ComponentResourceKey CheckBoxBackgroundDown => _checkBoxBackgroundDown ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBackgroundDown));

        public static ComponentResourceKey CheckBoxBorderDown => _checkBoxBorderDown ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBorderDown));

        public static ComponentResourceKey CheckBoxGlyphDown => _checkBoxGlyphDown ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxGlyphDown));

        public static ComponentResourceKey CheckBoxTextDown => _checkBoxTextDown ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxTextDown));

        public static ComponentResourceKey CheckBoxBackgroundDisabled => _checkBoxBackgroundDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBackgroundDisabled));

        public static ComponentResourceKey CheckBoxBorderDisabled => _checkBoxBorderDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxBorderDisabled));

        public static ComponentResourceKey CheckBoxGlyphDisabled => _checkBoxGlyphDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxGlyphDisabled));

        public static ComponentResourceKey CheckBoxTextDisabled => _checkBoxTextDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxTextDisabled));


        public static ComponentResourceKey MainWindowActiveShadowAndBorderColor =>
            _mainWindowActiveShadowAndBorderColor ??=
                new ComponentResourceKey(typeof(EnvironmentColors), "MainWindowActiveShadowAndBorderColor");

        public static ComponentResourceKey MainWindowInactiveShadowAndBorderColor =>
            _mainWindowInactiveShadowAndBorderColor ??= new ComponentResourceKey(typeof(EnvironmentColors),
                "MainWindowInactiveShadowAndBorderColor");

        public static ComponentResourceKey MainWindowTitleBarBackground => _mainWindowTitleBarBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "MainWindowTitleBarBackground");

        public static ComponentResourceKey MainWindowTitleBarForeground => _mainWindowTitleBarForeground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "MainWindowTitleBarForeground");

        public static ComponentResourceKey MainWindowTitleBarForegroundInactive =>
            _mainWindowTitleBarForegroundInactive ??=
                new ComponentResourceKey(typeof(EnvironmentColors), "MainWindowTitleBarForegroundInactive");



        public static ComponentResourceKey WindowTitleBarButtonBackground => _windowTitleBarButtonBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonBackground");

        public static ComponentResourceKey WindowTitleBarButtonBorder => _windowTitleBarButtonBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonBorder");

        public static ComponentResourceKey WindowTitleBarButtonForeground => _windowTitleBarButtonForeground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonForeground");

        public static ComponentResourceKey WindowTitleBarButtonHoverBackground =>
            _windowTitleBarButtonHoverBackground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonHoverBackground");

        public static ComponentResourceKey WindowTitleBarButtonHoverBorder => _windowTitleBarButtonHoverBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonHoverBorder");

        public static ComponentResourceKey WindowTitleBarButtonHoverForeground =>
            _windowTitleBarButtonHoverForeground ??=
                new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonHoverForeground");

        public static ComponentResourceKey WindowTitleBarButtonDownBackground => _windowTitleBarButtonDownBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonDownBackground");

        public static ComponentResourceKey WindowTitleBarButtonDownBorder => _windowTitleBarButtonDownBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonDownBorder");

        public static ComponentResourceKey WindowTitleBarButtonDownForeground => _windowTitleBarButtonDownForeground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonDownForeground");

        public static ComponentResourceKey WindowTitleBarButtonInactiveBackground => _windowTitleBarButtonInactiveBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonInactiveBackground");

        public static ComponentResourceKey WindowTitleBarButtonInactiveBorder => _windowTitleBarButtonInactiveBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonInactiveBorder");

        public static ComponentResourceKey WindowTitleBarButtonInactiveForeground => _windowTitleBarButtonInactiveForeground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonInactiveForeground");

        public static ComponentResourceKey WindowTitleBarButtonInactiveHoverBackground => _windowTitleBarButtonInactiveHoverBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonInactiveHoverBackground");

        public static ComponentResourceKey WindowTitleBarButtonInactiveHoverBorder => _windowTitleBarButtonInactiveHoverBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonInactiveHoverBorder");

        public static ComponentResourceKey WindowTitleBarButtonInactiveHoverForeground => _windowTitleBarButtonInactiveHoverForeground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), "WindowTitleBarButtonInactiveHoverForeground");


        


        public static ComponentResourceKey TextBoxBackground => _textBoxBackground ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBackground));

        public static ComponentResourceKey TextBoxBorder => _textBoxBorder ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBorder));

        public static ComponentResourceKey TextBoxWatermark => _textBoxWatermark ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxWatermark));

        public static ComponentResourceKey TextBoxTextDisabled => _textBoxTextDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxTextDisabled));

        public static ComponentResourceKey TextBoxBackgroundFocused => _textBoxBackgroundFocused ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBackgroundFocused));

        public static ComponentResourceKey TextBoxBorderFocused => _textBoxBorderFocused ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBorderFocused));

        public static ComponentResourceKey TextBoxBackgroundHover => _textBoxBackgroundHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBackgroundHover));

        public static ComponentResourceKey TextBoxBorderHover => _textBoxBorderHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBorderHover));

        public static ComponentResourceKey TextBoxBackgroundDisabled => _textBoxBackgroundDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBackgroundDisabled));

        public static ComponentResourceKey TextBoxBorderDisabled => _textBoxBorderDisabled ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxBorderDisabled));

        public static ComponentResourceKey TextBoxTextUnfocused => _textBoxTextUnfocused ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxTextUnfocused));

        public static ComponentResourceKey TextBoxTextFocused => _textBoxTextFocused ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxTextFocused));

        public static ComponentResourceKey TextBoxTextHover => _textBoxTextHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxTextHover));

        public static ComponentResourceKey TextBoxSelection => _textBoxSelection ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxSelection));

        public static ComponentResourceKey TextBoxWatermarkHover => _textBoxWatermarkHover ??=
            new ComponentResourceKey(typeof(EnvironmentColors), nameof(TextBoxWatermarkHover));

    }
}
