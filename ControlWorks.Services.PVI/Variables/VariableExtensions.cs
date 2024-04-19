using System.Diagnostics;

using BR.AN.PviServices;

namespace ControlWorks.Services.PVI.Variables
{
    public static class VariableExtensions
    {
        public static bool AssignChecked(this Variable variable, object value)
        {
            try
            {
                variable.Value.Assign(value);
                return true;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError($"Error assigning value {value} to Variable {variable.Name}\r\nVariable data type: {variable.Value.IECDataType}," +
                                 $" value data type: {value.GetType().Name}.\r\nException message: {ex.Message}");
            }
            return false;
        }
    }
}
