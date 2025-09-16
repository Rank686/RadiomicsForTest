using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.Exceptions
{
    public enum Errors
    {
        [Description("成功")]
        OK = 0,
        [Description("系统运行错误")]
        SystemError = 100,
        [Description("图像不是灰度图")]
        NotGrayImage = 101,
        [Description("重采样参数必须大于0")]
        ResampleParamsError = 102,
        [Description("范围过滤参数错误")]
        RangeFilterParamsError = 103,
        [Description("直方图数据处理错误")]
        HistogramDataError = 104,
        [Description("ROI数据错误")]
        ROIDataError = 105,
        [Description("调用参数错误")]
        ParamsError = 106,
        [Description("滤波器错误")]
        WaveFilterError = 500,
    }

    static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var field = val.GetType().GetField(val.ToString());
            var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if (customAttribute == null) { return val.ToString(); }
            else { return ((DescriptionAttribute)customAttribute).Description; }
        }
    }
}
