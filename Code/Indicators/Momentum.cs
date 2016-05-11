using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Momentum Indicator")]
    public class Momentum : IndicatorBase
    {
        private readonly Array<double> _vals;

        public Momentum()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("Momentum({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("Momentum({0})", IndicatorPeriod));

            PriceType = PriceConstants.PRICE_CLOSE;
            _vals = new Array<double>();
        }

        [Description("Period of the Momentum Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch Momentum will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("Momentum({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("Momentum({0})", IndicatorPeriod));
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod)
                pos = Bars - IndicatorPeriod - 1;
            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (baseArray.Count == 0)
                return 0;

            while (pos >= 0)
            {
                _vals[pos] = baseArray[pos]*100/baseArray[pos + IndicatorPeriod];
                pos--;
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