using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Chaikin Money Flow")]
    public class CMF : IndicatorBase
    {
        private readonly Array<double> cmfBuffer;

        public CMF()
        {
            indicator_chart_window = false;
            indicator_buffers = 1;
            indicator_level1 = 0;
            indicator_levelstyle = STYLE_DOT;
            indicator_levelcolor = Colors.Red;
            indicator_color1 = Colors.Red;
            cmfBuffer = new Array<double>();
            periods = 20;
        }

        [Category("Settings")]
        [Description("Periods")]
        [DisplayName("Periods")]
        public int periods { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, cmfBuffer);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexLabel(0, string.Format("CMF({0})", periods));
            IndicatorShortName(string.Format("CMF({0})", periods));
            SetIndexDrawBegin(0, periods);
            return 0;
        }

        protected override int Start()
        {
            int shift = 0;
            int limit = 0;
            int pos = IndicatorCounted();
            if (Bars <= periods)
                return 0;
            if (pos < 1)
                for (int i = 1; i <= periods; i++)
                    cmfBuffer[Bars - i] = 0;
            if (pos > 0)
                limit = Bars - pos;
            if (pos == 0)
                limit = Bars - periods - 1;
            for (shift = limit; shift >= 0; shift--)
            {
                double sum = 0;
                double volume = 0;
                for(int i = 0; i < periods - 1; i++)
                {
                    volume += Volume[shift + i];
                    if (High[shift + i] - Low[shift + i] > 0)
                        sum += Volume[shift + i] * (Close[shift + i] - Open[shift + i]) / (High[shift + i] - Low[shift + i]);
                }
                cmfBuffer[shift] = sum / volume;
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
            if (!(values[2] is int) || (int)values[2] != periods)
                return false;

            return true;
        }
    }
}
