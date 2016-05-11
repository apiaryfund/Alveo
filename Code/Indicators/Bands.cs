using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bollinger Bands Indicator")]
    public class Bands : IndicatorBase
    {
        private readonly Array<double> _lowVals;
        private readonly Array<double> _upVals;
        private readonly Array<double> _vals;

        public Bands()
        {
            indicator_buffers = 3;
            indicator_chart_window = true;

            IndicatorPeriod = 10;
            Deviation = 2;

            indicator_color1 = Colors.Blue;
            SetIndexLabel(0, string.Format("Bands({0},{1})", IndicatorPeriod, Deviation));
            indicator_color2 = Colors.Green;
            SetIndexLabel(1, "Bands_High");
            indicator_color3 = Colors.Red;
            SetIndexLabel(2, "Bands_Low");

            IndicatorShortName(string.Format("Bands({0},{1})", IndicatorPeriod, Deviation));

            PriceType = PriceConstants.PRICE_CLOSE;
            _vals = new Array<double>();
            _upVals = new Array<double>();
            _lowVals = new Array<double>();
        }

        [Description("Averaging period to calculate the main line")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Deviation from the main line")]
        [Category("Settings")]
        public int Deviation { get; set; }

        [Description("Price type on witch Bollinger Bands will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("Bands({0},{1})", IndicatorPeriod, Deviation));
            IndicatorShortName(string.Format("Bands({0},{1})", IndicatorPeriod, Deviation));

            SetIndexBuffer(0, _vals);
            SetIndexBuffer(1, _upVals);
            SetIndexBuffer(2, _lowVals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod)
                pos = Bars - IndicatorPeriod;

            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (baseArray.Count == 0)
                return 0;
            double sma, deviation, sum, newres;
            while (pos >= 0)
            {
                sma = iMA(Symbol, TimeFrame, IndicatorPeriod, 0, (int)MovingAverageType.MODE_SMA, (int)PriceType, pos);
                sum = 0;
                for (var i = 0; i < IndicatorPeriod; i++)
                {
                    newres = baseArray[pos + i] - sma;
                    sum += newres*newres;
                }
                deviation = Deviation*MathSqrt(sum/IndicatorPeriod);

                _vals[pos] = sma;
                _upVals[pos] = sma + deviation;
                _lowVals[pos] = sma - deviation;
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
            if (!(values[3] is int) || (int)values[3] != Deviation)
                return false;
            if (!(values[4] is PriceConstants) || (PriceConstants)values[4] != PriceType)
                return false;

            return true;
        }
    }
}