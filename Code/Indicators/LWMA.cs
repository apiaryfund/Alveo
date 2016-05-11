using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Linear Weighted Moving Average Indicator")]
    public class LWMA : IndicatorBase
    {
        private readonly Array<double> _values;

        public LWMA()
        {
            indicator_buffers = 1;
            indicator_chart_window = true;
            indicator_color1 = Colors.Red;
            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("LWMA({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("LWMA({0})", IndicatorPeriod));
            PriceType = PriceConstants.PRICE_CLOSE;
            _values = new Array<double>();
        }

        [Description("Period of the LWMA Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch LWMA will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("LWMA({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("LWMA({0})", IndicatorPeriod));
            SetIndexBuffer(0, _values);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (baseArray.Count == 0)
                return 0;
            pos += IndicatorPeriod;
            if (pos >= Bars)
                pos = Bars - 1;
            double price, sum = 0, lsum = 0, weight = 0;
            int i;
            for (i = 1; i <= IndicatorPeriod; i++, pos--)
            {
                price = baseArray[pos];
                sum += price*i;
                lsum += price;
                weight += i;
            }
            _values[pos + 1] = sum/weight;
            //---- main calculation loop
            i = pos + IndicatorPeriod;
            while (pos >= 0)
            {
                price = Close[pos];
                sum = sum - lsum + price*IndicatorPeriod;
                lsum -= Close[i];
                lsum += price;
                _values[pos] = sum/weight;
                pos--;
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