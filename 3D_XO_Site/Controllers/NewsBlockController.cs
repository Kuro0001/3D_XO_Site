using Microsoft.AspNetCore.Mvc;
using _3D_XO_Site.Models;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Web;

namespace _3D_XO_Site.Controllers
{
    public class NewsBlockController : Controller
    {
        // GET: NewsBlock
        public ActionResult Index()
        {
            return View(LoadFromAPI());
        }

        // GET: NewsBlock/Index_News_Feed
        public ActionResult Index_News_Feed()
        {
            return View(LoadFromAPI(5));
        }

        // GET: NewsBlock/Create
        public ActionResult Create()
        {
            return View();
        }
        // POST: NewsBlock/Create
        [HttpPost]
        public ActionResult Create(NewsBlock newsBlock)
        {
            if (ModelState.IsValid)
            {
                newsBlock.Date = DateTime.Now;
                HttpStatusCode code = SendToAPI(newsBlock);
                switch (code)
                {
                    case HttpStatusCode.NoContent:
                        {
                            return RedirectToAction("Index");
                        }
                    case HttpStatusCode.OK:
                        {
                            return RedirectToAction("Index");
                        }
                    default:
                        {
                            return BadRequest();
                        }
                }
            }
            return View();
        }

        // GET: NewsBlock/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            else
            {
                NewsBlock block = LoadFromAPI(1, id);
                if (block == null)
                    return NotFound();
                else
                    return View(block);
            }
        }
        // GET: NewsBlock/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit(NewsBlock newsBlock)
        {
            if (ModelState.IsValid)
            {
                HttpStatusCode code = SendToAPIToAppdate(newsBlock);
                switch (code)
                {
                    case HttpStatusCode.NoContent:
                        {
                            return RedirectToAction("Index");
                        }
                    case HttpStatusCode.OK:
                        {
                            return RedirectToAction("Index");
                        }
                    default:
                        {
                            return BadRequest();
                        }
                }
            }
            return View(newsBlock);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            else
            {
                NewsBlock block = LoadFromAPI(1, id);
                if (block == null)
                    return NotFound();
                else
                    return View(block);
            }
        }
        // POST: Clients/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            HttpStatusCode code = DeleteFromAPI(id);
            switch (code)
            {
                case HttpStatusCode.NoContent:
                    {
                        return RedirectToAction("Index");
                    }
                default:
                    {
                        return BadRequest();
                    }
            }
        }
        // GET: NewsBlock/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            else
            {
                NewsBlock block = LoadFromAPI(1, id);
                if (block == null)
                    return NotFound();
                else
                    return View(block);
            }
        }


        public List<NewsBlock> LoadFromAPI()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}/newsblock", Settings.url));
            request.Accept = "application/xml";
            request.Method = "GET";
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            WebResponse response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            doc.Load(response.GetResponseStream());
            XmlElement root = doc.DocumentElement;
            List<NewsBlock> listNB = new List<NewsBlock>();
            foreach (XmlElement node in root)
            {
                NewsBlock block = new NewsBlock()
                {
                    Id = Convert.ToInt32(node["Id"].InnerText),
                    Title = node["Title"].InnerText,
                    Text = node["Text"].InnerText,
                    Date = Convert.ToDateTime(node["Date"].InnerText)
                };
                listNB.Add(block);
            }
            return listNB;
        }
        public List<NewsBlock> LoadFromAPI(int pageLength)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}/newsblock/{1}", Settings.url, pageLength.ToString()));
            request.Accept = "application/xml";
            request.Method = "GET";
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            WebResponse response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            doc.Load(response.GetResponseStream());
            XmlElement root = doc.DocumentElement;
            List<NewsBlock> listNB = new List<NewsBlock>();
            foreach (XmlElement node in root)
            {
                NewsBlock block = new NewsBlock()
                {
                    Id = Convert.ToInt32(node["Id"].InnerText),
                    Title = node["Title"].InnerText,
                    Text = node["Text"].InnerText,
                    Date = Convert.ToDateTime(node["Date"].InnerText)
                };
                listNB.Add(block);
            }
            return listNB;
        }

        public NewsBlock LoadFromAPI(int pageLength, int? id)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}/newsblock/oneblock/{1}", Settings.url, id.ToString()));
            request.Accept = "application/xml";
            request.Method = "GET";
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            WebResponse response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            doc.Load(response.GetResponseStream());
            XmlElement root = doc.DocumentElement;
            NewsBlock resultBlock = new NewsBlock()
            {
                    Id = Convert.ToInt32(root["Id"].InnerText),
                    Title = root["Title"].InnerText,
                    Text = root["Text"].InnerText,
                    Date = Convert.ToDateTime(root["Date"].InnerText)
                };
            return resultBlock;
        }
        private HttpStatusCode SendToAPI(NewsBlock note)
        {
            //формируем xml документ
            XmlDocument doc = new XmlDocument();
            doc.CreateXmlDeclaration("1.0", "utf-8", "no");
            XmlElement workNode = PackToXml(doc, note);
            doc.AppendChild(workNode);
            //получаем строку в формате XML
            string xml = doc.OuterXml;
            //преобразуем строку в массив байт
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            //создаем http запрос, указываем метод и тип содержимого
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}/newsblock", Settings.url));

            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);

            request.Method = "POST";
            request.ContentType = "application/xml; encoding=utf-8";
            request.ContentLength = byteArray.Length;
            //в поток (в тело) запроса записываем подготовленный массив байт
            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            reqStream.Close();
            //отправляем запрос и получаем ответ
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode;
        }
        private HttpStatusCode SendToAPIToAppdate(NewsBlock note)
        {
            //формируем xml документ
            XmlDocument doc = new XmlDocument();
            doc.CreateXmlDeclaration("1.0", "utf-8", "no");
            XmlElement workNode = PackToXml(doc, note);
            doc.AppendChild(workNode);
            //получаем строку в формате XML
            string xml = doc.OuterXml;
            //преобразуем строку в массив байт
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            //создаем http запрос, указываем метод и тип содержимого
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}/newsblock/{1}", Settings.url, note.Id.ToString()));

            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);

            request.Method = "PUT";
            request.ContentType = "application/xml; encoding=utf-8";
            request.ContentLength = byteArray.Length;
            //в поток (в тело) запроса записываем подготовленный массив байт
            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            reqStream.Close();
            //отправляем запрос и получаем ответ
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode;
        }
        private XmlElement PackToXml(XmlDocument doc, NewsBlock note)
        {
            XmlElement workNode = doc.CreateElement("NewsBlock");

            XmlElement IdNode = doc.CreateElement("Id");
            IdNode.InnerText = note.Id.ToString();
            workNode.AppendChild(IdNode);

            XmlElement Node1 = doc.CreateElement("Title");
            Node1.InnerText = note.Title;
            workNode.AppendChild(Node1);

            XmlElement Node2 = doc.CreateElement("Text");
            Node2.InnerText = note.Text;
            workNode.AppendChild(Node2);

            XmlElement Node3 = doc.CreateElement("Date");
            Node3.InnerText = GetDate(note.Date);
            workNode.AppendChild(Node3);

            return workNode;
        }
        public static string GetDate(DateTime date)
        {
            string year = string.Empty;
            string result = string.Empty;
            string month = string.Empty;
            string day = string.Empty;
            string hour = string.Empty;
            string minute = string.Empty;
            string second = string.Empty;

            if (date.Year < 10)
                year = "000" + date.Year.ToString();
            else
                if (date.Year < 100)
                year = "00" + date.Year.ToString();
            else if (date.Year < 1000)
                year = "0" + date.Year.ToString();
            else
                year = date.Year.ToString();

            if (date.Month < 10)
                month = '0' + date.Month.ToString();
            else
                month = date.Month.ToString();
            if (date.Day < 10)
                day = '0' + date.Day.ToString();
            else
                day = date.Day.ToString();
            if (date.Hour < 10)
                hour = '0' + date.Hour.ToString();
            else
                hour = date.Hour.ToString();
            if (date.Minute < 10)
                minute = '0' + date.Minute.ToString();
            else
                minute = date.Minute.ToString();
            if (date.Second < 10)
                second = '0' + date.Second.ToString();
            else
                second = date.Second.ToString();

            // format: 1997-06-10T00:00:00
            result += year + '-' + month + '-' + day + "T" + hour + ':' + minute + ':' + second;
            return result;
        }

        private HttpStatusCode DeleteFromAPI(int id)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}/newsblock/{1}", Settings.url, id.ToString()));
            UserController user = new UserController();
            if (Settings.token != null)
                request.Headers.Add("Authorization", "Bearer " + Settings.token);
            request.Method = "DELETE";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode;
        }
    }
}
