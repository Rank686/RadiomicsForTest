using Radiomics.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Radiomics.Net.ImageProcess
{
    public class WaveletFilters
    {

        /**
         * Represent decomposition and reconstrution low-pass and high-pass filters.
         */
        public double[] dlf, dhf, rlf, rhf;

        /**
         * Reads paremeters of wavelet filters from corresponding files.
         * @param filterName - Name of the filter file.
         * @return String - Content of the file.
         */
        private String GetText(String filterName)
        {
            String text = "";
            try
            {
                // get the text resource as a stream
                ResourceManager manager = new ResourceManager("Radiomics.Net.Resource1", Assembly.GetExecutingAssembly());
                byte[] bytes = (byte[])manager.GetObject(filterName);
                if (bytes == null)
                {
                    throw new CustomException((int)Errors.WaveFilterError,"未找到该滤波器:" + filterName);
                }
                MemoryStream isr = new MemoryStream(bytes);
                StreamReader sr = new StreamReader(isr);
                StringBuilder sb = new StringBuilder();
                string? content;
                //read a block and append any characters
                while (true)
                {
                    content = sr.ReadLine();
                    if (content != null)
                    {
                        sb.AppendLine(content);
                    }
                    else 
                    {
                        break;
                    }
                }
                text = sb.ToString();
            }
            catch (IOException e)
            {
                throw new CustomException((int)Errors.WaveFilterError, "滤波器读取参数错误:" + filterName);
            }
            return text;
        }

        public void SetFilter(String filterName)
        {
            filterName= filterName.Split('.')[0];
            String content = GetText(filterName);
            String[] lines;

            if (content.Length>0)
                lines = content.Split("\r\n");
            else
                return;

            int filterSize = (lines.Length - 3) / 4;

            dlf = new double[filterSize];
            dhf = new double[filterSize];
            rlf = new double[filterSize];
            rhf = new double[filterSize];

            int filterNumber = 0;
            int filterIndex = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length==0)
                {
                    filterIndex = 0;
                    filterNumber++;
                }
                else
                {
                    switch (filterNumber)
                    {
                        case 0:
                            dlf[filterIndex] = Double.Parse(lines[i]);
                            break;
                        case 1:
                            dhf[filterIndex] = Double.Parse(lines[i]);
                            break;
                        case 2:
                            rlf[filterIndex] = Double.Parse(lines[i]);
                            break;
                        case 3:
                            rhf[filterIndex] = Double.Parse(lines[i]);
                            break;
                    }
                    filterIndex++;
                }
            }
        }
    }
}
