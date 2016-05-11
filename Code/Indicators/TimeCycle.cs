using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{

    public class TimeCycle : IndicatorBase
    {

        private Array<double> bullishTime;
        private Array<double> bearishTime;
        private Array<double> bothTime;

        private Dictionary<int, double> pivotHighs;
        private Dictionary<int, double> pivotLows;

        private Array<double> bullishBars;
        private Array<double> bearishBars;
        private Array<double> bothBars;

        public TimeCycle()
        {
            indicator_buffers = 0;
            indicator_chart_window = false;

            timeDiff = 3;
            pipDiff = 0.8;
            period = 3;

            useBullTime = true;
            useBearTime = false;
            useBothTime = false;
            bullColor = Colors.Green;
            bearColor = Colors.Blue;
            bothColor = Colors.Red;

            pivotHighs = new Dictionary<int, double>();
            pivotLows = new Dictionary<int, double>();
        }


        //Declare Our Time Cycle Values
        [Category("Settings")]
        [DisplayName("Time Variation")]
        public int timeDiff { get; set; }

        [Category("Settings")]
        [DisplayName("Pip Variation")]
        public double pipDiff { get; set; }

        [Category("Settings")]
        [DisplayName("Period")]
        public int period { get; set; }

        [Category("Settings")]
        [DisplayName("Bullish")]
        public bool useBullTime { get; set; }

        [Category("Settings")]
        [DisplayName("Bearish")]
        public bool useBearTime { get; set; }

        [Category("Settings")]
        [DisplayName("Combined")]
        public bool useBothTime { get; set; }

        [Category("Settings")]
        [DisplayName("Bullish Color")]
        public Color bullColor { get; set; }

        [Category("Settings")]
        [DisplayName("Bearish Color")]
        public Color bearColor { get; set; }

        [Category("Settings")]
        [DisplayName("Combined Color")]
        public Color bothColor { get; set; }

        protected override int Init()
        {
            bullishBars = new Array<double>();
            bearishBars = new Array<double>();
            bothBars = new Array<double>();
            for (int i = 0; i < indicator_buffers; i++)
            {
                SetIndexBuffer(i, null);
            }
            indicator_buffers = 0;
            if (useBullTime)
            {
                indicator_buffers++;
                bullishTime = new Array<double>();
                SetIndexStyle(indicator_buffers - 1, DRAW_SECTION, STYLE_SOLID, 1, bullColor);
                SetIndexBuffer(indicator_buffers - 1, bullishTime);
                SetIndexLabel(indicator_buffers - 1, "Bull Time Cycle");
            }
            if (useBearTime)
            {
                indicator_buffers++;
                bearishTime = new Array<double>();
                SetIndexStyle(indicator_buffers - 1, DRAW_SECTION, STYLE_SOLID, 1, bearColor);
                SetIndexBuffer(indicator_buffers - 1, bearishTime);
                SetIndexLabel(indicator_buffers - 1, "Bear Time Cycle");
            }
            if (useBothTime)
            {
                indicator_buffers++;
                bothTime = new Array<double>();
                SetIndexStyle(indicator_buffers - 1, DRAW_SECTION, STYLE_SOLID, 1, bothColor);
                SetIndexBuffer(indicator_buffers - 1, bothTime);
                SetIndexLabel(indicator_buffers - 1, "Combined Time Cycle");
            }
            return 0;
        }

        protected override int Deinit()
        {
            // ENTER YOUR CODE HERE
            return 0;
        }

        protected override int Start()
        {

            int pos = Bars - 1;
            int lastPos = 0;
            while (pos >= 0)
            {
                pivotHighs[pos] = iPivots(Symbol, TimeFrame, pipDiff, timeDiff, MODE_UPPER, pos);
                pivotLows[pos] = iPivots(Symbol, TimeFrame, pipDiff, timeDiff, MODE_LOWER, pos);
                if (pivotHighs[pos] != EMPTY_VALUE || pivotLows[pos] != EMPTY_VALUE) lastPos = pos;
                pos--;
            }
            
            bullishBars.Clear();
            bearishBars.Clear();
            bothBars.Clear();

            try
            {
                //Calculate Time Cycles
                int bullishTimeStart = 0;
                int bearishTimeStart = 0;
                int bothTimeStart = 0;
                for (int x = Bars - 1; x >= pos; x--)
                {
                    //Time Cycles
                    while (useBothTime)
                    {
                        if (bothBars.Count < period) break;

                        if (bothBars.Count % period != 0) break;

                        double sum = 0.0;
                        for (int f = bothBars.Count - period; f < bothBars.Count; f++)
                        {
                            sum += bothBars[f];
                        }

                        double average = sum / period;
                        int v = bothTimeStart;
                        while (v >= x)
                        {
                            bothTime[v] = average;
                            v--;
                        }

                        break;
                    }
                    while (true)
                    {
                        if (pivotLows[x] == EMPTY_VALUE) break;

                        if (bullishBars.Count % period == 0) bullishTimeStart = x;

                        int y = x;
                        while (y >= 0 && pivotHighs[y] == EMPTY_VALUE)
                        {
                            y--;
                        }

                        if (pivotHighs[y] == EMPTY_VALUE) break;

                        double val = Math.Abs(x - y);
                        bullishBars.Add(val);
                        if (useBothTime)
                        {
                            if (bothBars.Count % period == 0) bothTimeStart = x;
                            bothBars.Add(val);
                        }

                        if (!useBullTime) break;

                        if (bullishBars.Count < period) break;

                        if (bullishBars.Count % period != 0) break;

                        double sum = 0.0;
                        for (int f = bullishBars.Count - period; f < bullishBars.Count; f++)
                        {
                            sum += bullishBars[f];
                        }

                        double average = sum / period;
                        int v = bullishTimeStart;
                        while (v >= y)
                        {
                            bullishTime[v] = average;
                            v--;
                        }

                        break;
                    }
                    while (true)
                    {
                        if (pivotHighs[x] == EMPTY_VALUE) break;

                        if (bearishBars.Count % period == 0) bearishTimeStart = x;

                        int y = x;
                        while (y >= 0 && pivotLows[y] == EMPTY_VALUE)
                        {
                            y--;
                        }

                        if (pivotLows[y] == EMPTY_VALUE) break;

                        double val = Math.Abs(x - y);
                        bearishBars.Add(val);
                        if (useBothTime)
                        {
                            if (bothBars.Count % period == 0) bothTimeStart = x;
                            bothBars.Add(val);
                        }

                        if (!useBearTime) break;

                        if (bearishBars.Count < period) break;

                        if (bearishBars.Count % period != 0) break;

                        double sum = 0.0;
                        for (int f = bearishBars.Count - period; f < bearishBars.Count; f++)
                        {
                            sum += bearishBars[f];
                        }

                        double average = sum / period;
                        int v = bearishTimeStart;
                        while (v >= y)
                        {
                            bearishTime[v] = average;
                            v--;
                        }

                        break;
                    }

                }
            }
            catch (Exception e)
            {
            }
            return 0;
        }


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
    }
}
