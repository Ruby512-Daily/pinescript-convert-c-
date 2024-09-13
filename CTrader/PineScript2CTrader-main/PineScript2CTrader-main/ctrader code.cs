// This work is licensed under a Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0) https://creativecommons.org/licenses/by-nc-sa/4.0/
// © LuxAlgo

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.API.Requests;

namespace cAlgo
{
    // Define custom types
    public class ZZ
    {
        public int[] d;
        public int[] x;
        public double[] y;
        public bool[] b;
    }

    public class ln_d
    {
        public TrendLine l;
        public int d;
    }

    public class _2ln_lb
    {
        public TrendLine l1;
        public TrendLine l2;
        public Label lb;
    }

    public class bx_ln
    {
        public Box b;
        public TrendLine l;
    }

    public class bx_ln_lb
    {
        public Box bx;
        public TrendLine ln;
        public Label lb;
    }

    public class mss
    {
        public int dir;
        public TrendLine[] l_mssBl;
        public TrendLine[] l_mssBr;
        public TrendLine[] l_bosBl;
        public TrendLine[] l_bosBr;
        public Label[] lbMssBl;
        public Label[] lbMssBr;
        public Label[] lbBosBl;
        public Label[] lbBosBr;
    }

    public class liq
    {
        public Box bx;
        public bool broken;
        public bool brokenTop;
        public bool brokenBtm;
        public TrendLine ln;
    }

    public class ob
    {
        public double top = double.NaN;
        public double btm = double.NaN;
        public int loc;
        public bool breaker = false;
        public int break_loc;
    }

    public class swing
    {
        public double y = double.NaN;
        public int x;
        public bool crossed = false;
    }

    public class FVG
    {
        public Box box;
        public bool active;
        public int pos;
    }

    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class ICT_Concepts : Indicator
    {
        //Strings
        private const string o = "Options";
        private const string sp1 = "       ";
        private const string sp2 = "              ";
        private const string hl = "High / Low    " + sp1;
        private const string ny_ = "New York" + sp1;
        private const string lo_ = "London Open";
        private const string lc_ = "London Close";
        private const string as_ = "Asian";

        //Settings
        private DataSeries high;
        private DataSeries low;

        //Market Structure Shift
        private bool showMS = true;
        private int len = 5;

        private bool iMSS = true;
        private Color cMSSbl = Colors.LightSeaGreen;
        private Color cMSSbr = Colors.Red;

        private bool iBOS = true;
        private Color cBOSbl = Colors.LightSeaGreen;
        private Color cBOSbr = Colors.Red;

        //Displacement
        private bool sDispl = false;
        private double perc_Body = 0.36;
        private int bxBack = 10;

        //Volume Imbalance
        private bool sVimbl = true;
        private int visVim = 2;
        private Color cVimbl = Colors.DodgerBlue;

        //Order Blocks
        private bool showOB = true;
        private int length = 10;
        private int showBull = 1;
        private int showBear = 1;
        private bool useBody = true;

        //OB Style
        private Color bullCss = Colors.LightSkyBlue;
        private Color bullBrkCss = Colors.CornflowerBlue;
        private Color bearCss = Colors.Red;
        private Color bearBrkCss = Colors.Yellow;

        private bool showLabels = false;

        //Liquidity
        private bool showLq = true;
        private double a = 10 / 4;
        private int visLiq = 2;
        private Color cLIQ_B = Colors.Coral;
        private Color cLIQ_S = Colors.DeepSkyBlue;

        //FVG
        private bool shwFVG = true;
        private bool i_BPR = false;
        private string i_FVG = "FVG";
        private int visBxs = 2;

        //FVG Style
        private Color cFVGbl = Colors.LightGreen;
        private Color cFVGblBR = Colors.Olive;
        private Color cFVGbr = Colors.Red;
        private Color cFVGbrBR = Colors.OrangeRed;

        //NWOG/NDOG
        private bool iNWOG = true;
        private Color cNWOG1 = Colors.Red;
        private Color cNWOG2 = Colors.Gray;
        private int maxNWOG = 3;

        private bool iNDOG = false;
        private Color cNDOG1 = Colors.Orange;
        private Color cNDOG2 = Colors.DeepSkyBlue;
        private int maxNDOG = 1;

        //Fibonacci
        private string iFib = "NONE";
        private bool iExt = false;

        //Killzones
        private bool showKZ = false;
        private bool showNy = true;
        private Color nyCss = Colors.Orange;
        private bool showLdno = true;
        private Color ldnoCss = Colors.DeepSkyBlue;
        private bool showLdnc = true;
        private Color ldncCss = Colors.LightSkyBlue;
        private bool showAsia = true;
        private Color asiaCss = Colors.Magenta;

        // General Calculations
        private int n;
        private double hi;
        private double lo;
        private int tf_msec;
        private const int maxSize = 50;
        private double atr;
        private bool per;
        private bool perB;
        private object xloc;
        private Extend ext;
        private int plus;
        private double mx;
        private double mn;
        private double body;
        private double meanBody;
        private double max;
        private double min;
        private double blBrkConf;
        private double brBrkConf;
        private double r;
        private double g;
        private double b;
        private bool isDark;
        private ZZ zz;
        private ln_d lnd;
        private _2ln_lb _2ln_lb;
        private bx_ln bx_ln;
        private bx_ln_lb bx_ln_lb;
        private mss mss;
        private liq liq;
        private ob ob;
        private swing swing;
        private FVG fvg;

        // Variables
        private double maxVimb = 2;
        private double friCp;
        private int friCi;
        private double monOp;
        private int monOi;
        private double prDCp;
        private int prDCi;
        private double cuDOp;
        private int cuDOi;
        private _2ln_lb[] Vimbal;
        private liq[] b_liq_B;
        private liq[] b_liq_S;
        private ob[] bullish_ob;
        private ob[] bearish_ob;
        private bx_ln[] bl_NWOG;
        private bx_ln[] bl_NDOG;
        private bx_ln_lb[] a_bx_ln_lb;
        private FVG[] bFVG_UP;
        private FVG[] bFVG_DN;
        private FVG[] bBPR_UP;
        private FVG[] bBPR_DN;
        private ZZ aZZ;
        private TrendLine _diag;
        private TrendLine _vert;
        private TrendLine _zero;
        private TrendLine _0236;
        private TrendLine _0382;
        private TrendLine _0500;
        private TrendLine _0618;
        private TrendLine _0786;
        private TrendLine _one_;
        private TrendLine _1618;

         // Methods
        private void in_out(ZZ aZZ, int d, int x1, double y1, int x2, double y2, Color col, bool b)
        {
            aZZ.d.Unshift(d);
            aZZ.x.Unshift(x2);
            aZZ.y.Unshift(y2);
            aZZ.b.Unshift(b);
            aZZ.d.Pop();
            aZZ.x.Pop();
            aZZ.y.Pop();
            aZZ.b.Pop();
        }

        private bool timeinrange(string res, string sess)
        {
            return !IsNaN(TimeFrame.GetResult(res).StartTime) && !IsNaN(TimeFrame.GetResult(res).EndTime) && TimeFrame.GetResult(res).StartTime <= Time.CurrentTime && Time.CurrentTime <= TimeFrame.GetResult(res).EndTime;
        }

        private void setLine(TrendLine ln, int x1, double y1, int x2, double y2)
        {
            ln.SetXY(x1, y1);
            ln.SetXY(x2, y2);
        }

        private void clear_aLine(TrendLine[] l)
        {
            if (l.Length > 0)
            {
                for (int i = l.Length - 1; i >= 0; i--)
                {
                    l[i].Delete();
                }
            }
        }

        private void clear_aLabLin(Label[] l)
        {
            if (l.Length > 0)
            {
                for (int i = l.Length - 1; i >= 0; i--)
                {
                    l[i].Delete();
                }
            }
        }

        private void clear_aLabLin(TrendLine[] l)
        {
            if (l.Length > 0)
            {
                for (int i = l.Length - 1; i >= 0; i--)
                {
                    l[i].Delete();
                }
            }
        }

        private void display(ob id, Color css, Color break_css, string str)
        {
            if (showOB)
            {
                if (id.breaker)
                {
                    a_bx_ln_lb.Unshift(bx_ln_lb(new Box(), new TrendLine(), new Label()));
                }
                else
                {
                    double y = str == "bl" ? id.btm : id.top;
                    string s = str == "bl" ? LabelStyle.LabelUp : LabelStyle.LabelDown;
                    a_bx_ln_lb.Unshift(bx_ln_lb(new Box(), new TrendLine(id.loc, y, id.loc + (tf_msec * 10), y, LineStyle.Solid, css, 2), new Label(id.loc + (tf_msec * 10), y, str == "bl" ? "+OB" : "-OB", Color.Transparent, color, LabelAlignment.Center, str == "bl" ? LabelStyle.LabelUp : LabelStyle.LabelDown, LabelOrientation.Horizontal, 9)));
                }
            }
        }
        // Swing function
        private swing[] swings(int len)
        {
            int os = 0;
            swing top = new swing(double.NaN, int.NaN);
            swing btm = new swing(double.NaN, int.NaN);

            double upper = MarketSeries.High.Maximum(len);
            double lower = MarketSeries.Low.Minimum(len);

            for (int i = 0; i < MarketSeries.Close.Count; i++)
            {
                os = MarketSeries.High[i] > upper ? 0 : MarketSeries.Low[i] < lower ? 1 : os;

                if (os == 0 && os != 0)
                {
                    top = new swing(MarketSeries.High[i - len], i - len);
                }

                if (os == 1 && os != 1)
                {
                    btm = new swing(MarketSeries.Low[i - len], i - len);
                }
            }

            return new swing[] { top, btm };
        }

        // Set Label function
        private void set_lab(int i, string str)
        {
            string style = str == "Bl" ? LabelStyle.LabelDown : LabelStyle.LabelUp;
            Color txcol = str == "Bl" ? Colors.Lime : Colors.Red;
            ChartObjects.DrawText("label" + i, "BOS", Math.Round((aZZ.x[i] + Time.CurrentTime), MidpointRounding.ToEven), aZZ.y[i], VerticalAlignment.Center, HorizontalAlignment.Center, txcol);
        }

        // Set Line function
        private void set_lin(int i, string str)
        {
            Color color = str == "Bl" ? Colors.Lime : Colors.Red;
            ChartObjects.DrawLine("line" + i, aZZ.x[i], aZZ.y[i], Time.CurrentTime, aZZ.y[i], color, 2, LineStyle.Dots);
        }

        // Draw function
        private void draw(int left, Color col)
        {
            // Initialize variables
            int dir = int.NaN;
            int x1 = int.NaN;
            double y1 = double.NaN;
            int x2 = int.NaN;
            double y2 = double.NaN;

            // Calculate size
            int sz = aZZ.d.Count;

            // Set x2
            x2 = MarketSeries.OpenTime.LastValue;

            // Find pivot high
            double ph = Indicators.PivotHigh(MarketSeries.High, left, 1);

            // Find pivot low
            double pl = Indicators.PivotLow(MarketSeries.Low, left, 1);

            // Handle pivot high
            if (!double.IsNaN(ph))
            {
                dir = aZZ.d[0];
                x1 = aZZ.x[0];
                y1 = aZZ.y[0];
                y2 = Bars.HighPrices.Last(1);

                // Check direction
                if (dir < 1)
                {
                    aZZ.in_out(1, x1, y1, x2, y2, col, true);
                }
                else
                {
                    if (dir == 1 && ph > y1)
                    {
                        aZZ.x[0] = x2;
                        aZZ.y[0] = y2;
                    }
                }

                // Liquidity
                if (showLq && per && sz > 0)
                {
                    int count = 0;
                    double st_P = 0;
                    int st_B = 0;
                    double minP = 0;
                    double maxP = 10e6;

                    for (int i = 0; i < Math.Min(sz, 50); i++)
                    {
                        if (aZZ.d[i] == 1)
                        {
                            if (aZZ.y[i] > ph + (atr / a))
                                break;
                            else
                            {
                                if (aZZ.y[i] > ph - (atr / a) && aZZ.y[i] < ph + (atr / a))
                                {
                                    count++;
                                    st_B = aZZ.x[i];
                                    st_P = aZZ.y[i];
                                    if (aZZ.y[i] > minP)
                                        minP = aZZ.y[i];
                                    if (aZZ.y[i] < maxP)
                                        maxP = aZZ.y[i];
                                }
                            }
                        }
                    }

                    if (count > 2)
                    {
                        var getB = b_liq_B[0];
                        if (st_B == getB.bx.Left)
                        {
                            getB.bx.Top = (minP + maxP) / 2 + (atr / a);
                            getB.bx.RightBottom = new PointD(x2 + 10, (minP + maxP) / 2 - (atr / a));
                        }
                        else
                        {
                            b_liq_B.Insert(0, new liq(new Box(st_B, (minP + maxP) / 2 + (atr / a), x2 + 10, (minP + maxP) / 2 - (atr / a)), false, false, false, new TrendLine(st_B, st_P, x2 - 1, st_P, Colors.Transparent)));
                        }

                        if (b_liq_B.Count > visLiq)
                        {
                            var getLast = b_liq_B[b_liq_B.Count - 1];
                            getLast.bx.Delete();
                            getLast.ln.Delete();
                        }
                    }
                }
            }

            // Handle pivot low
            if (!double.IsNaN(pl))
            {
                dir = aZZ.d[0];
                x1 = aZZ.x[0];
                y1 = aZZ.y[0];
                y2 = Bars.LowPrices.Last(1);

                // Check direction
                if (dir > -1)
                {
                    aZZ.in_out(-1, x1, y1, x2, y2, col, true);
                }
                else
                {
                    if (dir == -1 && pl < y1)
                    {
                        aZZ.x[0] = x2;
                        aZZ.y[0] = y2;
                    }
                }

                // Liquidity
                if (showLq && per && sz > 0)
                {
                    int count = 0;
                    double st_P = 0;
                    int st_B = 0;
                    double minP = 0;
                    double maxP = 10e6;

                    for (int i = 0; i < Math.Min(sz, 50); i++)
                    {
                        if (aZZ.d[i] == -1)
                        {
                            if (aZZ.y[i] < pl - (atr / a))
                                break;
                            else
                            {
                                if (aZZ.y[i] > pl - (atr / a) && aZZ.y[i] < pl + (atr / a))
                                {
                                    count++;
                                    st_B = aZZ.x[i];
                                    st_P = aZZ.y[i];
                                    if (aZZ.y[i] > minP)
                                        minP = aZZ.y[i];
                                    if (aZZ.y[i] < maxP)
                                        maxP = aZZ.y[i];
                                }
                            }
                        }
                    }

                    if (count > 2)
                    {
                        var getB = b_liq_S[0];
                        if (st_B == getB.bx.Left)
                        {
                            getB.bx.Top = (minP + maxP) / 2 + (atr / a);
                            getB.bx.RightBottom = new PointD(x2 + 10, (minP + maxP) / 2 - (atr / a));
                        }
                        else
                        {
                            b_liq_S.Insert(0, new liq(new Box(st_B, (minP + maxP) / 2 + (atr / a), x2 + 10, (minP + maxP) / 2 - (atr / a)), false, false, false, new TrendLine(st_B, st_P, x2 - 1, st_P, Colors.Transparent)));
                        }

                        if (b_liq_S.Count > visLiq)
                        {
                            var getLast = b_liq_S[b_liq_S.Count - 1];
                            getLast.bx.Delete();
                            getLast.ln.Delete();
                        }
                    }
                }
            }

            // Market Structure Shift
            if (showMS)
            {
                int iH = aZZ.d[2] == 1 ? 2 : 1;
                int iL = aZZ.d[2] == -1 ? 2 : 1;

                switch (MarketSeries.Close.LastValue > aZZ.y[iH] && aZZ.d[iH] == 1 && MSS.dir < 1)
                {
                    case true:
                        MSS.dir = 1;
                        if (i_mode == "Present")
                        {
                            MSS.l_bosBl.clear_aLabLin();
                            MSS.l_bosBr.clear_aLabLin();
                            MSS.lbBosBl.clear_aLabLin();
                            MSS.lbBosBr.clear_aLabLin();
                            MSS.l_mssBl.clear_aLabLin();
                            MSS.l_mssBr.clear_aLabLin();
                            MSS.lbMssBl.clear_aLabLin();
                            MSS.lbMssBr.clear_aLabLin();
                        }

                        MSS.l_mssBl.Insert(0, new TrendLine(aZZ.x[iH], aZZ.y[iH], x2, aZZ.y[iH], cMSSbl));
                        MSS.lbMssBl.Insert(0, new Label(Math.Round((aZZ.x[iH] + x2) / 2), aZZ.y[iH], "MSS", Colors.Transparent, size.tiny, DrawingText.HorizontalAlign.Center, DrawingText.VerticalAlign.Center));
                        break;

                    case false:
                        if (MarketSeries.Close.LastValue < aZZ.y[iL] && aZZ.d[iL] == -1 && MSS.dir > -1)
                        {
                            MSS.dir = -1;
                            if (i_mode == "Present")
                            {
                                MSS.l_bosBl.clear_aLabLin();
                                MSS.l_bosBr.clear_aLabLin();
                                MSS.lbBosBl.clear_aLabLin();
                                MSS.lbBosBr.clear_aLabLin();
                                MSS.l_mssBl.clear_aLabLin();
                                MSS.l_mssBr.clear_aLabLin();
                                MSS.lbMssBl.clear_aLabLin();
                                MSS.lbMssBr.clear_aLabLin();
                            }

                            MSS.l_mssBr.Insert(0, new TrendLine(aZZ.x[iL], aZZ.y[iL], x2, aZZ.y[iL], cMSSbr));
                            MSS.lbMssBr.Insert(0, new Label(Math.Round((aZZ.x[iL] + x2) / 2), aZZ.y[iL], "MSS", Colors.Transparent, size.tiny, DrawingText.HorizontalAlign.Center, DrawingText.VerticalAlign.Center));
                        }
                        break;
                }

                // Reset if iMSS is false
                if (!iMSS)
                {
                    MSS.l_mssBl[0].Color = Colors.Transparent;
                    MSS.lbMssBl[0].TextColor = Colors.Transparent;
                    MSS.l_mssBr[0].Color = Colors.Transparent;
                    MSS.lbMssBr[0].TextColor = Colors.Transparent;
                }
            }
        }

        protected override void onBar()
        {
            // Calculations
            // Draw function call
            draw(len, Colors.Yellow);

            // Check and delete old MSS.bosBl and MSS.bosBr objects
            if (MSS.l_bosBl.Count > 200)
            {
                MSS.l_bosBl.LastValue.Delete();
                MSS.lbBosBl.LastValue.Delete();
            }

            if (MSS.l_bosBr.Count > 200)
            {
                MSS.l_bosBr.LastValue.Delete();
                MSS.lbBosBr.LastValue.Delete();
            }

            // Killzones
            bool ny = Time.ToTimezone(Timeframe.CurrentTimeframe, "America/New_York").Hour >= 7 && Time.ToTimezone(Timeframe.CurrentTimeframe, "America/New_York").Hour < 9 && showNy;
            bool ldn_open = Time.ToTimezone(Timeframe.CurrentTimeframe, "Europe/London").Hour >= 7 && Time.ToTimezone(Timeframe.CurrentTimeframe, "Europe/London").Hour < 10 && showLdno;
            bool ldn_close = Time.ToTimezone(Timeframe.CurrentTimeframe, "Europe/London").Hour >= 15 && Time.ToTimezone(Timeframe.CurrentTimeframe, "Europe/London").Hour < 17 && showLdnc;
            bool asian = Time.ToTimezone(Timeframe.CurrentTimeframe, "Asia/Tokyo").Hour >= 10 && Time.ToTimezone(Timeframe.CurrentTimeframe, "Asia/Tokyo").Hour < 14 && showAsia;

            // Pivot points
            double ph = Indicators.PivotHigh(3, 1);
            double pl = Indicators.PivotLow(3, 1);

            // Candles
            bool L_body = (High[0] - Max[0] < Body * perc_Body) && (Min[0] - Low[0] < Body * perc_Body);
            bool L_bodyUP = Body > meanBody && L_body && Close > Open;
            bool L_bodyDN = Body > meanBody && L_body && Close < Open;
            int bsNOTbodyUP = BarsSince(!L_bodyUP);
            int bsNOTbodyDN = BarsSince(!L_bodyDN);
            int bsIs_bodyUP = BarsSince(L_bodyUP);
            int bsIs_bodyDN = BarsSince(L_bodyDN);
            double lwst = Math.Min(lPh[bsNOTbodyUP[1]], Low[bsNOTbodyUP[1]]);
            double hgst = Math.Max(High[bsNOTbodyDN[1]], lPl[bsNOTbodyDN[1]]);

            // Imbalance
            bool imbalanceUP = L_bodyUP[1] && (i_FVG == "FVG" ? Low > High[2] : Low < High[2]);
            bool imbalanceDN = L_bodyDN[1] && (i_FVG == "FVG" ? High < Low[2] : High > Low[2]);

            // Volume Imbalance
            bool vImb_Bl = Open > Close[1] && High[1] > Low && Close > Close[1] && Open > Open[1] && High[1] < Min;
            bool vImb_Br = Open < Close[1] && Low[1] < High && Close < Close[1] && Open < Open[1] && Low[1] > Max;

            // Display volume imbalance
            if (sVimbl)
            {
                if (vImb_Bl)
                {
                    Vimbal.Insert(0, new _2ln_lb(
                        new TrendLine(Timeframe.CurrentBar - 1, Max[1], Timeframe.CurrentBar + 3, Max[1], cVimbl),
                        new TrendLine(Timeframe.CurrentBar, Min, Timeframe.CurrentBar + 3, Min, cVimbl),
                        new Label(Timeframe.CurrentBar + 3, (Max[1] + Min) / 2, "VI", cVimbl, size.tiny, DrawingText.HorizontalAlign.Center, DrawingText.VerticalAlign.Center)
                    ));
                }

                if (vImb_Br)
                {
                    Vimbal.Insert(0, new _2ln_lb(
                        new TrendLine(Timeframe.CurrentBar - 1, Min[1], Timeframe.CurrentBar + 3, Min[1], cVimbl),
                        new TrendLine(Timeframe.CurrentBar, Max, Timeframe.CurrentBar + 3, Max, cVimbl),
                        new Label(Timeframe.CurrentBar + 3, (Min[1] + Max) / 2, "VI", cVimbl, size.tiny, DrawingText.HorizontalAlign.Center, DrawingText.VerticalAlign.Center)
                    ));
                }

                if (Vimbal.Count > visVim)
                {
                    var pop = Vimbal.Pop();
                    pop.l1.Delete();
                    pop.l2.Delete();
                    pop.lb.Delete();
                }
            }

            if (barstate.IsFirst)
            {
                for (int i = 0; i < visBxs; i++)
                {
                    bFVG_UP.Insert(0, new FVG(new Box(), false));
                    bFVG_DN.Insert(0, new FVG(new Box(), false));
                    if (i_BPR)
                    {
                        bBPR_UP.Insert(0, new FVG(new Box(), false));
                        bBPR_DN.Insert(0, new FVG(new Box(), false));
                    }
                }
            }

            // Fair Value Gap
            if (imbalanceUP && per && shwFVG)
            {
                if (imbalanceUP[1])
                {
                    bFVG_UP[0].Box.SetLeftTop(n - 2, Low);
                    bFVG_UP[0].Box.SetRightBottom(n + 8, High[2]);
                }
                else
                {
                    bFVG_UP.Insert(0, new FVG(
                        new Box(
                            n - 2,
                            i_FVG == "FVG" ? Low : High[2],
                            n,
                            i_FVG == "FVG" ? High[2] : Low,
                            bgcolor: i_BPR ? Colors.Transparent : cFVGbl.ColorWithAlpha(90),
                            border_color: i_BPR ? Colors.Transparent : cFVGbl.ColorWithAlpha(65),
                            text_color: i_BPR ? Colors.Transparent : cFVGbl.ColorWithAlpha(65),
                            text_size: Size.Small,
                            text: i_FVG
                        ),
                        true
                    ));
                    bFVG_UP.RemoveAt(bFVG_UP.Count - 1)?.Box.Delete();
                }
            }

            // Check for imbalanceDN, per, and shwFVG
            if (ImbalanceDN && Per && ShwFVG)
            {
                // Check if there was an imbalanceDN in the previous bar
                if (ImbalanceDN[1])
                {
                    // Update the existing FVG box coordinates if there was an imbalanceDN in the previous bar
                    bFVG_DN[0].Box.SetLeftTop(Bars.Range.To - 2, Low[2]);
                    bFVG_DN[0].Box.SetRightBottom(Bars.Range.To + 8, High);
                }
                else
                {
                    // Create a new FVG box if there was no imbalanceDN in the previous bar
                    var newLeft = Bars.Range.To - 2;
                    var newTop = i_FVG == "FVG" ? Low[2] : High;
                    var newRight = Bars.Range.To;
                    var newBottom = i_FVG == "FVG" ? High : Low[2];
                    
                    // Define box colors based on i_BPR condition
                    var bgcolor = i_BPR ? Colors.Transparent : new Color(cFVGbr, 90);
                    var borderColor = i_BPR ? Colors.Transparent : new Color(cFVGbr, 65);
                    var textColor = i_BPR ? Colors.Transparent : new Color(cFVGbr, 65);
                    
                    // Create a new FVG box and add it to the collection
                    var newFVG_DN = new FVG(new ChartRectangle(newLeft, newTop, newRight, newBottom)
                    {
                        BorderColor = borderColor,
                        BackgroundColor = bgcolor,
                        BorderWidth = 1,
                        BorderDashStyle = ChartDashStyle.Solid
                    });
                    
                    // Set text properties
                    newFVG_DN.Box.SetText(i_FVG, textColor, ChartTextHorizontalAlignment.Center, ChartTextVerticalAlignment.Center, 10);

                    // Add the new FVG box to the collection
                    bFVG_DN.Insert(0, newFVG_DN);

                    // Remove the last FVG box if the collection size exceeds the limit
                    if (bFVG_DN.Count > 0)
                        bFVG_DN.RemoveAt(bFVG_DN.Count - 1);
                }
            }


            // Balance Price Range
            if (i_BPR && bFVG_UP.Count > 0 && bFVG_DN.Count > 0)
            {
                var bxUP = bFVG_UP[0];
                var bxDN = bFVG_DN[0];
                var bxUPbtm = bxUP.Box.GetBottom();
                var bxDNbtm = bxDN.Box.GetBottom();
                var bxUPtop = bxUP.Box.GetTop();
                var bxDNtop = bxDN.Box.GetTop();
                var left = Math.Min(bxUP.Box.GetLeft(), bxDN.Box.GetLeft());
                var right = Math.Max(bxUP.Box.GetRight(), bxDN.Box.GetRight());

                if (bxUPbtm < bxDNtop && bxDNbtm < bxUPbtm)
                {
                    if (left == bBPR_UP[0].Box.GetLeft())
                    {
                        if (bBPR_UP[0].Active)
                        {
                            bBPR_UP[0].Box.SetRight(right);
                        }
                    }
                    else
                    {
                        bBPR_UP.Insert(0, new FVG(
                            new Box(
                                left,
                                bxDNtop,
                                right,
                                bxUPbtm,
                                bgcolor: i_BPR ? cFVGbl.ColorWithAlpha(90) : Colors.Transparent,
                                border_color: i_BPR ? cFVGbl.ColorWithAlpha(65) : Colors.Transparent,
                                text_color: i_BPR ? cFVGbl.ColorWithAlpha(65) : Colors.Transparent,
                                text_size: Size.Small,
                                text: "BPR"
                            ),
                            true,
                            close > bxUPbtm ? 1 : close < bxDNtop ? -1 : 0
                        ));
                        bBPR_UP.RemoveAt(bBPR_UP.Count - 1)?.Box.Delete();
                    }
                }

                // Check if bxDNbtm is less than bxUPtop and bxUPbtm is less than bxDNbtm
                if (bxDNbtm < bxUPtop && bxUPbtm < bxDNbtm)
                {
                    // Define the left coordinate
                    var newLeft = Math.Min(bxUP.Box.Left, bxDN.Box.Left);

                    // Define the right coordinate
                    var newRight = Math.Max(bxUP.Box.Right, bxDN.Box.Right);

                    // Check if the left coordinate matches the left coordinate of the existing BPR box
                    if (newLeft == bBPR_DN[0].Box.Left)
                    {
                        // Check if the existing BPR box is active
                        if (bBPR_DN[0].Active)
                        {
                            // Update the right coordinate of the existing BPR box
                            bBPR_DN[0].Box.SetRight(newRight);
                        }
                    }
                    else
                    {
                        // Define box colors based on i_BPR condition
                        var bgcolor = i_BPR ? new Color(cFVGbr, 90) : Colors.Transparent;
                        var borderColor = i_BPR ? new Color(cFVGbr, 65) : Colors.Transparent;
                        var textColor = i_BPR ? new Color(cFVGbr, 65) : Colors.Transparent;

                        // Create a new BPR box
                        var newBPR_DN = new FVG(new ChartRectangle(newLeft, bxUPtop, newRight, bxDNbtm)
                        {
                            BorderColor = borderColor,
                            BackgroundColor = bgcolor,
                            BorderWidth = 1,
                            BorderDashStyle = ChartDashStyle.Solid
                        });

                        // Set text properties
                        newBPR_DN.Box.SetText("BPR", textColor, ChartTextHorizontalAlignment.Center, ChartTextVerticalAlignment.Center, 10);

                        // Add the new BPR box to the collection
                        bBPR_DN.Insert(0, newBPR_DN);

                        // Remove the last BPR box if the collection size exceeds the limit
                        if (bBPR_DN.Count > 0)
                            bBPR_DN.RemoveAt(bBPR_DN.Count - 1);
                    }
                }

            }
            // FVG's breaks for bFVG_UP
            for (int i = 0; i < Math.Min(bxBack, bFVG_UP.Count); i++)
            {
                var getUPi = bFVG_UP[i];
                if (getUPi.Active)
                {
                    getUPi.Box.SetRight(barIndex + 8);
                    if (low < getUPi.Box.Top && !i_BPR)
                        getUPi.Box.BorderStyle = ChartDashStyle.Dash;
                    if (low < getUPi.Box.Bottom)
                    {
                        if (!i_BPR)
                        {
                            getUPi.Box.BackgroundColor = new Color(cFVGblBR, 95);
                            getUPi.Box.BorderStyle = ChartDashStyle.Dot;
                        }
                        getUPi.Box.SetRight(barIndex);
                        getUPi.Active = false;
                    }
                }
            }

            // FVG's breaks for bFVG_DN
            for (int i = 0; i < Math.Min(bxBack, bFVG_DN.Count); i++)
            {
                var getDNi = bFVG_DN[i];
                if (getDNi.Active)
                {
                    getDNi.Box.SetRight(barIndex + 8);
                    if (high > getDNi.Box.Bottom && !i_BPR)
                        getDNi.Box.BorderStyle = ChartDashStyle.Dash;
                    if (high > getDNi.Box.Top)
                    {
                        if (!i_BPR)
                        {
                            getDNi.Box.BackgroundColor = new Color(cFVGbrBR, 95);
                            getDNi.Box.BorderStyle = ChartDashStyle.Dot;
                        }
                        getDNi.Box.SetRight(barIndex);
                        getDNi.Active = false;
                    }
                }
            }
            if (i_BPR)
            {
                // BPR_UP
                for (int i = 0; i < Math.Min(bxBack, bBPR_UP.Count); i++)
                {
                    var getUPi = bBPR_UP[i];
                    if (getUPi.Active)
                    {
                        getUPi.Box.SetRight(barIndex + 8);
                        switch (getUPi.Pos)
                        {
                            case -1:
                                if (high > getUPi.Box.Bottom)
                                    getUPi.Box.BorderStyle = ChartDashStyle.Dash;
                                if (high > getUPi.Box.Top)
                                {
                                    getUPi.Box.BackgroundColor = new Color(cFVGblBR, 95);
                                    getUPi.Box.BorderStyle = ChartDashStyle.Dot;
                                    getUPi.Box.SetRight(barIndex);
                                    getUPi.Active = false;
                                }
                                break;
                            case 1:
                                if (low < getUPi.Box.Top)
                                    getUPi.Box.BorderStyle = ChartDashStyle.Dash;
                                if (low < getUPi.Box.Bottom)
                                {
                                    getUPi.Box.BackgroundColor = new Color(cFVGblBR, 95);
                                    getUPi.Box.BorderStyle = ChartDashStyle.Dot;
                                    getUPi.Box.SetRight(barIndex);
                                    getUPi.Active = false;
                                }
                                break;
                        }
                    }
                }

                // BPR_DN
                for (int i = 0; i < Math.Min(bxBack, bBPR_DN.Count); i++)
                {
                    var getDNi = bBPR_DN[i];
                    if (getDNi.Active)
                    {
                        getDNi.Box.SetRight(barIndex + 8);
                        switch (getDNi.Pos)
                        {
                            case -1:
                                if (high > getDNi.Box.Bottom)
                                    getDNi.Box.BorderStyle = ChartDashStyle.Dash;
                                if (high > getDNi.Box.Top)
                                {
                                    getDNi.Box.BackgroundColor = new Color(cFVGbrBR, 95);
                                    getDNi.Box.BorderStyle = ChartDashStyle.Dot;
                                    getDNi.Box.SetRight(barIndex);
                                    getDNi.Active = false;
                                }
                                break;
                            case 1:
                                if (low < getDNi.Box.Top)
                                    getDNi.Box.BorderStyle = ChartDashStyle.Dash;
                                if (low < getDNi.Box.Bottom)
                                {
                                    getDNi.Box.BackgroundColor = new Color(cFVGbrBR, 95);
                                    getDNi.Box.BorderStyle = ChartDashStyle.Dot;
                                    getDNi.Box.SetRight(barIndex);
                                    getDNi.Active = false;
                                }
                                break;
                        }
                    }
                }
            }
            // NWOG/NDOG
            if (Bars.IsFirstBarOfSession)
            {
                for (int i = 0; i < maxNWOG; i++)
                    bl_NWOG.Insert(0, new bx_ln(new ChartBox(), new ChartLine()));
                for (int i = 0; i < maxNDOG; i++)
                    bl_NDOG.Insert(0, new bx_ln(new ChartBox(), new ChartLine()));
            }

            if (Bars.TimeFrame.DayOfWeek == DayOfWeek.Friday)
            {
                friCp = Bars.Close;
                friCi = Bars.CurrentIndex;
            }

            if (Bars.IsNewSession)
            {
                if (Bars.TimeFrame.DayOfWeek == DayOfWeek.Monday && iNWOG)
                {
                    monOp = Bars.Open;
                    monOi = Bars.CurrentIndex;
                    bl_NWOG.Insert(0, new bx_ln(
                        new ChartBox(
                            friCi,
                            Math.Max(friCp, monOp),
                            monOi,
                            Math.Min(friCp, monOp),
                            Color.Transparent,
                            cNWOG2,
                            ChartExtend.Right
                        ),
                        new ChartLine(
                            monOi,
                            Math.Avg(friCp, monOp),
                            monOi + 1,
                            Math.Avg(friCp, monOp),
                            cNWOG1,
                            ChartLineStyle.Dotted,
                            ChartExtend.Right
                        )
                    ));
                    var bl = bl_NWOG[bl_NWOG.Count - 1];
                    bl_NWOG.RemoveAt(bl_NWOG.Count - 1);
                    bl.b.Delete();
                    bl.l.Delete();
                }

                if (iNDOG)
                {
                    cuDOp = Bars.Open;
                    cuDOi = Bars.CurrentIndex;
                    prDCp = Bars.Close[1];
                    prDCi = Bars.CurrentIndex - 1;

                    bl_NDOG.Insert(0, new bx_ln(
                        new ChartBox(
                            prDCi,
                            Math.Max(prDCp, cuDOp),
                            cuDOi,
                            Math.Min(prDCp, cuDOp),
                            Color.Transparent,
                            cNDOG2,
                            ChartExtend.Right
                        ),
                        new ChartLine(
                            cuDOi,
                            Math.Avg(prDCp, cuDOp),
                            cuDOi + 1,
                            Math.Avg(prDCp, cuDOp),
                            cNDOG1,
                            ChartLineStyle.Dotted,
                            ChartExtend.Right
                        )
                    ));
                    var bl = bl_NDOG[bl_NDOG.Count - 1];
                    bl_NDOG.RemoveAt(bl_NDOG.Count - 1);
                    bl.b.Delete();
                    bl.l.Delete();
                }
            }
            // Liquidity
            for (int i = 0; i < b_liq_B.Count; i++)
            {
                var x = b_liq_B[i];
                if (!x.broken)
                {
                    x.bx.Right = Bars.CurrentIndex + 3;
                    x.ln.X2 = Bars.CurrentIndex + 3;
                    if (!x.brokenTop)
                    {
                        if (Bars.Close > x.bx.Top)
                            x.brokenTop = true;
                    }
                    if (!x.brokenBtm)
                    {
                        if (Bars.Close > x.bx.Bottom)
                            x.brokenBtm = true;
                    }
                    if (x.brokenBtm)
                    {
                        x.bx.BackgroundColor = Color.FromArgb(90, cLIQ_B);
                        x.ln.Delete();
                        if (x.brokenTop)
                        {
                            x.broken = true;
                            x.bx.Right = Bars.CurrentIndex;
                        }
                    }
                }
            }

            for (int i = 0; i < b_liq_S.Count; i++)
            {
                var x = b_liq_S[i];
                if (!x.broken)
                {
                    x.bx.Right = Bars.CurrentIndex + 3;
                    x.ln.X2 = Bars.CurrentIndex + 3;
                    if (!x.brokenTop)
                    {
                        if (Bars.Close < x.bx.Top)
                            x.brokenTop = true;
                    }
                    if (!x.brokenBtm)
                    {
                        if (Bars.Close < x.bx.Bottom)
                            x.brokenBtm = true;
                    }
                    if (x.brokenTop)
                    {
                        x.bx.BackgroundColor = Color.FromArgb(90, cLIQ_S);
                        x.ln.Delete();
                        if (x.brokenBtm)
                        {
                            x.broken = true;
                            x.bx.Right = Bars.CurrentIndex;
                        }
                    }
                }
            }
            var tops = CalculateSwings(true, 1, out var bottoms);

            if (showOB && per)
            {
                if (Bars.Close > tops.Last().Price && !tops.Last().Crossed)
                {
                    tops.Last().Crossed = true;
                    
                    double minima = bottoms[tops.LastIndex()].Price;
                    double maxima = bottoms[tops.LastIndex()].Price;
                    DateTime loc = bottoms[tops.LastIndex()].Time;
                    
                    for (int i = 1; i < (Bars.CurrentIndex - tops.Last().Index) - 1; i++)
                    {
                        minima = Math.Min(bottoms[tops.Last().Index + i].Price, minima);
                        maxima = minima == bottoms[tops.Last().Index + i].Price ? bottoms[tops.Last().Index + i].Price : maxima;
                        loc = minima == bottoms[tops.Last().Index + i].Price ? bottoms[tops.Last().Index + i].Time : loc;
                    }
                    
                    bullish_ob.Add(new OB(maxima, minima, loc));
                }
                
                if (bullish_ob.Count > 0)
                {
                    for (int i = bullish_ob.Count - 1; i >= 0; i--)
                    {
                        var element = bullish_ob[i];
                        
                        if (!element.Breaker)
                        {
                            if (Math.Min(Bars.Close, Bars.Open) < element.Bottom)
                            {
                                element.Breaker = true;
                                element.BreakLocation = Bars.Time;
                            }
                        }
                        else
                        {
                            if (Bars.Close > element.Top)
                            {
                                bullish_ob.RemoveAt(i);
                            }
                            else if (i < showBull && tops.Last().Price < element.Top && tops.Last().Price > element.Bottom)
                            {
                                blBrkConf = 1;
                            }
                        }
                    }
                }

                // Set label
                if (blBrkConf > blBrkConf[1] && showLabels)
                {
                    ChartObjects.DrawText("label", "▼", tops.Last().Index, tops.Last().Price, VerticalAlignment.Top, HorizontalAlignment.Center, Colors.Transparent, bearCss.WithoutTransparency(), FontSize.Small);
                }
            }
            var bottoms = CalculateSwings(false, 1, out var tops);

            if (showOB && per)
            {
                if (Bars.Close < bottoms.Last().Price && !bottoms.Last().Crossed)
                {
                    bottoms.Last().Crossed = true;

                    double minima = bottoms.Last().Price;
                    double maxima = bottoms.Last().Price;
                    DateTime loc = bottoms.Last().Time;

                    for (int i = 1; i < (Bars.CurrentIndex - bottoms.Last().Index) - 1; i++)
                    {
                        maxima = Math.Max(tops.Last().Price, maxima);
                        minima = maxima == tops.Last().Price ? tops.Last().Price : minima;
                        loc = maxima == tops.Last().Price ? tops.Last().Time : loc;
                    }
                    bearish_ob.Add(new OB(maxima, minima, loc));
                }

                if (bearish_ob.Count > 0)
                {
                    for (int i = bearish_ob.Count - 1; i >= 0; i--)
                    {
                        var element = bearish_ob[i];

                        if (!element.Breaker)
                        {
                            if (Math.Max(Bars.Close, Bars.Open) > element.Top)
                            {
                                element.Breaker = true;
                                element.BreakLocation = Bars.Time;
                            }
                        }
                        else
                        {
                            if (Bars.Close < element.Bottom)
                            {
                                bearish_ob.RemoveAt(i);
                            }
                            else if (i < showBear && bottoms.Last().Price > element.Bottom && bottoms.Last().Price < element.Top)
                            {
                                brBrkConf = 1;
                            }
                        }
                    }
                }

                // Set label
                if (brBrkConf > brBrkConf[1] && showLabels)
                {
                    ChartObjects.DrawText("label", "▲", bottoms.Last().Index, bottoms.Last().Price, VerticalAlignment.Bottom, HorizontalAlignment.Center, Colors.Transparent, bullCss.WithoutTransparency(), FontSize.Small);
                }
                // Set Order Blocks
                if (IsLastBar && showOB)
                {
                    if (a_bx_ln_lb.Count > 0)
                    {
                        for (int i = a_bx_ln_lb.Count - 1; i >= 0; i--)
                        {
                            var item = a_bx_ln_lb[i];
                            item.Bx.Delete();
                            item.Ln.Delete();
                            item.Lb.Delete();
                            a_bx_ln_lb.RemoveAt(i);
                        }
                    }
                    
                    // Bullish
                    if (showBull > 0)
                    {
                        int blSz = bullish_ob.Count;
                        if (blSz > 0)
                        {
                            for (int i = 0; i < Math.Min(showBull, bullish_ob.Count); i++)
                            {
                                var get_ob = bullish_ob[i];
                                get_ob.Display(bullCss, bullBrkCss, "bl");
                            }
                        }
                    }
                    
                    // Bearish
                    if (showBear > 0)
                    {
                        int brSz = bearish_ob.Count;
                        if (brSz > 0)
                        {
                            for (int i = 0; i < Math.Min(showBear, bearish_ob.Count); i++)
                            {
                                var get_ob = bearish_ob[i];
                                get_ob.Display(bearCss, bearBrkCss, "br");
                            }
                        }
                    }
                }
                // Fibonacci
                if (IsLastBar)
                {
                    double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                    var up = null, dn = null;
                    
                    switch (iFib)
                    {
                        case "FVG":
                            if (bFVG_UP.Count > 0 && bFVG_DN.Count > 0)
                            {
                                up = bFVG_UP[0].Box;
                                dn = bFVG_DN[0].Box;
                                bool dnFirst = up.GetLeft() > dn.GetLeft();
                                bool dnBottom = up.GetTop() > dn.GetTop();
                                
                                x1 = dnFirst ? dn.GetLeft() : up.GetLeft();
                                x2 = dnFirst ? up.GetRight() : dn.GetRight();
                                y1 = dnFirst ? (dnBottom ? dn.GetBottom() : dn.GetTop()) : (dnBottom ? up.GetTop() : up.GetBottom());
                                y2 = dnFirst ? (dnBottom ? up.GetTop() : up.GetBottom()) : (dnBottom ? dn.GetBottom() : dn.GetTop());
                            }
                            break;
                            
                        case "BPR":
                            if (bBPR_UP.Count > 0 && bBPR_DN.Count > 0)
                            {
                                up = bBPR_UP[0].Box;
                                dn = bBPR_DN[0].Box;
                                bool dnFirst = up.GetLeft() > dn.GetLeft();
                                bool dnBottom = up.GetTop() > dn.GetTop();
                                
                                x1 = dnFirst ? dn.GetLeft() : up.GetLeft();
                                x2 = dnFirst ? up.GetRight() : dn.GetRight();
                                y1 = dnFirst ? (dnBottom ? dn.GetBottom() : dn.GetTop()) : (dnBottom ? up.GetTop() : up.GetBottom());
                                y2 = dnFirst ? (dnBottom ? up.GetTop() : up.GetBottom()) : (dnBottom ? dn.GetBottom() : dn.GetTop());
                            }
                            break;
                            
                        case "OB":
                            int oSz = a_bx_ln_lb.Count;
                            if (oSz > 1)
                            {
                                double xA = a_bx_ln_lb[oSz - 1].Ln.GetX1();
                                double xB = a_bx_ln_lb[oSz - 2].Ln.GetX1();
                                bool AFirst = xB > xA;
                                
                                double yAT = a_bx_ln_lb[oSz - 1].Ln.GetY1();
                                double yAB = a_bx_ln_lb[oSz - 1].Ln.GetY1();
                                double yBT = a_bx_ln_lb[oSz - 2].Ln.GetY1();
                                double yBB = a_bx_ln_lb[oSz - 2].Ln.GetY1();
                                bool ABottom = yAB < yBB;
                                
                                x1 = AFirst ? xA : xB;
                                x2 = AFirst ? xB : xA;
                                y1 = AFirst ? (ABottom ? yAB : yAT) : (ABottom ? yBT : yBB);
                                y2 = AFirst ? (ABottom ? yBT : yBB) : (ABottom ? yAB : yAT);
                            }
                            break;
                            
                        case "Liq":
                            if (b_liq_B.Count > 0 && b_liq_S.Count > 0)
                            {
                                double xA = b_liq_B[0].Ln.GetX1();
                                double xB = b_liq_S[0].Ln.GetX1();
                                bool AFirst = xB > xA;
                                
                                double yAT = b_liq_B[0].Ln.GetY1();
                                double yAB = b_liq_B[0].Ln.GetY1();
                                double yBT = b_liq_S[0].Ln.GetY1();
                                double yBB = b_liq_S[0].Ln.GetY1();
                                bool ABottom = yAB < yBB;
                                
                                x1 = AFirst ? xA : xB;
                                x2 = AFirst ? xB : xA;
                                y1 = AFirst ? (ABottom ? yAB : yAT) : (ABottom ? yBT : yBB);
                                y2 = AFirst ? (ABottom ? yBT : yBB) : (ABottom ? yAB : yAT);
                            }
                            break;
                            
                        case "VI":
                            if (Vimbal.Count > 1)
                            {
                                double AxA = Vimbal[1].L2.GetX1();
                                double AxB = Vimbal[1].L1.GetX1();
                                double BxA = Vimbal[0].L2.GetX1();
                                double BxB = Vimbal[0].L1.GetX1();
                                
                                double AyA = Vimbal[1].L2.GetY1();
                                double AyB = Vimbal[1].L1.GetY1();
                                double ByA = Vimbal[0].L2.GetY1();
                                double ByB = Vimbal[0].L1.GetY1();
                                
                                bool ABt = Math.Min(ByA, ByB) > Math.Min(AyA, AyB);
                                x1 = Math.Max(AxA, AxB);
                                x2 = Math.Max(BxA, BxB);
                                y1 = ABt ? Math.Min(AyA, AyB) : Math.Max(AyA, AyB);
                                y2 = ABt ? Math.Max(ByA, ByB) : Math.Min(ByA, ByB);
                            }
                            break;
                            
                        case "NWOG":
                            if (bl_NWOG.Count > 1)
                            {
                                up = bl_NWOG[0].B;
                                dn = bl_NWOG[1].B;
                                bool dnFirst = up.GetLeft() > dn.GetLeft();
                                bool dnBottom = up.GetTop() > dn.GetTop();
                                
                                x1 = dnFirst ? dn.GetLeft() : up.GetLeft();
                                x2 = dnFirst ? up.GetRight() : dn.GetRight();
                                y1 = dnFirst ? (dnBottom ? dn.GetBottom() : dn.GetTop()) : (dnBottom ? up.GetTop() : up.GetBottom());
                                y2 = dnFirst ? (dnBottom ? up.GetTop() : up.GetBottom()) : (dnBottom ? dn.GetBottom() : dn.GetTop());
                            }
                            break;
                    }
                    
                    if (iFib != "NONE")
                    {
                        double rt = Math.Max(x1, x2);
                        double lt = Math.Min(x1, x2);
                        double tp = Math.Max(y1, y2);
                        double bt = Math.Min(y1, y2);
                        double _0 = rt == x1 ? y1 : y2;
                        double _1 = rt == x1 ? y2 : y1;
                        
                        double df = _1 - _0;
                        double m0236 = df * 0.236;
                        double m0382 = df * 0.382;
                        double m0500 = df * 0.500;
                        double m0618 = df * 0.618;
                        double m0786 = df * 0.786;
                        double m1618 = df * 1.618;
                        
                        _diag.SetLine(x1, y1, x2, y2);
                        _vert.SetLine(rt, _0, rt, _0 + m1618);
                        _zero.SetLine(rt, _0, rt + plus, _0);
                        _0236.SetLine(rt, _0 + m0236, rt + plus, _0 + m0236);
                        _0382.SetLine(rt, _0 + m0382, rt + plus, _0 + m0382);
                        _0500.SetLine(rt, _0 + m0500, rt + plus, _0 + m0500);
                        _0618.SetLine(rt, _0 + m0618, rt + plus, _0 + m0618);
                        _0786.SetLine(rt, _0 + m0786, rt + plus, _0 + m0786);
                        _one_.SetLine(rt, _1, rt + plus, _1);
                        _1618.SetLine(rt, _0 + m1618, rt + plus, _0 + m1618);
                    }
                }
            }
            // Displacement
            if (sDispl && per)
            {
                if (L_bodyUP)
                {
                    ChartObjects.DrawText("DisplacementUP", "▲", StaticPosition.Bottom, Colors.Lime);
                }
                if (L_bodyDN)
                {
                    ChartObjects.DrawText("DisplacementDN", "▼", StaticPosition.Top, Colors.Red);
                }
            }

            // Background - Killzones
            if (per)
            {
                if (ny)
                {
                    ChartObjects.DrawRectangle("NewYork", true, Timeframe.Opening, Timeframe.Closing, null, Colors.DarkBlue, null);
                }
                if (ldn_open)
                {
                    ChartObjects.DrawRectangle("LondonOpen", true, Timeframe.Opening, null, null, Colors.DarkOrange, null);
                }
                if (ldn_close)
                {
                    ChartObjects.DrawRectangle("LondonClose", true, null, Timeframe.Closing, null, Colors.DarkRed, null);
                }
                if (asian)
                {
                    ChartObjects.DrawRectangle("AsianSession", true, Timeframe.Opening, Timeframe.Closing, null, Colors.DarkGreen, null);
                }
            }


        }
        
        
        protected override void Initialize()
        {
            high = MarketData.High;
            low = MarketData.Low;
        }

        public override void Calculate(int index)
        {
            // Your calculation logic goes here
        }
    }
}
