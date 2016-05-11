using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Moving Average of Oscillator Indicator")]
    public class MACD : IndicatorBase
    {
        private readonly Array<double> signalVals;
        private readonly Array<double> vals;

        public MACD()
        {
            indicator_buffers = 2;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            indicator_color2 = Colors.Blue;

            FastPeriod = 12;
            SlowPeriod = 26;
            SignalPeriod = 9;
            SetIndexLabel(1, "Signal");
            SetIndexLabel(0, string.Format("MACD({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));
            IndicatorShortName(string.Format("MACD({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));

            PriceType = PriceConstants.PRICE_CLOSE;
            vals = new Array<double>();
            signalVals = new Array<double>();
        }

        [Description("Fast Period of the OsMA Indicator")]
        [Category("Settings")]
        [DisplayName("Slow Period")]
        public int SlowPeriod { get; set; }

        [Description("Slow Period of the OsMA Indicator")]
        [Category("Settings")]
        [DisplayName("Signal Period")]
        public int SignalPeriod { get; set; }

        [Description("Signal Period of the OsMA Indicator")]
        [Category("Settings")]
        [DisplayName("Fast Period")]
        public int FastPeriod { get; set; }

        [Description("Price type on witch OsMA will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("MACD({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));
            IndicatorShortName(string.Format("MACD({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));

            SetIndexStyle(0, DRAW_HISTOGRAM);
            SetIndexStyle(1, DRAW_LINE);

            SetIndexBuffer(0, vals);
            SetIndexBuffer(1, signalVals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            pos += SignalPeriod;
            if (pos >= Bars)
                pos = Bars - 1;

            double sum = 0, fastMA, slowMA;
            for (var i = 1; i < SignalPeriod; i++, pos--)
            {
                fastMA = iMA(Symbol, TimeFrame, FastPeriod, 0, (int)MovingAverageType.MODE_EMA, (int)PriceType, pos);
                slowMA = iMA(Symbol, TimeFrame, SlowPeriod, 0, (int)MovingAverageType.MODE_EMA, (int)PriceType, pos);
                sum += (fastMA - slowMA);
            }

            while (pos >= 0)
            {
                fastMA = iMA(Symbol, TimeFrame, FastPeriod, 0, (int)MovingAverageType.MODE_EMA, (int)PriceType, pos);
                slowMA = iMA(Symbol, TimeFrame, SlowPeriod, 0, (int)MovingAverageType.MODE_EMA, (int)PriceType, pos);
                sum += (fastMA - slowMA);
                vals[pos] = fastMA - slowMA;
                signalVals[pos] = sum/SignalPeriod;
                fastMA = iMA(Symbol, TimeFrame, FastPeriod, 0, (int)MovingAverageType.MODE_EMA, (int)PriceType,
                    pos + SignalPeriod - 1);
                slowMA = iMA(Symbol, TimeFrame, SlowPeriod, 0, (int)MovingAverageType.MODE_EMA, (int)PriceType,
                    pos + SignalPeriod - 1);
                sum -= (fastMA - slowMA);
                pos--;
            }
            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 6)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != FastPeriod)
                return false;
            if (!(values[3] is int) || (int)values[3] != SlowPeriod)
                return false;
            if (!(values[4] is int) || (int)values[4] != SignalPeriod)
                return false;
            if (!(values[5] is PriceConstants) || (PriceConstants)values[5] != PriceType)
                return false;

            return true;
        }
    }
}