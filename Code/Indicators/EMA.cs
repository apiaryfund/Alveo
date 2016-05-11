using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Exponential Moving Avarage Indicator")]
    public class EMA : IndicatorBase
    {
        private readonly Array<double> values;

        public EMA()
        {
            indicator_buffers = 1;
            indicator_chart_window = true;
            indicator_color1 = Colors.Red;
            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("EMA({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("EMA({0})", IndicatorPeriod));
            PriceType = PriceConstants.PRICE_CLOSE;
            values = new Array<double>();
        }

        [Description("Period of the EMA Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch EMA will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("EMA({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("EMA({0})", IndicatorPeriod));

            SetIndexBuffer(0, values);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos >= Bars)
                pos = Bars - 1;
            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (baseArray.Count == 0)
                return 0;
            var exp = 2/(double)(IndicatorPeriod + 1);

            if (pos == Bars - 1)
            {
                values[pos] = baseArray[pos];
                pos--;
            }
            while (pos >= 0)
            {
                values[pos] = baseArray[pos]*exp + values[pos + 1]*(1 - exp);
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