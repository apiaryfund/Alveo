using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Detrended Price Oscillator")]
    public class DPO : IndicatorBase
    {

        private readonly Array<double> dpoBuffer;

        public DPO()
        {
            x_prd = 14;
            CountBars = 300;

            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            dpoBuffer = new Array<double>();
        }

        //Decalare External Variables
        [Category("Settings")]
        [DisplayName("X Period")]
        public int x_prd { get; set; }

        [Category("Settings")]
        [DisplayName("Number of Bars")]
        public int CountBars { get; set; }

        //+------------------------------------------------------------------+");
        //| Custom indicator initialization function                       |");
        //+------------------------------------------------------------------+");
        protected override int Init()
        {
            IndicatorShortName(string.Format("DPO({0})", x_prd));
            SetIndexStyle(0, DRAW_LINE);
            SetIndexBuffer(0, dpoBuffer, false);
            SetIndexLabel(0, string.Format("DPO({0})", x_prd));
            if (CountBars >= Bars)
                CountBars = Bars;
            int startIndex = x_prd + (x_prd / 2);
            SetIndexDrawBegin(0, Bars - CountBars + startIndex);

            return 0;
        }

        //+------------------------------------------------------------------+");
        //| Custom indicator iteration function                              |");
        //+------------------------------------------------------------------+");
        protected override int Start()
        {
            int i = 0;
            int pos = IndicatorCounted();
            int t_prd = 0;

            if (Bars <= x_prd)
                return 0;

            if (pos < x_prd)
            {
                for (i = 1; i <= x_prd; i++)
                    dpoBuffer[CountBars - i] = 0;
            }

            i = CountBars - x_prd - 1;
            t_prd = x_prd / 2 + 1;
            while (i >= 0)
            {
                dpoBuffer[i] = Close[i] - iMA(null, 0, x_prd, t_prd, MODE_SMA, PRICE_CLOSE, i);
                i--;
            }
            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 4)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || !x_prd.Equals((int)values[2]))
                return false;
            if (!(values[3] is int) || !CountBars.Equals((int)values[3]))
                return false;

            return true;
        }
    }
}
