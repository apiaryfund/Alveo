using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Elder's Force Index")]
    public class EFI : IndicatorBase
    {
        private readonly Array<double> ForceBuffer;

        public EFI()
        {
            indicator_chart_window = false;
            indicator_buffers = 1;
            indicator_color1 = Colors.DodgerBlue;

            ForcePeriod = 13;
            MAMethod = 0;
            AppliedPrice = 0;

            ForceBuffer = new Array<double>();
        }

        [Category("Settings")]
        [Description("Force Period")]
        [DisplayName("Foce Period")]
        public int ForcePeriod { get; set; }

        [Category("Settings")]
        [Description("MA Method")]
        [DisplayName("MA Method")]
        public int MAMethod { get; set; }

        [Category("Settings")]
        [Description("Applied Price")]
        [DisplayName("Applied Price")]
        public int AppliedPrice { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, ForceBuffer);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexLabel(0, string.Format("EFI({0})", ForcePeriod));
            SetIndexDrawBegin(0, ForcePeriod);

            IndicatorShortName(string.Format("EFI({0})", ForcePeriod));

            return 0;
        }

        protected override int Start()
        {
            int pos = IndicatorCounted();

            if (Bars <= ForcePeriod)
                return 0;

            if (pos > ForcePeriod)
                pos--;

            int limit = Bars - pos;

            for (int i = 0; i < limit; i++)
                ForceBuffer[i] = Volume[i] * (iMA(null, 0, ForcePeriod, 0, MAMethod, AppliedPrice, i) -
                                 iMA(null, 0, ForcePeriod, 0, MAMethod, AppliedPrice, i + 1));

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
            if (!(values[2] is int) || (int)values[2] != ForcePeriod)
                return false;
            if (!(values[3] is int) || (int)values[3] != MAMethod)
                return false;
            if (!(values[4] is int) || (int)values[4] != AppliedPrice)
                return false;
            return true;
        }
    }
}
