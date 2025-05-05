using BR.AN.PviServices;

namespace ControlWorks.Services.PVI
{
    public static class Utils
    {
        public static string FormatPviEventMessage(string message, PviEventArgs e)
        {
            if (e != null)
            {
                return
                    $"{message}; Action={e.Action}, Address={e.Address}, Error Code={e.ErrorCode}, Error Text={e.ErrorText}, Name={e.Name} ";
            }
            return
                $"{message}";
        }

    }
}
