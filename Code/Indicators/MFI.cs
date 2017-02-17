using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Money Flow Index Indicator")]
    public class MFI : IndicatorBase
    {
        private readonly Array<double> _vals;
        private readonly Array<double> _level1;
        private readonly Array<double> _level2;

        public MFI()
        {
            indicator_buffers = 3;
            indicator_chart_window = false;
            indicator_color1 = Colors.Blue;
            indicator_color2 = Colors.Green;
            indicator_color3 = Colors.Red;

            IndicatorPeriod = 10;
            Level1 = 80;
            Level2 = 20;
            SetIndexLabel(0, string.Format("MFI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("MFI({0})", IndicatorPeriod));

            _vals = new Array<double>();
            _level1 = new Array<double>();
            _level2 = new Array<double>();
        }

        [Description("Period of the MFI Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Overbought level")]
        [Category("Settings")]
        [DisplayName("Level 1")]
        public double Level1 { get; set; }


        [Description("Oversold level")]
        [Category("Settings")]
        [DisplayName("Level 2")]
        public double Level2 { get; set; }


        protected override int Init()
        {
            SetIndexLabel(0, string.Format("MFI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("MFI({0})", IndicatorPeriod));
            SetIndexBuffer(0, _vals);

            SetIndexBuffer(1, _level1);
            SetIndexLabel(1, "Level 1");
            SetIndexBuffer(2, _level2);
            SetIndexLabel(2, "Level 2");

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

            double dPositiveMF;
            double dNegativeMF;
            double dCurrentTP;
            double dPreviousTP;
            int j;

            while (pos > 0)
            {
                dPositiveMF = 0;
                dNegativeMF = 0;
                dCurrentTP = (High[pos] + Low[pos] + Close[pos]) / 3;
                for (j = 0; j < IndicatorPeriod; j++)
                {
                    dPreviousTP = (High[pos + j + 1] + Low[pos + j + 1] + Close[pos + j + 1]) / 3;
                    if (dCurrentTP > dPreviousTP)
                        dPositiveMF += data[pos + j].Volume * dCurrentTP;
                    else
                    {
                        if (dCurrentTP < dPreviousTP)
                            dNegativeMF += Volume[pos + j] * dCurrentTP;
                    }
                    dCurrentTP = dPreviousTP;
                }
                //----
                if (dNegativeMF != 0)
                    _vals[pos] = 100 - 100 / (1 + dPositiveMF / dNegativeMF);
                else
                    _vals[pos] = 100;
                //----
                _level1[pos] = Level1;
                _level2[pos] = Level2;
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

