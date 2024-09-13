/*  CTRADER GURU --> Template 1.0.6

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/

*/

using System;
using cAlgo.API;

namespace cAlgo.Indicators
{

    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class ZigZag : Indicator
    {

        #region Enums

        public enum MyColors
        {

            AliceBlue,
            AntiqueWhite,
            Aqua,
            Aquamarine,
            Azure,
            Beige,
            Bisque,
            Black,
            BlanchedAlmond,
            Blue,
            BlueViolet,
            Brown,
            BurlyWood,
            CadetBlue,
            Chartreuse,
            Chocolate,
            Coral,
            CornflowerBlue,
            Cornsilk,
            Crimson,
            Cyan,
            DarkBlue,
            DarkCyan,
            DarkGoldenrod,
            DarkGray,
            DarkGreen,
            DarkKhaki,
            DarkMagenta,
            DarkOliveGreen,
            DarkOrange,
            DarkOrchid,
            DarkRed,
            DarkSalmon,
            DarkSeaGreen,
            DarkSlateBlue,
            DarkSlateGray,
            DarkTurquoise,
            DarkViolet,
            DeepPink,
            DeepSkyBlue,
            DimGray,
            DodgerBlue,
            Firebrick,
            FloralWhite,
            ForestGreen,
            Fuchsia,
            Gainsboro,
            GhostWhite,
            Gold,
            Goldenrod,
            Gray,
            Green,
            GreenYellow,
            Honeydew,
            HotPink,
            IndianRed,
            Indigo,
            Ivory,
            Khaki,
            Lavender,
            LavenderBlush,
            LawnGreen,
            LemonChiffon,
            LightBlue,
            LightCoral,
            LightCyan,
            LightGoldenrodYellow,
            LightGray,
            LightGreen,
            LightPink,
            LightSalmon,
            LightSeaGreen,
            LightSkyBlue,
            LightSlateGray,
            LightSteelBlue,
            LightYellow,
            Lime,
            LimeGreen,
            Linen,
            Magenta,
            Maroon,
            MediumAquamarine,
            MediumBlue,
            MediumOrchid,
            MediumPurple,
            MediumSeaGreen,
            MediumSlateBlue,
            MediumSpringGreen,
            MediumTurquoise,
            MediumVioletRed,
            MidnightBlue,
            MintCream,
            MistyRose,
            Moccasin,
            NavajoWhite,
            Navy,
            OldLace,
            Olive,
            OliveDrab,
            Orange,
            OrangeRed,
            Orchid,
            PaleGoldenrod,
            PaleGreen,
            PaleTurquoise,
            PaleVioletRed,
            PapayaWhip,
            PeachPuff,
            Peru,
            Pink,
            Plum,
            PowderBlue,
            Purple,
            Red,
            RosyBrown,
            RoyalBlue,
            SaddleBrown,
            Salmon,
            SandyBrown,
            SeaGreen,
            SeaShell,
            Sienna,
            Silver,
            SkyBlue,
            SlateBlue,
            SlateGray,
            Snow,
            SpringGreen,
            SteelBlue,
            Tan,
            Teal,
            Thistle,
            Tomato,
            Transparent,
            Turquoise,
            Violet,
            Wheat,
            White,
            WhiteSmoke,
            Yellow,
            YellowGreen

        }

        public enum ModeZigZag
        {

            HighLow,
            OpenClose

        }

        #endregion

        #region Identity

        public const string NAME = "ZigZag";

        public const string VERSION = "1.0.5";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://www.google.com/search?q=ctrader+guru+zigzag")]
        public string ProductInfo { get; set; }

        [Parameter("Mode", DefaultValue = ModeZigZag.HighLow, Group = "Params")]
        public ModeZigZag MyModeZigZag { get; set; }

        [Parameter(DefaultValue = 12, Group = "Params")]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 5, Group = "Params")]
        public int Deviation { get; set; }

        [Parameter(DefaultValue = 3, Group = "Params")]
        public int BackStep { get; set; }

        [Parameter("Show", DefaultValue = true, Group = "Label")]
        public bool ShowLabel { get; set; }

        [Parameter("Color High", DefaultValue = MyColors.DodgerBlue, Group = "Label")]
        public MyColors ColorHigh { get; set; }

        [Parameter("Color Low", DefaultValue = MyColors.Red, Group = "Label")]
        public MyColors ColorLow { get; set; }

        [Output("ZigZag", LineColor = "DodgerBlue", LineStyle = LineStyle.Lines)]
        public IndicatorDataSeries Result { get; set; }

        #endregion

        #region Property

        private double _lastLow;
        private double _lastHigh;
        private double _low;
        private double _high;
        private int _lastHighIndex;
        private int _lastLowIndex;
        private int _type;
        private double _point;
        private double _currentLow;
        private double _currentHigh;

        private int _countHigh = 0;
        private int _countLow = 0;
        private int _waiting = 0;

        private IndicatorDataSeries _highZigZags;
        private IndicatorDataSeries _lowZigZags;

        #endregion

        #region Indicator Events

        protected override void Initialize()
        {

            Print("{0} : {1}", NAME, VERSION);

            _highZigZags = CreateDataSeries();
            _lowZigZags = CreateDataSeries();
            _point = Symbol.TickSize;

        }

        public override void Calculate(int index)
        {

            switch (MyModeZigZag)
            {

                case ModeZigZag.HighLow:

                    PerformIndicatorHighLow(index);

                    break;

                case ModeZigZag.OpenClose:

                    PerformIndicatorOpenClose(index);

                    break;

            }

            if (_highZigZags[index] == _lastHigh && _waiting > -1)
            {

                _waiting = -1;
                _countHigh++;

            }
            
            if (_lowZigZags[index] == _lastLow && _waiting < 1)
            {

                _waiting = 1;
                _countLow++;

            }

            if (ShowLabel)
            {

                if (_highZigZags[index] > 0)
                {

                    ChartText cth = Chart.DrawText("zzh-" + _countHigh, _highZigZags[_lastHighIndex].ToString("N" + Symbol.Digits), _lastHighIndex, _highZigZags[_lastHighIndex], Color.FromName(ColorHigh.ToString("G")));
                    cth.HorizontalAlignment = HorizontalAlignment.Center;
                    cth.VerticalAlignment = VerticalAlignment.Top;

                }

                if (_lowZigZags[index] > 0)
                {

                    ChartText ctl = Chart.DrawText("zzl-" + _countLow, _lowZigZags[_lastLowIndex].ToString("N" + Symbol.Digits), _lastLowIndex, _lowZigZags[_lastLowIndex], Color.FromName(ColorLow.ToString("G")));
                    ctl.HorizontalAlignment = HorizontalAlignment.Center;
                    ctl.VerticalAlignment = VerticalAlignment.Bottom;

                }

            }            

        }

        #endregion

        #region Private Methods

        private void PerformIndicatorHighLow(int index)
        {

            if (index < Depth)
            {
                Result[index] = 0;
                _highZigZags[index] = 0;
                _lowZigZags[index] = 0;
                return;
            }

            _currentLow = Functions.Minimum(Bars.LowPrices, Depth);

            if (Math.Abs(_currentLow - _lastLow) < double.Epsilon)
                _currentLow = 0.0;
            else
            {

                _lastLow = _currentLow;

                if ((Bars.LowPrices[index] - _currentLow) > (Deviation * _point))
                    _currentLow = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_lowZigZags[index - i]) > double.Epsilon && _lowZigZags[index - i] > _currentLow)
                            _lowZigZags[index - i] = 0.0;
                    }
                }
            }
            if (Math.Abs(Bars.LowPrices[index] - _currentLow) < double.Epsilon)
                _lowZigZags[index] = _currentLow;
            else
                _lowZigZags[index] = 0.0;

            _currentHigh = Bars.HighPrices.Maximum(Depth);

            if (Math.Abs(_currentHigh - _lastHigh) < double.Epsilon)
                _currentHigh = 0.0;
            else
            {

                _lastHigh = _currentHigh;

                if ((_currentHigh - Bars.HighPrices[index]) > (Deviation * _point))
                    _currentHigh = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_highZigZags[index - i]) > double.Epsilon && _highZigZags[index - i] < _currentHigh)
                            _highZigZags[index - i] = 0.0;
                    }
                }
            }

            if (Math.Abs(Bars.HighPrices[index] - _currentHigh) < double.Epsilon)
                _highZigZags[index] = _currentHigh;
            else
                _highZigZags[index] = 0.0;


            switch (_type)
            {
                case 0:
                    if (Math.Abs(_low - 0) < double.Epsilon && Math.Abs(_high - 0) < double.Epsilon)
                    {
                        if (Math.Abs(_highZigZags[index]) > double.Epsilon)
                        {
                            _high = Bars.HighPrices[index];
                            _lastHighIndex = index;
                            _type = -1;
                            Result[index] = _high;
                        }
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon)
                        {
                            _low = Bars.LowPrices[index];
                            _lastLowIndex = index;
                            _type = 1;
                            Result[index] = _low;
                        }
                    }
                    break;
                case 1:
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && _lowZigZags[index] < _low && Math.Abs(_highZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastLowIndex] = double.NaN;
                        _lastLowIndex = index;
                        _low = _lowZigZags[index];
                        Result[index] = _low;
                    }
                    if (Math.Abs(_highZigZags[index] - 0.0) > double.Epsilon && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        _high = _highZigZags[index];
                        _lastHighIndex = index;
                        Result[index] = _high;
                        _type = -1;
                    }
                    break;
                case -1:
                    if (Math.Abs(_highZigZags[index]) > double.Epsilon && _highZigZags[index] > _high && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastHighIndex] = double.NaN;
                        _lastHighIndex = index;
                        _high = _highZigZags[index];
                        Result[index] = _high;
                    }
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && Math.Abs(_highZigZags[index]) < double.Epsilon)
                    {
                        _low = _lowZigZags[index];
                        _lastLowIndex = index;
                        Result[index] = _low;
                        _type = 1;
                    }
                    break;
                default:
                    return;
            }

        }

        private void PerformIndicatorOpenClose(int index)
        {

            if (index < Depth)
            {
                Result[index] = 0;
                _highZigZags[index] = 0;
                _lowZigZags[index] = 0;
                return;
            }

            _currentLow = Functions.Minimum(Bars.OpenPrices, Depth);

            if (Math.Abs(_currentLow - _lastLow) < double.Epsilon)
                _currentLow = 0.0;
            else
            {

                _lastLow = _currentLow;

                if ((Bars.OpenPrices[index] - _currentLow) > (Deviation * _point))
                    _currentLow = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_lowZigZags[index - i]) > double.Epsilon && _lowZigZags[index - i] > _currentLow)
                            _lowZigZags[index - i] = 0.0;
                    }
                }
            }
            if (Math.Abs(Bars.OpenPrices[index] - _currentLow) < double.Epsilon)
                _lowZigZags[index] = _currentLow;
            else
                _lowZigZags[index] = 0.0;

            _currentHigh = Bars.OpenPrices.Maximum(Depth);

            if (Math.Abs(_currentHigh - _lastHigh) < double.Epsilon)
                _currentHigh = 0.0;
            else
            {

                _lastHigh = _currentHigh;

                if ((_currentHigh - Bars.OpenPrices[index]) > (Deviation * _point))
                    _currentHigh = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_highZigZags[index - i]) > double.Epsilon && _highZigZags[index - i] < _currentHigh)
                            _highZigZags[index - i] = 0.0;
                    }
                }
            }

            if (Math.Abs(Bars.OpenPrices[index] - _currentHigh) < double.Epsilon)
                _highZigZags[index] = _currentHigh;
            else
                _highZigZags[index] = 0.0;


            switch (_type)
            {
                case 0:
                    if (Math.Abs(_low - 0) < double.Epsilon && Math.Abs(_high - 0) < double.Epsilon)
                    {
                        if (Math.Abs(_highZigZags[index]) > double.Epsilon)
                        {
                            _high = Bars.OpenPrices[index];
                            _lastHighIndex = index;
                            _type = -1;
                            Result[index] = _high;
                        }
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon)
                        {
                            _low = Bars.OpenPrices[index];
                            _lastLowIndex = index;
                            _type = 1;
                            Result[index] = _low;
                        }
                    }
                    break;
                case 1:
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && _lowZigZags[index] < _low && Math.Abs(_highZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastLowIndex] = double.NaN;
                        _lastLowIndex = index;
                        _low = _lowZigZags[index];
                        Result[index] = _low;
                    }
                    if (Math.Abs(_highZigZags[index] - 0.0) > double.Epsilon && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        _high = _highZigZags[index];
                        _lastHighIndex = index;
                        Result[index] = _high;
                        _type = -1;
                    }
                    break;
                case -1:
                    if (Math.Abs(_highZigZags[index]) > double.Epsilon && _highZigZags[index] > _high && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastHighIndex] = double.NaN;
                        _lastHighIndex = index;
                        _high = _highZigZags[index];
                        Result[index] = _high;
                    }
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && Math.Abs(_highZigZags[index]) < double.Epsilon)
                    {
                        _low = _lowZigZags[index];
                        _lastLowIndex = index;
                        Result[index] = _low;
                        _type = 1;
                    }
                    break;
                default:
                    return;
            }

        }

        #endregion


    }

}
