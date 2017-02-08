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
    [Description("Chaikin Money Flow")]
    public class CMF : IndicatorBase
    {
        #region Properties

        private readonly Array<double> cmfBuffer;

        #endregion


        public CMF()
        {
            // Basic indicator initialization. Don't use this constructor to calculate values

            indicator_buffers = 1;
            periods = 20;

            indicator_levelcolor = Colors.Red;
            indicator_color1 = Colors.Red;

            cmfBuffer = new Array<double>();

            copyright = "Apiary Fund LLC";
            link = "https://apiaryfund.com";
            indicator_separate_window = true;
        }

        //+------------------------------------------------------------------+");
        //| Custom indicator initialization function                         |");
        //+------------------------------------------------------------------+");
        protected override int Init()
        {
            // ENTER YOUR CODE HERE
            SetIndexBuffer(0, cmfBuffer);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexLabel(0, string.Format("CMF({0})", periods));
            IndicatorShortName(string.Format("CMF({0})", periods));
            SetIndexDrawBegin(0, periods);
            return 0;
        }


        //+-------- EXTERNAL PARAMETERS HERE --------+
        [Category("Settings")]
        [DisplayName("Periods")]
        public int periods { get; set; }


        //+------------------------------------------------------------------+");
        //| Custom indicator deinitialization function                       |");
        //+------------------------------------------------------------------+");
        protected override int Deinit()
        {
            // ENTER YOUR CODE HERE
            return 0;
        }

        //+------------------------------------------------------------------+");
        //| Custom indicator iteration function                              |");
        //+------------------------------------------------------------------+");
        protected override int Start()
        {
            int counted_bars = IndicatorCounted();
            // ENTER YOUR CODE HERE
            if (counted_bars <= periods)
            {
                return 0;
            }
            double moneyFlowMultiplier;
            double[] moneyFlowVolume = new double[counted_bars];
            double moneyFlowVolumeSum = 0;
            double volumeSum = 0;
            double cmf;
            for (int i = counted_bars - 1; i >= 0; i--)
            {
                moneyFlowVolume[i] = (Close[i] == High[i] && Close[i] == Low[i] || High[i] == Low[i]) ? 0 : ((2 * Close[i] - Low[i] - High[i]) / (High[i] - Low[i])) * Volume[i];
                moneyFlowVolumeSum = 0;
                volumeSum = 0;
                if (i <= (counted_bars - periods))
                {
                    for (int j = 0; j < periods; j++)
                    {
                        moneyFlowVolumeSum += moneyFlowVolume[i + j];
                        volumeSum += Volume[i + j];
                    }
                    cmf = moneyFlowVolumeSum / volumeSum;
                    cmfBuffer[i] = cmf;
                }
            }
            return 0;
        }

        //+------------------------------------------------------------------+
        //| AUTO GENERATED CODE. THIS METHODS USED FOR INDICATOR CACHING     |
        //+------------------------------------------------------------------+
        #region Auto Generated Code

        [Description("Parameters order Symbol, TimeFrame")]
        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 2)
                return false;

            if (!CompareString(Symbol, (string)values[0]))
                return false;

            if (TimeFrame != (int)values[1])
                return false;

            return true;
        }

        [Description("Parameters order Symbol, TimeFrame")]
        public override void SetIndicatorParameters(params object[] values)
        {
            if (values.Length != 2)
                throw new ArgumentException("Invalid parameters number");

            Symbol = (string)values[0];
            TimeFrame = (int)values[1];

        }

        #endregion
    }
}

