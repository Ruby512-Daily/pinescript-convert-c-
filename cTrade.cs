using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ATRBasedStoplossTakeProfit : Indicator
    {
        [Parameter("ATR Lookback Period", DefaultValue = 30, Group = "Main", MinValue = 1)]
        public int ATRLookbackPeriod { get; set; }

        [Parameter("ATR Length", DefaultValue = 14, Group = "Main", MinValue = 1)]
        public int ATRLength { get; set; }

        [Parameter("Stop Loss Take Profit Type", DefaultValue = "ATR Surge Average", Group = "Main")]
        public StopLossTakeProfitType StopLossTakeProfitTypeParameter { get; set; }

        [Parameter("ATR Profit Multiplier", DefaultValue = 4, Group = "Main", MinValue = 0.1)]
        public double ATRProfitMultiplier { get; set; }

        [Parameter("ATR Loss Multiplier", DefaultValue = 1, Group = "Main", MinValue = 0.1)]
        public double ATRLossMultiplier { get; set; }

        [Parameter("MA Length", DefaultValue = 8, Group = "Main", MinValue = 1)]
        public int MALength { get; set; }

        [Output("Moving Average", LineColor = "blue",Thickness =2)]
        public IndicatorDataSeries MovingAverage { get; set; }

        [Output("Long Take Profit", LineColor = "Lime", Thickness = 2)]
        public IndicatorDataSeries LongTakeProfit { get; set; }

        [Output("Short Take Profit", LineColor = "Maroon", Thickness = 2)]
        public IndicatorDataSeries ShortTakeProfit { get; set; }

        [Output("Short Stop Loss", LineColor = "Fuchsia", Thickness = 2)]
        public IndicatorDataSeries ShortStopLoss { get; set; }

        [Output("Long Stop Loss", LineColor = "Orange", Thickness = 2)]
        public IndicatorDataSeries LongStopLoss { get; set; }

        private AverageTrueRange _atr;
        private IndicatorDataSeries tnatrSim;
        private IndicatorDataSeries natrSim;
        private IndicatorDataSeries natr;
        private IndicatorDataSeries natrSH;
        private bool surgeReset;

        private List<double> _natrSHArray;
        private List<double> _tnatrSHArray;
        private double _psnatr;
        private double _lsnatr;
        private double _prftpcnt;
        private double _losspcnt;

        protected override void Initialize()
        {
            _atr = Indicators.AverageTrueRange(ATRLength,MovingAverageType.Simple);
            _natrSHArray = new List<double>();
            _tnatrSHArray = new List<double>();
            _psnatr = 0;
            _lsnatr = 0;
            _prftpcnt = 0;
            _losspcnt = 0;
            tnatrSim = CreateDataSeries();
            natr = CreateDataSeries();
            natrSim = CreateDataSeries();
            natrSH = CreateDataSeries();
        }

        public override void Calculate(int index)
        {
            tnatrSim[index] = 100 * _atr.Result.LastValue / Bars.LastBar.Close;
            natr[index] = _atr.Result.LastValue;
            natrSim[index] = Indicators.AverageTrueRange(1,MovingAverageType.Simple).Result.LastValue;
            natrSH[index] = GetHighest(natrSim.LastValue, ATRLookbackPeriod, natrSim.Count);
            surgeReset = (natrSH.LastValue > natrSH[1] ? 0 : natrSH.Count) > 5;
            

            double natrSurgeH = 0;
            natrSurgeH = natrSH[index] > natrSH[1] ? 1 : surgeReset ? 0 : natrSurgeH;

            bool freqnatrSurge = IsCrossover(natrSurgeH, 0);
            double simVal = 0;
            simVal = freqnatrSurge ? natrSim[index] : simVal;

            if (freqnatrSurge)
                _natrSHArray.Add(natrSH[index]);

            double natrSHAvg =  _natrSHArray.Any() ? _natrSHArray.Average() : double.NaN;

            bool natrSurge = freqnatrSurge && tnatrSim[index] > 1;

            double natrP = StopLossTakeProfitTypeParameter == StopLossTakeProfitType.ATRSurgeAverage ? natrSHAvg * ATRProfitMultiplier : natr[index] * ATRProfitMultiplier;
            double natrL = StopLossTakeProfitTypeParameter == StopLossTakeProfitType.ATRSurgeAverage ? natrSHAvg * ATRLossMultiplier : natr[index] * ATRLossMultiplier;

            _psnatr = natrSurge ? natrP : _psnatr;
            _lsnatr = natrSurge ? natrL : _lsnatr;
            // The MA
            
            double theMA = Indicators.SimpleMovingAverage(Bars.MedianPrices, MALength).Result.LastValue;
            double theMAPlus = theMA + _psnatr;
            double theMANeg = theMA - _psnatr;
            double theplusstop = theMA + _lsnatr;
            double thenegstop = theMA - _lsnatr;

            MovingAverage[index] = theMA;
            LongTakeProfit[index] = theMAPlus;
            ShortTakeProfit[index] = theMANeg;
            ShortStopLoss[index] = theplusstop;
            LongStopLoss[index] = thenegstop;

            bool natrSDump = natrSurge && Bars.LastBar.Open > Bars.LastBar.Close;

            double tnatr = 100 * Indicators.AverageTrueRange(ATRLength,MovingAverageType.Simple).Result.LastValue / Bars.LastBar.Close;
            double tnatrSH = GetHighest(tnatrSim[index], ATRLookbackPeriod, index);

            if (freqnatrSurge)
                _tnatrSHArray.Add(tnatrSH);

            double tnatrSHAvg = _tnatrSHArray.Any() ? _tnatrSHArray.Average() : double.NaN;

            double tnatrP = StopLossTakeProfitTypeParameter == StopLossTakeProfitType.ATRSurgeAverage ? tnatrSHAvg * ATRProfitMultiplier : tnatr * ATRProfitMultiplier;
            double tnatrL = StopLossTakeProfitTypeParameter == StopLossTakeProfitType.ATRSurgeAverage ? tnatrSHAvg * ATRLossMultiplier : tnatr * ATRLossMultiplier;

            _prftpcnt = natrSurge ? tnatrP : _prftpcnt;
            _losspcnt = natrSurge ? tnatrL : _losspcnt;

            double risk = 0.3;
            double fund = (risk / _losspcnt) * 99.85;
            double profit = fund * (1 + (_prftpcnt / 100)) - fund;

            Color lCol = Color.FromArgb(60, Color.Red);

            // Draw table
            DrawTable("Fund", "$" + Math.Round(fund, 2), Color.Black, Color.White);
            DrawTable("Profit", _prftpcnt.ToString("P2") + "/" + "$" + Math.Round(profit, 2), Color.Black, Color.White);
            DrawTable("ATR", natr.ToString(), Color.Black, Color.White);
            DrawTable("Loss", _losspcnt.ToString("P2") + "/" + "$" + Math.Round(risk, 2), lCol, Color.White);
        }

        private double GetHighest(double value, int period, int index)
        {
            double highest = double.MinValue;
            for (int i = 0; i < period; i++)
            {
                if (index - i >= 0 && Bars.ClosePrices[index - i] > highest)
                    highest = Bars.ClosePrices[index - i];
            }
            return highest;
        }

        private bool IsCrossover(double current, double previous)
        {
            return current > 0 && previous <= 0;
        }

        private void DrawTable(string title, string value, Color bgColor, Color textColor)
        {
            Chart.DrawText(title, title + "\n" + value, Bars.LastBar.OpenTime, Bars.LastBar.Close, textColor);
        }

        public enum StopLossTakeProfitType
        {
            ATR,
            ATRSurgeAverage
        }
    }
}