using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Average Directional Movement Indicator")]
    public class ADX : IndicatorBase
    {
        private readonly Array<double> _adx;
        private readonly Array<double> _minusDi;
        private readonly Array<double> _plusDi;

        public ADX()
        {
            indicator_buffers = 3;
            IndicatorPeriod = 10;
            indicator_chart_window = false;

            indicator_color1 = Colors.Blue;
            SetIndexLabel(0, string.Format("ADX({0})", IndicatorPeriod));
            indicator_color2 = Colors.Green;
            SetIndexLabel(1, "Plus_Di");
            indicator_color3 = Colors.Red;
            SetIndexLabel(2, "Minus_Di");

            PriceType = PriceConstants.PRICE_CLOSE;
            _adx = new Array<double>();
            _plusDi = new Array<double>();
            _minusDi = new Array<double>();
        }

        [Description("Period of the ADX Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch ADX will be calculated")]
        [Category("Settings")]
        [DisplayName("Price type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("ADX({0})", IndicatorPeriod));
            SetIndexLabel(1, "Plus_Di");
            SetIndexLabel(2, "Minus_Di");

            IndicatorShortName(string.Format("ADX({0})", IndicatorPeriod));

            SetIndexBuffer(0, _adx);
            SetIndexBuffer(1, _plusDi);
            SetIndexBuffer(2, _minusDi);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            var data = GetHistory(Symbol, TimeFrame);
            if (data.Count == 0)
                return 0;
            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (pos >= Bars - 1)
            {
                pos = Bars - 1;
                _plusDi[pos] = 0;
                _minusDi[pos] = 0;
                _adx[pos] = 0;
                pos--;
            }

            double pdm;
            double mdm;
            double tr;
            double price_high;
            double price_low;
            double plusSdi;
            double minusSdi;
            double temp;
            var exp = 2/(double)(IndicatorPeriod + 1);

            while (pos >= 0)
            {
                //Quick Fix. Need to rewrite
                if (pos >= data.Count)
                {
                    --pos;
                    continue;
                }
                //-------------------------

                price_low = (double)data[pos].Low;
                price_high = (double)data[pos].High;
                pdm = price_high - (double)data[pos + 1].High;
                mdm = (double)data[pos + 1].Low - price_low;
                if (pdm < 0)
                    pdm = 0; // +DM
                if (mdm < 0)
                    mdm = 0; // -DM
                if (pdm.Equals(mdm))
                {
                    pdm = 0;
                    mdm = 0;
                }
                else if (pdm < mdm)
                    pdm = 0;
                else if (mdm < pdm)
                    mdm = 0;

                var num1 = MathAbs(price_high - price_low);
                var num2 = MathAbs(price_high - baseArray[pos + 1]);
                var num3 = MathAbs(price_low - baseArray[pos + 1]);
                tr = MathMax(num1, num2);
                tr = MathMax(tr, num3);

                if (tr.Equals(0))
                {
                    plusSdi = 0;
                    minusSdi = 0;
                }
                else
                {
                    plusSdi = 100.0*pdm/tr;
                    minusSdi = 100.0*mdm/tr;
                }

                _plusDi[pos] = plusSdi*exp + _plusDi[pos + 1]*(1 - exp);
                _minusDi[pos] = minusSdi*exp + _minusDi[pos + 1]*(1 - exp);

                var div = MathAbs(_plusDi[pos] + _minusDi[pos]);
                if (div.Equals(0.00))
                    temp = 0;
                else
                    temp = 100*(MathAbs(_plusDi[pos] - _minusDi[pos])/div);

                _adx[pos] = temp*exp + _adx[pos + 1]*(1 - exp);

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