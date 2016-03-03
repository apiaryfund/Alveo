using System;
using System.ComponentModel;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Volume indicator")]
    public class Volume : IndicatorBase
    {
        #region Properties

        #endregion

        private readonly Array<double> _volume = new Array<double>();

        public Volume()
        {
            indicator_buffers = 1;

            indicator_separate_window = true;
            SetIndexStyle(0, DRAW_HISTOGRAM, STYLE_SOLID, 4, "#0000FF");
        }

        protected override int Init()
        {
            SetIndexBuffer(0, _volume);
            SetIndexLabel(0, Chart.Symbol + "_Volume");
            return 0;
        }

        protected override int Deinit()
        {
            return 0;
        }

        protected override int Start()
        {
            var countedBars = IndicatorCounted();
            for (var i = countedBars; i < Bars; i++)
                _volume[i, false] = Volume[i, false];
            return 0;
        }

        #region Indicator parameters check

        [Description("Parameters order Symbol, TimeFrame")]
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

                var val1 = (int)values[1];
                if (val1 != TimeFrame)
                    return false;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("IsSameParameters", ex);
            }

            return true;
        }

        [Description("Parameters order Symbol, TimeFrame")]
        public override void SetIndicatorParameters(params object[] values)
        {
            if (values.Length != 2)
                return;

            try
            {
                Symbol = (string)values[0];
                TimeFrame = (int)values[1];
            }
            catch (Exception ex)
            {
                Logger.ErrorException("SetIndicatorParameters", ex);
            }
        }

        #endregion
    }
}