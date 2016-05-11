using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Larry William's percent range Indicator")]
    public class WPR : IndicatorBase
    {
        private readonly Array<double> _vals;

        public WPR()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("WPR({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("WPR({0})", IndicatorPeriod));

            _vals = new Array<double>();
        }

        [Description("Period of the WPR Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("WPR({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("WPR({0})", IndicatorPeriod));
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod)
                pos = Bars - IndicatorPeriod;
            var data = GetHistory(Symbol, TimeFrame);
            if (data.Count == 0)
                return 0;
            while (pos >= 0)
            {
                var maxH = data[pos].High;
                var minL = data[pos].Low;

                for (var i = 1; i < IndicatorPeriod; i++)
                {
                    if (data[pos + i].High > maxH)
                        maxH = data[pos + i].High;
                    if (data[pos + i].Low < minL)
                        minL = data[pos + i].Low;
                }
                if ((maxH - minL).Equals(0))
                    _vals[pos] = 0;
                else
                    _vals[pos] = (double)((maxH - data[pos].Close)/(maxH - minL))*-100;

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