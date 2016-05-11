using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("On Balance Volume Indicator")]
    public class OBV : IndicatorBase
    {
        private readonly Array<double> _vals;

        public OBV()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            SetIndexLabel(0, "OBV");
            IndicatorShortName("OBV");
            PriceType = PriceConstants.PRICE_CLOSE;
            _vals = new Array<double>();
        }

        [Description("Price type on witch OBV will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, _vals);
            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();

            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            var data = GetHistory(Symbol, TimeFrame);

            if (data.Count == 0)
                return 0;

            double curPrice;
            double prevPrice;

            if (pos >= Bars - 1)
            {
                pos = Bars - 1;
                _vals[pos] = data[pos].Volume;
            }
            while (pos >= 0)
            {
                curPrice = baseArray[pos];
                prevPrice = baseArray[pos + 1];
                if (curPrice.Equals(prevPrice))
                    _vals[pos] = _vals[pos + 1];
                else
                {
                    if (curPrice < prevPrice)
                        _vals[pos] = _vals[pos + 1] - data[pos].Volume;
                    else
                        _vals[pos] = _vals[pos + 1] + data[pos].Volume;
                }
                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 3)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is PriceConstants) || (PriceConstants)values[2] != PriceType)
                return false;

            return true;
        }
    }
}