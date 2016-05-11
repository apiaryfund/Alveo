using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bill Williams' Alligator Indicator")]
    public class Alligator : IndicatorBase
    {
        private readonly Array<double> _jawBuff;
        private readonly Array<double> _lipsBuff;
        private readonly Array<double> _teethBuff;

        public Alligator()
        {
            indicator_buffers = 3;
            indicator_chart_window = true;

            JawPeriod = 13;
            JawShift = 8;
            TeethPeriod = 8;
            TeethShift = 5;
            LipsPeriod = 5;
            LipsShift = 3;
            MAType = MovingAverageType.MODE_SMA;
            PriceType = PriceConstants.PRICE_MEDIAN;

            SetIndexLabel(0, string.Format("Jaw({0},{1})", JawPeriod, JawShift));
            indicator_color1 = Colors.Blue;
            SetIndexLabel(1, string.Format("Teeth({0},{1})", TeethPeriod, TeethShift));
            indicator_color2 = Colors.Red;
            SetIndexLabel(2, string.Format("Lips({0},{1})", LipsPeriod, LipsShift));
            indicator_color3 = Colors.Lime;
            IndicatorShortName("Alligator");

            _jawBuff = new Array<double>();
            _teethBuff = new Array<double>();
            _lipsBuff = new Array<double>();
        }

        [Description("Jaw Period of the Alligator Indicator")]
        [Category("Settings")]
        [DisplayName("Jaw Period")]
        public int JawPeriod { get; set; }

        [Description("Jaw Shift of the Alligator Indicator")]
        [Category("Settings")]
        [DisplayName("Jaw Shift")]
        public int JawShift { get; set; }

        [Description("Teeth Period of the Alligator Indicator")]
        [Category("Settings")]
        [DisplayName("Teeth Period")]
        public int TeethPeriod { get; set; }

        [Description("Teeth Shift of the Alligator Indicator")]
        [Category("Settings")]
        [DisplayName("Teeth Shift")]
        public int TeethShift { get; set; }

        [Description("Lips Period of the Alligator Indicator")]
        [Category("Settings")]
        [DisplayName("Lips Period")]
        public int LipsPeriod { get; set; }

        [Description("Lips Shift of the Alligator Indicator")]
        [Category("Settings")]
        [DisplayName("Lips Shift")]
        public int LipsShift { get; set; }

        [Description("Moving Average type on witch Alligator will be calculated")]
        [Category("Settings")]
        [DisplayName("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Description("Price type on witch Alligator will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("Jaw({0},{1})", JawPeriod, JawShift));
            SetIndexLabel(1, string.Format("Teeth({0},{1})", TeethPeriod, TeethShift));
            SetIndexLabel(2, string.Format("Lips({0},{1})", LipsPeriod, LipsShift));

            IndicatorShortName("Alligator");

            SetIndexBuffer(0, _jawBuff);
            SetIndexBuffer(1, _teethBuff);
            SetIndexBuffer(2, _lipsBuff);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos >= Bars)
                pos = Bars - 1;

            while (pos >= 0)
            {
                _jawBuff[pos] = iMA(Symbol, TimeFrame, JawPeriod, JawShift, (int)MAType, (int)PriceType, pos);
                _teethBuff[pos] = iMA(Symbol, TimeFrame, TeethPeriod, TeethShift, (int)MAType, (int)PriceType, pos);
                _lipsBuff[pos] = iMA(Symbol, TimeFrame, LipsPeriod, LipsShift, (int)MAType, (int)PriceType, pos);
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