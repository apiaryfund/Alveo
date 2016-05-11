using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bill Williams Market Facilitation Index Indicator")]
    public class BWMFI : IndicatorBase
    {
        private readonly Array<double> _mfiDownVDown;
        private readonly Array<double> _mfiDownVUp;
        private readonly Array<double> _mfiupVDown;
        private readonly Array<double> _mfiupVUp;
        private readonly Array<double> _vals;

        public BWMFI()
        {
            _vals = new Array<double>();
            _mfiupVUp = new Array<double>();
            _mfiDownVDown = new Array<double>();
            _mfiupVDown = new Array<double>();
            _mfiDownVUp = new Array<double>();

            indicator_buffers = 5;
            indicator_chart_window = false;

            SetIndexLabel(0, "BWMFI");
            indicator_color1 = Colors.Black;
            SetIndexLabel(1, "Up_Up");
            indicator_color2 = Colors.Lime;
            SetIndexLabel(2, "Down_Down");
            indicator_color3 = Colors.SaddleBrown;
            SetIndexLabel(3, "Up_Down");
            indicator_color4 = Colors.Blue;
            SetIndexLabel(4, "Down_Up");
            indicator_color5 = Colors.Pink;
            indicator_width1 = 0;
            indicator_width2 = 2;
            indicator_width3 = 2;
            indicator_width4 = 2;
            indicator_width5 = 2;

            IndicatorShortName("BWMFI");
        }

        protected override int Init()
        {
            indicator_color1 = Colors.Transparent;
            SetIndexBuffer(0, _vals);
            SetIndexBuffer(1, _mfiupVUp);
            SetIndexBuffer(2, _mfiDownVDown);
            SetIndexBuffer(3, _mfiupVDown);
            SetIndexBuffer(4, _mfiDownVUp);

            SetIndexStyle(0, DRAW_NONE);
            SetIndexStyle(1, DRAW_HISTOGRAM);
            SetIndexStyle(2, DRAW_HISTOGRAM);
            SetIndexStyle(3, DRAW_HISTOGRAM);
            SetIndexStyle(4, DRAW_HISTOGRAM);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            if (pos >= Bars)
                pos = Bars - 1;

            var data = GetHistory(Symbol, TimeFrame);
            if (data.Count == 0)
                return 0;

            while (pos >= 0)
            {
                if (data[pos].Volume.Equals(0))
                {
                    if (pos == Bars - 1)
                        _vals[pos] = ErrorResult;
                    else
                        _vals[pos] = _vals[pos + 1];
                }
                else
                    _vals[pos] = 100000*(double)(data[pos].High - data[pos].Low)/(data[pos].Volume);
                pos--;
            }

            pos = Bars - IndicatorCounted();
            if (pos >= Bars)
                pos = Bars - 1;
            bool bMfiUp = true, bVolUp = true;
            if (pos < Bars - 1)
            {
                if (_mfiupVUp[pos + 1].Equals(0))
                {
                    bMfiUp = true;
                    bVolUp = true;
                }
                if (_mfiDownVDown[pos + 1].Equals(0))
                {
                    bMfiUp = false;
                    bVolUp = false;
                }
                if (_mfiupVDown[pos + 1].Equals(0))
                {
                    bMfiUp = true;
                    bVolUp = false;
                }
                if (_mfiDownVUp[pos + 1].Equals(0))
                {
                    bMfiUp = false;
                    bVolUp = true;
                }
            }
            pos--;
            while (pos >= 0)
            {
                if (_vals[pos] > _vals[pos + 1])
                    bMfiUp = true;
                if (_vals[pos] < _vals[pos + 1])
                    bMfiUp = false;
                if (Volume[pos] > Volume[pos + 1])
                    bVolUp = true;
                if (Volume[pos] < Volume[pos + 1])
                    bVolUp = false;

                if (bMfiUp && bVolUp)
                {
                    _mfiupVUp[pos] = _vals[pos];
                    _mfiDownVDown[pos] = 0.0;
                    _mfiupVDown[pos] = 0.0;
                    _mfiDownVUp[pos] = 0.0;
                }
                else if (!bMfiUp && !bVolUp)
                {
                    _mfiupVUp[pos] = 0.0;
                    _mfiDownVDown[pos] = _vals[pos];
                    _mfiupVDown[pos] = 0.0;
                    _mfiDownVUp[pos] = 0.0;
                }
                else if (bMfiUp && !bVolUp)
                {
                    _mfiupVUp[pos] = 0.0;
                    _mfiDownVDown[pos] = 0.0;
                    _mfiupVDown[pos] = _vals[pos];
                    _mfiDownVUp[pos] = 0.0;
                }
                else if (!bMfiUp && bVolUp)
                {
                    _mfiupVUp[pos] = 0.0;
                    _mfiDownVDown[pos] = 0.0;
                    _mfiupVDown[pos] = 0.0;
                    _mfiDownVUp[pos] = _vals[pos];
                }
                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 2)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;

            return true;
        }
    }
}