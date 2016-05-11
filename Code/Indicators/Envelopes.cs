using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Envelopes Indicator")]
    public class Envelopes : IndicatorBase
    {
        private readonly Array<double> _minusVals;
        private readonly Array<double> _plusVals;

        public Envelopes()
        {
            indicator_buffers = 2;
            indicator_chart_window = true;
            indicator_color1 = Colors.Green;
            indicator_color2 = Colors.Red;
            IndicatorPeriod = 10;
            Deviation = 0.1;

            SetIndexLabel(0, string.Format("Env({0})Upper", IndicatorPeriod));
            SetIndexLabel(1, string.Format("Env({0})Lower", IndicatorPeriod));
            IndicatorShortName(string.Format("Envelopes({0})", IndicatorPeriod));

            PriceType = PriceConstants.PRICE_CLOSE;
            _plusVals = new Array<double>();
            _minusVals = new Array<double>();
        }

        [Description("Period of the Envelopes Indicator")]
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

        [Description("Deviation of the Envelopes Indicator")]
        [Category("Settings")]
        public double Deviation { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("Env({0})Upper", IndicatorPeriod));
            SetIndexLabel(1, string.Format("Env({0})Lower", IndicatorPeriod));
            IndicatorShortName(string.Format("Envelopes({0})", IndicatorPeriod));

            SetIndexBuffer(0, _plusVals);
            SetIndexBuffer(1, _minusVals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod)
                pos = Bars - IndicatorPeriod;

            var devPlus = (1 + Deviation/100);
            var devMinus = (1 - Deviation/100);
            double ma;

            while (pos >= 0)
            {
                ma = iMA(Symbol, TimeFrame, IndicatorPeriod, 0, (int)MovingAverageType.MODE_SMA, (int)PriceType, pos);
                _plusVals[pos] = devPlus*ma;
                _minusVals[pos] = devMinus*ma;
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
            if (!(values[2] is int) || (int)values[2] != IndicatorPeriod)
                return false;
            if (!(values[3] is MovingAverageType) || (MovingAverageType)values[3] != MAType)
                return false;
            if (!(values[4] is PriceConstants) || (PriceConstants)values[4] != PriceType)
                return false;

            return values[5] is double && (double)values[5] == Deviation;
        }
    }
}