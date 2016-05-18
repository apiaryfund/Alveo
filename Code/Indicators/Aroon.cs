using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
  [Serializable]
  [Description("Aroon Indicator")]
  public class Aroon : IndicatorBase
  {

    #region Properties

    //Decalare Global Variables
    private readonly Array<double> upBuffer;
    private readonly Array<double> downBuffer;

    #endregion

    public Aroon()
    {
      //Basic indicator initialization
      indicator_buffers = 2;
      indicator_chart_window = false;

      indicator_minimum = 0;
      indicator_maximum = 100;
      indicator_level1 = 30;
      indicator_level2 = 70;

      //External Variables
      period = 14;

      indicator_color1 = Colors.Red;
      indicator_color2 = Colors.Green;

      upBuffer = new Array<double>();
      downBuffer = new Array<double>();

      IndicatorShortName(string.Format("Aroon({0})", period));

    }

    //Decalare External Values
    [Category("Settings")]
    [DisplayName("priceber of Periods")]
    public int period {get; set;}

    //+------------------------------------------------------------------+");
    //| Custom indicator initialization function                         |");
    //+------------------------------------------------------------------+");
    protected override int Init()
    {

        //Set attributes for each chart buffer
        SetIndexStyle(0, DRAW_LINE);
        SetIndexBuffer(0, upBuffer);
        SetIndexLabel(0, "UP");
        SetIndexDrawBegin(0, period);

        SetIndexStyle(1, DRAW_LINE);
        SetIndexBuffer(1, downBuffer);
        SetIndexLabel(1, "DOWN");
        SetIndexDrawBegin(1, period);

        return 0;
    }

    //+------------------------------------------------------------------+");
    //| Custom indicator deinitialization function                       |");
    //+------------------------------------------------------------------+");
    protected override int Deinit()
    {
        return 0;
    }

    //+------------------------------------------------------------------+");
    //| Custom indicator iteration function                              |");
    //+------------------------------------------------------------------+");
    protected override int Start()
    {
      int i = 0;
      int pos = IndicatorCounted();
      if(Bars <= period)
        return 0;

      if(pos < 1)
      {
        for(i = 1; i <= period ; i++)
        {
          upBuffer[Bars - i] = 0;
          downBuffer[Bars - i] = 0;
        }
      }

      i = Bars - period - 1;
      if(pos >= period)
        i = Bars - pos - 1;
      int high = i;
      int low = i;
      while(i >= 0)
      {
        double max = -100000;
        double min = 100000;
        for(int k = i; k < (i + period); k++)
        {
          double price = Close[k];
          if(price > max)
          {
            max = price;
            high = k;
          }
          if(price < min){
            min = price;
            low = k;
          }
        }

        upBuffer[i]= 100*(period - (high - i))/period;
        downBuffer[i] = 100*(period - (low - i))/period;
        i--;
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
        if (!(values[2] is int) || (int)values[3] != period)
            return false;

        return true;
    }
  }
}
