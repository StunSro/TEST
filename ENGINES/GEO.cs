using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;

public class GEO
{
    public static string GetGeo(string IP)
    {

        string key = "63bd212b1ac08ceb56ed2e6a353b47770ac1a9ba14331a17101a8c45c1539333"; //replace with your actual key. 
        string url = "http://api.ipinfodb.com/v3/ip-city/?format=xml&key=" + key + "&ip=";

        HttpWebResponse res = null;

        try
        {
            var req = WebRequest.Create(url + IP) as HttpWebRequest;

            XmlDocument xob = null;

            res = req.GetResponse() as HttpWebResponse;
            if (req.HaveResponse == true && res != null)
            {
                xob = new XmlDocument();
                xob.Load(res.GetResponseStream());
            }
            else return "Unknown";

            if (xob != null)
            {
                var x = xob.SelectSingleNode("Response");
                if (x != null)
                {
                    CountryName = x["countryName"].InnerText;
                }
            }
            else return "Unknown";

        }
        catch { return "Unknown"; }
        finally
        {
            if (res != null)
            { res.Close(); }
        }
        return CountryName;

    }

    //access the following properties after a successful lookup.
    public static String CountryName { get; set; }

}