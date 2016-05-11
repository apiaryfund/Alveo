using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Relative Vigor index Indicator")]
    public class RVI : IndicatorBase
    {
        private readonly Array<double> _signalVals;
        private readonly Array<double> _vals;

        public RVI()
        {
            indicator_buffers = 2;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            indicator_color2 = Colors.Blue;

            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("RVI({0})", IndicatorPeriod));
            SetIndexLabel(1, "Signal");
            IndicatorShortName(string.Format("RVI({0})", IndicatorPeriod));

            _vals = new Array<double>();
            _signalVals = new Array<double>();
        }

        [Description("Period of the RVI Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("RVI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("RVI({0})", IndicatorPeriod));

            SetIndexBuffer(0, _vals);
            SetIndexBuffer(1, _signalVals);

            return 0;
        }

        protected override int Start()
        {
            if (Bars < IndicatorPeriod)
                return 0;

            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod - 3)
                pos = Bars - IndicatorPeriod - 3;

            while (pos >= 0)
            {
                var dNum = 0.0;
                var dDeNum = 0.0;
                double dValueUp;
                double dValueDown;
                int j;

                for (var i = 0; i < IndicatorPeriod; i++)
                {
                    j = pos + i;
                    dValueUp = ((Close[j] - Open[j]) + 2*(Close[j + 1] - Open[j + 1]) + 2*(Close[j + 2] - Open[j + 2])
                                + (Close[j + 3] - Open[j + 3]))/6;
                    dValueDown = ((High[j] - Low[j]) + 2*(High[j + 1] - Low[j + 1]) + 2*(High[j + 2] - Low[j + 2])
                                  + (High[j + 3] - Low[j + 3]))/6;
                    dNum += dValueUp;
                    dDeNum += dValueDown;
                }

                if (!dDeNum.Equals(0.0))
                    _vals[pos] = dNum/dDeNum;
                else
                    _vals[pos] = dNum;

                pos--;
            }

            pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod - 6)
                pos = Bars - IndicatorPeriod - 6;
            while (pos >= 0)
            {
                _signalVals[pos] = (_vals[pos] + 2*_vals[pos + 1] + 2*_vals[pos + 2] + _vals[pos + 3])/6;
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
            if (!(values[2] is int) || (int)values[2] != IndicatorPeriod)
                return false;

            return true;
        }
    }
}