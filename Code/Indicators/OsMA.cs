using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Moving Average of Oscillator Indicator")]
    public class OsMA : IndicatorBase
    {
        private readonly Array<double> MacdBuffer = new Array<double>();
        private readonly Array<double> OsmaBuffer = new Array<double>();
        private readonly Array<double> SignalBuffer = new Array<double>();

        public OsMA()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Silver;
            indicator_width1 = 3;

            FastPeriod = 12;
            SlowPeriod = 26;
            SignalPeriod = 9;
            PriceType = PriceConstants.PRICE_CLOSE;
            IndicatorShortName(string.Format("OsMA({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));
            SetIndexLabel(0, string.Format("OsMA({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));
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
            IndicatorBuffers(3);

            SetIndexStyle(0, DRAW_HISTOGRAM);
            SetIndexDrawBegin(0, SignalPeriod);
            IndicatorDigits(Digits + 2);

            SetIndexBuffer(0, OsmaBuffer);
            SetIndexBuffer(1, MacdBuffer);
            SetIndexBuffer(2, SignalBuffer);

            IndicatorShortName(string.Format("OsMA({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));
            SetIndexLabel(0, string.Format("OsMA({0},{1},{2})", FastPeriod, SlowPeriod, SignalPeriod));

            return 0;
        }

        protected override int Start()
        {
            var counted_bars = IndicatorCounted();

            if (counted_bars > 0)
                counted_bars--;
            var limit = Bars - counted_bars;

            int i;
            for (i = 0; i < limit; i++)
            {
                MacdBuffer[i] = iMA(null, 0, FastPeriod, 0, MODE_EMA, PRICE_CLOSE, i)
                                - iMA(null, 0, SlowPeriod, 0, MODE_EMA, PRICE_CLOSE, i);
            }

            for (i = 0; i < limit; i++)
                SignalBuffer[i] = iMAOnArray(MacdBuffer, Bars, SignalPeriod, 0, MODE_SMA, i);

            for (i = 0; i < limit; i++)
                OsmaBuffer[i] = MacdBuffer[i] - SignalBuffer[i];

            return (0);
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