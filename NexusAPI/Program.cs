using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace NexusAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            string token = null;
            string url = String.Format("https://pacsci.formed.ai/api/users/getToken");
            WebRequest requestObjectGet = WebRequest.Create(url);
            requestObjectGet.Method = "POST";
            requestObjectGet.ContentType = "application/json";

            //setting username/password statically
            string postData = "{\"username\":\"admin\",\"password\":\"HelloNexus!987prod\"}";

            //login and get a token
            using (var streamWriter = new StreamWriter(requestObjectGet.GetRequestStream()))
            {
                streamWriter.Write(postData);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)requestObjectGet.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result2 = streamReader.ReadToEnd();
                    dynamic stuff = JsonConvert.DeserializeObject(result2);
                    token = stuff.responseData.token;
                }
            }


            //perform a search to find the records that are status=Complete
            string searchUrl = String.Format("https://pacsci.formed.ai/api/form-instances/search");
            WebRequest requestObjectSearch = WebRequest.Create(searchUrl);
            requestObjectSearch.PreAuthenticate = true;
            requestObjectSearch.Headers.Add("Authorization", "Bearer " + token);
            requestObjectSearch.Method = "POST";
            requestObjectSearch.ContentType = "application/json";

            //setting the field and search value to status=Complete
            string postDataSearch = "{\"code\":\"status\",\"value\":\"Complete\"}";

            using (var streamWriter = new StreamWriter(requestObjectSearch.GetRequestStream()))
            {
                streamWriter.Write(postDataSearch);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse2 = (HttpWebResponse)requestObjectSearch.GetResponse();

                using (var streamReader = new StreamReader(httpResponse2.GetResponseStream()))
                {
                    var result2 = streamReader.ReadToEnd();
                    dynamic stuff = JsonConvert.DeserializeObject(result2);

                    dynamic form = stuff.responseData.search_results.form_instances;
                    foreach (dynamic item in form.Children())
                    {
                        FindFields((int)item.id.Value, token);
                    }

                }
            }
        }
        
        static void FindFields(int formId, string token)
        {
            string formUrl = String.Format("https://pacsci.formed.ai/api/form-instances/" + formId);
            WebRequest requestObjectForm = WebRequest.Create(formUrl);
            requestObjectForm.PreAuthenticate = true;
            requestObjectForm.Headers.Add("Authorization", "Bearer " + token);
            requestObjectForm.Method = "GET";
            requestObjectForm.ContentType = "application/json";

            HttpWebResponse responseObjectGet = null;
            responseObjectGet = (HttpWebResponse)requestObjectForm.GetResponse();
            string result = null;

            using (Stream stream = responseObjectGet.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                result = sr.ReadToEnd();
                sr.Close();

                dynamic stuff = JsonConvert.DeserializeObject(result);

                dynamic form = stuff.responseData.FormInstance.form.section_properties;
                foreach (dynamic item in form.Children())
                {
                    string value = null;
                    string formId2 = item.id;
                    var lines = new List<string>();
                    dynamic res = item.responses;
                    string field = item.property.label;
                    
                    if (res.Count > 1)
                    {
                        foreach (dynamic line in res.Children())
                        {
                            lines.Add(line.value.Value);
                        }
                        for (int i = 0; i < lines.Count; i++)
                        {
                            Console.WriteLine("form id:" + formId + ", " + "line item field: " + field);
                            Console.WriteLine($"line # {i} = {lines[i]}");
                        }
                    }
                    else
                    {
                        value = res.First.value;
                        Console.WriteLine("form id:" + formId + ", summary field:" + field + ": " + value);
                    }
                }
            }
        }
    }
}
