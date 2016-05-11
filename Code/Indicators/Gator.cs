using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Gator oscillator Indicator")]
    public class Gactor : IndicatorBase
    {
        private readonly Array<double> _lowBuff;
        private readonly Array<double> _upBuff;

        public Gactor()
        {
            indicator_buffers = 2;
            indicator_chart_window = false;
            indicator_color1 = Colors.Green;
            indicator_color2 = Colors.Red;
            SetIndexLabel(0, "Up");
            SetIndexLabel(1, "Down");
            IndicatorShortName("Gactor");

            JawPeriod = 13;
            JawShift = 8;
            TeethPeriod = 8;
            TeethShift = 5;
            LipsPeriod = 5;
            LipsShift = 3;
            MAType = MovingAverageType.MODE_SMA;
            PriceType = PriceConstants.PRICE_MEDIAN;

            _upBuff = new Array<double>();
            _lowBuff = new Array<double>();
        }

        [Description("Jaw Period of the Gator Indicator")]
        [Category("Settings")]
        [DisplayName("Jaw Period")]
        public int JawPeriod { get; set; }

        [Description("Jaw Shift of the Gator Indicator")]
        [Category("Settings")]
        [DisplayName("Jaw Shift")]
        public int JawShift { get; set; }

        [Description("Teeth Period of the Gator Indicator")]
        [Category("Settings")]
        [DisplayName("Teeth Period")]
        public int TeethPeriod { get; set; }

        [Description("Teeth Shift of the Gator Indicator")]
        [Category("Settings")]
        [DisplayName("Teeth Shift")]
        public int TeethShift { get; set; }

        [Description("Lips Period of the Gator Indicator")]
        [Category("Settings")]
        [DisplayName("Lips Period")]
        public int LipsPeriod { get; set; }

        [Description("Lips Shift of the Gator Indicator")]
        [Category("Settings")]
        [DisplayName("Lips shift")]
        public int LipsShift { get; set; }

        [Description("Moving Average type on witch Gator will be calculated")]
        [Category("Settings")]
        [DisplayName("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Description("Price type on witch Gator will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexStyle(0, DRAW_HISTOGRAM);
            SetIndexStyle(1, DRAW_HISTOGRAM);
            SetIndexBuffer(0, _upBuff);
            SetIndexBuffer(1, _lowBuff);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos >= Bars)
                pos = Bars - 1;

            double jaw, teeth, lips;
            var maxUp = Math.Max(JawPeriod + JawShift, TeethPeriod + TeethShift);
            var maxDn = Math.Max(TeethPeriod + TeethShift, LipsPeriod + LipsPeriod);
            var startUp = Bars - maxUp;
            var startDn = Bars - maxDn;
            double tmpVal;

            while (pos >= 0)
            {
                jaw = iAlligator(Symbol, TimeFrame, JawPeriod, JawShift, TeethPeriod, TeethShift, LipsPeriod, LipsShift,
                    (int)MAType, (int)PriceType, (int)IndicatorAligatorLineType.MODE_GATORJAW, pos);
                teeth = iAlligator(Symbol, TimeFrame, JawPeriod, JawShift, TeethPeriod, TeethShift, LipsPeriod,
                    LipsShift, (int)MAType, (int)PriceType, (int)IndicatorAligatorLineType.MODE_GATORTEETH, pos);
                lips = iAlligator(Symbol, TimeFrame, JawPeriod, JawShift, TeethPeriod, TeethShift, LipsPeriod, LipsShift,
                    (int)MAType, (int)PriceType, (int)IndicatorAligatorLineType.MODE_GATORLIPS, pos);
                if (pos <= startUp)
                {
                    tmpVal = teeth - jaw;
                    if (tmpVal < 0)
                        tmpVal *= -1;
                    _upBuff[pos] = tmpVal;
                }
                if (pos <= startDn)
                {
                    tmpVal = teeth - lips;
                    if (tmpVal > 0)
                        tmpVal *= -1;
                    _lowBuff[pos] = tmpVal;
                }

                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 10)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != JawPeriod)
                return false;
            if (!(values[3] is int) || (int)values[3] != JawShift)
                return false;
            if (!(values[4] is int) || (int)values[4] != TeethPeriod)
                return false;
            if (!(values[5] is int) || (int)values[5] != TeethShift)
                return false;
            if (!(values[6] is int) || (int)values[6] != LipsPeriod)
                return false;
            if (!(values[7] is int) || (int)values[7] != LipsShift)
                return false;
            if (!(values[8] is MovingAverageType) || (MovingAverageType)values[8] != MAType)
                return false;
            if (!(values[9] is PriceConstants) || (PriceConstants)values[9] != PriceType)
                return false;

            return true;
        }
    }
}