using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Demo.DataAccess
{
    static class Extensions
    {
        public static List<T> GetEntityList<T>(DataTable data) where T : class, new()
        {
            List<T> result = new List<T>();

            if (data == null)
            {
                return result;
            }

            if (data.Rows.Count == 0)
            {
                return result;
            }

            foreach (DataRow row in data.Rows)
            {
                T newInstance = new T();
                LoadEntityFromDataRow<T>(newInstance, row);
                result.Add(newInstance);
            }

            return result;
        }

        /// <summary>
        /// Obtiene un objeto cargado con base en un dataRow
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="targetEntity"></param>
        /// <param name="data"></param>
        public static void LoadEntityFromDataRow<T1>(T1 targetEntity, DataRow data)
        {
            foreach (var property in targetEntity.GetType().GetProperties())
            {
                if (property != null && property.CanWrite)
                {
                    if (data.Table.Columns.Contains(property.Name))
                    {
                        if (data[property.Name] != System.DBNull.Value)
                        {
                            object propertyValue = null;

                            if (property.PropertyType == typeof(DateTime?))
                                propertyValue = Convert.ToDateTime(data[property.Name]);
                            else if (property.PropertyType == typeof(int?))
                                propertyValue = Convert.ToInt32(data[property.Name]);
                            else if (property.PropertyType.BaseType.FullName.Equals("System.Enum"))
                            {
                                propertyValue = (object)System.Enum.Parse(property.PropertyType, data[property.Name].ToString());
                            }
                            else
                                propertyValue = System.Convert.ChangeType(
                                       data[property.Name],
                                       property.PropertyType
                                   );
                            property.SetValue(targetEntity, propertyValue, null);
                        }
                    }
                }
            }
        }

        //public T LoadEntityFromDataReader<T>(SqlDataReader reader) where T : class, new()
        //{
        //    T instance = new T();

        //    foreach (var property in typeof(T).GetProperties())
        //    {
        //        if (property.PropertyType == typeof(DateTime) ||
        //          property.PropertyType == typeof(DateTime?) ||
        //          property.PropertyType == typeof(int) ||
        //          property.PropertyType == typeof(string) ||
        //          property.PropertyType == typeof(decimal) ||
        //          property.PropertyType == typeof(decimal?) ||
        //          property.PropertyType == typeof(bool) ||
        //          property.PropertyType == typeof(double?) ||
        //          property.PropertyType == typeof(double))
        //            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
        //            {
        //                Type convertTo = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        //                property.SetValue(instance, Convert.ChangeType(reader[property.Name], convertTo), null);
        //            }
        //    }
        //    return instance;
        //}

    }
}
