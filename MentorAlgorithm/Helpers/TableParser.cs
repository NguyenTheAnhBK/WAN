using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorAlgorithm.Helpers
{
    public static class TableParser
    {
        public static string ToStringTable<T>(this IEnumerable<T> values, string[] columnsHeaders, params Func<T, object>[] valueSelectors)
        {
            return ToStringTable(values.ToArray(), columnsHeaders, valueSelectors);
        }

        public static string ToStringTable<T>(this Dictionary<Tuple<T, T>, double> values, string name, List<T> columnsHeader, params Func<T, string>[] columnsHeaderSelector)
        {
            var k = values.Keys;
            var v = values.Values;

            int count = columnsHeader.Count() + 1;
            string[,] matrix = new string[count, count];

            matrix[0, 0] = name;

            for (int col = 1; col < count; col++)
                matrix[0, col] = columnsHeaderSelector[0].Invoke(columnsHeader[col - 1]);
            for (int row = 1; row < count; row++)
                matrix[row, 0] = columnsHeaderSelector[0].Invoke(columnsHeader[row - 1]);

            for(int row = 1; row < count; row++)
            {
                for (int col = 1; col < count; col++)
                {
                    var tuple = Tuple.Create(columnsHeader[row - 1], columnsHeader[col - 1]);
                    if (values.ContainsKey(tuple))
                        matrix[row, col] = values[tuple].ToString("f2");
                    else
                        matrix[row, col] = "0.00";
                }
            }
            return ToStringTable(matrix);
        }

        public static string ToStringTable<T>(this T[] values, string[] columnHeaders, params Func<T, object>[] valueSelectors)
        {
            var arrValues = new string[values.Length + 1, values.Length];

            for (int col = 0; col < arrValues.GetLength(1); col++)
                arrValues[0, col] = columnHeaders[col];

            for (int row = 1; row < arrValues.GetLength(0); row++)
                for (int col = 0; col < arrValues.GetLength(1); col++)
                    arrValues[row, col] = valueSelectors[col].Invoke(values[row - 1]).ToString();

            return ToStringTable(arrValues);
        }

        public static string ToStringTable(this string[,] matrix)
        {
            int[] maxColumnsWidth = GetMaxColumnsWidth(matrix);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(len => len + 3) - 1);

            var sb = new StringBuilder();
            for(int row = 0; row < matrix.GetLength(0); row++)
            {
                for(int col = 0; col < matrix.GetLength(1); col++)
                {
                    string cell = matrix[row, col];
                    cell = cell.PadRight(maxColumnsWidth[col]); // width cell = max width cell
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                //Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                //Print splitter
                if (row == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        
        //Lấy chiều dài lớn nhất của mỗi cột (để chia chiều dài động nhìn cho nó đẹp)
        private static int[] GetMaxColumnsWidth(string[,] matrix)
        {
            int[] maxColumnsWidth = new int[matrix.GetLength(1)];
            for(int col = 0; col < matrix.GetLength(1); col++)
            {
                int maxLength = 0;
                for(int row = 0; row < matrix.GetLength(0); row++)
                    if (matrix[row, col].Length > maxLength)
                        maxLength = matrix[row, col].Length;

                maxColumnsWidth[col] = maxLength;
            }
            return maxColumnsWidth;
        }
    }
}
