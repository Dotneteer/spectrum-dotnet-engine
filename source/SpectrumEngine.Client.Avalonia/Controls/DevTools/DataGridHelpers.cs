using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Helpers for datagrid
/// </summary>
public static class DataGridHelpers
{
    /// <summary>
    /// Obtain the current viewport information of the specified data grid control
    /// </summary>
    /// <param name="dataGrid">DataGrid to obtain its viewport information</param>
    /// <param name="itemsCount">Total number of items in the data grid</param>
    /// <returns></returns>
    public static (int Top, int Height) GetViewportInfo(this DataGrid dataGrid, int itemsCount)
    {
        var vBar = dataGrid.GetType().GetField("_vScrollBar", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var vBarValue = vBar == null ? null : (ScrollBar)vBar.GetValue(dataGrid)!;
        var valueProp = vBarValue?.GetType().GetTypeInfo().GetProperty("Value");
        if (valueProp == null) return (0, 0);
        var barValue = (double)valueProp.GetValue(vBarValue)!;
        var maxValueProp = vBarValue?.GetType().GetTypeInfo().GetProperty("Maximum");
        var barMaxValue = (double)maxValueProp?.GetValue(vBarValue)! + dataGrid.Bounds.Height;
        var index = (int)(barValue / barMaxValue * itemsCount);
        var height = (int)(dataGrid.Bounds.Height / dataGrid.RowHeight);
        return (index, height);
    }
}