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

    public class PriceCycle : IndicatorBase
    {

        private Array<double> bullishPrice;
        private Array<double> bearishPrice;
        private Array<double> bothPrice;

        private Dictionary<int, double> pivotHighs;
        private Dictionary<int, double> pivotLows;

        private Array<double> bullishPips;
        private Array<double> bearishPips;
        private Array<double> bothPips;


        public PriceCycle()
        {
            indicator_buffers = 0;
            indicator_chart_window = false;

            timeDiff = 3;
            pipDiff = 0.8;
            period = 3;
            useBullPrice = true;
            useBearPrice = false;
            useBothPrice = false;
            bullColor = Colors.Green;
            bearColor = Colors.Blue;
            bothColor = Colors.Red;


            pivotHighs = new Dictionary<int, double>();
            pivotLows = new Dictionary<int, double>();
        }


        //Declare Our Price Cycle Values
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
        public bool useBullPrice { get; set; }

        [Category("Settings")]
        [DisplayName("Bearish")]
        public bool useBearPrice { get; set; }

        [Category("Settings")]
        [DisplayName("Combined")]
        public bool useBothPrice { get; set; }

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
            bullishPips = new Array<double>();
            bearishPips = new Array<double>();
            bothPips = new Array<double>();
            for (int i = 0; i < indicator_buffers; i++)
            {
                SetIndexBuffer(i, null);
            }
            indicator_buffers = 0;
            if (useBullPrice)
            {
                indicator_buffers++;
                bullishPrice = new Array<double>();
                SetIndexStyle(indicator_buffers - 1, DRAW_SECTION, STYLE_SOLID, 1, bullColor);
                SetIndexBuffer(indicator_buffers - 1, bullishPrice);
                SetIndexLabel(indicator_buffers - 1, "Bull Price Cycle");
            }
            if (useBearPrice)
            {
                indicator_buffers++;
                bearishPrice = new Array<double>();
                SetIndexStyle(indicator_buffers - 1, DRAW_SECTION, STYLE_SOLID, 1, bearColor);
                SetIndexBuffer(indicator_buffers - 1, bearishPrice);
                SetIndexLabel(indicator_buffers - 1, "Bear Price Cycle");
            }
            if (useBothPrice)
            {
                indicator_buffers++;
                bothPrice = new Array<double>();
                SetIndexStyle(indicator_buffers - 1, DRAW_SECTION, STYLE_SOLID, 1, bothColor);
                SetIndexBuffer(indicator_buffers - 1, bothPrice);
                SetIndexLabel(indicator_buffers - 1, "Combined Price Cycle");
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

            bullishPips.Clear();
            bearishPips.Clear();
            bothPips.Clear();

            try
            {
                //Calculate Price And Time Cycles
                int bullishIndex = 0;
                int bearishIndex = 0;
                int bothIndex = 0;
                for (int x = Bars - 1; x >= pos; x--)
                {
                    //Price Cycles
                    while (useBothPrice)
                    {
                        if (bothPips.Count < period) break;

                        if (bothPips.Count % period != 0) break;

                        double sum = 0.0;
                        for (int f = bothPips.Count - period; f < bothPips.Count; f++)
                        {
                            sum += bothPips[f];
                        }

                        double average = sum / period;
                        int v = bothIndex;
                        while (v >= x)
                        {
                            bothPrice[v] = average;
                            v--;
                        }

                        break;
                    }
                    while (true)
                    {
                        if (pivotLows[x] == EMPTY_VALUE) break;

                        if (bullishPips.Count % period == 0) bullishIndex = x;

                        int y = x;
                        while (y >= 0 && pivotHighs[y] == EMPTY_VALUE)
                        {
                            y--;
                        }

                        if (pivotHighs[y] == EMPTY_VALUE) break;

                        double val = Math.Abs(pivotHighs[y] - pivotLows[x]);
                        bullishPips.Add(val);
                        if (useBothPrice)
                        {
                            if (bothPips.Count % period == 0) bothIndex = x;
                            bothPips.Add(val);
                        }

                        if (!useBullPrice) break;

                        if (bullishPips.Count < period) break;

                        if (bullishPips.Count % period != 0) break;

                        double sum = 0.0;
                        for (int f = bullishPips.Count - period; f < bullishPips.Count; f++)
                        {
                            sum += bullishPips[f];
                        }

                        double average = sum / period;
                        int v = bullishIndex;
                        while (v >= y)
                        {
                            bullishPrice[v] = average;
                            v--;
                        }

                        break;
                    }
                    while (true)
                    {
                        if (pivotHighs[x] == EMPTY_VALUE) break;

                        if (bearishPips.Count % period == 0) bearishIndex = x;

                        int y = x;
                        while (y >= 0 && pivotLows[y] == EMPTY_VALUE)
                        {
                            y--;
                        }

                        if (pivotLows[y] == EMPTY_VALUE) break;

                        double val = Math.Abs(pivotHighs[x] - pivotLows[y]);
                        bearishPips.Add(val);
                        if (useBothPrice)
                        {
                            if (bothPips.Count % period == 0) bothIndex = x;
                            bothPips.Add(val);
                        }

                        if (!useBearPrice) break;

                        if (bearishPips.Count < period) break;

                        if (bearishPips.Count % period != 0) break;

                        double sum = 0.0;
                        for (int f = bearishPips.Count - period; f < bearishPips.Count; f++)
                        {
                            sum += bearishPips[f];
                        }

                        double average = sum / period;
                        int v = bearishIndex;
                        while (v >= y)
                        {
                            bearishPrice[v] = average;
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
