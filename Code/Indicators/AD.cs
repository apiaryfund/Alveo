using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Common.Classes;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Accumulation/Distribution Indicator")]
    public class AD : IndicatorBase
    {
        private readonly Array<double> _vals;

        public AD()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            SetIndexLabel(0, "AD");
            IndicatorShortName("AD");
            _vals = new Array<double>();
        }

        protected override int Init()
        {
            SetIndexLabel(0, "AD");
            IndicatorShortName("AD");
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos >= Bars)
                pos = Bars - 1;

            var baseBars = GetHistory(Symbol, TimeFrame);
            if (baseBars.Count == 0)
                return 0;
            double value;
            double prevValue = 0;
            double tr;
            Bar currentBar;
            if (pos < Bars - 1)
                prevValue = _vals[pos + 1];

            while (pos >= 0)
            {
                currentBar = baseBars[pos];
                value = (double)(currentBar.Close - currentBar.Low) - (double)(currentBar.High - currentBar.Close);
                if (!value.Equals(0))
                {
                    tr = (double)(currentBar.High - currentBar.Low);
                    if (tr.Equals(0))
                        value = 0;
                    else
                    {
                        value /= tr;
                        value *= currentBar.Volume;
                    }
                }
                value += prevValue;
                _vals[pos] = value;
                prevValue = value;
                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 2)
                return false;

            try
            {
                var val0 = (string)values[0];
                if (!string.IsNullOrEmpty(val0) && !string.IsNullOrEmpty(Symbol))
                {
                    if (!Symbol.Equals(val0, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else if (!(string.IsNullOrEmpty(Symbol) && string.IsNullOrEmpty(val0)))
                    return false;

                if ((int)values[1] != TimeFrame)
                    return false;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("IsSameParameters", ex);
            }

            return true;
        }
    }
}