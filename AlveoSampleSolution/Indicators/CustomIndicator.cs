//References to include
using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
  public class CustomIndicator : IndicatorBase
  {

    //Decalare Global Variables
    //The buffer is used to store the values of the indicator
    //The index of the buffer marks the value at that bar with the zero index being the most recent bar
    private readonly Array<double> HighLowBuffer;
    private readonly Array<double> OpenCloseBuffer;

    //Initialize variables in constructor
    public CustomIndicator()
    {
      //Basic indicator initialization

    
      indicator_buffers = 2; //Number of buffers the indicator will use (i.e number of lines drawn by indicator). There can be more than one buffer
      indicator_chart_window = false; // True if indicator will draw on chart window, false if indicator will be in seperate window

      minBars = 14;

      indicator_color1 = Colors.Red;
      indicator_color2 = Colors.Green;

      HighLowBuffer = new Array<double>();
      OpenCloseBuffer = new Array<double>();


    }

    //Decalare External Values
    //These are values that can be changed in the indicator settings
    [Category("Settings")]
    [DisplayName("priceber of Periods")]
    public int minBars {get; set;}

    //+------------------------------------------------------------------+");
    //| Custom indicator initialization function                         |");
    //+------------------------------------------------------------------+");
    //This is called once when the indicator is first added to the chart
    protected override int Init()
    {
        SetIndexBuffer(0, HighLowBuffer); //Set the first buffer
        SetIndexStyle(0, DRAW_LINE); //Set the style for the buffer, this will draw the values as a line
        SetIndexLabel(0, "HIGH"); //Set the label for the buffer
        SetIndexDrawBegin(0, minBars); //Set the start index for the buffer

        SetIndexStyle(1, DRAW_LINE);
        SetIndexBuffer(1, OpenCloseBuffer);
        SetIndexLabel(1, "LOW");
        SetIndexDrawBegin(1, minBars);

        return 0;
    }

    //+------------------------------------------------------------------+");
    //| Custom indicator deinitialization function                       |");
    //+------------------------------------------------------------------+");
    //This is called when the indicator is removed from the chart
    protected override int Deinit()
    {
        return 0;
    }

    //+------------------------------------------------------------------+");
    //| Custom indicator iteration function                              |");
    //+------------------------------------------------------------------+");
    //This is called on every new tick
    protected override int Start()
    {
      int totalBars = Bars; //Bars is the number of bars on the chart. (Bars - 1) is also the index of the oldest bar on the chart
      int i = 0;
      int pos = IndicatorCounted(); //IndicatorCounted is the number of bars on the chart

      if (totalBars <= minBars) //If there are fewer bars than our specified value then nothing is drawn
      {
          return 0;
      }

      //Here we iterate through all the bars on the chart
      for (i = 0; i < totalBars; i++)
      {

          HighLowBuffer[i] = High[i] - Low[i]; //High[i] gets the high of the specified bar and Low[i] gets the Low of the specified bar
          OpenCloseBuffer[i] = Open[i] - Close[i]; //Open[i] gets the open of the specified bar and Close[i] gets the close of the specified bar
      }
      return 0;
    }
  }
}
