using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class SupertrendedRSI_modified : Indicator
    {
        [Parameter("RSI Length", Group = "RSI Settings", DefaultValue = 14)]
        public int RelativeStrengthIndexLength { get; set; }

        [Parameter("RSI Smoothing Length", Group = "RSI Settings", DefaultValue = 21)]
        public int SmoothingLength { get; set; }

        [Parameter("RSI Source", Group = "RSI Settings", DefaultValue = "Close")]
        public DataSeries RSIInputSource { get; set; }

        [Parameter("RSI Upper Limit", Group = "RSI Settings", DefaultValue = 70)]
        public int UpperLimit { get; set; }

        [Parameter("RSI Lower Limit", Group = "RSI Settings", DefaultValue = 30)]
        public int LowerLimit { get; set; }

        [Parameter("RSI Middle Line", Group = "RSI Settings", DefaultValue = 50, MinValue = 20, MaxValue = 90)]
        public int MiddleLine { get; set; }

        [Parameter("RSI Middle Line Offset", Group = "RSI Settings", DefaultValue = 0, MinValue = 0, MaxValue = 10)]
        public int MiddleLineOffset { get; set; }

        [Parameter("Factor", Group = "Super Trend Settings", DefaultValue = 0.8)]
        public double TrendFactor { get; set; }

        [Parameter("ATR Length", Group = "Super Trend Settings", DefaultValue = 10)]
        public int AverageTrueRangeLength { get; set; }

        [Parameter("Lot Size", Group = "Trading Parameters", DefaultValue = 0.5, MinValue = 0.2, MaxValue = 2.0)]
        public double StLotSize { get; set; }

        [Parameter("Account Value", Group = "Trading Parameters", DefaultValue = 1500)]
        public double AccountValue { get; set; }

        [Parameter("Up Color", Group = "Colors", DefaultValue = "00FFBB")]
        public string UpColor { get; set; }

        [Parameter("Down Color", Group = "Colors", DefaultValue = "FF1100")]
        public string DownColor { get; set; }

        [Parameter("Use Real Close?", Group = "Heikin Ashi [Improved]", DefaultValue = false)]
        public bool UseRealClose { get; set; }

        [Parameter("Add Original Heikin Ashi Smoothness?", Group = "Heikin Ashi [Improved]", DefaultValue = true)]
        public bool AddOriginalHeikinAshiSmoothness { get; set; }

        [Parameter("Candle Smoothness", Group = "Heikin Ashi [Improved]", DefaultValue = 1, MinValue = 1, MaxValue = 100)]
        public int CandleSmoothness { get; set; }

        [Output("RSI_Trend", LineColor = "blue", Thickness = 2)]
        public IndicatorDataSeries RSI_Trend { get; set; }

        [Output("SuperTrend_line", LineColor = "Red", Thickness = 2)]
        public IndicatorDataSeries SuperTrend_line { get; set; }




        private IndicatorDataSeries _rsiValues;
        private IndicatorDataSeries _superTrend;
        private IndicatorDataSeries _upperBand;
        private IndicatorDataSeries _lowerBand;
        private IndicatorDataSeries o_;//= AddOriginalHeikinAshiSmoothness ? haOpen : open0;
        private IndicatorDataSeries h_;//= AddOriginalHeikinAshiSmoothness ? haHigh : Bars.HighPrices[index];
        private IndicatorDataSeries l_;//= AddOriginalHeikinAshiSmoothness ? haLow : Bars.LowPrices[index];
        private IndicatorDataSeries c_; // = Ad
        private IndicatorDataSeries trueRange;
        private IndicatorDataSeries atrSeries;
        private RelativeStrengthIndex _rsi;
        private AverageTrueRange _atr;
        private IndicatorDataSeries lowerBandSeries;
        private IndicatorDataSeries upperBandSeries;

        private double supertrendValue;
        private double _previousSupertrend;
        private int trendDirection;
        private bool triggerBuy, triggerSell, triggerCloseBuy, triggerCloseSell;

        protected override void Initialize()
        {
            bool v = System.Diagnostics.Debugger.Launch();
            _rsiValues = CreateDataSeries();
            _superTrend = CreateDataSeries();
            _upperBand = CreateDataSeries();
            _lowerBand = CreateDataSeries();
            o_ = CreateDataSeries();
            h_ = CreateDataSeries();
            l_ = CreateDataSeries();
            c_= CreateDataSeries();
            trueRange = CreateDataSeries();
            atrSeries = CreateDataSeries();
            lowerBandSeries = CreateDataSeries();
            upperBandSeries = CreateDataSeries();
            _rsi = Indicators.RelativeStrengthIndex(RSIInputSource, RelativeStrengthIndexLength);
            _atr = Indicators.AverageTrueRange(AverageTrueRangeLength, MovingAverageType.Simple);
            _previousSupertrend = double.NaN;
            trendDirection = 1;
        }

        public override void Calculate(int index)
        {
            // Calculate Heikin Ashi
            var close0 = (Bars.OpenPrices[index] + Bars.HighPrices[index] + Bars.LowPrices[index] + Bars.ClosePrices[index]) / 4;
            var open0 = double.IsNaN(Bars.OpenPrices[index - CandleSmoothness]) ? (Bars.OpenPrices[index] + close0) / 2 : (Bars.OpenPrices[index - CandleSmoothness] + close0) / 2;

            var haClose = (open0 + Bars.HighPrices[index] + Bars.LowPrices[index] + close0) / 4;
            var haOpen = double.IsNaN(open0) ? (Bars.OpenPrices[index] + close0) / 2 : (Bars.OpenPrices[index - 1] + haClose) / 2;

            var haHigh = Math.Max(Bars.HighPrices[index], Math.Max(haOpen, haClose));
            var haLow = Math.Min(Bars.LowPrices[index], Math.Min(haOpen, haClose));

            o_[index] = AddOriginalHeikinAshiSmoothness ? haOpen : open0;
            h_[index] = AddOriginalHeikinAshiSmoothness ? haHigh : Bars.HighPrices[index];
            l_[index] = AddOriginalHeikinAshiSmoothness ? haLow : Bars.LowPrices[index];
            c_[index] = AddOriginalHeikinAshiSmoothness ? (UseRealClose ? Bars.ClosePrices[index] : haClose) : (UseRealClose ? Bars.ClosePrices[index] : close0);

            // Calculating RSI
             _rsi = Indicators.RelativeStrengthIndex(c_, RelativeStrengthIndexLength);
            var rsiValue = _rsi.Result[index];
           

            // Calculating Supertrend
             CalculateSupertrend(index, c_);


            //  IndicatorArea.SetYRange(10, 90);
            // Plotting Supertrend
            //   IndicatorArea.DrawTrendLine("Supertrend", index - 1, _superTrend[index - 1], index, _superTrend[index], trendDirection == -1 ? Color.FromHex(UpColor) : Color.FromHex(DownColor), 2, LineStyle.Solid);
            SuperTrend_line[index] = _superTrend[index];
            // Plotting RSI
            //   IndicatorArea.DrawTrendLine("RSI", index - 1, rsiValue, index, rsiValue, Color.Blue, 2, LineStyle.Solid);
            RSI_Trend[index] = rsiValue;

            // Plotting Overbought/Oversold Lines


            IndicatorArea.DrawHorizontalLine("OverboughtLine", UpperLimit, Color.White);
            IndicatorArea.DrawHorizontalLine("OversoldLine", LowerLimit, Color.White);

            // Plotting Mid and Offset Lines
            IndicatorArea.DrawHorizontalLine("MiddleLine", MiddleLine, Color.White);
            IndicatorArea.DrawHorizontalLine("OffsetUp", MiddleLine + MiddleLineOffset, Color.Orange);
            IndicatorArea.DrawHorizontalLine("OffsetDown", MiddleLine - MiddleLineOffset, Color.Orange);
            IndicatorArea.DrawHorizontalLine("MiddleLine", MiddleLine, Color.White);



            // Trading Logic
            // Implement the trading logic based on the conditions provided in Pine Script

            // Alerts
            if (triggerBuy)
                Print("Buy signal triggered");

            if (triggerSell)
                Print("Sell signal triggered");

            if (triggerCloseBuy)
                Print("Close buy order");

            if (triggerCloseSell)
                Print("Close sell order");
        }

        private double CalculateATR(IndicatorDataSeries source, int atrLength,int index)
        {
            double highestHigh = double.NegativeInfinity;
            double lowestLow = double.PositiveInfinity;
            double value = 0;

            for (int i = 0; i < atrLength; i++)
            {
                highestHigh = Math.Max(highestHigh, Bars.HighPrices[i]);
                lowestLow = Math.Min(lowestLow, Bars.LowPrices[i]);
            }

              trueRange[index] = Math.Max(highestHigh - lowestLow, Math.Max(Math.Abs(highestHigh - source[1]), Math.Abs(lowestLow - source[1])));
            value= CalculateRMA(trueRange, atrLength, index);
            return value; // Replace with your desired simplistic ATR calculation here
        }

        private double CalculateRMA(IndicatorDataSeries source, int length, int index)
        {
            double alpha = 1.0 / length;

            if (index == 0)
            {
                return source[index]; // Starting value
            }
            else
            {
                return (source[index] * alpha) + (source[index - 1] * (1 - alpha));
            }
        }

        public void CalculateSupertrend(int index, IndicatorDataSeries priceSource1)
        {
            // Calculate ATR (Implement this function separately based on your requirements)
            double atr = CalculateATR(priceSource1, AverageTrueRangeLength, index);
            atrSeries[index]= atr;
            // Calculate upper and lower bands
            double priceSource = priceSource1[index];
            double upperBand = priceSource + TrendFactor * atr;
            double lowerBand = priceSource - TrendFactor * atr;

            // Get previous bands
            double prevLowerBand = double.IsNaN(lowerBandSeries[index - 1]) ? 0 : lowerBandSeries[index - 1];
            double prevUpperBand = double.IsNaN(upperBandSeries[index - 1]) ? 0 : upperBandSeries[index - 1];



            // Adjust lower and upper bands based on conditions
            lowerBand = (lowerBand > prevLowerBand || priceSource1[index - 1] < prevLowerBand) ? lowerBand : prevLowerBand;
            upperBand = (upperBand < prevUpperBand || priceSource1[index - 1] > prevUpperBand) ? upperBand : prevUpperBand;

            // Initialize trend direction and supertrend value
            trendDirection = 0;
            supertrendValue = double.NaN;

            // Get previous supertrend value
            double prevSupertrend = double.IsNaN(_superTrend[index - 1]) ? 0 : _superTrend[index - 1];

            // Determine trend direction
            if (double.IsNaN(atrSeries[index - 1]))
            {
                trendDirection = 1;
            }
            else if (prevSupertrend == prevUpperBand)
            {
                trendDirection = (priceSource1[index] > upperBand) ? -1 : 1;
            }
            else
            {
                trendDirection = (priceSource1[index] < lowerBand) ? 1 : -1;
            }

            // Calculate supertrend value based on trend direction
            supertrendValue = (trendDirection == -1) ? lowerBand : upperBand;

            // Store calculated values in series for future reference
            lowerBandSeries[index] = lowerBand;
            upperBandSeries[index] = upperBand;
            _superTrend[index] = supertrendValue;

        }


       

        


    }
}
