using CancelXML;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Xml;

class Program
{
    static async Task Main(string[] args)
    {
        XmlDocument xmlDoc = new XmlDocument();
        string file = string.Empty;
        try
        {
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlNamespaceManager nsm = new XmlNamespaceManager(xmlDoc.NameTable);
            nsm.AddNamespace("", "http://cancelacfd.sat.gob.mx");
            nsm.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            nsm.AddNamespace("TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital");

            XmlNode cancel = xmlDoc.CreateNode(XmlNodeType.Element, "Cancelacion", nsm.DefaultNamespace);
            

            XmlNode folio = xmlDoc.CreateNode(XmlNodeType.Element, "Folio", nsm.DefaultNamespace);

            XmlAttribute attribute = xmlDoc.CreateAttribute("Fecha");
            attribute.Value = DateTime.Now.ToString("s");
            folio.Attributes.Append(attribute);

            XmlAttribute attributeRcf = xmlDoc.CreateAttribute("RFCEmisor");
            attributeRcf.Value = System.Configuration.ConfigurationManager.AppSettings["RFCEmisor"];
            folio.Attributes.Append(attributeRcf);

            string[] files = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings["XMLPath"], "*.xml", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                file = f;
                XmlDocument original = new XmlDocument();
                original.Load(f);
                var uuid = (XmlAttribute?)original.SelectSingleNode("//tfd:TimbreFiscalDigital/@UUID", nsm);
                if (uuid != null)
                {
                    XmlNode folios = xmlDoc.CreateNode(XmlNodeType.Element, "Folios", nsm.DefaultNamespace);
                    
                    XmlAttribute attributefolio = xmlDoc.CreateAttribute("FolioSustitucion");
                    attributefolio.Value = string.Empty;
                    folios.Attributes.Append(attributefolio);

                    XmlAttribute attributemotivo = xmlDoc.CreateAttribute("Motivo");
                    attributemotivo.Value = "02";
                    folios.Attributes.Append(attributemotivo);

                    XmlAttribute attributeUUID = xmlDoc.CreateAttribute("UUID");
                    attributeUUID.Value = uuid.Value;
                    folios.Attributes.Append(attributeUUID);

                    folio.AppendChild(folios);
                }

            }
            xmlDoc.AppendChild(folio);
            xmlDoc.Save(Path.Combine(System.Configuration.ConfigurationManager.AppSettings["XMLProcesedPath"] ?? string.Empty, Path.GetFileName(file)));
        }
        catch (Exception ex)
        {
            string message = ex.Message + " " + ex.StackTrace;
            Log.WriteLog(message);
            xmlDoc.Save(Path.Combine(System.Configuration.ConfigurationManager.AppSettings["XMLFailedPath"], Path.GetFileName(file)));
        }
    }

    static async Task MakeRequest(XmlDocument xmlDoc)
    {
        var client = new HttpClient();
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        // Request headers
        client.DefaultRequestHeaders.Add("licenseId", System.Configuration.ConfigurationManager.AppSettings["licence"]);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", System.Configuration.ConfigurationManager.AppSettings["cancelKey"]);

        var uri = System.Configuration.ConfigurationManager.AppSettings["cancelUrl"] + queryString;

        HttpResponseMessage response;

        // Request body
        byte[] byteData = Encoding.UTF8.GetBytes(xmlDoc.OuterXml);

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(uri, content);
        }

    }
}
