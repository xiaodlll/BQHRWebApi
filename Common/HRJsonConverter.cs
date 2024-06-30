using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;

namespace BQHRWebApi.Common
{

    public class HRJsonConverter
    {
        public static string SerializeAExcludingParentInDetails<T>(T[] entities)
        {
            JArray resultArray = new JArray();
            foreach (var entity in entities)
            {
                JObject jo = new JObject();

                var properties = typeof(T).GetProperties();

                foreach (var property in properties)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                    {
                        JArray detailsArray = new JArray();
                        IEnumerable sourceCollection = (IEnumerable)property.GetValue(entity);


                        foreach (var b in sourceCollection)
                        {
                            JObject bObject = new JObject();
                            var bProperties = b.GetType().GetProperties();

                            foreach (var bProperty in bProperties)
                            {
                                if (bProperty.Name != "Parent" && bProperty.Name != "ExtendedProperties")
                                {
                                    var pValue = bProperty.GetValue(b);
                                    if (pValue != null)
                                    {
                                        bObject.Add(bProperty.Name, JToken.FromObject(pValue));
                                    }
                                }
                            }

                            detailsArray.Add(bObject);
                        }
                        if (property.Name != "ExtendedProperties")
                        {
                            jo.Add(property.Name, detailsArray);
                        }
                    }
                    else
                    {
                        if (property.Name != "Parent")
                        {
                            var pValue = property.GetValue(entity);
                            if (pValue != null)
                            {
                                jo.Add(property.Name, JToken.FromObject(pValue));
                            }
                        }
                    }
                }

                resultArray.Add(jo);
            }
            return resultArray.ToString();
        }
    }
}