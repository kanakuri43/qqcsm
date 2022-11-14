using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace qqcs
{
    public class CsvOperator
    {
        public void OutputCsv(DataTable dt, string path, bool writeHeader = true, string separator = ",")
        {
            Encoding enc = Encoding.GetEncoding("Shift_JIS");

            System.IO.StreamWriter sr = new System.IO.StreamWriter(path, false, enc);

            int colCount = dt.Columns.Count;
            int lastColIndex = colCount - 1;

            if (writeHeader)
            {
                for (int i = 0; i < colCount; i++)
                {
                    string field = dt.Columns[i].Caption;
                    field = EncloseDoubleQuotesIfNeed(field);
                    sr.Write(field);
                    if (lastColIndex > i)
                    {
                        sr.Write(',');
                    }
                }

                sr.Write("\r\n");
            }

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < colCount; i++)
                {
                    string field = row[i].ToString();
                    field = EncloseDoubleQuotesIfNeed(field);
                    sr.Write(field);
                    if (lastColIndex > i)
                    {
                        sr.Write(',');
                    }
                }

                sr.Write("\r\n");
            }
            sr.Close();
        }

        private string EncloseDoubleQuotesIfNeed(string field)
        {
            if (NeedEncloseDoubleQuotes(field))
            {
                return EncloseDoubleQuotes(field);
            }
            return field;
        }

        private string EncloseDoubleQuotes(string field)
        {
            if (field.IndexOf('"') > -1)
            {
                field = field.Replace("\"", "\"\"");
            }
            return "\"" + field + "\"";
        }

        private bool NeedEncloseDoubleQuotes(string field)
        {
            return field.IndexOf('"') > -1 ||
                field.IndexOf(',') > -1 ||
                field.IndexOf('\r') > -1 ||
                field.IndexOf('\n') > -1 ||
                field.StartsWith(" ") ||
                field.StartsWith("\t") ||
                field.EndsWith(" ") ||
                field.EndsWith("\t");
        }
    }
}
