using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Radiomics.Net.Exceptions;
using Radiomics.Net.Features;
using RadiomicsNet.Features;

namespace Radiomics.Net
{
    public class ResultMessage
    {
        //是否成功
        public bool IsSuccess { get; set; }
        //错误码
        public int ErrorCode { get; set; }
        //错误信息
        public string? ErrorMessage { get; set; }

        public ResultMessage(int code,string message)
        {
            ErrorCode = code;
            ErrorMessage = message;
        }
        public ResultMessage(Errors error)
        {
            ErrorCode = (int)error;
            ErrorMessage = error.GetDescription();
            if (error.Equals(Errors.OK)) 
            {
                IsSuccess = true;
            }
        }
    }
}
