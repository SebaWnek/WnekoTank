using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace WnekoTankControlApp.CommandControl
{
    abstract class OxyModel
    {
        public PlotModel DataModel { get; set; }
        public abstract void AddPoint(string name, double time, double[] values);
    }
    class ElectricPlotModel : OxyModel
    {
        LineSeries voltageC;
        LineSeries voltageL;
        LineSeries voltageR;
        LineSeries currentC;
        LineSeries currentL;
        LineSeries currentR;
        public ElectricPlotModel()
        {
            DataModel = new PlotModel { Title = "Electric data" };

            LinearAxis primaryAxis = new LinearAxis();
            primaryAxis.Position = AxisPosition.Left;
            primaryAxis.Title = "Voltage";
            primaryAxis.Key = "V";
            primaryAxis.Maximum = 20;
            primaryAxis.Minimum = 0;

            LinearAxis secondaryAxis = new LinearAxis();
            secondaryAxis.Position = AxisPosition.Right;
            secondaryAxis.Title = "Current";
            secondaryAxis.Key = "A";
            //secondaryAxis.Maximum = 10;
            secondaryAxis.Minimum = 0;

            DataModel.Axes.Add(primaryAxis);
            DataModel.Axes.Add(secondaryAxis);

            DataModel.LegendPlacement = LegendPlacement.Outside;

            voltageC = new LineSeries();
            voltageL = new LineSeries();
            voltageR = new LineSeries();
            currentC = new LineSeries();
            currentL = new LineSeries();
            currentR = new LineSeries();

            voltageC.YAxisKey = "V";
            voltageL.YAxisKey = "V";
            voltageR.YAxisKey = "V";
            currentC.YAxisKey = "A";
            currentL.YAxisKey = "A";
            currentR.YAxisKey = "A";

            voltageC.Title = "Central voltage";
            voltageL.Title = "Left voltage";
            voltageR.Title = "Right voltage";
            currentC.Title = "Central current";
            currentL.Title = "Left current";
            currentR.Title = "Right current";

            //voltageC.MarkerType = MarkerType.Cross;
            //voltageL.MarkerType = MarkerType.Diamond;
            //voltageR.MarkerType = MarkerType.Circle;
            //currentC.MarkerType = MarkerType.Plus;
            //currentL.MarkerType = MarkerType.Star;
            //currentR.MarkerType = MarkerType.Triangle;

            voltageC.LineStyle = LineStyle.Dash;
            voltageL.LineStyle = LineStyle.Dash;
            voltageR.LineStyle = LineStyle.Dash;
            currentC.LineStyle = LineStyle.Dot;
            currentL.LineStyle = LineStyle.Dot;
            currentR.LineStyle = LineStyle.Dot;

            voltageC.StrokeThickness = 0.5;
            voltageL.StrokeThickness = 0.5;
            voltageR.StrokeThickness = 0.5;
            currentC.StrokeThickness = 0.5;
            currentL.StrokeThickness = 0.5;
            currentR.StrokeThickness = 0.5;

            voltageC.Color = OxyColors.Blue;
            voltageL.Color = OxyColors.Green;
            voltageR.Color = OxyColors.Red;
            currentC.Color = OxyColors.CadetBlue;
            currentL.Color = OxyColors.LightGreen;
            currentR.Color = OxyColors.IndianRed;

            DataModel.Series.Add(voltageC);
            DataModel.Series.Add(voltageL);
            DataModel.Series.Add(voltageR);
            DataModel.Series.Add(currentC);
            DataModel.Series.Add(currentL);
            DataModel.Series.Add(currentR);
        }

        public override void AddPoint(string name, double time, double[] values)
        {
            switch (name)
            {
                case "C":
                    voltageC.Points.Add(new DataPoint(time, values[0]));
                    currentC.Points.Add(new DataPoint(time, values[1]));
                    break;
                case "L":
                    voltageL.Points.Add(new DataPoint(time, values[0]));
                    currentL.Points.Add(new DataPoint(time, values[1]));
                    break;
                case "R":
                    voltageR.Points.Add(new DataPoint(time, values[0]));
                    currentR.Points.Add(new DataPoint(time, values[1]));
                    break;
                default:
                    throw new ArgumentException("Invalid name!");
            }
        }
    }
}
