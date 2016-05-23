using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Price Volume Trend")]
    public class PVT : IndicatorBase
    {
        private readonly Array<double> pvtBuffer;

        public PVT()
        {
            indicator_chart_window = false;
            indicator_buffers = 1;
            indicator_color1 = Colors.DodgerBlue;

            appliedPrice = 0;
            pvtBuffer = new Array<double>();
        }

        [Category("Settings")]
        [Description("PVT Applied Price")]
        [DisplayName("PVT Applied Price")]
        public int appliedPrice { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, pvtBuffer);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexLabel(0, "PVT");
            IndicatorDigits(0);
            IndicatorShortName("PVT");

            return 0;
        }

        protected override int Start()
        {
            int i = 0;
            int limit = 0;
            int pos = IndicatorCounted();
            double currentPrice = 0;
            double previousPrice = 0;

            if (pos > 0)
                pos--;

            limit = Bars - pos - 1;

            for (i = limit; i >= 0; i--)
            {
                if (i == Bars - 1)
                    pvtBuffer[i] = Volume[i];
                else
                {
                    currentPrice = getAppliedPrice(appliedPrice, i);
                    previousPrice = getAppliedPrice(appliedPrice, i + 1);
                    pvtBuffer[i] = pvtBuffer[i + 1] + Volume[i] * (currentPrice - previousPrice) / previousPrice;
                }
            }

            return 0;
        }

        private double getAppliedPrice(int appliedPrice, int index)
        {
            double price = 0;
            switch(appliedPrice)
            {
                case 0: price = Close[index]; break;
                case 1: price = Open[index]; break;
                case 2: price = High[index]; break;
                case 3: price = Low[index]; break;
                case 4: price = (High[index] + Low[index]) / 2.0; break;
                case 5: price = (High[index] + Low[index] + Close[index]) / 3.0; break;
                case 6: price = (High[index] + Low[index] + 2 * Close[index]) / 4.0; break;
                default: price = 0; break;
            }
            return price;
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
            if (!(values[2] is int) || (int)values[2] != appliedPrice)
                return false;
            return true;
        }

    }
}
