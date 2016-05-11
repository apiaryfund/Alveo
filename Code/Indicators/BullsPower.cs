using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bulls Power Indicator")]
    public class BullsPower : IndicatorBase
    {
        private readonly Array<double> BullsBuffer = new Array<double>();
        private readonly Array<double> TempBuffer = new Array<double>();

        public BullsPower()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("BullPower({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("BullPower({0})", IndicatorPeriod));
            PriceType = PriceConstants.PRICE_CLOSE;
        }

        [Description("Period of the Bulls Power Indicator")]
        [Category("Settings")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch Bulls Power will be calculated")]
        [Category("Settings")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("BullPower({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("BullPower({0})", IndicatorPeriod));

            IndicatorBuffers(2);
            IndicatorDigits(Digits);
            SetIndexStyle(0, DRAW_HISTOGRAM);
            SetIndexBuffer(0, BullsBuffer);
            SetIndexBuffer(1, TempBuffer);

            return 0;
        }

        protected override int Start()
        {
            int i;
            var counted_bars = IndicatorCounted();

            if (Bars <= IndicatorPeriod)
                return (0);

            var limit = Bars - counted_bars;
            if (counted_bars > 0)
                limit++;
            for (i = 0; i < limit; i++)
                TempBuffer[i] = iMA(null, 0, IndicatorPeriod, 0, MODE_EMA, PRICE_CLOSE, i);

            i = Bars - counted_bars - 1;
            while (i >= 0)
            {
                BullsBuffer[i] = High[i] - TempBuffer[i];
                i--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 4)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != IndicatorPeriod)
                return false;
            if (!(values[3] is PriceConstants) || (PriceConstants)values[3] != PriceType)
                return false;

            return true;
        }
    }
}