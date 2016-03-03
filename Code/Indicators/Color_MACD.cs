
using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Display MACD  Momentum in Color based on Direction")]
    public class Color_MACD : IndicatorBase
     {
        #region Properties
        
        [Category("Settings")]
        public int FastEMA{ get; set; }

        [Category("Settings")]
        public int SlowEMA{ get; set; }

        [Category("Settings")]
        public int SignalSMA{ get; set; }

        [Category("Settings")]
        public double MinDiff{ get; set; }

        #endregion
        
        
        public Color_MACD()
        {
            
            copyright = "JF";
            link = "";
            indicator_separate_window = true;
            indicator_levelcolor = Gray;
            indicator_buffers = 4;
            indicator_color1 = Green;
            indicator_color2 = Red;
            indicator_color3 = Black;
            indicator_color4 = Yellow;
            indicator_level1 = 0;

            FastEMA = 8;
            SlowEMA = 17;
            SignalSMA = 9;
            MinDiff = 0.0;
        }
        Array<double> gda_104 = new Array<double>();
        Array<double> g_ibuf_108 = new Array<double>();
        Array<double> g_ibuf_112 = new Array<double>();
        Array<double> g_ibuf_116 = new Array<double>();
        Array<double> g_ibuf_120 = new Array<double>();
        protected override int Init()
        {
        	SetIndexStyle(0, DRAW_HISTOGRAM,1,4);
            SetIndexStyle(1, DRAW_HISTOGRAM,1,4);
            SetIndexStyle(2, DRAW_HISTOGRAM);
            SetIndexStyle(3, DRAW_LINE);

            SetIndexDrawBegin(1, SignalSMA);
            IndicatorDigits(1);
            SetIndexBuffer(0, g_ibuf_108);
            SetIndexBuffer(1, g_ibuf_112);
            SetIndexBuffer(2, g_ibuf_116);
            SetIndexBuffer(3, g_ibuf_120);
            IndicatorShortName(WindowExpertName() + " (" + FastEMA + "," + SlowEMA + "," + SignalSMA + ")");
            SetIndexLabel(0, "MACD UP");
            SetIndexLabel(1, "MACD DN");
            SetIndexLabel(2, "MACD EQ");
            SetIndexLabel(3, "Signal");
        	return 0;
        }
        
        protected override int Deinit()
        {
        	string l_name_0 = WindowExpertName() + "," + Symbol() + "," + Period();
            ObjectDelete(l_name_0);
        	return 0;
        }
        
        protected override int Start()
        {
        	Array<double> lda_20 = new Array<double>();
            int l_ind_counted_4 = IndicatorCounted();
            int li_0 = l_ind_counted_4 + 1 ;//MathMin(Bars - SlowEMA, Bars - l_ind_counted_4 + 1);
            ArrayResize(gda_104, li_0);
            ArraySetAsSeries(gda_104, true);
            int li_8;
            for (li_8 = 0; li_8 < li_0; li_8++) gda_104[li_8] = (iMA(null, 0, FastEMA, 0, MODE_EMA, PRICE_CLOSE, li_8) - iMA(null, 0, SlowEMA, 0, MODE_EMA, PRICE_CLOSE, li_8)) / Point / 10.0;
            for (li_8 = li_0 - 2; li_8 >= 0; li_8--) {
                if (MathAbs(gda_104[li_8] - (gda_104[li_8 + 1])) < MinDiff) {
                    g_ibuf_116[li_8] = gda_104[li_8];
                    g_ibuf_108[li_8] = 0;
                    g_ibuf_112[li_8] = 0;
                    } else {
                    if (gda_104[li_8] > gda_104[li_8 + 1]) {
                        g_ibuf_108[li_8] = gda_104[li_8];
                        g_ibuf_112[li_8] = 0;
                        g_ibuf_116[li_8] = 0;
                        } else {
                        g_ibuf_112[li_8] = gda_104[li_8];
                        g_ibuf_108[li_8] = 0;
                        g_ibuf_116[li_8] = 0;
                    }
                }
            }
         
            for (li_8 = 0; li_8 < li_0; li_8++) g_ibuf_120[li_8] = iMAOnArray(gda_104, Bars, SignalSMA, 0, MODE_EMA, li_8);
            double ld_12 = (iMA(null, 0, FastEMA, 0, MODE_EMA, PRICE_CLOSE, 1) - iMA(null, 0, SlowEMA, 0, MODE_EMA, PRICE_CLOSE, 1)) / Point / 10.0;
            ArrayResize(lda_20, Bars);
            ArraySetAsSeries(lda_20, true);
            ArrayCopy(lda_20, Close, 0, 0, ArraySize(lda_20));
            double ld_24 = (iMAOnArray(lda_20, 0, FastEMA, 0, MODE_EMA, 0) - iMAOnArray(lda_20, 0, SlowEMA, 0, MODE_EMA, 0)) / Point / 10.0;
            if (ld_24 < ld_12) {
                while (ld_24 < ld_12) {
                    lda_20[0] += Point / 0.1;
                    ld_24 = (iMAOnArray(lda_20, 0, FastEMA, 0, MODE_EMA, 0) - iMAOnArray(lda_20, 0, SlowEMA, 0, MODE_EMA, 0)) / Point / 10.0;
                }
                } else {
                while (ld_24 > ld_12) {
                    lda_20[0] = lda_20[0] - Point / 0.1;
                    ld_24 = (iMAOnArray(lda_20, 0, FastEMA, 0, MODE_EMA, 0) - iMAOnArray(lda_20, 0, SlowEMA, 0, MODE_EMA, 0)) / Point / 10.0;
                }
            }

        	return 0;
        }	
        
        #region Indicator parameters check

        [Description("Parameters order Symbol, TimeFrame, FastEMA, SlowEMA, SignalSMA MinDiff")]
        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 6)
            return false;

            try
            {

                var val0 = (string)values[0];
                if (!string.IsNullOrEmpty(val0) && !string.IsNullOrEmpty(Symbol))
                {
                    if (!Symbol.Equals(val0, StringComparison.OrdinalIgnoreCase))
                    return false;
                }
                else if(!(string.IsNullOrEmpty(Symbol) && string.IsNullOrEmpty(val0)))
                return false;

                var val1 = (int) values[1];
                if (val1 != TimeFrame)
                return false;

                var val2 = (int) values[2];
                if (val2 != FastEMA)
                return false;

                var val3 = (int) values[3];
                if (val3 != SlowEMA)
                return false;

                var val4 = (int) values[4];
                if (val4 != SignalSMA)
                return false;

                var val5 = (double) values[5];
                if (val5 != MinDiff)
                return false;

            }
            catch { return false; }

            return true;
        }

        [Description("Parameters order Symbol, TimeFrame, FastEMA, SlowEMA, SignalSMA MinDiff")]
        public override void SetIndicatorParameters(params object[] values)
        {
            if (values.Length != 6)
            return;

            try
            {

                Symbol = (string)values[0];
                TimeFrame = (int) values[1];
                FastEMA = (int)values[2];
                SlowEMA = (int)values[3];
                SignalSMA = (int)values[4];
                MinDiff = (double)values[5];

            }
            catch {}
        }

        #endregion

    }
}