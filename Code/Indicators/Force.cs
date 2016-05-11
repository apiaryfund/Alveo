using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bulls Power Indicator")]
    public class Force : IndicatorBase
    {
        private readonly Array<double> _vals;

        public Force()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("Force({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("Force({0})", IndicatorPeriod));
            MAType = MovingAverageType.MODE_SMA;
            PriceType = PriceConstants.PRICE_CLOSE;
            _vals = new Array<double>();
        }

        [Description("Period of the Bulls Power Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Type of Moving Avarange the Envelopes Indicator")]
        [Category("Settings")]
        [DisplayName("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Description("Price type on witch Envelopes will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("Force({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("Force({0})", IndicatorPeriod));
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos + 1 > Bars - IndicatorPeriod)
                pos = Bars - IndicatorPeriod - 1;
            double ema, emaPrev;
            var volume = GetHistory(Symbol, TimeFrame);

            if (volume.Count == 0)
                return 0;

            while (pos >= 0)
            {
                ema = iMA(Symbol, TimeFrame, IndicatorPeriod, 0, (int)MAType, (int)PriceType, pos);
                emaPrev = iMA(Symbol, TimeFrame, IndicatorPeriod, 0, (int)MAType, (int)PriceType, pos + 1);
                _vals[pos] = volume[pos].Volume*(ema - emaPrev);
                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 5)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != IndicatorPeriod)
                return false;
            if (!(values[3] is MovingAverageType) || (MovingAverageType)values[3] != MAType)
                return false;
            if (!(values[4] is PriceConstants) || (PriceConstants)values[4] != PriceType)
                return false;

            return true;
        }
    }
}