using System.Data;
using System.Collections;
using System.ComponentModel;
using System;

namespace Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// convert ienumerable to dataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable data, string tableName = "")
        {
            if (data == null)
            {
                return null;
            }

            PropertyDescriptorCollection properties = null;
            var type = typeof(T);

            properties = TypeDescriptor.GetProperties(type);

            if (properties == null || properties.Count == 0)
            {
                return null;
            }
            DataTable dt = new DataTable(tableName);
            foreach (PropertyDescriptor prop in properties)
            {
                if (prop != null)
                {
                    dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }
            foreach (T item in data)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? null;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
