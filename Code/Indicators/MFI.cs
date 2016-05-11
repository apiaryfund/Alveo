using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Money Flow Index Indicator")]
    public class MFI : IndicatorBase
    {
        private readonly Array<double> _vals;

        public MFI()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("MFI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("MFI({0})", IndicatorPeriod));

            _vals = new Array<double>();
        }

        [Description("Period of the MFI Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("MFI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("MFI({0})", IndicatorPeriod));
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod - 1)
                pos = Bars - IndicatorPeriod - 1;

            var data = GetHistory(Symbol, TimeFrame);

            if (data.Count == 0)
                return 0;

            decimal dPositiveMF;
            decimal dNegativeMF;
            decimal dCurrentTP;
            decimal dPreviousTP;
            int j;

            while (pos >= 0)
            {
                dPositiveMF = 0;
                dNegativeMF = 0;
                dCurrentTP = (data[pos].High + data[pos].Low + data[pos].Close)/3;
                for (j = 0; j < IndicatorPeriod; j++)
                {
                    dPreviousTP = (data[pos + j + 1].High + data[pos + j + 1].Low + data[pos + j + 1].Close)/3;
                    if (dCurrentTP > dPreviousTP)
                        dPositiveMF += data[pos + j].Volume*dCurrentTP;
                    else
                    {
                        if (dCurrentTP < dPreviousTP)
                            dNegativeMF += data[pos + j].Volume*dCurrentTP;
                    }
                    dCurrentTP = dPreviousTP;
                }
                //----
                if (!dNegativeMF.Equals(0.0))
                    _vals[pos] = 100 - 100/(double)(1 + dPositiveMF/dNegativeMF);
                else
                    _vals[pos] = 100;
                //----
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