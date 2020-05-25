using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorAlgorithm.Helpers
{
    public static class Helper
    {
        public static int Evaluate(string expression)
        {
            System.Data.DataTable table = new System.Data.DataTable();
            return Convert.ToInt32(table.Compute(expression, String.Empty));
        }
    }
}
