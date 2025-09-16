using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Exceptions
{
    public class CustomException : Exception
    {
        public ResultMessage Result;
        public CustomException(int code, string message)
        {
            Result = new ResultMessage(code, message);
        }
        public CustomException(Errors errors)
        {
            Result = new ResultMessage(errors);
        }
    }
}
