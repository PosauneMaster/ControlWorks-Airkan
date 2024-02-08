using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Models
{
    public class RecipeAddedEventArgs : EventArgs
    {
        public string ReferenceNumber { get; set; }
        public int SuccessCode { get; set; }
        public string Message { get; set; }

        public RecipeAddedEventArgs()
        {
        }

        public RecipeAddedEventArgs(string referenceNumber, int successCode, string message)
        {
            ReferenceNumber = referenceNumber;
            SuccessCode = successCode;
            Message = message;
        }


    }
}
