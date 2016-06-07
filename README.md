## Alveo
Alveo is a real-time trading application for charting and performing technical analysis of the markets. 

To download the platform: https://apiaryfund.com/software

To request features & report bugs: https://apiaryfund.com/develop  

## Setting Up Visual Studio for Alveo Development
<!--TODO:Insert screenshots-->
1. Open the Visual Studio Solution from the repository
2. Locate the dll's
  *  The dll's can be found in the Alveo folder under C:\\Program Files\\Alveo
  *  **You may need to show hidden files in windows explorer to view the dll files**
3. Reference the dlls in the Visual Studio solution
  *  In the UserCode Solution Right click on References and select *Add Reference*
  *  Click *Browse* and navigate to the location of the dll files
  *  Select the dll files and press *Add*

## Creating an Indicator
Look at an [example](https://github.com/marlais/Alveo/blob/master/Code/Indicators/Aroon.cs) indicator.
<!--TODO: Explain basic indicator creation-->

Look at the [Wiki](/wiki) for additional information on creating an indicator

## Using Custom Indicators
<!--TODO: Add screenshots-->
Custom indicators can be used in Alveo  
1.  Open Alveo  
2.  Select *Code*  
3.  Select *Editor*  
4.  Paste the code for the indicator into the editor and press the green *build* button  
5.  Select the indicator from the indicators list  
